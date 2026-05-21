#region Licence
/****************************************************************
 *  Filename: StoreStatusResultContainer.cs
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

namespace SteamWebAPI2.Models.GameEconomy
{
    internal class StoreStatusResult
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("store_status")]
        public int StoreStatus { get; set; }
    }

    internal class StoreStatusResultContainer
    {
        [JsonProperty("result")]
        public StoreStatusResult Result { get; set; }
    }
}