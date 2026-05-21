#region Licence
/****************************************************************
 *  Filename: SteamStore.cs
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
using Steam.Models.SteamStore;
using SteamWebAPI2.Models.SteamStore;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamWebAPI2.Utilities;
using System;
using System.Threading;

namespace SteamWebAPI2.Interfaces
{
    public class SteamStore : SteamStoreInterface, ISteamStore
    {
        public async Task<StoreAppDetailsDataModel> GetStoreAppDetailsAsync(
                int appId, string language = null, string contryCode = null)
        {
            var retval = await GetStoreAppDetailsAsync(appId, language, contryCode,null);
            return retval;
        }
        /// <summary>
        /// Maps to the steam store api endpoint: GET http://store.steampowered.com/api/appdetails/
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="language"></param>
        /// <param name="contryCode"></param>
        /// <returns></returns>
        public async Task<StoreAppDetailsDataModel> GetStoreAppDetailsAsync(int appId, string language = null, string contryCode = null,TimeSpan? timeout = null)
        {
            List<SteamWebRequestParameter> parameters = new List<SteamWebRequestParameter>();

            parameters.AddIfHasValue(appId, "appids");
            parameters.AddIfHasValue(language, "l");
            parameters.AddIfHasValue(contryCode, "cc");

            var appDetails = await CallMethodAsync<AppDetailsContainer>("appdetails", parameters,timeout);

            // Steam returns 200 OK with {"<appid>":{"success":false}} for delisted,
            // region-locked, family-share-only, or unauthenticated-query titles.
            // The container's Data is null in that case - dereferencing it would NRE.
            if (appDetails == null || appDetails.Data == null) return null;

            var appDetailsModel = AutoMapperConfiguration.Mapper.Map<Data, StoreAppDetailsDataModel>(appDetails.Data);

            return appDetailsModel;
        }

        /// <summary>
        /// Maps to the steam store api endpoint: GET http://store.steampowered.com/api/featuredcategories/
        /// </summary>
        /// <returns></returns>
        public async Task<StoreFeaturedCategoriesModel> GetStoreFeaturedCategoriesAsync()
        {
            var featuredCategories = await CallMethodAsync<FeaturedCategoriesContainer>("featuredcategories");

            var featuredCategoriesModel = AutoMapperConfiguration.Mapper.Map<FeaturedCategoriesContainer, StoreFeaturedCategoriesModel>(featuredCategories);

            return featuredCategoriesModel;
        }

        /// <summary>
        /// Maps to the steam store api endpoint: GET http://store.steampowered.com/api/featured/
        /// </summary>
        /// <returns></returns>
        public async Task<StoreFeaturedProductsModel> GetStoreFeaturedProductsAsync()
        {
            var featuredProducts = await CallMethodAsync<FeaturedProductsContainer>("featured");

            var featuredProductsModel = AutoMapperConfiguration.Mapper.Map<FeaturedProductsContainer, StoreFeaturedProductsModel>(featuredProducts);

            return featuredProductsModel;
        }
    }
}
