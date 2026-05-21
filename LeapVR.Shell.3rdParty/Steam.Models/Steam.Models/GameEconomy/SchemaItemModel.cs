#region Licence
/****************************************************************
 *  Filename: SchemaItemModel.cs
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
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;

namespace Steam.Models.GameEconomy
{
    public class SchemaItemModel
    {
        public int DefIndex { get; set; }

        public string Name { get; set; }
        
        public string ImageInventoryPath { get; set; }

        public string ItemName { get; set; }

        public string ItemDescription { get; set; }
        
        public string ItemTypeName { get; set; }

        public string ItemClass { get; set; }
        
        public bool ProperName { get; set; }

        public string ItemSlot { get; set; }

        public string ModelPlayer { get; set; }

        public int ItemQuality { get; set; }
        
        public int MinIlevel { get; set; }

        public int MaxIlevel { get; set; }

        public string ImageUrl { get; set; }

        public string ImageUrlLarge { get; set; }

        public string CraftClass { get; set; }

        public string CraftMaterialType { get; set; }

        public SchemaCapabilitiesModel Capabilities { get; set; }

        public IReadOnlyCollection<string> UsedByClasses { get; set; }

        public IReadOnlyCollection<SchemaStyleModel> Styles { get; set; }
        
        public IReadOnlyCollection<SchemaItemAttributeModel> Attributes { get; set; }

        public string DropType { get; set; }

        public string ItemSet { get; set; }

        public string HolidayRestriction { get; set; }

        public SchemaPerClassLoadoutSlotsModel PerClassLoadoutSlots { get; set; }

        public SchemaToolModel Tool { get; set; }

        public string Prefab { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string TournamentUrl { get; set; }

        public string ImageBannerPath { get; set; }

        public string ItemRarity { get; set; }

        public SchemaItemPriceInfoModel PriceInfo { get; set; }

        public IList<string> UsedByHeroes { get; set; }

        public IList<string> BundledItems { get; set; }
    }
}