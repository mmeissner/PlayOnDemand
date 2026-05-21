#region Licence
/****************************************************************
 *  Filename: TemplateVariableKey.cs
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
namespace Pod.Enums {
    public enum TemplateVariableKey
    {
        #region Reserved
        //0-99
        #endregion

        #region System
        /// <summary>
        /// Web root from config as https://www.mydomain.com/
        /// </summary>
        WebHostRoot = 100,
        #endregion

        #region User
        /// <summary>
        /// The Username
        /// </summary>
        Username = 200,
        /// <summary>
        /// The Users EMail Address
        /// </summary>
        UserEmailAddress,
        #endregion

        #region Tokens
        /// <summary>
        /// The Token for an email verification
        /// </summary>
        EMailVerificationToken = 300,
        /// <summary>
        /// The Token for an password reset
        /// </summary>
        PasswordResetToken,
        #endregion

        #region Links
        /// <summary>
        /// A link for a email verification reset including all required data as query params
        /// </summary>
        EMailVerificationTokenLink = 400,
        /// <summary>
        /// A link for a Password reset including all required data as query params
        /// </summary>
        PasswordResetTokenLink
        #endregion
    }

    /// <summary>
    /// Scope for variables for Emails
    /// </summary>
    public enum EmailVariableType
    {
        /// <summary>
        /// Variable is in the subject
        /// </summary>
        Subject,
        /// <summary>
        /// Variable is in the Textual Content
        /// </summary>
        Content,
        /// <summary>
        /// Variable is in the HTML Content
        /// </summary>
        ContentHtml
    }
}