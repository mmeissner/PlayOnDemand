#region Licence
/****************************************************************
 *  Filename: GlobalAchievementPercentagesResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class GlobalAchievementPercentage
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("percent")]
        public double Percent { get; set; }
    }

    internal class GlobalAchievementPercentagesResult
    {
        [JsonProperty("achievements")]
        public IList<GlobalAchievementPercentage> AchievementPercentages { get; set; }
    }

    internal class GlobalAchievementPercentagesResultContainer
    {
        [JsonProperty("achievementpercentages")]
        public GlobalAchievementPercentagesResult Result { get; set; }
    }
}