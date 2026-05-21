#region Licence
/****************************************************************
 *  Filename: GoldenWrenchResultContainer.cs
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
using System;
using System.Collections.Generic;

namespace SteamWebAPI2.Models.TF2
{
    internal class GoldenWrench
    {
        [JsonProperty("steamID")]
        public object SteamId { get; set; }

        [JsonConverter(typeof(UnixTimeJsonConverter))]
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("itemID")]
        public int ItemId { get; set; }

        [JsonProperty("wrenchNumber")]
        public int WrenchNumber { get; set; }
    }

    internal class GoldenWrenchResult
    {
        [JsonProperty("wrenches")]
        public IList<GoldenWrench> GoldenWrenches { get; set; }
    }

    internal class GoldenWrenchResultContainer
    {
        [JsonProperty("results")]
        public GoldenWrenchResult Result { get; set; }
    }
}