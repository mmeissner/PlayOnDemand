#region Licence
/****************************************************************
 *  Filename: StationSupportService.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Expressions;

namespace Pod.Services.Support
{
    /// <summary>
    /// Customer Support Service for Stations
    /// </summary>
    public class StationSupportService
    {
        private readonly ILogger<StationSupportService> _logger;
        private readonly PodDbContext _podContext;

        public StationSupportService(ILogger<StationSupportService> logger, PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        /// <summary>
        /// Get all current states of a Users Station
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <returns>Collection with current state of users stations</returns>
        public async Task<IResult<ICollection<StationCurrentStateViewModel>>> GetStationsCurrentState(Guid userId)
        {
            var result = new Result<ICollection<StationCurrentStateViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if (result.HasError()) return result;
            return result.Add(
                    await _podContext.Stations.Where(x => x.ApplicationUserId == userId).
                                      Select(ToStationCurrentStateVm.FromStation()).
                                      AsNoTracking().
                                      ToArrayAsync());

        }

        /// <summary>
        /// Get finished Sessions for a Station
        /// </summary>
        /// <param name="stationId">The Stations Id</param>
        /// <param name="take">The amount of results</param>
        /// <param name="skip">The amount of results to skip</param>
        /// <returns></returns>
        public async Task<IResult<IEnumerable<SessionLogViewModel>>> GetStationsSessionLogs(Guid stationId, int take = 50, int skip = 0)
        {
            var result = new Result<IEnumerable<SessionLogViewModel>>();
            if (!result.ArgNotEmpty(stationId, nameof(stationId), UserError.StationInvalidStationId))
            {
                return result;
            }

            return result.Add(
                    (await _podContext.Sessions.
                                       Where(x => x.StationId == stationId && x.IsClosed).
                                       Include(x => x.ChangeRequests).
                                       OrderBy(x => x.RequestedOnUtc).
                                       Skip(skip).
                                       Take(take).
                                       AsNoTracking().
                                       ToArrayAsync()).Select(ToSessionLogVm.FuncFromSession));
        }

        /// <summary>
        /// Get the connection log for a Station
        /// </summary>
        /// <param name="stationId">The Stations Id</param>
        /// <param name="take">The maximum amount to return</param>
        /// <param name="skip">The amount of entries to skip</param>
        /// <returns>Collection of Log entries</returns>
        public async Task<IResult<ICollection<StationConnectionLogViewModel>>> GetStationConnectionLog(Guid stationId, int take, int skip)
        {
            var result = new Result<ICollection<StationConnectionLogViewModel>>();
            return result.Add(
                    await _podContext.ClosedConnections.
                                      Where(x => x.StationId == stationId).
                                      Skip(skip).
                                      Take(take).
                                      Select(ToConnectionLogVm.FromClosedConnection()).
                                      AsNoTracking().
                                      ToArrayAsync());
        }
    }
}
