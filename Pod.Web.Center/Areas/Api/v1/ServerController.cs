#region Licence
/****************************************************************
 *  Filename: ServerController.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pod.Data.Config;
using Pod.DtoModels;
using Pod.Services.ServerManager;
using Pod.ViewModels.ShellServer;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/internal/system/[controller]")]
    [ApiController]
    [SwaggerTag("Server Management functions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesConfig.ServerManagerRole)]
    public class ServerController : ControllerBase
    {
        private readonly ServerManagerService _serverService;
        private readonly IApplicationLifetime _appLifetime;

        public ServerController(ServerManagerService serverService, IApplicationLifetime appLifetime)
        {
            _serverService = serverService;
            _appLifetime = appLifetime;
        }

        [ProducesResponseType(typeof(ICollection<ShellServerDetailsViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet()]
        public async Task<ActionResult> GetServers()
        {
            return ResultPresenter.GetResult(await _serverService.GetAllServers());
        }

        [ProducesResponseType(typeof(ShellServerDetailsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{serverId}")]
        public async Task<ActionResult> GeServer([FromRoute, BindRequired] Guid serverId)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.GetServer(serverId));
        }


        [ProducesResponseType(typeof(ICollection<ShellServerConnectedClientViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{serverId}/stations")]
        public async Task<ActionResult> GetConnectedStations([FromRoute, BindRequired] Guid serverId)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.ViewConnectedStations(serverId));
        }

        [ProducesResponseType(typeof(ShellServerViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost()]
        public async Task<ActionResult> CreateServer([FromBody, BindRequired] RequestNewServerDto createServer)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.CreateNewServer(
                    createServer.DisplayName,
                    createServer.HostAddress,
                    createServer.HostPort,
                    createServer.InterfaceVersion));
        }


        [ProducesResponseType(typeof(ShellServerViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{serverId}/displaySettings")]
        public async Task<ActionResult> SetDisplayName(
                [FromRoute, BindRequired] Guid serverId,
                [FromBody, BindRequired] RequestServerDisplayNameUpdateDto displayNameUpdate)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(
                    await _serverService.SetDisplayName(serverId, displayNameUpdate.DisplayName));
        }

        [ProducesResponseType(typeof(ShellServerViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{serverId}/connection")]
        public async Task<ActionResult> SetConnectionSettings(
                [FromRoute, BindRequired] Guid serverId,
                [FromBody, BindRequired] RequestServerConnectionSettingsUpdateDto connectionSettingsUpdate)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.SetConnectionSettings(
                    serverId,
                    connectionSettingsUpdate.HostAddress,
                    connectionSettingsUpdate.HostPort,
                    connectionSettingsUpdate.InterfaceVersion));
        }

        [ProducesResponseType(typeof(ShellServerViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{serverId}/timing")]
        public async Task<ActionResult> SetTimingSettings(
                [FromRoute, BindRequired] Guid serverId,
                [FromBody, BindRequired] RequestServerTimeSettingsUpdateDto timeSettingsUpdate)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.SetTimeSettings(
                    serverId,
                    timeSettingsUpdate.HeartbeatInterval,
                    timeSettingsUpdate.HeartbeatTimeout,
                    timeSettingsUpdate.ConnectTimeout));
        }
        
        [ProducesResponseType(typeof(ShellServerViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{serverId}/enabled")]
        public async Task<ActionResult> SetServerEnabled(
                [FromRoute, BindRequired] Guid serverId,
                [FromQuery, BindRequired] bool isEnabled)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _serverService.SetActiveState(serverId, isEnabled));
        }
    }
}