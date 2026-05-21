#region Licence
/****************************************************************
 *  Filename: StoreCarouselDataModel.cs
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
    public class StoreCarouselDataModel
    {
        public int MaxDisplayBanners { get; set; }

        public IReadOnlyCollection<StoreBannerModel> Banners { get; set; }
    }
}