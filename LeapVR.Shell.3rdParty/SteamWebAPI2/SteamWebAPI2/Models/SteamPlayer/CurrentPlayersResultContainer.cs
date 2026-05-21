#region Licence
/****************************************************************
 *  Filename: CurrentPlayersResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamPlayer
{
    internal class CurrentPlayersResult
    {
        [JsonProperty("player_count")]
        public int PlayerCount { get; set; }

        [JsonProperty("result")]
        public int Result { get; set; }
    }

    internal class CurrentPlayersResultContainer
    {
        [JsonProperty("response")]
        public CurrentPlayersResult Result { get; set; }
    }
}