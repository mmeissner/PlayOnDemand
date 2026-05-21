#region Licence
/****************************************************************
 *  Filename: LiveLeagueGamePlayerModel.cs
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
    public class LiveLeagueGamePlayerModel
    {
        public uint AccountId { get; set; }
        public string Name { get; set; }
        public int HeroId { get; set; }
        public string HeroName { get; set; }
        public string HeroAvatarImageFilePath { get; set; }
        public int Team { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        public int AssistCount { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string HeroUrl { get; set; }
        public LiveLeagueGameItemModel Item0 { get; set; }
        public LiveLeagueGameItemModel Item1 { get; set; }
        public LiveLeagueGameItemModel Item2 { get; set; }
        public LiveLeagueGameItemModel Item3 { get; set; }
        public LiveLeagueGameItemModel Item4 { get; set; }
        public LiveLeagueGameItemModel Item5 { get; set; }
        public int RespawnTimer { get; set; }
        public int NetWorth { get; set; }
        public int Gold { get; set; }
        public int Level { get; set; }
        public int LastHits { get; set; }
        public int Denies { get; set; }
        public int GoldPerMinute { get; set; }
        public int XpPerMinute { get; set; }
        public int UltimateState { get; set; }
        public int UltimateCooldown { get; set; }
    }
}