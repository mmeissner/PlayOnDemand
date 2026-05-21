#region Licence
/****************************************************************
 *  Filename: PlayerBansModel.cs
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
    public class PlayerBansModel
    {
        public string SteamId { get; set; }

        public bool CommunityBanned { get; set; }

        public bool VACBanned { get; set; }

        public int NumberOfVACBans { get; set; }

        public int DaysSinceLastBan { get; set; }

        public int NumberOfGameBans { get; set; }

        public string EconomyBan { get; set; }
    }
}