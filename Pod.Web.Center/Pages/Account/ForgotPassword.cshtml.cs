#region Licence
/****************************************************************
 *  Filename: ForgotPassword.cshtml.cs
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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.DtoModels;
using Pod.Enums;
using Pod.Services.Email;
using Pod.Services.User;
using Pod.Web.Center.Presenter;

namespace Pod.Web.Center.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserService _userService;
        private readonly EMailService _emailSender;

        public bool IsEmailSend = false;
        public ForgotPasswordModel(UserService userService, EMailService emailSender)
        {
            _userService = userService;
            _emailSender = emailSender;
        }

        [BindProperty]
        public RequestForgotPasswordInputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            IsEmailSend = false;
            if (ModelState.IsValid)
            {
                var passwordResult = await _userService.ForgotPassword(Input.Username);
                if(passwordResult.HasError())
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong, please check your username and if your email is confirmed");
                }
                else
                {
                    var sendMailResult = await _emailSender.CreateEmailSendOrder(
                            EMailTemplateIdentifier.ResetPassword,
                            passwordResult.ReturnValue.EMail,
                            this.MailVariables().ForRequestPassword(
                                    passwordResult.ReturnValue.PasswordResetToken,
                                    passwordResult.ReturnValue.Username,
                                    passwordResult.ReturnValue.EMail));
                    if(sendMailResult.HasError())
                    {
                        foreach (var error in sendMailResult.Values)
                        {
                            foreach (var errorString in error)
                            {
                                ModelState.AddModelError(string.Empty, errorString);
                            }
                        }
                    }
                    else
                    {
                        IsEmailSend = true;
                    }
                }
            }
            return Page();
        }
    }
}
