#region Licence
/****************************************************************
 *  Filename: MatchHistoryBySequenceNumberResultContainer.cs
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

namespace SteamWebAPI2.Models.DOTA2
{
    internal class MatchHistoryBySequenceNumberResult
    {
        public int Status { get; set; }
        public IList<MatchHistoryMatch> Matches { get; set; }
    }

    internal class MatchHistoryBySequenceNumberResultContainer
    {
        public MatchHistoryBySequenceNumberResult Result { get; set; }
    }
}