#region Licence
/****************************************************************
 *  Filename: StationsController.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Pod.DtoModels;
using Pod.Enums;
using Pod.Services.Station;
using Pod.ViewModels.Customer;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    /// <summary>
    /// Provides Functionality for remote control of an Station with an Users Access Token
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [SwaggerTag("Functions for Stations with Access Token Authentication")]
    public class StationsController : ControllerBase
    {
        private readonly ILogger<StationsController> _logger;
        private readonly StationService _stationService;
        private readonly StationApiKeyService _apiKeyService;
        public StationsController(ILogger<StationsController> logger, StationService stationService, StationApiKeyService apiKeyService)
        {
            _logger = logger;
            _stationService = stationService;
            _apiKeyService = apiKeyService;
        }

        /// <summary>
        /// Creates a Station
        /// </summary>
        /// <param name="createStation">Create Station Dto</param>
        /// <returns>Station State</returns>
        [ProducesResponseType(typeof(StationSettingsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPut()]
        public async Task<ActionResult> CreateStation([FromBody] RequestCreateStationDto createStation)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.CreateNewStation(userId, createStation.DisplayName, createStation.Password));
        }

        /// <summary>
        /// Creates a APIKey for a Station
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="keyName">The Display name for the ApiKey</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(StationApiKeyViewModel), 200),]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPut("{stationId}/apikeys")]
        public async Task<ActionResult> CreateApiKey(
                [FromRoute, BindRequired] Guid stationId,
                [FromQuery] string keyName)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _apiKeyService.CreateStationApiKey(
                            userId,
                            stationId,
                            keyName));
        }

        /// <summary>
        /// Gets all APIKeys for a Station
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<StationApiKeyViewModel>), 200),]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{stationId}/apikeys")]
        public async Task<ActionResult> GetApiKeys(
                [FromRoute, BindRequired] Guid stationId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _apiKeyService.GetStationApiKeys(
                            userId,
                            stationId));
        }

        /// <summary>
        /// Deletes a APIKey for a Station
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="publicKey">The ApiPublicKey component</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpDelete("{stationId}/apikeys/{publicKey}")]
        public async Task<ActionResult> DeleteApiKey(
                [FromRoute, BindRequired] Guid stationId, [FromRoute, BindRequired] string publicKey)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _apiKeyService.DeleteStationApiKey(
                            userId,stationId,publicKey));
        }


        /// <summary>
        /// Get the Station's State
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <returns>Station State ViewModels</returns>
        [ProducesResponseType(typeof(StationCurrentStateViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{stationId}")]
        public async Task<ActionResult> GetState([BindRequired, FromRoute] Guid stationId)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationCurrentState(userId, stationId));
        }

        /// <summary>
        /// Gets Collection of Station States
        /// </summary>
        /// <param name="networkStateFilter">Optional Network State Filter</param>
        /// <returns>Collection of Station State ViewModels</returns>
        [ProducesResponseType(typeof(IEnumerable<StationCurrentStateViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet()]
        public async Task<ActionResult> GetStates([FromQuery] NetworkState? networkStateFilter)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationsCurrentState(userId, networkStateFilter));
        }

        /// <summary>
        /// Gets the Settings of all Stations
        /// </summary>
        /// <returns>Collection of Station Settings ViewModels</returns>
        [ProducesResponseType(typeof(IEnumerable<StationSettingsViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("settings")]
        public async Task<ActionResult> GetSettings()
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationsDisplayDetails(userId));
        }

        /// <summary>
        /// Get the Settings of an Station
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <returns>Station Settings ViewModel</returns>
        [ProducesResponseType(typeof(StationSettingsViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{stationId}/settings")]
        public async Task<ActionResult> GetSettingsByStationId([BindRequired, FromRoute] Guid stationId)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(await _stationService.GetStationsDisplayDetails(userId, stationId));
        }

        /// <summary>
        /// Sets the Settings for an Station 
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="setSettingsRequest">Settings Dto</param>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/settings")]
        public async Task<ActionResult> SetSettings(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestStationSettingsDto setSettingsRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
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
        /// <param name="stationId">Station Id</param>
        /// <param name="setQrCodeRequest">QrCode</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/settings/qrcode")]
        public async Task<ActionResult> SetQrCode(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestStationQrCodeDto setQrCodeRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationQrCode(
                            userId,
                            stationId,
                            setQrCodeRequest.QrCode));
        }

        /// <summary>
        /// Sets the Operation Mode
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="setModeRequest">The Operation Mode</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/settings/mode")]
        public async Task<ActionResult> SetMode(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestStationModeDto setModeRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationMode(
                            userId,
                            stationId,
                            setModeRequest.Mode));
        }

        /// <summary>
        /// Changes the Password
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="setPasswordRequest">New Password</param>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/settings/password")]
        public async Task<ActionResult> SetPassword(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestSetStationPasswordDto setPasswordRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.SetStationPassword(userId, stationId, setPasswordRequest.Password));
        }

        /// <summary>
        /// Creates a Session
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="createSessionRequest">New Session Request Dto</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(CreatedSessionViewModel), 200),]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPut("{stationId}/sessions")]
        public async Task<ActionResult> CreateSession(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestNewStationSessionDto createSessionRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.RequestStationSession(
                            userId,
                            HttpContext.Connection.RemoteIpAddress.ToString(),
                            createSessionRequest.ToStationSessionRequest(stationId)));
        }

        /// <summary>
        /// Updates a running Session.
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="updateSessionRequest">The Update Request Dto</param>
        [ProducesResponseType(typeof(UpdatedSessionViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/sessions/current/update")]
        public async Task<ActionResult> UpdateSession(
                [FromRoute, BindRequired] Guid stationId,
                [FromBody] RequestSessionUpdateDto updateSessionRequest)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
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
        /// <param name="stationId">Station Id</param>
        [ProducesResponseType(typeof(StoppedSessionViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("{stationId}/sessions/current/stop")]
        public async Task<ActionResult> StopSession([FromRoute, BindRequired] Guid stationId)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.RequestStationSessionStop(userId, stationId));
        }

        /// <summary>
        /// Gets closed Sessions for one Station
        /// </summary>
        /// <param name="stationId">Station Id</param>
        /// <param name="take">Entries to return</param>
        /// <param name="skip">Entries to skip</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<SessionLogViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("{stationId}/sessions")]
        public async Task<ActionResult> GetSessionLogsById(
                [FromRoute, BindRequired] Guid stationId,
                [FromQuery, Range(1, 300)] int take = 50,
                [FromQuery] int skip = 0)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.GetStationsSessionLogs(
                            userId,
                            stationId,
                            take,
                            skip));
        }

        /// <summary>
        /// Gets closed Sessions from all stations
        /// </summary>
        /// <param name="take">Entries to return</param>
        /// <param name="skip">Entries to skip</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(IEnumerable<SessionLogViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("sessions")]
        public async Task<ActionResult> GetSessionLogs(
                [FromQuery, Range(1, 300)] int take = 50,
                [FromQuery] int skip = 0)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(!User.GetUserId(out var userId)) return BadRequest();
            return ResultPresenter.GetResult(
                    await _stationService.GetStationsSessionLogs(
                            userId,
                            take,
                            skip));
        }
    }
}