#region Licence
/****************************************************************
 *  Filename: MatchHistoryBySequenceNumberMatchModel.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Models.DOTA2
{
    public class MatchHistoryBySequenceNumberMatchModel
    {
        public IList<MatchHistoryBySequenceNumberMatchPlayerModel> Players { get; set; }
        
        public bool RadiantWin { get; set; }
        
        public int Duration { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public long MatchId { get; set; }
        
        public int MatchSequenceNumber { get; set; }

        public TowerStateModel TowerStatesRadiant { get { return new TowerStateModel(TowerStatusRadiant); } }


        public int TowerStatusRadiant { get; set; }

        public TowerStateModel TowerStatesDire { get { return new TowerStateModel(TowerStatusRadiant); } }

        public int TowerStatusDire { get; set; }

        public BarracksStateModel BarracksStatesRadiant { get { return new BarracksStateModel(BarracksStatusRadiant); } }

        public int BarracksStatusRadiant { get; set; }

        public BarracksStateModel BarracksStatesDire { get { return new BarracksStateModel(BarracksStatusDire); } }

        public int BarracksStatusDire { get; set; }
        
        public int Cluster { get; set; }
        
        public long FirstBloodTime { get; set; }
        
        public int LobbyType { get; set; }
        
        public int HumanPlayers { get; set; }
        
        public int LeagueId { get; set; }
        
        public int PositiveVotes { get; set; }
        
        public int NegativeVotes { get; set; }
        
        public int GameMode { get; set; }
        
        public int Flags { get; set; }
        
        public int Engine { get; set; }
        
        public int RadiantScore { get; set; }
        
        public int DireScore { get; set; }
    }
}
