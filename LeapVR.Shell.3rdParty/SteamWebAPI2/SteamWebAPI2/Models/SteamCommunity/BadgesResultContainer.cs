#region Licence
/****************************************************************
 *  Filename: BadgesResultContainer.cs
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
    internal class Badge
    {
        [JsonProperty("badgeid")]
        public int BadgeId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("completion_time")]
        public int CompletionTime { get; set; }

        [JsonProperty("xp")]
        public int Xp { get; set; }

        [JsonProperty("scarcity")]
        public int Scarcity { get; set; }

        [JsonProperty("appid")]
        public int? AppId { get; set; }

        [JsonProperty("communityitemid")]
        public string CommunityItemId { get; set; }

        [JsonProperty("border_color")]
        public int? BorderColor { get; set; }
    }

    internal class BadgesResult
    {
        [JsonProperty("badges")]
        public IList<Badge> Badges { get; set; }

        [JsonProperty("player_xp")]
        public int PlayerXp { get; set; }

        [JsonProperty("player_level")]
        public int PlayerLevel { get; set; }

        [JsonProperty("player_xp_needed_to_level_up")]
        public int PlayerXpNeededToLevelUp { get; set; }

        [JsonProperty("player_xp_needed_current_level")]
        public int PlayerXpNeededCurrentLevel { get; set; }
    }

    internal class BadgesResultContainer
    {
        [JsonProperty("response")]
        public BadgesResult Result { get; set; }
    }
}