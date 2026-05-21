#region Licence
/****************************************************************
 *  Filename: DotaHeroPrimaryAttributeType.cs
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
    public sealed class DotaHeroPrimaryAttributeType : DotaEnumType
    {
        public static readonly DotaHeroPrimaryAttributeType STRENGTH = new DotaHeroPrimaryAttributeType("DOTA_ATTRIBUTE_STRENGTH", "Strength", "Strength health and health regeneration.");
        public static readonly DotaHeroPrimaryAttributeType AGILITY = new DotaHeroPrimaryAttributeType("DOTA_ATTRIBUTE_AGILITY", "Agility", "Agility increases armor.");
        public static readonly DotaHeroPrimaryAttributeType INTELLECT = new DotaHeroPrimaryAttributeType("DOTA_ATTRIBUTE_INTELLECT", "Intellect", "Intellect increases mana and mana regeneration.");

        public DotaHeroPrimaryAttributeType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}