#region Licence
/****************************************************************
 *  Filename: AssetClassInfoModel.cs
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

namespace Steam.Models.SteamEconomy
{
    public class AssetClassInfoModel
    {
        public string IconUrl { get; set; }

        public string IconUrlLarge { get; set; }

        public string IconDragUrl { get; set; }

        public string Name { get; set; }

        public string MarketHashName { get; set; }

        public string MarketName { get; set; }

        public string NameColor { get; set; }

        public string BackgroundColor { get; set; }

        public string Type { get; set; }

        public string Tradable { get; set; }

        public string Marketable { get; set; }

        public string Commodity { get; set; }

        public string MarketTradableRestriction { get; set; }

        public string MarketMarketableRestriction { get; set; }

        public string FraudWarnings { get; set; }

        public IReadOnlyCollection<AssetClassDescriptionModel> Descriptions { get; set; }

        public IReadOnlyCollection<AssetClassActionModel> Actions { get; set; }

        public IReadOnlyCollection<AssetClassMarketActionModel> MarketActions { get; set; }

        public IReadOnlyCollection<AssetClassTagModel> Tags { get; set; }

        public AssetClassAppDataModel AppData { get; set; }

        public long ClassId { get; set; }
    }
}