#region Licence
/****************************************************************
 *  Filename: CommunityBadgeProgressResultContainer.cs
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
    internal class BadgeQuest
    {
        [JsonProperty("questid")]
        public int QuestId { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }
    }

    internal class CommunityBadgeProgressResult
    {
        [JsonProperty("quests")]
        public IList<BadgeQuest> Quests { get; set; }
    }

    internal class CommunityBadgeProgressResultContainer
    {
        [JsonProperty("response")]
        public CommunityBadgeProgressResult Result { get; set; }
    }
}