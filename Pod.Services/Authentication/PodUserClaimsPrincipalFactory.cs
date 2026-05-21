#region Licence
/****************************************************************
 *  Filename: PodUserClaimsPrincipalFactory.cs
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Pod.Data.Models.Users;

namespace Pod.Services.Authentication
{
    /// <summary>
    /// Factory for User Claims
    /// </summary>
    public class PodUserClaimsPrincipalFactory: UserClaimsPrincipalFactory <ApplicationUser>
    {
        public PodUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, 
                IOptions<IdentityOptions> optionsAccessor):base(userManager, optionsAccessor)
        {}
        /// <summary>
        /// Creates an UserClaim with domain specific Claims
        /// </summary>
        /// <param name="user">The Application User to create a Claim for</param>
        /// <returns></returns>
        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await base.CreateAsync(user);
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(PodClaimsTypes.UserId,user.Id.ToString()));
            return principal;
        }
    }
}
