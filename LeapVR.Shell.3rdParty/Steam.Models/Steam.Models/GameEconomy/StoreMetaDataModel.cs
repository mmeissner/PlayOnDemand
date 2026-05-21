#region Licence
/****************************************************************
 *  Filename: StoreMetaDataModel.cs
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
    public class StoreMetaDataModel
    {
        public StoreCarouselDataModel CarouselData { get; set; }

        public IReadOnlyCollection<StoreTabModel> Tabs { get; set; }

        public IReadOnlyCollection<StoreFilterModel> Filters { get; set; }

        public StoreSortingModel Sorting { get; set; }

        public StoreDropdownDataModel DropdownData { get; set; }

        public IReadOnlyCollection<StorePlayerClassDataModel> PlayerClassData { get; set; }

        public StoreHomePageDataModel HomePageData { get; set; }
    }
}