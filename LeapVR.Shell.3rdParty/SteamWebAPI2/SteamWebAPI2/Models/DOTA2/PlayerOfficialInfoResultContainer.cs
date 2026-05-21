#region Licence
/****************************************************************
 *  Filename: PlayerOfficialInfoResultContainer.cs
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
namespace SteamWebAPI2.Models.DOTA2
{
    internal class PlayerOfficialInfoResult
    {
        public string Name { get; set; }
        public string TeamName { get; set; }
        public string TeamTag { get; set; }
        public string Sponsor { get; set; }
        public int FantasyRole { get; set; }
    }

    internal class PlayerOfficialInfoResultContainer
    {
        public PlayerOfficialInfoResult Result { get; set; }
    }
}