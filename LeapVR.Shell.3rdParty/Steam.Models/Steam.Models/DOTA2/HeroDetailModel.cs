#region Licence
/****************************************************************
 *  Filename: HeroDetailModel.cs
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

namespace Steam.Models.DOTA2
{
    public class HeroDetailModel
    {
        private const int baseHealth = 150;
        private const int healthPerStrength = 19;
        private const int baseMana = 0;
        private const int manaPerIntellect = 13;
        private const double armorFactor = 0.14;

        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string NameInSchema { get; set; }
        public string Description { get; set; }
        public string AvatarImagePath { get; set; }
        public int BaseStrength { get; set; }
        public int BaseAgility { get; set; }
        public int BaseIntelligence { get; set; }
        public int BaseDamageMin { get; set; }
        public int BaseDamageMax { get; set; }
        public int BaseMoveSpeed { get; set; }
        public double BaseArmor { get; set; }
        public string Team { get; set; }
        public double AttackRange { get; set; }
        public double AttackRate { get; set; }
        public double TurnRate { get; set; }
        public string AttackType { get; set; }
        public double StrengthGain { get; set; }
        public double AgilityGain { get; set; }
        public double IntelligenceGain { get; set; }
        public DotaHeroPrimaryAttributeType PrimaryAttribute { get; set; }
        public string ActiveTab { get; set; }
        public string MinimapIconPath { get; set; }
        public bool IsEnabled { get; set; }

        public IReadOnlyCollection<HeroRoleModel> Roles { get; set; }
        public IReadOnlyCollection<HeroAbilityDetailModel> Abilities { get; set; }

        public double GetBaseHealth()
        {
            return GetHealth(0);
        }

        public double GetBaseMana()
        {
            return GetMana(0);
        }

        public double GetHealth(int level)
        {
            return Math.Round(baseHealth + (healthPerStrength * (BaseStrength + (level * StrengthGain))));
        }

        public double GetMana(int level)
        {
            return Math.Round(baseMana + (manaPerIntellect * (BaseIntelligence + (level * IntelligenceGain))));
        }

        public double GetMinDamage(int level)
        {
            if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.STRENGTH.Key)
            {
                return Math.Round(BaseDamageMin + (BaseStrength + (level * StrengthGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.INTELLECT.Key)
            {
                return Math.Round(BaseDamageMin + (BaseIntelligence + (level * IntelligenceGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.AGILITY.Key)
            {
                return Math.Round(BaseDamageMin + (BaseAgility + (level * AgilityGain)));
            }
            else
            {
                return 0;
            }
        }

        public double GetMaxDamage(int level)
        {
            if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.STRENGTH.Key)
            {
                return Math.Round(BaseDamageMax + (BaseStrength + (level * StrengthGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.INTELLECT.Key)
            {
                return Math.Round(BaseDamageMax + (BaseIntelligence + (level * IntelligenceGain)));
            }
            else if (PrimaryAttribute.Key == DotaHeroPrimaryAttributeType.AGILITY.Key)
            {
                return Math.Round(BaseDamageMax + (BaseAgility + (level * AgilityGain)));
            }
            else
            {
                return 0;
            }
        }

        public double GetArmor(int level)
        {
            return BaseArmor + (GetAgility(level) * armorFactor);
        }

        public double GetStrength(int level)
        {
            return Math.Round(BaseStrength + (level * StrengthGain));
        }

        public double GetAgility(int level)
        {
            return Math.Round(BaseAgility + (level * AgilityGain));
        }

        public double GetIntelligence(int level)
        {
            return Math.Round(BaseIntelligence + (level * IntelligenceGain));
        }
    }
}