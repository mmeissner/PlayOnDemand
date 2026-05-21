#region Licence
/****************************************************************
 *  Filename: MatchDetailModel.cs
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
    public class MatchDetailModel
    {
        public IReadOnlyCollection<MatchPlayerModel> Players { get; set; }

        public bool RadiantWin { get; set; }

        public int Duration { get; set; }

        public int StartTime { get; set; }

        public long MatchId { get; set; }

        public int MatchSequenceNumber { get; set; }

        public int TowerStatusRadiant { get; set; }

        public TowerStateModel TowerStatesRadiant { get { return new TowerStateModel(TowerStatusRadiant); } }

        public int TowerStatusDire { get; set; }

        public TowerStateModel TowerStatesDire { get { return new TowerStateModel(TowerStatusDire); } }

        public int BarracksStatusRadiant { get; set; }

        public BarracksStateModel BarracksStatesRadiant { get { return new BarracksStateModel(BarracksStatusRadiant); } }

        public int BarracksStatusDire { get; set; }

        public BarracksStateModel BarracksStatesDire { get { return new BarracksStateModel(BarracksStatusDire); } }

        public int Cluster { get; set; }

        public int FirstBloodTime { get; set; }

        public int LobbyType { get; set; }

        public int HumanPlayers { get; set; }

        public int LeagueId { get; set; }

        public int PositiveVotes { get; set; }

        public int NegativeVotes { get; set; }

        public int GameMode { get; set; }

        public int Engine { get; set; }

        public int RadiantTeamId { get; set; }

        public string RadiantName { get; set; }

        public long RadiantLogo { get; set; }

        public int RadiantTeamComplete { get; set; }

        public int DireTeamId { get; set; }

        public string DireName { get; set; }

        public long DireLogo { get; set; }

        public int DireTeamComplete { get; set; }

        public int RadiantCaptain { get; set; }

        public int DireCaptain { get; set; }

        public IReadOnlyCollection<MatchPickBanModel> PicksAndBans { get; set; }
    }
}