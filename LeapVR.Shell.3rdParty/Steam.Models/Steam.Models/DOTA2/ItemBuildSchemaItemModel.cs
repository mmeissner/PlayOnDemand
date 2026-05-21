#region Licence
/****************************************************************
 *  Filename: ItemBuildSchemaItemModel.cs
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

namespace Steam.Models.DOTA2
{
    public class ItemBuildSchemaItemModel
    {
        public string Author { get; set; }
        public string Hero { get; set; }
        public string Title { get; set; }

        public IList<ItemBuildGroupSchemaItemModel> Items { get; set; }
    }
}