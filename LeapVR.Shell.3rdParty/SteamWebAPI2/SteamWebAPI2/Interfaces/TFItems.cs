#region Licence
/****************************************************************
 *  Filename: TFItems.cs
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
using Steam.Models.TF2;
using SteamWebAPI2.Models.TF2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SteamWebAPI2.Interfaces
{
    public class TFItems : SteamWebInterface, ITFItems
    {
        public TFItems(string steamWebApiKey)
            : base(steamWebApiKey, "ITFItems_440")
        {
        }

        /// <summary>
        /// Returns a collection of golden wrench and their collection details.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<GoldenWrenchModel>> GetGoldenWrenchesAsync()
        {
            var goldenWrenchesResult = await CallMethodAsync<GoldenWrenchResultContainer>("GetGoldenWrenches", 2);

            var goldenWrenchModels = AutoMapperConfiguration.Mapper.Map<IList<GoldenWrench>, IList<GoldenWrenchModel>>(goldenWrenchesResult.Result.GoldenWrenches);

            return new ReadOnlyCollection<GoldenWrenchModel>(goldenWrenchModels);
        }
    }
}