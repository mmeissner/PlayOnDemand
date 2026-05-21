#region Licence
/****************************************************************
 *  Filename: BadgesResultModel.cs
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

namespace Steam.Models.SteamCommunity
{
    public class BadgesResultModel
    {
        public IReadOnlyCollection<BadgeModel> Badges { get; set; }

        public int PlayerXp { get; set; }

        public int PlayerLevel { get; set; }

        public int PlayerXpNeededToLevelUp { get; set; }

        public int PlayerXpNeededCurrentLevel { get; set; }
    }
}