#region Licence
/****************************************************************
 *  Filename: UserStatsForGameResultContainer.cs
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

namespace SteamWebAPI2.Models
{
    internal class UserStat
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    internal class UserStatAchievement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("achieved")]
        public int Achieved { get; set; }
    }

    internal class UserStatsForGameResult
    {
        [JsonProperty("steamID")]
        public string SteamId { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("stats")]
        public IList<UserStat> Stats { get; set; }

        [JsonProperty("achievements")]
        public IList<UserStatAchievement> Achievements { get; set; }
    }

    internal class UserStatsForGameResultContainer
    {
        [JsonProperty("playerstats")]
        public UserStatsForGameResult Result { get; set; }
    }
}