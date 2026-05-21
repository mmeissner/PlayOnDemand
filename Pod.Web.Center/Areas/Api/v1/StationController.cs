#region Licence
/****************************************************************
 *  Filename: StationController.cs
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Pod.DtoModels;
using Pod.Services.Station;
using Pod.ViewModels.Customer;
using Pod.Web.Authentication.ApiKeySecret;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiKeySecretHandler.AuthenticationScheme)]
    [SwaggerTag("Functions for Stations with API Key/Secret Authentication")]
    public class StationController : ControllerBase
    {
        private readonly ILogger<StationController> _logger;
        private readonly StationService _stationService;
        public StationController(ILogger<StationController> logger, StationService stationService)
        {
            _logger = logger;
            _stationService = stationService;
        }

        /// <summary>
        /// Get the Station's State
        /// </summary>
        /// <returns>Station State ViewModels</returns>
        [ProducesResponseType(typeof(StationCurrentStateViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet()]
        public async Task<ActionResult> GetState()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationCurrentState(userId, stationId));
        }

        /// <summary>
        /// Get the Settings of an Station
        /// </summary>
        /// <returns>Station Settings ViewModel</returns>
        [ProducesResponseType(typeof(StationSettingsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("settings")]
        public async Task<ActionResult> GetSettings()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationsDisplayDetails(userId, stationId));
        }

        /// <summary>
        /// Sets the Settings for an Station 
        /// </summary>
        /// <param name="setSettingsRequest">Settings Dto</param>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("settings")]
        public async Task<ActionResult> SetSettings([FromBody] RequestStationSettingsDto setSettingsRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationSettings(
                            userId,
                            stationId,
                            setSettingsRequest.DisplayName,
                            setSettingsRequest.Mode,
                            setSettingsRequest.QrCode));
        }

        /// <summary>
        /// Sets the QrCode
        /// </summary>
        /// <param name="setQrCodeRequest">QrCode</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("settings/qrcode")]
        public async Task<ActionResult> SetQrCode([FromBody] RequestStationQrCodeDto setQrCodeRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationQrCode(
                            userId,
                            stationId,
                            setQrCodeRequest.QrCode));
        }

        /// <summary>
        /// Sets the Operation Mode
        /// </summary>
        /// <param name="setModeRequest">The Operation Mode</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("settings/mode")]
        public async Task<ActionResult> SetMode([FromBody] RequestStationModeDto setModeRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationMode(
                            userId,
                            stationId,
                            setModeRequest.Mode));
        }

        /// <summary>
        /// Creates a Session
        /// </summary>
        /// <param name="createSessionRequest">New Session Request Dto</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(CreatedSessionViewModel), 200),]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPut("{stationId}/sessions")]
        public async Task<ActionResult> CreateSession([FromBody] RequestNewStationSessionDto createSessionRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.RequestStationSession(
                            userId,
                            HttpContext.Connection.RemoteIpAddress.ToString(),
                            createSessionRequest.ToStationSessionRequest(stationId)));
        }

        /// <summary>
        /// Updates a running Session.
        /// </summary>
        /// <param name="updateSessionRequest">The Update Request Dto</param>
        [ProducesResponseType(typeof(UpdatedSessionViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/sessions/current/update")]
        public async Task<ActionResult> UpdateSession([FromBody] RequestSessionUpdateDto updateSessionRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.RequestStationSessionChange(
                            userId,
                            stationId,
                            HttpContext.Connection.RemoteIpAddress.ToString(),
                            updateSessionRequest.ToChangeRequest()));
        }

        /// <summary>
        /// Stops a running Session
        /// </summary>
        [ProducesResponseType(typeof(StoppedSessionViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/sessions/current/stop")]
        public async Task<ActionResult> StopSession()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.RequestStationSessionStop(userId, stationId));
        }

        /// <summary>
        /// Gets closed Sessions a Station
        /// </summary>
        /// <param name="take">Entries to return</param>
        /// <param name="skip">Entries to skip</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<SessionLogViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{stationId}/sessions")]
        public async Task<ActionResult> GetSessionLogsById(
                [FromQuery, Range(1, 300)] int take = 50,
                [FromQuery] int skip = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetStationApiKeyData(out var userId, out var stationId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.GetStationsSessionLogs(
                            userId,
                            stationId,
                            take,
                            skip));
        }
    }
}
