#region Licence
/****************************************************************
 *  Filename: AssetModel.cs
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
    public class AssetModel
    {
        public AssetPricesModel Prices { get; set; }

        public string Name { get; set; }

        public string Date { get; set; }

        public IReadOnlyCollection<AssetClassModel> Class { get; set; }

        public string Classid { get; set; }

        public IReadOnlyCollection<string> Tags { get; set; }

        public IReadOnlyCollection<long> TagIds { get; set; }
    }
}