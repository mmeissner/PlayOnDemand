#region Licence
/****************************************************************
 *  Filename: UserService.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Authentication;
using Pod.Services.System;
using Pod.ViewModels.User;

namespace Pod.Services.User
{
    /// <summary>
    /// Service to handle Accounts for Users
    /// </summary>
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly SystemSettingsService _settingsService;
        private readonly RefreshAccessTokenProviderOptions _refreshTokenOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserService(
                ILogger<UserService> logger, SignInManager<ApplicationUser> signInManager,
                SystemSettingsService settingsService,
                RefreshAccessTokenProviderOptions refreshTokenOptions)
        {
            _logger = logger;
            _settingsService = settingsService;
            _refreshTokenOptions = refreshTokenOptions;
            _userManager = signInManager.UserManager;
        }

        /// <summary>
        /// Allows to register a new User Account
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="password">The Password</param>
        /// <param name="email">The Email</param>
        /// <returns>The created User Info</returns>
        public async Task<IResult<RegisterUserViewModel>> RegisterUser(string username, string password, string email)
        {
            var result = new Result<RegisterUserViewModel>();
            if(!_settingsService.GetSystemSettings.UserRegistrationEnabled)
            {
                result.Add(
                        "Registration of new accounts is currently not allowed",
                        UserError.UserAccountRegistrationUnavailable);
            }
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(password, nameof(password), UserError.UserIdentityPasswordTooShort);
            result.ArgNotNullOrWhitespace(email, nameof(email), UserError.UserIdentityInvalidEmail);
            if(result.HasError()) return result;
            var user = new ApplicationUser {UserName = username, Email = email};
            var createUserResult = await _userManager.CreateAsync(user, password);
            if(result.Add(createUserResult).HasError()) return result;
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            result.Add(
                    new RegisterUserViewModel()
                    {
                            UserId = user.Id,
                            EMail = email,
                            EMailVerificationToken = code,
                            Username = username,
                    });
            return result;
        }

        /// <summary>
        /// Confirms an E-Mail for an User Account
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="emailConfirmationCode">The Email confirmation code</param>
        /// <returns>The Result</returns>
        public async Task<IResult> ConfirmEMail(string username, string emailConfirmationCode)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(
                    emailConfirmationCode,
                    nameof(emailConfirmationCode),
                    UserError.UserIdentityInvalidEMailConfirmationCode);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;

            var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, emailConfirmationCode);
            if(!confirmEmailResult.Succeeded)
                return result.Add(
                        "Invalid EMail Confirmation Code",
                        UserError.UserIdentityInvalidEMailConfirmationCode);

            return result;
        }

        /// <summary>
        /// Allows a user to receive a PasswordResetToken to reset the Password
        /// </summary>
        /// <param name="username">The Username</param>
        /// <returns>The Password Reset Information</returns>
        public async Task<IResult<UserForgotPasswordViewModel>> ForgotPassword(string username)
        {
            var result = new Result<UserForgotPasswordViewModel>();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            if(!result.ValueTrue(
                    await _userManager.IsEmailConfirmedAsync(user),
                    nameof(_userManager.IsEmailConfirmedAsync),
                    UserError.UserIdentityEMailUnconfirmed)) return result;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            return result.Add(
                    new UserForgotPasswordViewModel()
                    {
                            UserId = user.Id,
                            Username = user.UserName,
                            EMail = user.Email,
                            PasswordResetToken = code
                    });
        }

        /// <summary>
        /// Allows to change a Password for an User Account and invalidates the latest RefreshToken
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="currentPassword">The current Password</param>
        /// <param name="newPassword">The new Password</param>
        /// <returns>The Reset Information</returns>
        public async Task<IResult<ChangedPasswordUserViewModel>> ChangePassword(
                string username, string currentPassword, string newPassword)
        {
            var result = new Result<ChangedPasswordUserViewModel>();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(newPassword, nameof(newPassword), UserError.UserIdentityPasswordTooShort);
            result.ArgNotNullOrWhitespace(
                    currentPassword,
                    nameof(currentPassword),
                    UserError.UserIdentityPasswordTooShort);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if(result.Add(changePasswordResult).HasError()) return result;
            var identityResult = await _userManager.RemoveAuthenticationTokenAsync(
                    user,
                    _refreshTokenOptions.Name,
                    _refreshTokenOptions.RefreshTokenKey);
            if(result.Add(identityResult).IsSuccess())
            {
                var token = await _userManager.GenerateUserTokenAsync(
                        user,
                        _refreshTokenOptions.Name,
                        _refreshTokenOptions.RefreshTokenKey);
                result.Add(new ChangedPasswordUserViewModel() {RefreshToken = token});
            }

            return result.Add(identityResult);
        }

        /// <summary>
        /// Allows to set a new Password with a Password Recovery Token
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="recoveryCode">The PasswordResetToken</param>
        /// <param name="newPassword">The new Password to set</param>
        /// <returns>The Result</returns>
        public async Task<IResult> RecoverForgottenPassword(
                string username, string recoveryCode, string newPassword)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(
                    recoveryCode,
                    nameof(recoveryCode),
                    UserError.UserInvalidPasswordRecoveryCode);
            result.ArgNotNullOrWhitespace(newPassword, nameof(newPassword), UserError.UserIdentityPasswordTooShort);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, recoveryCode, newPassword);
            if(result.Add(resetPasswordResult).HasError()) return result;
            var identityResult = await _userManager.RemoveAuthenticationTokenAsync(
                    user,
                    _refreshTokenOptions.Name,
                    _refreshTokenOptions.RefreshTokenKey);
            return result.Add(identityResult);
        }

        /// <summary>
        /// Requests to Resend an Email confirmation and creates a new EMail Confirmation Token
        /// </summary>
        /// <param name="username">The Username</param>
        /// <returns>The EmailConfirmation Info</returns>
        public async Task<IResult<UserResendEmailConfirmation>> ResendEmailConfirmation(string username)
        {
            var result = new Result<UserResendEmailConfirmation>();
            if(!result.ArgNotNullOrWhitespace(
                    username,
                    nameof(username),
                    UserError.UserIdentityInvalidUserName)) return result;
            var user = await _userManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return result.Add(
                    new UserResendEmailConfirmation()
                    {
                            UserId = user.Id,
                            Username = user.UserName,
                            EMail = user.Email,
                            EMailVerificationToken = code
                    });
        }
    }
}