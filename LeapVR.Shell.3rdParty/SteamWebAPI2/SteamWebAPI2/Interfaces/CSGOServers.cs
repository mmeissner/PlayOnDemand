#region Licence
/****************************************************************
 *  Filename: CSGOServers.cs
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
using Steam.Models.CSGO;
using SteamWebAPI2.Models.CSGO;
using System.Threading.Tasks;
using System.Linq;

namespace SteamWebAPI2.Interfaces
{
    /// <summary>
    /// Represents a Steam Web API interface endpoint located at ICSGOServers_730
    /// </summary>
    public class CSGOServers : SteamWebInterface, ICSGOServers
    {
        /// <summary>
        /// Default constructor established the Steam Web API key and initializes for subsequent method calls
        /// </summary>
        /// <param name="steamWebApiKey"></param>
        public CSGOServers(string steamWebApiKey)
            : base(steamWebApiKey, "ICSGOServers_730")
        { }

        /// <summary>
        /// Maps to the Steam Web API interface/method of ICSGOServers_730/GetGameServersStatus/v1
        /// </summary>
        /// <returns></returns>
        public async Task<ServerStatusModel> GetGameServerStatusAsync()
        {
            var gameServerStatus = await CallMethodAsync<ServerStatusResultContainer>("GetGameServersStatus", 1);

            var gameServerStatusModel = AutoMapperConfiguration.Mapper.Map<ServerStatusResult, ServerStatusModel>(gameServerStatus.Result);

            return gameServerStatusModel;
        }
    }
}