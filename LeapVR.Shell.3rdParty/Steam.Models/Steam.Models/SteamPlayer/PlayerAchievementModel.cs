#region Licence
/****************************************************************
 *  Filename: PlayerAchievementModel.cs
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
namespace Steam.Models.SteamPlayer
{
    public class PlayerAchievementModel
    {
        public string APIName { get; set; }

        public int Achieved { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}