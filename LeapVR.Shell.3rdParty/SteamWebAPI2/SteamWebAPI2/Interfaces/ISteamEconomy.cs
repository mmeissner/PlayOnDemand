#region Licence
/****************************************************************
 *  Filename: ISteamEconomy.cs
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
using System.Threading.Tasks;
using SteamWebAPI2.Models.SteamEconomy;
using Steam.Models.SteamEconomy;

namespace SteamWebAPI2.Interfaces
{
    public interface ISteamEconomy
    {
        Task<AssetClassInfoResultModel> GetAssetClassInfoAsync(int appId, IReadOnlyList<long> classIds, string language);
        Task<AssetPriceResultModel> GetAssetPricesAsync(int appId, string currency, string language);
    }
}