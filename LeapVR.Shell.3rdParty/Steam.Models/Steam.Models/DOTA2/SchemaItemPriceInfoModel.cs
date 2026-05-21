#region Licence
/****************************************************************
 *  Filename: SchemaItemPriceInfoModel.cs
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

namespace Steam.Models.DOTA2
{
    public class SchemaItemPriceInfoModel
    {
        public string Bucket { get; set; }

        public string Class { get; set; }

        public string CategoryTags { get; set; }

        public DateTime? Date { get; set; }

        public decimal? Price { get; set; }

        public bool? IsPackItem { get; set; }
    }
}