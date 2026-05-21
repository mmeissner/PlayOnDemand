#region Licence
/****************************************************************
 *  Filename: DotaAttackType.cs
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
    public sealed class DotaAttackType : DotaEnumType
    {
        public static readonly DotaAttackType UNKNOWN = new DotaAttackType("DOTA_UNIT_CAP_UNKNOWN", "Unknown", "This attack type is unknown.");
        public static readonly DotaAttackType RANGED = new DotaAttackType("DOTA_UNIT_CAP_RANGED_ATTACK", "Ranged", "The attack can be performed from a distance.");
        public static readonly DotaAttackType MELEE = new DotaAttackType("DOTA_UNIT_CAP_MELEE_ATTACK", "Melee", "The attack must be performed within arm's reach.");

        public DotaAttackType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}