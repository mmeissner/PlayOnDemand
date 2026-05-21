#region Licence
/****************************************************************
 *  Filename: AvailableGameStatsModel.cs
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

namespace Steam.Models
{
    public class AvailableGameStatsModel
    {
        public IReadOnlyCollection<SchemaGameStatModel> Stats { get; set; }

        public IReadOnlyCollection<SchemaGameAchievementModel> Achievements { get; set; }
    }
}