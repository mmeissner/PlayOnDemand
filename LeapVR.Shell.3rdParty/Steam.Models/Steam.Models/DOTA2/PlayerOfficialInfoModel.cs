#region Licence
/****************************************************************
 *  Filename: PlayerOfficialInfoModel.cs
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
    public class PlayerOfficialInfoModel
    {
        public string Name { get; set; }
        public string TeamName { get; set; }
        public string TeamTag { get; set; }
        public string Sponsor { get; set; }
        public int FantasyRole { get; set; }
    }
}