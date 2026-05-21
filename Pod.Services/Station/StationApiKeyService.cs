#region Licence
/****************************************************************
 *  Filename: StationApiKeyService.cs
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Expressions;

namespace Pod.Services.Station
{
    /// <summary>
    /// Service to manage API Keys for Stations
    /// </summary>
    public class StationApiKeyService
    {
        private readonly ILogger<StationApiKeyService> _logger;
        private readonly PodDbContext _podContext;

        /// <summary>
        /// Creates a new API Key Service
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="podContext"></param>
        public StationApiKeyService(ILogger<StationApiKeyService> logger, PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        /// <summary>
        /// Provides information about an API Key
        /// </summary>
        /// <param name="apiPublicKey">The public key</param>
        /// <returns>The APIKey data</returns>
        public async Task<IResult<StationApiKey>> GetStationApiKey(string apiPublicKey)
        {
            var result = new Result<StationApiKey>();
            Guid apiPublicKeyGuid = Guid.Empty;
            if (result.ArgNotNullOrWhitespace(apiPublicKey, nameof(apiPublicKey), UserError.StationPublicKeyInvalid))
            {
                if (!Guid.TryParseExact(apiPublicKey, "N", out apiPublicKeyGuid))
                {
                    result.Add("Invalid ApiPublicKey", UserError.StationPublicKeyInvalid);
                }
            }
            if (result.HasError()) return result;
            var publicKey = await _podContext.StationApiKeys.
                                              Where(x=>x.PublicKey == apiPublicKeyGuid).
                                              Include(x=> x.Station).
                                              FirstOrDefaultAsync();
            if (!result.ArgNotNull(publicKey, nameof(publicKey), UserError.StationPublicKeyNotFound)) return result;
            return result.Add(publicKey);
        }

        /// <summary>
        /// Provides all ApiKeys for a Station
        /// </summary>
        /// <param name="userId">The User Id the station belongs to</param>
        /// <param name="stationId">The station id</param>
        /// <returns>Collection of ApiKeys</returns>
        public async Task<IResult<IEnumerable<StationApiKeyViewModel>>> GetStationApiKeys(Guid userId, Guid stationId)
        {
            var result = new Result<IEnumerable<StationApiKeyViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if (result.HasError()) return result;
            var station = await _podContext.Stations.
                                                     Where(
                                                             x => x.Id == stationId &&
                                                                  x.ApplicationUserId == userId).
                                                     Include(x => x.ApiKeys).
                                                     SingleOrDefaultAsync();
            if (!result.ArgNotNull(station, nameof(station), UserError.StationNotFound)) return result;
            // List projection MUST NOT include the SecretKey - that's mint-only.
            return result.Add(station.ApiKeys.Select(ToStationApiKeyVm.FuncFromStationApiKeyNoSecret));
        }

        /// <summary>
        /// Creates an ApiKey for an new Station
        /// </summary>
        /// <param name="userId">The user Id the station belongs to</param>
        /// <param name="stationId">The station Id</param>
        /// <param name="displayName">A custom name for the created ApiKey</param>
        /// <returns>The created ApiKey</returns>
        public async Task<IResult<StationApiKeyViewModel>> CreateStationApiKey(Guid userId, Guid stationId, string displayName)
        {
            var result = new Result<StationApiKeyViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEqual(stationId, nameof(stationId), Guid.Empty);
            if (result.HasError()) return result;
            var station = await _podContext.Stations.
                                            Where(
                                                    x => x.Id == stationId &&
                                                         x.ApplicationUserId == userId).
                                            Include(x => x.ApiKeys).
                                            SingleOrDefaultAsync();
            if (!result.ArgNotNull(station, nameof(station), UserError.StationNotFound)) return result;
            var apiKeyResult = station.CreateStationApiKey(displayName);
            if (apiKeyResult.HasError()) return result.Add(apiKeyResult);
            await _podContext.SaveChangesAsync();
            return result.Add(ToStationApiKeyVm.FuncFromStationApiKey(apiKeyResult.ReturnValue));
        }

        /// <summary>
        /// Deletes a Station ApiKey
        /// </summary>
        /// <param name="userId">The user Id the station belongs to</param>
        /// <param name="stationId">The station Id the station belongs to</param>
        /// <param name="apiPublicKey">The public key of the api key to delete</param>
        /// <returns>The result</returns>
        public async Task<IResult> DeleteStationApiKey(Guid userId, Guid stationId, string apiPublicKey)
        {
            var result = new Result();
            Guid apiPublicKeyGuid = Guid.Empty;
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if (result.ArgNotNullOrWhitespace(apiPublicKey, nameof(apiPublicKey), UserError.StationPublicKeyInvalid))
            {
                if (!Guid.TryParseExact(apiPublicKey, "N", out apiPublicKeyGuid))
                {
                    result.Add("Invalid ApiPublicKey", UserError.StationPublicKeyInvalid);
                }
            }
            if (result.HasError()) return result;
            var publicKey = await _podContext.StationApiKeys.
                                              Where(
                                                      x => x.PublicKey == apiPublicKeyGuid &&
                                                           x.StationId == stationId &&
                                                           x.Station.ApplicationUserId == userId).
                                              Include(x => x.Station).
                                              ThenInclude(x => x.ApiKeys).FirstOrDefaultAsync();
            if (!result.ArgNotNull(publicKey, nameof(publicKey), UserError.StationPublicKeyNotFound)) return result;
            result.Add(publicKey.Station.RemoveStationApiKey(publicKey));
            if (result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
            }
            return result;
        }


        /// <summary>
        /// Deletes a Station ApiKey
        /// </summary>
        /// <param name="userId">The user Id the station belongs to</param>
        /// <param name="apiPublicKey">The public key of the api key to delete</param>
        /// <returns>The result</returns>
        public async Task<IResult> DeleteStationApiKey(Guid userId, string apiPublicKey)
        {
            var result = new Result();
            Guid apiPublicKeyGuid = Guid.Empty;
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if (!result.ArgNotNullOrWhitespace(apiPublicKey, nameof(apiPublicKey), UserError.StationPublicKeyInvalid))
            {
                if (!Guid.TryParseExact(apiPublicKey, "N", out apiPublicKeyGuid))
                {
                    result.Add("Invalid ApiPublicKey", UserError.StationPublicKeyInvalid);
                }
            }
            if (result.HasError()) return result;
            var publicKey = await _podContext.StationApiKeys.
                                              Where(
                                                      x => x.PublicKey == apiPublicKeyGuid &&
                                                           x.Station.ApplicationUserId == userId).
                                              Include(x => x.Station).
                                              ThenInclude(x => x.ApiKeys).FirstOrDefaultAsync();
            if (!result.ArgNotNull(publicKey, nameof(publicKey), UserError.StationPublicKeyNotFound)) return result;
            result.Add(publicKey.Station.RemoveStationApiKey(publicKey));
            if (result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
            }
            return result;
        }
    }
}
