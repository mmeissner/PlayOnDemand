#region Licence
/****************************************************************
 *  Filename: AccountModels.cs
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
using System.ComponentModel.DataAnnotations;

namespace Pod.DtoModels
{

    public class RequestForgotPasswordDto
    {
        /// <summary>
        /// The Username for the new Account
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }
    }

    public class RequestForgotPasswordInputModel
    {
        [Display(Name = "Username")]
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }
    }

    public class RequestResendConfirmationEmailDto
    {
        /// <summary>
        /// The Username for the new Account
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }
    }


    /// <summary>
    /// New User Account Register Model
    /// </summary>
    public class RequestRegisterUserDto
    {
        /// <summary>
        /// The Username for the new Account
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }

        /// <summary>
        /// The E-Mail Address for the new Account
        /// </summary>
        [Required(), EmailAddress(), MinLength(6),MaxLength(100)]
        public string EMailAddress { get; set; }

        /// <summary>
        /// The Password for the new Account
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string Password { get; set; }
    }

    /// <summary>
    /// EMail Confirmation Model
    /// Allows the User to confirm his EMail by providing the
    /// EMailConfirmationToken send to his e-mail
    /// </summary>
    public class RequestEmailConfirmationDto
    {
        /// <summary>
        /// The Username for the Account to confirm the email
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }

        /// <summary>
        /// The Token provided by e-mail for confirmation
        /// </summary>
        [Required, MinLength(100), MaxLength(350)]
        public string EmailConfirmationToken { get; set; }
    }

    /// <summary>
    /// Reset Password Model
    /// Allows to Reset a Password in case it was forgotten
    /// </summary>
    public class RequestResetPasswordDto
    {
        /// <summary>
        /// The Username for the Account to reset the password
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }

        /// <summary>
        /// The Token provided by e-mail for the password reset
        /// </summary>
        [Required, MinLength(100), MaxLength(350)]
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// The new Password to be set
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Reset Password Model through a Page Input
    /// Allows to Reset a Password in case it was forgotten
    /// </summary>
    public class RequestResetPasswordInputModel
    {
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }

        [Required]
        [StringLength(80, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [StringLength(350, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 100)]
        public string PasswordResetToken { get; set; }
    }

    /// <summary>
    /// Password Change Request Model
    /// Allows to change a Password for an User Account
    /// </summary>
    public class RequestChangePasswordDto
    {
        /// <summary>
        /// The current Password
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// The new Password
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string NewPassword { get; set; }
    }
}
