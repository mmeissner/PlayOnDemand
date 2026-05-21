#region Licence
/****************************************************************
 *  Filename: ProPlayerLeaderboardModel.cs
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
    public class ProPlayerLeaderboardModel
    {
        public int Division { get; set; }

        public IReadOnlyCollection<uint> AccountIds { get; set; }
    }
}