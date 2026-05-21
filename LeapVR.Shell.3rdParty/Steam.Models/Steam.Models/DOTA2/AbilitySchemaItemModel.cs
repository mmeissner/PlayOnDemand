#region Licence
/****************************************************************
 *  Filename: AbilitySchemaItemModel.cs
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
using System.Collections.Generic;

namespace Steam.Models.DOTA2
{
    public class AbilitySchemaItemModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AbilityType { get; set; }

        public string AbilityBehavior { get; set; }

        public bool OnCastbar { get; set; }

        public bool OnLearnbar { get; set; }

        public string FightRecapLevel { get; set; }

        public string AbilityCastRange { get; set; }

        public int AbilityRangeBuffer { get; set; }

        public string AbilityCastPoint { get; set; }

        public string AbilityChannelTime { get; set; }

        public string AbilityCooldown { get; set; }

        public string AbilityDuration { get; set; }

        public string AbilitySharedCooldown { get; set; }

        public string AbilityDamage { get; set; }

        public string AbilityManaCost { get; set; }

        public double AbilityModifierSupportValue { get; set; }

        public double AbilityModifierSupportBonus { get; set; }

        public string AbilityUnitTargetTeam { get; set; }

        public string AbilityUnitDamageType { get; set; }

        public string SpellImmunityType { get; set; }

        public string AbilityUnitTargetFlags { get; set; }

        public string AbilityUnitTargetType { get; set; }

        public IList<AbilitySpecialSchemaItemModel> AbilitySpecials { get; set; }
    }
}