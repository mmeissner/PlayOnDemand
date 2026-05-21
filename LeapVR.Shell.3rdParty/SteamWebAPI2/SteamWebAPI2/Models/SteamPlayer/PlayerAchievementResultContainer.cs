#region Licence
/****************************************************************
 *  Filename: PlayerAchievementResultContainer.cs
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
using System.Collections.Generic;

namespace SteamWebAPI2.Models.SteamPlayer
{
    internal class PlayerAchievement
    {
        [JsonProperty("apiname")]
        public string APIName { get; set; }

        [JsonProperty("achieved")]
        public int Achieved { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
    }

    internal class PlayerAchievementResult
    {
        [JsonProperty("steamID")]
        public string SteamId { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("achievements")]
        public IList<PlayerAchievement> Achievements { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }

    internal class PlayerAchievementResultContainer
    {
        [JsonProperty("playerstats")]
        public PlayerAchievementResult Result { get; set; }
    }
}