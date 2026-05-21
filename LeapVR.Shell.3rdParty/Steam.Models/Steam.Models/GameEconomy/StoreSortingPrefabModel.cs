#region Licence
/****************************************************************
 *  Filename: StoreSortingPrefabModel.cs
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
    public class StoreSortingPrefabModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string UrlHistoryParamName { get; set; }

        public IReadOnlyCollection<StoreSorterIdModel> SorterIds { get; set; }
    }
}