#region Licence
/****************************************************************
 *  Filename: CacheableSteamStoreInfo.cs
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
using LeapVR.Shell.Domain.Models.Module;

namespace LeapVR.Shell.Modules.Interfaces.Platform.Steam
{
    
    public class CacheableSteamStoreInfo : ICacheEntity,ISteamAppStoreInfo
    {
        public Guid PersistenceId { get; set; }
        public Guid CacheObjId { get ; set; }
        public CacheableSteamStoreInfo(){}
        public CacheableSteamStoreInfo(Guid applicationId, ISteamAppStoreInfo storeInfo)
        {
            CacheObjId = applicationId;
            AppId = storeInfo.AppId;
            Title = storeInfo.Title;
            Description = storeInfo.Description;
            Image = storeInfo.Image;
            Categories = storeInfo.Categories;
            MovieUrls = storeInfo.MovieUrls;
        }
        public UInt32 AppId { get; set; }
        public string Title { get; set;}
        public Byte[] Image { get; set;}
        public string Description { get;set; }
        public HashSet<string> Categories { get;set; }
        public List<string> MovieUrls { get; set; }
    }
}
