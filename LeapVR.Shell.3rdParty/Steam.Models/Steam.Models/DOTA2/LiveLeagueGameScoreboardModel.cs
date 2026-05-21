#region Licence
/****************************************************************
 *  Filename: LiveLeagueGameScoreboardModel.cs
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
    public class LiveLeagueGameScoreboardModel
    {
        public double Duration { get; set; }

        public int RoshanRespawnTimer { get; set; }

        public LiveLeagueGameTeamRadiantDetailModel Radiant { get; set; }
        public LiveLeagueGameTeamDireDetailModel Dire { get; set; }
    }
}