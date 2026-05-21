#region Licence
/****************************************************************
 *  Filename: SupportController.cs
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
using Pod.Data.Config;
using Pod.Services.Support;
using Pod.ViewModels.Customer;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/internal")]
    [ApiController]
    [SwaggerTag("Functions to provide Support for Users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = RolesConfig.CustomerSupportRole)]
    public class SupportController : ControllerBase
    {
        private readonly CustomerSupportService _supportService;
        private readonly StationSupportService _stationService;
        public SupportController(CustomerSupportService supportService, StationSupportService stationService)
        {
            _supportService = supportService;
            _stationService = stationService;
        }

        [ProducesResponseType(typeof(ICollection<UserViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("accounts")]
        public async Task<ActionResult> GetUsers(
                [FromQuery, Range(1, 1000)] int take = 50,
                [FromQuery] int skip = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _supportService.GetUsers(take, skip));
        }

        [ProducesResponseType(typeof(ICollection<UserViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("accounts/email")]
        public async Task<ActionResult> GetUsersByEmail([FromQuery, BindRequired] string emailAddress)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _supportService.FindUserByEmail(emailAddress));
        }

        [ProducesResponseType(typeof(ICollection<UserViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("accounts/{userId}/email/verified")]
        public async Task<ActionResult> SetUserVerifiedByEmail([BindRequired,FromRoute] Guid userId, [FromQuery, BindRequired] bool isVerified)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _supportService.SetEMailConfirmed(userId,isVerified));
        }

        [ProducesResponseType(typeof(ICollection<StationCurrentStateViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("accounts/{userId}/stations")]
        public async Task<ActionResult> GetStationStates([FromRoute, BindRequired] Guid userId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _stationService.GetStationsCurrentState(userId));
        }

        [ProducesResponseType(typeof(ICollection<StationConnectionLogViewModel>), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpGet("stations/{stationId}/connections")]
        public async Task<ActionResult> GetStationConnectionLog(
                [FromRoute, BindRequired] Guid stationId,
                [FromQuery, Range(1, 1000)] int take = 50,
                [FromQuery] int skip = 0)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(await _stationService.GetStationConnectionLog(stationId,take,skip));
        }

        /// <summary>
        /// Gets closed Sessions for a Station
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return ResultPresenter.GetResult(
                    await _stationService.GetStationsSessionLogs(
                            stationId,
                            take,
                            skip));
        }
    }
}
