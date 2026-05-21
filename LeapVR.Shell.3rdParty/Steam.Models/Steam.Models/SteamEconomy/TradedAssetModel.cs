#region Licence
/****************************************************************
 *  Filename: TradedAssetModel.cs
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
    public class TradedAssetModel
    {
        public uint AppId { get; set; }

        public uint ContextId { get; set; }

        public ulong AssetId { get; set; }

        public uint AmountTraded { get; set; }

        /// <summary>
        /// Together with instanceid, uniquely identifies the display of the item
        /// </summary>
        public uint ClassId { get; set; }

        /// <summary>
        /// Together with classid, uniquely identifies the display of the item
        /// </summary>
        public uint InstanceId { get; set; }

        /// <summary>
        /// The asset ID given to the item after the trade completed
        /// </summary>
        public ulong AssetIdAfterTrade { get; set; }

        /// <summary>
        /// The context ID the item was placed in
        /// </summary>
        public ulong ContextIdAfterTrade { get; set; }

        /// <summary>
        /// If the trade has been rolled back, the new asset ID given in the rollback
        /// </summary>
        public ulong AssetIdAfterRollback { get; set; }

        /// <summary>
        /// If the trade has been rolled back, the context ID the new asset was placed in
        /// </summary>
        public ulong ContextIdAfterRollback { get; set; }
    }
}