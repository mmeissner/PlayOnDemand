#region Licence
/****************************************************************
 *  Filename: AccountsController.cs
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
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Pod.Data.Infrastructure;
using Pod.DtoModels;
using Pod.Enums;
using Pod.Services.Email;
using Pod.Services.User;
using Pod.ViewModels;
using Pod.ViewModels.User;
using Pod.Web.Center.Config;
using Pod.Web.Center.Presenter;
using Pod.Web.Center.Swagger.Examples;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [SwaggerTag("User Account functions")]
    public class AccountsController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EMailService _emailService;
        public AccountsController(UserService userService, EMailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        /// <summary>
        /// Registers a User Account
        /// </summary>
        /// <param name="registerRequest"></param>
        // POST api/v1/account/register
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 429)]
        [HttpPost("register")]
        public async Task<ActionResult> Register([BindRequired, FromBody] RequestRegisterUserDto registerRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var retval = new Result();
            var registerResult = await
                    _userService.RegisterUser(
                            registerRequest.Username,
                            registerRequest.Password,
                            registerRequest.EMailAddress);
            if(registerResult.HasError()) return ResultPresenter.GetResult(retval.Add(registerResult));

            retval.Add(
                    await _emailService.CreateEmailSendOrder(
                            EMailTemplateIdentifier.RegisterAccount,
                            registerResult.ReturnValue.EMail,
                            this.MailVariables().
                                 ForEmailConfirmation(
                                         registerResult.ReturnValue.EMailVerificationToken,
                                         registerResult.ReturnValue.Username,
                                         registerResult.ReturnValue.EMail)));

            return ResultPresenter.GetResult(retval);
        }

        /// <summary>
        /// Changes the Password and provides a new Refresh Token
        /// </summary>
        /// <param name="changePasswordRequest">The change password request.</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ChangedPasswordUserViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("password/change")]
        public async Task<ActionResult<ChangedPasswordUserViewModel>> ChangePassword(
                [BindRequired, FromBody] RequestChangePasswordDto changePasswordRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            string username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(username == null) return BadRequest();

            return ResultPresenter.GetResult(
                    await
                            _userService.ChangePassword(
                                    username,
                                    changePasswordRequest.CurrentPassword,
                                    changePasswordRequest.NewPassword));
        }

        /// <summary>
        /// Receives a Reset Token that is required to reset the Password
        /// </summary>
        /// <param name="forgotPasswordRequest">Request object holding the required parameter</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 429)]
        [HttpPost("password/forgot")]
        public async Task<ActionResult> RequestPassword(
                [BindRequired, FromBody] RequestForgotPasswordDto forgotPasswordRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            var retval = new Result();
            var passwordResult = await _userService.ForgotPassword(forgotPasswordRequest.Username);
            if(passwordResult.HasError()) return ResultPresenter.GetResult(retval.Add(passwordResult));
            retval.Add(
                    await _emailService.CreateEmailSendOrder(
                            EMailTemplateIdentifier.ResetPassword,
                            passwordResult.ReturnValue.EMail,
                            this.MailVariables().
                                 ForRequestPassword(
                                         passwordResult.ReturnValue.PasswordResetToken,
                                         passwordResult.ReturnValue.Username,
                                         passwordResult.ReturnValue.EMail)));
            return ResultPresenter.GetResult(retval);
        }

        /// <summary>
        /// Resets the Password with an Reset Token
        /// </summary>
        /// <param name="resetPasswordRequest">The request reset password.</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 429)]
        [HttpPost("password/reset")]
        public async Task<ActionResult> ResetPassword(
                [BindRequired, FromBody] RequestResetPasswordDto resetPasswordRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            return ResultPresenter.GetResult(
                    await
                            _userService.RecoverForgottenPassword(
                                    resetPasswordRequest.Username,
                                    resetPasswordRequest.PasswordResetToken,
                                    resetPasswordRequest.NewPassword));
        }

        /// <summary>
        /// Confirms ownership of an E-Mail Address by an E-Mail Confirmation Token
        /// </summary>
        /// <param name="emailConfirmationRequest">The request email confirmation.</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 429)]
        [HttpPost("email/confirmation")]
        public async Task<ActionResult> ConfirmEmail(
                [BindRequired, FromBody] RequestEmailConfirmationDto emailConfirmationRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await
                            _userService.ConfirmEMail(
                                    emailConfirmationRequest.Username,
                                    emailConfirmationRequest.EmailConfirmationToken));
        }

        /// <summary>
        /// Re-sends a message with a EMail Confirmation Token.
        /// </summary>
        /// <param name="resendConfirmationEmailRequest">The resend email confirmation request.</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [ProducesResponseType(typeof(string), 429)]
        [HttpPost("email/confirmation/send")]
        public async Task<ActionResult> ResendEMailConfirmation(
                [BindRequired, FromBody] RequestResendConfirmationEmailDto resendConfirmationEmailRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var retval = new Result();
            var resendEmailConfirmationResult = await
                    _userService.ResendEmailConfirmation(resendConfirmationEmailRequest.Username);
            if(resendEmailConfirmationResult.HasError())
            {
                return ResultPresenter.GetResult(retval.Add(resendEmailConfirmationResult));
            }

            retval.Add(
                    await _emailService.CreateEmailSendOrder(
                            EMailTemplateIdentifier.ResendEMailVerification,
                            resendEmailConfirmationResult.ReturnValue.EMail,
                            new Dictionary<TemplateVariableKey, string>
                            {
                                    {
                                            TemplateVariableKey.EMailVerificationToken,
                                            resendEmailConfirmationResult.ReturnValue.EMailVerificationToken
                                    },
                                    {
                                            TemplateVariableKey.Username,
                                            resendEmailConfirmationResult.ReturnValue.Username
                                    },
                                    {
                                            TemplateVariableKey.UserEmailAddress,
                                            resendEmailConfirmationResult.ReturnValue.EMail
                                    }
                            }));
            return ResultPresenter.GetResult(new Result().Add(resendEmailConfirmationResult));
        }
    }
}