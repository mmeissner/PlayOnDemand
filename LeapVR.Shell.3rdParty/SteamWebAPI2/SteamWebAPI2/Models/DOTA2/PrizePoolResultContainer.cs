#region Licence
/****************************************************************
 *  Filename: PrizePoolResultContainer.cs
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

namespace SteamWebAPI2.Models.DOTA2
{
    internal class PrizePoolResult
    {
        [JsonProperty(PropertyName = "prize_pool")]
        public int PrizePool { get; set; }

        [JsonProperty(PropertyName = "league_id")]
        public int LeagueId { get; set; }

        public int Status { get; set; }
    }

    internal class PrizePoolResultContainer
    {
        public PrizePoolResult Result { get; set; }
    }
}