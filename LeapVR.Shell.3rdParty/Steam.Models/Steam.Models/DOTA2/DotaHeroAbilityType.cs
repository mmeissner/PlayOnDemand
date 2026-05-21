#region Licence
/****************************************************************
 *  Filename: DotaHeroAbilityType.cs
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
    public sealed class DotaHeroAbilityType : DotaEnumType
    {
        public static readonly DotaHeroAbilityType UNKNOWN = new DotaHeroAbilityType("DOTA_ABILITY_TYPE_UNKNOWN", "Unknown", "This is a an unknown ability type.");
        public static readonly DotaHeroAbilityType BASIC = new DotaHeroAbilityType("DOTA_ABILITY_TYPE_BASIC", "Basic", "This is a basic ability learnable at any level.");
        public static readonly DotaHeroAbilityType ULTIMATE = new DotaHeroAbilityType("DOTA_ABILITY_TYPE_ULTIMATE", "Ultimate", "This is a unique ultimate ability learnable at certain levels.");
        public static readonly DotaHeroAbilityType ATTRIBUTES = new DotaHeroAbilityType("DOTA_ABILITY_TYPE_ATTRIBUTES", "Talent", "This is a talent selectable at levels 10, 15, 20, and 25. Previously known as an 'Attribute'.");

        public DotaHeroAbilityType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}