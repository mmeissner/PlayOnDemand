#region Licence
/****************************************************************
 *  Filename: StoreAppDetailsDataModel.cs
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
    public class StoreAppDetailsDataModel
    {
        public string Type { get; set; }
        
        public string Name { get; set; }
        
        public int SteamAppid { get; set; }
        
        public int RequiredAge { get; set; }
        
        public bool IsFree { get; set; }
        
        public int[] Dlc { get; set; }
        
        public string DetailedDescription { get; set; }
        
        public string AboutTheGame { get; set; }
        
        public string SupportedLanguages { get; set; }
        
        public string HeaderImage { get; set; }
        
        public string Website { get; set; }
        
        public string[] Developers { get; set; }
        
        public string[] Publishers { get; set; }
        
        public string[] Packages { get; set; }
        
        public StorePackageGroupModel[] PackageGroups { get; set; }
        
        public StorePlatformsModel Platforms { get; set; }
        
        public StoreMetacriticModel Metacritic { get; set; }
        
        public StoreCategoryModel[] Categories { get; set; }
        
        public StoreGenreModel[] Genres { get; set; }
        
        public StoreScreenshotModel[] Screenshots { get; set; }
        
        public StoreMovieModel[] Movies { get; set; }
        
        public StoreRecommendationsModel Recommendations { get; set; }
        
        public StoreReleaseDateModel ReleaseDate { get; set; }
        
        public StoreSupportInfoModel SupportInfo { get; set; }
        
        public string Background { get; set; }
    }
}
