#region Licence
/****************************************************************
 *  Filename: LiveLeagueGamePlayerDetailModel.cs
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
namespace Steam.Models.DOTA2
{
    public class LiveLeagueGamePlayerDetailModel
    {
        public int PlayerSlot { get; set; }

        public uint AccountId { get; set; }

        public int HeroId { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Assists { get; set; }

        public int LastHits { get; set; }

        public int Denies { get; set; }
        public int Gold { get; set; }
        public int Level { get; set; }

        public int GoldPerMinute { get; set; }

        public int ExperiencePerMinute { get; set; }

        public int UltimateState { get; set; }

        public int UltimateCooldown { get; set; }

        public int Item0 { get; set; }
        public int Item1 { get; set; }
        public int Item2 { get; set; }
        public int Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }

        public int RespawnTimer { get; set; }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        public int NetWorth { get; set; }
    }
}