#region Licence
/****************************************************************
 *  Filename: BadgeModel.cs
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
namespace Steam.Models.SteamCommunity
{
    public class BadgeModel
    {
        public int BadgeId { get; set; }

        public int Level { get; set; }

        public int CompletionTime { get; set; }

        public int Xp { get; set; }

        public int Scarcity { get; set; }

        public int? AppId { get; set; }

        public string CommunityItemId { get; set; }

        public int? BorderColor { get; set; }
    }
}