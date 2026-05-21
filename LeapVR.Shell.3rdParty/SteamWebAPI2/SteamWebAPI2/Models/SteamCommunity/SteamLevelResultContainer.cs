#region Licence
/****************************************************************
 *  Filename: SteamLevelResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class SteamLevelResult
    {
        [JsonProperty("player_level")]
        public int PlayerLevel { get; set; }
    }

    internal class SteamLevelResultContainer
    {
        [JsonProperty("response")]
        public SteamLevelResult Result { get; set; }
    }
}