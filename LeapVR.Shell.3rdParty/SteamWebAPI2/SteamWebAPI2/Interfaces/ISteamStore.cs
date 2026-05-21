#region Licence
/****************************************************************
 *  Filename: ISteamStore.cs
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
using Steam.Models.SteamStore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamWebAPI2.Interfaces
{
    internal interface ISteamStore
    {
        Task<StoreAppDetailsDataModel> GetStoreAppDetailsAsync(int appId, string language, string contryCode, TimeSpan? timeout);
        Task<StoreAppDetailsDataModel> GetStoreAppDetailsAsync(int appId, string language, string contryCode);

        Task<StoreFeaturedCategoriesModel> GetStoreFeaturedCategoriesAsync();

        Task<StoreFeaturedProductsModel> GetStoreFeaturedProductsAsync();

        void Cancel();
    }
}