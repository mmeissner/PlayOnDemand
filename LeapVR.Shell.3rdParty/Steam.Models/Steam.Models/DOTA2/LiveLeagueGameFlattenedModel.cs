#region Licence
/****************************************************************
 *  Filename: LiveLeagueGameFlattenedModel.cs
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
using System.Collections.Generic;

namespace Steam.Models.DOTA2
{
    public class LiveLeagueGameFlattenedModel
    {
        public string LeagueName { get; set; }
        public int SpectatorCount { get; set; }
        public string RadiantTeamName { get; set; }
        public string DireTeamName { get; set; }
        public int RadiantKillCount { get; set; }
        public int DireKillCount { get; set; }
        public string ElapsedTimeDisplay { get; set; }
        public int GameNumber { get; set; }
        public int BestOf { get; set; }
        public int RadiantSeriesWins { get; set; }
        public int DireSeriesWins { get; set; }
        public string LeagueLogoPath { get; set; }
        public string RadiantTeamLogo { get; set; }
        public string DireTeamLogo { get; set; }
        public int RoshanRespawnTimer { get; set; }
        public long LobbyId { get; set; }
        public long MatchId { get; set; }
        public double StreamDelay { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerModel> Players { get; set; }
        public double ElapsedTime { get; set; }
        public TowerStateModel RadiantTowerStates { get; set; }
        public TowerStateModel DireTowerStates { get; set; }
        public IReadOnlyCollection<LiveLeagueGameHeroModel> RadiantPicks { get; set; }
        public IReadOnlyCollection<LiveLeagueGameHeroModel> DirePicks { get; set; }
        public IReadOnlyCollection<LiveLeagueGameHeroModel> RadiantBans { get; set; }
        public IReadOnlyCollection<LiveLeagueGameHeroModel> DireBans { get; set; }
        public string LeagueTier { get; set; }
    }
}