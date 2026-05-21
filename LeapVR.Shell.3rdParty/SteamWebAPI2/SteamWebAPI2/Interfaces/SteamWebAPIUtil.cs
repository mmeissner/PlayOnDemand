#region Licence
/****************************************************************
 *  Filename: SteamWebAPIUtil.cs
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
using Steam.Models;
using SteamWebAPI2.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SteamWebAPI2.Interfaces
{
    public class SteamWebAPIUtil : SteamWebInterface, ISteamWebAPIUtil
    {
        public SteamWebAPIUtil(string steamWebApiKey)
            : base(steamWebApiKey, "ISteamWebAPIUtil")
        { }

        /// <summary>
        /// Returns the Steam Servers' known dates and times.
        /// </summary>
        /// <returns></returns>
        public async Task<SteamServerInfoModel> GetServerInfoAsync()
        {
            var steamServerInfo = await CallMethodAsync<SteamServerInfo>("GetServerInfo", 1);

            var steamServerInfoModel = AutoMapperConfiguration.Mapper.Map<SteamServerInfo, SteamServerInfoModel>(steamServerInfo);

            return steamServerInfoModel;
        }

        /// <summary>
        /// Returns a collection of data related to all available supported Steam Web API endpoints.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<SteamInterfaceModel>> GetSupportedAPIListAsync()
        {
            var steamApiListContainer = await CallMethodAsync<SteamApiListContainer>("GetSupportedAPIList", 1);

            var steamApiListModel = AutoMapperConfiguration.Mapper.Map<IList<SteamInterface>, IList<SteamInterfaceModel>>(steamApiListContainer.Result.Interfaces);

            return new ReadOnlyCollection<SteamInterfaceModel>(steamApiListModel);
        }
    }
}