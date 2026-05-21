#region Licence
/****************************************************************
 *  Filename: LeagueResultContainer.cs
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

namespace SteamWebAPI2.Models.DOTA2
{
    internal class LeagueResult
    {
        public IList<League> Leagues { get; set; }
    }

    internal class League
    {
        public string Name { get; set; }
        public int LeagueId { get; set; }
        public string Description { get; set; }

        [JsonProperty(PropertyName = "tournament_url")]
        public string TournamentUrl { get; set; }

        public int ItemDef { get; set; }
    }

    internal class LeagueResultContainer
    {
        public LeagueResult Result { get; set; }
    }
}