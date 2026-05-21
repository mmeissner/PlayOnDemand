#region Licence
/****************************************************************
 *  Filename: SchemaURLResultContainer.cs
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
    internal class SchemaUrlResult
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("items_game_url")]
        public string ItemsGameUrl { get; set; }
    }

    internal class SchemaUrlResultContainer
    {
        [JsonProperty("result")]
        public SchemaUrlResult Result { get; set; }
    }
}