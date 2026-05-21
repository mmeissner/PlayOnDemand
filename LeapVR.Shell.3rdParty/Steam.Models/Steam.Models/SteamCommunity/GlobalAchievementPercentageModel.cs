#region Licence
/****************************************************************
 *  Filename: GlobalAchievementPercentageModel.cs
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
    public class GlobalAchievementPercentageModel
    {
        public string Name { get; set; }

        public double Percent { get; set; }
    }
}