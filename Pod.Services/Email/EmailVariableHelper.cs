#region Licence
/****************************************************************
 *  Filename: EmailVariableHelper.cs
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
using Microsoft.AspNetCore.Mvc;
using Pod.Enums;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pod.Services.Email
{
    /// <summary>
    /// An Helper Class to setup the Variable Dictionary for different E-Mail Types
    /// </summary>
    public static class EmailVariableHelper
    {
        public static string HttpScheme { get; set; } = "https";
        public static string ResetPasswordLinkRoute { get; set; } = "/account/resetpassword";
        public static string ConfirmEmailLinkRoute { get; set; } = "/account/confirmemail";

        /// <summary>
        /// Creates a Variable Dictionary for Request Password EMails
        /// </summary>
        /// <param name="urlHelper">Url Helper from Controller or Page</param>
        /// <param name="passwordResetToken">The Password Reset Token</param>
        /// <param name="userName">The Username</param>
        /// <param name="emailAddress">The EMail Address</param>
        /// <returns>Dictionary with TemplateVariables and Values</returns>
        public static Dictionary<TemplateVariableKey, string> ForRequestPassword(
                this IUrlHelper urlHelper,
                string passwordResetToken, string userName, string emailAddress)
        {
            var callbackUrl = urlHelper.Page(
                    ResetPasswordLinkRoute,
                    values: new
                            {
                                    username = userName,
                                    passwordresettoken = passwordResetToken
                            },
                    pageHandler: null,
                    protocol: HttpScheme);
            return new Dictionary<TemplateVariableKey, string>
                   {
                           {
                                   TemplateVariableKey.PasswordResetToken,
                                   passwordResetToken
                           },
                           {
                                   TemplateVariableKey.Username,
                                   userName
                           },
                           {
                                   TemplateVariableKey.UserEmailAddress,
                                   emailAddress
                           },
                           {
                                   TemplateVariableKey.PasswordResetTokenLink,
                                   HtmlEncoder.Default.Encode(callbackUrl)
                           }
                   };
        }

        /// <summary>
        /// Creates a Variable Dictionary for Confirming EMail Addresses
        /// </summary>
        /// <param name="urlHelper">Url Helper from Controller or Page</param>
        /// <param name="emailVerificationToken">The EMail Verification Token</param>
        /// <param name="userName">The Username</param>
        /// <param name="emailAddress">The EMail Address</param>
        /// <returns>Dictionary with TemplateVariables and Values</returns>
        public static Dictionary<TemplateVariableKey, string> ForEmailConfirmation(
                this IUrlHelper urlHelper,
                string emailVerificationToken, string userName, string emailAddress)
        {
            var callbackUrl = urlHelper.Page(
                    ConfirmEmailLinkRoute,
                    values: new
                            {
                                    username = userName,
                                    confirmationToken = emailVerificationToken
                            },
                    pageHandler: null,
                    protocol: HttpScheme);

            return new Dictionary<TemplateVariableKey, string>
                   {
                           {
                                   TemplateVariableKey.EMailVerificationToken,
                                   emailVerificationToken
                           },
                           {
                                   TemplateVariableKey.Username,
                                   userName
                           },
                           {
                                   TemplateVariableKey.UserEmailAddress,
                                   emailAddress
                           },
                           {
                                   TemplateVariableKey.EMailVerificationTokenLink,
                                   HtmlEncoder.Default.Encode(callbackUrl)
                           }
                   };
        }

        /// <summary>
        /// Helper class to get the UrlHelper from an Controller
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <returns>URLHelper</returns>
        public static IUrlHelper MailVariables(this ControllerBase controller) { return controller.Url; }

        /// <summary>
        /// Helper class to get the UrlHelper from an Page
        /// </summary>
        /// <param name="pageModel">Page Model</param>
        /// <returns>URLHelper</returns>
        public static IUrlHelper MailVariables(this PageModel pageModel) { return pageModel.Url; }
    }
}