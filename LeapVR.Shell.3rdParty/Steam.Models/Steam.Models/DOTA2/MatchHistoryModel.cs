#region Licence
/****************************************************************
 *  Filename: MatchHistoryModel.cs
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
    public class MatchHistoryModel
    {
        public int Status { get; set; }

        public int NumResults { get; set; }

        public int TotalResults { get; set; }

        public int ResultsRemaining { get; set; }

        public IReadOnlyCollection<MatchHistoryMatchModel> Matches { get; set; }
    }
}