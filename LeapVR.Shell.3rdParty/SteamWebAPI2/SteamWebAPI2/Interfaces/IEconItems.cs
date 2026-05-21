#region Licence
/****************************************************************
 *  Filename: IEconItems.cs
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
using System.Threading.Tasks;
using SteamWebAPI2.Models.GameEconomy;
using Steam.Models.GameEconomy;
using Steam.Models.DOTA2;

namespace SteamWebAPI2.Interfaces
{
    public interface IEconItems
    {
        Task<EconItemResultModel> GetPlayerItemsAsync(long steamId);
        Task<SchemaModel> GetSchemaAsync(string language = "");
        Task<string> GetSchemaUrlAsync();
        Task<StoreMetaDataModel> GetStoreMetaDataAsync(string language = "");
        Task<int> GetStoreStatusAsync();
    }
}