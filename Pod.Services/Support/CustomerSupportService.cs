#region Licence
/****************************************************************
 *  Filename: CustomerSupportService.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Accountant;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Expressions;

namespace Pod.Services.Support
{
    /// <summary>
    /// Service for Customer Support related to Accounts
    /// </summary>
    public class CustomerSupportService
    {
        private readonly PodDbContext _podContext;
        private readonly UserManager<ApplicationUser> _userManager;
        public CustomerSupportService(
                ILogger<CustomerSupportService> logger,
                PodDbContext podContext,
                UserManager<ApplicationUser> userManager)
        {
            _podContext = podContext;
            _userManager = userManager;
        }

        /// <summary>
        /// Get all User Accounts
        /// </summary>
        /// <param name="take">The maximum amount of entries to return</param>
        /// <param name="skip">The amount of entries to skip</param>
        /// <returns>Collection with User Accounts</returns>
        public async Task<IResult<ICollection<UserViewModel>>> GetUsers(int take, int skip)
        {
            var users = await _podContext.Users.Skip(skip).
                                          Take(take).
                                          Select(ToUserVm.FromApplicationUser()).
                                          AsNoTracking().
                                          ToArrayAsync();

            return new Result<ICollection<UserViewModel>>().Add(users);
        }

        public async Task<IResult<ICollection<DummyViewModel>>> GetPayedOrders(string userId, int take, int skip)
        {
            throw new NotImplementedException();
        }
        public async Task<IResult<ICollection<DummyViewModel>>> GetUnpaidOrders(string userId, int take, int skip)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the Email Confirmed state for an E-Mail of an User Account
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="isVerified">The Email Confirmed Value to set</param>
        /// <returns>The Result</returns>
        public async Task<IResult> SetEMailConfirmed(Guid userId, bool isVerified)
        {
            var result = new Result();
            if (result.HasError()) return result;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            user.EmailConfirmed = isVerified;
            await _podContext.SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Searches all Users by an Email Address
        /// </summary>
        /// <param name="email">The Email Address</param>
        /// <returns>All Users registered with this Email Address</returns>
        public async Task<IResult<ICollection<UserViewModel>>> FindUserByEmail(string email)
        {
            var result = new Result<ICollection<UserViewModel>>();
            result.ArgNotNullOrWhitespace(email, nameof(email));
            if (result.HasError()) return result;
            var users = await _podContext.Users.
                                          Where(x => x.Email.Equals(email)).
                                          Select(ToUserVm.FromApplicationUser()).
                                          AsNoTracking().
                                          ToArrayAsync();
            return result.Add(users);

        }
        
        public async Task<IResult<DummyViewModel>> GetOrderByPaymentReference(string paymentReference)
        {
            throw new NotImplementedException();
        }


        public async Task<IResult<DummyViewModel>> Reset2FactoryAuthentication(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<IResult<DummyViewModel>> ResetPassword(string userId) { throw new NotImplementedException(); }
    }
}