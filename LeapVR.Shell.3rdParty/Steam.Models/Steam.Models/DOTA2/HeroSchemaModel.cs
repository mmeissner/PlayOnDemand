#region Licence
/****************************************************************
 *  Filename: HeroSchemaModel.cs
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
    public class HeroSchemaModel
    {
        public int HeroId { get; set; }

        public string Name { get; set; }

        public string BaseClass { get; set; }

        public string Model { get; set; }

        public string SoundSet { get; set; }

        public bool Enabled { get; set; }

        public string Level { get; set; }

        public bool BotImplemented { get; set; }

        public string NewHero { get; set; }

        public string HeroPool1 { get; set; }

        public string HeroUnlockOrder { get; set; }

        public string CMEnabled { get; set; }

        public string CMTournamentIgnore { get; set; }

        public string NewPlayerEnable { get; set; }

        public string AbilityLayout { get; set; }

        public string Ability1 { get; set; }

        public string Ability2 { get; set; }

        public string Ability3 { get; set; }

        public string Ability4 { get; set; }

        public string Ability5 { get; set; }

        public string Ability6 { get; set; }

        public string Ability7 { get; set; }

        public string Ability8 { get; set; }

        public string Ability9 { get; set; }

        public string Ability10 { get; set; }

        public string Ability11 { get; set; }

        public string Ability12 { get; set; }

        public string Ability13 { get; set; }

        public string Ability14 { get; set; }

        public string Ability15 { get; set; }

        public string Ability16 { get; set; }

        public string Ability17 { get; set; }

        public string Ability18 { get; set; }

        public string Ability19 { get; set; }

        public string Ability20 { get; set; }

        public string Ability21 { get; set; }

        public string Ability22 { get; set; }

        public string Ability23 { get; set; }

        public string Ability24 { get; set; }

        public double ArmorPhysical { get; set; }

        public string MagicalResistance { get; set; }

        public string AttackCapabilities { get; set; }

        public int AttackDamageMin { get; set; }

        public int AttackDamageMax { get; set; }

        public string AttackDamageType { get; set; }

        public double AttackRate { get; set; }

        public double AttackAnimationPoint { get; set; }

        public int AttackAcquisitionRange { get; set; }

        public int AttackRange { get; set; }

        public string ProjectileModel { get; set; }

        public int ProjectileSpeed { get; set; }

        public string AttributePrimary { get; set; }

        public int AttributeBaseStrength { get; set; }

        public double AttributeStrengthGain { get; set; }

        public int AttributeBaseIntelligence { get; set; }

        public double AttributeIntelligenceGain { get; set; }

        public int AttributeBaseAgility { get; set; }

        public double AttributeAgilityGain { get; set; }

        public int BountyXP { get; set; }

        public int BountyGoldMin { get; set; }

        public int BountyGoldMax { get; set; }

        public string BoundsHullName { get; set; }

        public int RingRadius { get; set; }

        public string MovementCapabilities { get; set; }

        public int MovementSpeed { get; set; }

        public double MovementTurnRate { get; set; }

        public bool HasAggressiveStance { get; set; }

        public int StatusHealth { get; set; }

        public double StatusHealthRegen { get; set; }

        public int StatusMana { get; set; }

        public double StatusManaRegen { get; set; }

        public string TeamName { get; set; }

        public string Team { get; set; }

        public string CombatClassAttack { get; set; }

        public string CombatClassDefend { get; set; }

        public string UnitRelationshipClass { get; set; }

        public int VisionDaytimeRange { get; set; }

        public int VisionNighttimeRange { get; set; }

        public bool HasInventory { get; set; }

        public string VoiceBackgroundSound { get; set; }

        public int HealthBarOffset { get; set; }

        public string IdleExpression { get; set; }

        public string IdleSoundLoop { get; set; }

        public string AbilityDraftDisabled { get; set; }

        public string ARDMDisabled { get; set; }

        public string Url { get; set; }

        public string Role { get; set; }

        public string RoleLevels { get; set; }
    }
}