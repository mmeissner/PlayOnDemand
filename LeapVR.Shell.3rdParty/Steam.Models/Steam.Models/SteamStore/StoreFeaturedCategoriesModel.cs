#region Licence
/****************************************************************
 *  Filename: StoreFeaturedCategoriesModel.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Models.SteamStore
{
    public class StoreFeaturedCategoriesModel
    {
        public StoreSpecialsModel Specials { get; set; }
        
        public StoreComingSoonModel ComingSoon { get; set; }
        
        public StoreTopSellersModel TopSellers { get; set; }
        
        public StoreNewReleasesModel NewReleases { get; set; }
        
        public StoreFeaturedCategoryGenreModel Genres { get; set; }
        
        public StoreTrailerSlideshowModel Trailerslideshow { get; set; }
        
        public int Status { get; set; }
    }
}
