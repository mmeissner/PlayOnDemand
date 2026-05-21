#region Licence
/****************************************************************
 *  Filename: SteamAppStoreInfo.cs
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
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Modules.Interfaces.Platform.Steam;
using Steam.Models.SteamStore;

namespace LeapVR.Utilities.Steam.Steam
{
    public class SteamAppStoreInfo : ISteamAppStoreInfo
    {
        /// <summary>
        /// Constructor only for deserializer, initializes a new instance of the <see cref="SteamAppStoreInfo"/> class.
        /// </summary>
        public SteamAppStoreInfo()
        {}

        /// <summary>
        /// Initializes a fully constructed instance of the <see cref="SteamAppStoreInfo"/> class.
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <param name="image">The image.</param>
        /// <param name="categories">The categories.</param>
        /// <param name="storeAppDetailsDataModel">The Steam Store AppDetails Data Model</param>
        public SteamAppStoreInfo(uint appId,  byte[] image, HashSet<string> categories,StoreAppDetailsDataModel storeAppDetailsDataModel)
        {
            AppId = appId;
            Title = storeAppDetailsDataModel.Name;
            Image = image;
            Description = storeAppDetailsDataModel.AboutTheGame;
            Categories = categories;
            if(storeAppDetailsDataModel.Movies == null) MovieUrls = new List<string>();
            else MovieUrls = storeAppDetailsDataModel.Movies.Where(x => x.Webm != null && !String.IsNullOrEmpty(x.Webm.Max)).Select(x=> x.Webm.Max).ToList();
        }
        public UInt32 AppId { get; }
        public string Title { get; }
        public Byte[] Image { get; }
        public string Description { get; }
        public HashSet<string> Categories { get; }
        public List<string> MovieUrls { get; }

    }
}
