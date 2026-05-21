#region Licence
/****************************************************************
 *  Filename: GlobalStatsForGameResultContainer.cs
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
using Newtonsoft.Json;
using SteamWebAPI2.Utilities.JsonConverters;
using System.Collections.Generic;

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class GlobalStat
    {
        public string Name { get; set; }
        public int Total { get; set; }
    }

    internal class GlobalStatsForGameResult
    {
        [JsonConverter(typeof(GlobalStatJsonConverter))]
        [JsonProperty("globalstats")]
        public IList<GlobalStat> GlobalStats { get; set; }

        [JsonProperty("result")]
        public int Status { get; set; }
    }

    internal class GlobalStatsForGameResultContainer
    {
        [JsonProperty("response")]
        public GlobalStatsForGameResult Result { get; set; }
    }
}