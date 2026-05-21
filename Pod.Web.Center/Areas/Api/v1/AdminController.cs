#region Licence
/****************************************************************
 *  Filename: AdminController.cs
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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Pod.Data.Config;
using Pod.Data.Infrastructure;
using Pod.DtoModels;
using Pod.Services.Administrator;
using Pod.Services.System;
using Pod.ViewModels;
using Pod.ViewModels.Admin;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/internal")]
    [ApiController]
    [SwaggerTag("Administrative functions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesConfig.AdministratorRole)]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly SystemSettingsService _systemService;
        private readonly IApplicationLifetime _appLifetime;
        public AdminController(AdminService adminService, SystemSettingsService systemService, IApplicationLifetime appLifetime)
        {
            _adminService = adminService;
            _systemService = systemService;
            _appLifetime = appLifetime;
        }

        /// <summary>
        /// Get all available User Roles
        /// </summary>
        [ProducesResponseType(typeof(ICollection<UserRoleViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("users/roles")]
        public async Task<ActionResult> GetAllRoles()
        {
            var result = await _adminService.GetAllUserRoles();
            return ResultPresenter.GetResult(result);
        }

        /// <summary>
        /// Get all roles of a user
        /// </summary>
        [ProducesResponseType(typeof(ICollection<UserRoleViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("users/{username}/roles")]
        public async Task<ActionResult> GetUserRoles([BindRequired, FromRoute] string username)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _adminService.GetUserRoles(username));
        }

        /// <summary>
        /// Adds a user to the specified Role
        /// </summary>
        /// <param name="addUserToRole">the role</param>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("roles/add")]
        public async Task<ActionResult> AddUserToRole(
                [FromBody] RequestAddRemoveUserToRoleDto addUserToRole)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _adminService.AddUserRole(addUserToRole.Username, addUserToRole.Role);
            return ResultPresenter.GetResult(result);
        }

        /// <summary>
        /// Removes a user from the specified Role
        /// </summary>
        /// <param name="removeUserFromRole">the role</param>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("roles/remove")]
        public async Task<ActionResult> RemoveUserFromRole(
                [FromBody] RequestAddRemoveUserToRoleDto removeUserFromRole)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _adminService.RemoveUserRole(removeUserFromRole.Username, removeUserFromRole.Role);
            return ResultPresenter.GetResult(result);
        }

        /// <summary>
        /// Deletes a user
        /// </summary>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpDelete("users/{username}")]
        public async Task<ActionResult> DeleteUser([BindRequired, FromRoute] string username)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _adminService.DeleteUser(username);
            return ResultPresenter.GetResult(result);
        }

        /// <summary>
        /// Initializes the Shutdown of the Server
        /// </summary>
        // POST api/v1/internal/admin/system/shutdown
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/shutdown")]
        public ActionResult ShutdownServer()
        {
            _appLifetime.StopApplication();
            return ResultPresenter.GetResult(new Result());
        }

        /// <summary>
        /// Sets the current system settings
        /// </summary>
        // GET api/v1/internal/admin/system/settings
        [ProducesResponseType(typeof(SystemSettingsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("system/settings")]
        public ActionResult GetSystemSettings()
        {
            return ResultPresenter.GetResult(new Result<SystemSettingsViewModel>().Add(_systemService.GetSystemSettings));
        }


        /// <summary>
        /// Gets the current system settings
        /// </summary>
        // GET api/v1/internal/admin/system/settings
        [ProducesResponseType(typeof(SystemSettingsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("system/settings")]
        public ActionResult SetSystemSettings([Required,FromBody]RequestSetSystemSettings settings)
        {
            var newSettings = _systemService.SetSystemSettings(
                    settings.UserRegistrationEnabled,
                    settings.MaxStationsPerUser);
            return ResultPresenter.GetResult(new Result<SystemSettingsViewModel>().Add(newSettings));
        }
    }
}
