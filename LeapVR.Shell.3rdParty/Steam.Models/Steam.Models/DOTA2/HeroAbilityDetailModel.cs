#region Licence
/****************************************************************
 *  Filename: HeroAbilityDetailModel.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Steam.Models.DOTA2
{
    public class HeroAbilityDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Description { get; set; }
        public IReadOnlyCollection<HeroAbilitySpecialDetailModel> Attributes { get; set; }
        public string Behaviors { get; set; }
        public string TeamTargets { get; set; }
        public string TargetTypes { get; set; }
        public string TargetFlags { get; set; }
        public DotaSpellImmunityType SpellImmunityType { get; set; }
        public DotaDamageType DamageType { get; set; }
        public string CastRange { get; set; }
        public string CastPoint { get; set; }
        public string Cooldown { get; set; }
        public string Duration { get; set; }
        public string Damage { get; set; }
        public string ManaCost { get; set; }
        public DotaHeroAbilityType AbilityType { get; set; }
        public string Note0 { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public string Note4 { get; set; }
        public string Note5 { get; set; }
        public bool HasLinkedSpecialBonus
        {
            get
            {
                return Attributes.Any(x => !String.IsNullOrWhiteSpace(x.LinkedSpecialBonus));
            }
        }
    }
}