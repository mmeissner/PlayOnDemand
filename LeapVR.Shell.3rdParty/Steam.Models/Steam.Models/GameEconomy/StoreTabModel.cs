#region Licence
/****************************************************************
 *  Filename: StoreTabModel.cs
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

namespace Steam.Models.GameEconomy
{
    public class StoreTabModel
    {
        public string Label { get; set; }

        public string Id { get; set; }

        public object ParentId { get; set; }

        public bool UseLargeCells { get; set; }

        public bool Default { get; set; }

        public IReadOnlyCollection<StoreTabChildModel> Children { get; set; }

        public bool Home { get; set; }

        public long? DropdownPrefabId { get; set; }

        public string ParentName { get; set; }
    }
}