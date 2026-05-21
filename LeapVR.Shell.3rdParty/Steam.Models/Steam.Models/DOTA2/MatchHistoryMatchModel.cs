#region Licence
/****************************************************************
 *  Filename: MatchHistoryMatchModel.cs
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

namespace Steam.Models.DOTA2
{
    public class MatchHistoryMatchModel
    {
        public long MatchId { get; set; }

        public int MatchSequenceNumber { get; set; }

        public DateTime StartTime { get; set; }

        public int LobbyType { get; set; }

        public int RadiantTeamId { get; set; }

        public int DireTeamId { get; set; }

        public IReadOnlyCollection<MatchHistoryPlayerModel> Players { get; set; }
    }
}