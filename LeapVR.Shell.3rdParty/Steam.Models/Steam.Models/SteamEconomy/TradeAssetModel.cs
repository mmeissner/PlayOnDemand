#region Licence
/****************************************************************
 *  Filename: TradeAssetModel.cs
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
namespace Steam.Models.SteamEconomy
{
    public class TradeAssetModel
    {
        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        /// <summary>
        /// Either assetid or currencyid will be set
        /// </summary>
        public ulong AssetId { get; set; }

        /// <summary>
        /// Either assetid or currencyid will be set
        /// </summary>
        public ulong CurrencyId { get; set; }

        /// <summary>
        /// Together with instanceid, uniquely identifies the display of the item
        /// </summary>
        public uint ClassId { get; set; }

        /// <summary>
        /// Together with classid, uniquely identifies the display of the item
        /// </summary>
        public uint InstanceId { get; set; }

        /// <summary>
        /// The amount offered in the trade, for stackable items and currency
        /// </summary>
        public uint AmountOffered { get; set; }

        /// <summary>
        /// A boolean that indicates the item is no longer present in the user's inventory
        /// </summary>
        public bool IsMissing { get; set; }
    }
}