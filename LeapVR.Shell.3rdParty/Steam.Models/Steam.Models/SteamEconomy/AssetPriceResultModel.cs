#region Licence
/****************************************************************
 *  Filename: AssetPriceResultModel.cs
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
    public class AssetPriceResultModel
    {
        public bool Success { get; set; }

        public IReadOnlyCollection<AssetModel> Assets { get; set; }

        public AssetTagsModel Tags { get; set; }

        public AssetTagIdsModel TagIds { get; set; }
    }
}