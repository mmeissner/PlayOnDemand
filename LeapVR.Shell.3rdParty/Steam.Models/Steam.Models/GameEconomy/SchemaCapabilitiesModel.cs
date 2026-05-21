#region Licence
/****************************************************************
 *  Filename: SchemaCapabilitiesModel.cs
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
namespace Steam.Models.GameEconomy
{
    public class SchemaCapabilitiesModel
    {
        public bool Nameable { get; set; }

        public bool CanCraftMark { get; set; }

        public bool CanBeRestored { get; set; }

        public bool StrangeParts { get; set; }

        public bool CanCardUpgrade { get; set; }

        public bool CanStrangify { get; set; }

        public bool CanKillstreakify { get; set; }

        public bool CanConsume { get; set; }

        public bool? CanGiftWrap { get; set; }

        public bool? CanCollect { get; set; }

        public bool? Paintable { get; set; }

        public bool? CanCraftIfPurchased { get; set; }

        public bool? CanCraftCount { get; set; }

        public bool? UsableGc { get; set; }

        public bool? Usable { get; set; }

        public bool? CanCustomizeTexture { get; set; }

        public bool? UsableOutOfGame { get; set; }

        public bool? CanSpellPage { get; set; }

        public bool? DuckUpgradable { get; set; }

        public bool? Decodable { get; set; }
    }
}