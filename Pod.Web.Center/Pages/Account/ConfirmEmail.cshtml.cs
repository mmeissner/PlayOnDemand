#region Licence
/****************************************************************
 *  Filename: ConfirmEmail.cshtml.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Services.User;

namespace Pod.Web.Center.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserService _userService;

        public IResult ConfirmationResult;
        public ConfirmEmailModel(UserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string username, string confirmationToken)
        {
            ConfirmationResult = await _userService.ConfirmEMail(username, confirmationToken);
            return Page();
        }
    }
}
