#region Licence
/****************************************************************
 *  Filename: UserViewModels.cs
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
using System;
using System.Collections.Generic;
using System.Text;

namespace Pod.ViewModels.User
{
    /// <summary>
    /// Registration for a new User
    /// </summary>
    public class RegisterUserViewModel
    {
        /// <summary>
        /// The User Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The Username of the new User
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The EMail for the new User
        /// </summary>
        public string EMail { get; set; }

        /// <summary>
        /// The EMailVerification Token to be send to the users e-mail,
        /// where the user has to verify his email with
        /// </summary>
        public string EMailVerificationToken { get; set; }
    }

    /// <summary>
    /// Forgot Password for an User Account
    /// </summary>
    public class UserForgotPasswordViewModel
    {
        /// <summary>
        /// The User Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The Username of the new User
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The EMail of the User that forgot his password
        /// </summary>
        public string EMail { get; set; }
        public string PasswordResetToken { get; set; }
    }

    /// <summary>
    /// Resend an EMail Confirmation Message
    /// </summary>
    public class UserResendEmailConfirmation
    {       
        /// <summary>
        /// The User Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The Username of the new User
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The EMail of the UserAccount to resend a e-mail confirmation
        /// </summary>
        public string EMail { get; set; }

        /// <summary>
        /// The EMail Verification Token to verify the e-mail account with
        /// </summary>
        public string EMailVerificationToken { get; set; }
    }

    /// <summary>
    /// Changed a Account Password
    /// </summary>
    public class ChangedPasswordUserViewModel
    {
        /// <summary>
        /// The new Refresh Token that was generated during the Password change
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
