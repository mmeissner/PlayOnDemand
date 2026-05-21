#region Licence
/****************************************************************
 *  Filename: AdminService.cs
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
using Pod.Data.Exceptions;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Accountant;
using Pod.ViewModels.Admin;

namespace Pod.Services.Administrator
{
    /// <summary>
    /// Administrative functions to manage the system enviroment
    /// </summary>
    public class AdminService
    {
        private readonly ILogger<AdminService> _logger;

        private readonly PodDbContext _podContext;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminService(
                ILogger<AdminService> logger, PodDbContext podContext, RoleManager<ApplicationRole> roleManager,
                UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _podContext = podContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Provides all User Roles available
        /// </summary>
        /// <returns>All User Roles</returns>
        public async Task<IResult<ICollection<UserRoleViewModel>>> GetAllUserRoles()
        {
            var retval = new Result<ICollection<UserRoleViewModel>>();
            var roles = await _roleManager.Roles.ToArrayAsync();
            var retRoles = new List<UserRoleViewModel>();
            foreach(ApplicationRole role in roles)
            {
                retRoles.Add(new UserRoleViewModel {Name = role.Name});
            }
            return retval.Add(retRoles);
        }

        /// <summary>
        /// Provides the Roles assigned to an specific Account
        /// </summary>
        /// <param name="username">The Username of that account</param>
        /// <returns>The roles assigned to this account</returns>
        public async Task<IResult<ICollection<UserRoleViewModel>>> GetUserRoles(string username)
        {
            var result = new Result<ICollection<UserRoleViewModel>>();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.AdminInvalidUsername);
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.AdminRoleInvalid);
            if (result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            result.ArgNotNull(user, nameof(user), UserError.AdminUsernameNotFound);
            if (result.HasError()) return result;
            var currentRoles = await _userManager.GetRolesAsync(user);
            var retRoles = new List<UserRoleViewModel>();
            foreach (string role in currentRoles)
            {
                retRoles.Add(new UserRoleViewModel { Name = role });
            }
            return result.Add(retRoles);
        }

        /// <summary>
        /// Add a role to an User
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="roleId">The RoleId to add</param>
        /// <returns>The Result of the operation</returns>
        public async Task<Result> AddUserRole(string username, string roleId)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username,nameof(username),UserError.AdminInvalidUsername);
            result.ArgNotNullOrWhitespace(username,nameof(username),UserError.AdminRoleInvalid);
            if(result.HasError()) return result;
            var role = await _roleManager.FindByIdAsync(roleId);
            result.ArgNotNull(role,nameof(role),UserError.AdminRoleNotFound);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            result.ArgNotNull(user,nameof(user),UserError.AdminUsernameNotFound);
            if(result.HasError()) return result;
            var currentRoles = await _userManager.GetRolesAsync(user);
            if(currentRoles.Contains(role.Name)) return result;
            var identityResult = await _userManager.AddToRoleAsync(user, role.Name);
            if(identityResult.Succeeded) return result;
            foreach(IdentityError error in identityResult.Errors)
            {
                result.Add(error.Description, UserError.AdminRoleFailedToAdd);
            }
            return result;
        }

        /// <summary>
        /// Removes a Role from an User
        /// </summary>
        /// <param name="username">The Username</param>
        /// <param name="roleId">The RoleId to remove</param>
        /// <returns>the Result of the operation</returns>
        public async Task<Result> RemoveUserRole(string username, string roleId)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username,nameof(username),UserError.AdminInvalidUsername);
            result.ArgNotNullOrWhitespace(username,nameof(username),UserError.AdminRoleInvalid);
            if(result.HasError()) return result;
            var role = await _roleManager.FindByIdAsync(roleId);
            result.ArgNotNull(role,nameof(role),UserError.AdminRoleNotFound);
            if(result.HasError()) return result;
            var user = await _userManager.FindByNameAsync(username);
            result.ArgNotNull(user,nameof(user),UserError.AdminUsernameNotFound);
            if(result.HasError()) return result;
            var currentRoles = await _userManager.GetRolesAsync(user);
            if(!currentRoles.Contains(role.Name)) return result;
            var identityResult = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if(identityResult.Succeeded) return result;
            foreach(IdentityError error in identityResult.Errors)
            {
                result.Add(error.Description, UserError.AdminRoleFailedToRemove);
            }
            return result;
        }

        /// <summary>
        /// Delete a RUser
        /// </summary>
        /// <param name="username">The Username</param>
        /// <returns>the Result of the operation</returns>
        public async Task<Result> DeleteUser(string username)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.AdminInvalidUsername);
            var user = await _userManager.FindByNameAsync(username);
            result.ArgNotNull(user, nameof(user), UserError.AdminUsernameNotFound);
            if (result.HasError()) return result;
            //Gets list of Roles associated with current user
            var rolesForUser = await _userManager.GetRolesAsync(user);

            try
            {
                using (var transaction = _podContext.Database.BeginTransaction())
                {
                    if (rolesForUser.Count() > 0)
                    {
                        foreach (var item in rolesForUser.ToList())
                        {
                            // item should be the name of the role
                            var identityResult = await _userManager.RemoveFromRoleAsync(user, item);
                            if (identityResult.Succeeded) continue;
                            foreach (IdentityError error in identityResult.Errors)
                            {
                                result.Add(error.Description, UserError.AdminRoleFailedToRemove);
                            }
                        }
                    }
                    if (result.IsSuccess())
                    {
                        //Delete User
                        await _userManager.DeleteAsync(user);
                        transaction.Commit();
                    }
                }
            }
            catch(Exception exception)
            {
                result.Add($"Error during delete: {exception.Message}", UserError.AdminUserDeleteError);
            }

            return result;
        }
        public async Task<IResult> CreateSubscriptionPaymentOption(DummyViewModel mailServerSettings)
        {
            throw new NotImplementedException();
        }
        public async Task<IResult> UpdateSubscriptionPaymentOption(DummyViewModel mailServerSettings)
        {
            throw new NotImplementedException();
        }
    }
}