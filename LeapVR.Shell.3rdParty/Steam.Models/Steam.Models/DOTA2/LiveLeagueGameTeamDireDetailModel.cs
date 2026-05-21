#region Licence
/****************************************************************
 *  Filename: LiveLeagueGameTeamDireDetailModel.cs
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
    public class LiveLeagueGameTeamDireDetailModel
    {
        public int Score { get; set; }

        public int TowerState { get; set; }

        public int BarracksState { get; set; }

        public IReadOnlyCollection<LiveLeagueGamePickModel> Picks { get; set; }
        public IReadOnlyCollection<LiveLeagueGameBanModel> Bans { get; set; }
        public IReadOnlyCollection<LiveLeagueGamePlayerDetailModel> Players { get; set; }
        public IReadOnlyCollection<LiveLeagueGameAbilityModel> Abilities { get; set; }

        public TowerStateModel TowerStates { get { return new TowerStateModel(TowerState); } }
    }
}