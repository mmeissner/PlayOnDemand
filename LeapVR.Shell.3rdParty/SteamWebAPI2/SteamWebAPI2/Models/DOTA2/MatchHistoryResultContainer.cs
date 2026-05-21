#region Licence
/****************************************************************
 *  Filename: MatchHistoryResultContainer.cs
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

namespace SteamWebAPI2.Models.DOTA2
{
    internal class MatchHistoryMatch
    {
        [JsonProperty(PropertyName = "match_id")]
        public long MatchId { get; set; }

        [JsonProperty(PropertyName = "match_seq_num")]
        public int MatchSequenceNumber { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        public DateTime StartTime { get; set; }

        [JsonProperty(PropertyName = "lobby_type")]
        public int LobbyType { get; set; }

        [JsonProperty(PropertyName = "radiant_team_id")]
        public int RadiantTeamId { get; set; }

        [JsonProperty(PropertyName = "dire_team_id")]
        public int DireTeamId { get; set; }

        public List<MatchHistoryPlayer> Players { get; set; }
    }

    internal class MatchHistoryPlayer
    {
        [JsonProperty(PropertyName = "account_id")]
        public uint AccountId { get; set; }

        [JsonProperty(PropertyName = "player_slot")]
        public int PlayerSlot { get; set; }

        [JsonProperty(PropertyName = "hero_id")]
        public int HeroId { get; set; }
    }

    internal class MatchHistoryResult
    {
        public int Status { get; set; }

        [JsonProperty(PropertyName = "num_results")]
        public int NumResults { get; set; }

        [JsonProperty(PropertyName = "total_results")]
        public int TotalResults { get; set; }

        [JsonProperty(PropertyName = "results_remaining")]
        public int ResultsRemaining { get; set; }

        public IList<MatchHistoryMatch> Matches { get; set; }
    }

    internal class MatchHistoryResultContainer
    {
        public MatchHistoryResult Result { get; set; }
    }
}