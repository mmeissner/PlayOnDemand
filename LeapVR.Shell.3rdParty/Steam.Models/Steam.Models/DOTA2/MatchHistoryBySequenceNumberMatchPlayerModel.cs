#region Licence
/****************************************************************
 *  Filename: MatchHistoryBySequenceNumberMatchPlayerModel.cs
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
    public class MatchHistoryBySequenceNumberMatchPlayerModel
    {
        public IList<MatchHistoryBySequenceNumberMatchPlayerAbilityUpgradeModel> AbilityUpgrades { get; set; }
        
        public long AccountId { get; set; }
        
        public int PlayerSlot { get; set; }
        
        public int HeroId { get; set; }
        
        public int Item0 { get; set; }
        
        public int Item1 { get; set; }
        
        public int Item2 { get; set; }
        
        public int Item3 { get; set; }
        
        public int Item4 { get; set; }
        
        public int Item5 { get; set; }
        
        public int Kills { get; set; }
        
        public int Deaths { get; set; }
        
        public int Assists { get; set; }
        
        public int LeaverStatus { get; set; }
        
        public int LastHits { get; set; }
        
        public int Denies { get; set; }
        
        public int GoldPerMin { get; set; }
        
        public int XpPerMin { get; set; }
        
        public int Level { get; set; }
        
        public int Gold { get; set; }
        
        public int GoldSpent { get; set; }
        
        public int HeroDamage { get; set; }
        
        public int TowerDamage { get; set; }
        
        public int HeroHealing { get; set; }
    }
}
