#region Licence
/****************************************************************
 *  Filename: MatchPlayerAbilityUpgradeModel.cs
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
    public class MatchPlayerAbilityUpgradeModel
    {
        public int Ability { get; set; }
        public int Time { get; set; }
        public int Level { get; set; }
    }
}