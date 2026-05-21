#region Licence
/****************************************************************
 *  Filename: ResetPassword.cshtml.cs
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pod.DtoModels;
using Pod.Services.User;

namespace Pod.Web.Center.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserService _userService;

        public bool IsDone = false;
        public bool IsError = false;
        public ResetPasswordModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public RequestResetPasswordInputModel Input { get; set; }

       
        public IActionResult OnGet(string passwordResetToken = null, string username = null)
        {
            if (string.IsNullOrWhiteSpace(passwordResetToken)|| string.IsNullOrWhiteSpace(username))
            {
                IsError = true;
                ModelState.AddModelError(string.Empty, "A Password ResetToken must be supplied to reset your password");
                return Page();
            }
            Input = new RequestResetPasswordInputModel
            {
                            Username = username,
                            PasswordResetToken = passwordResetToken
                    };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IsDone = false;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var recoverPasswordResult = await _userService.RecoverForgottenPassword(Input.Username, Input.PasswordResetToken, Input.Password);

            if(recoverPasswordResult.HasError())
            {
                IsError = true;
                foreach (var error in recoverPasswordResult.Values)
                {
                    foreach(var errorString in error)
                    {
                        ModelState.AddModelError(string.Empty, errorString);
                    }
                }
            }
            else
            {
                IsDone = true;
            }

            return Page();
        }
    }
}
