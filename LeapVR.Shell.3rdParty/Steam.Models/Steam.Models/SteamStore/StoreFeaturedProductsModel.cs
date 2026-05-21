#region Licence
/****************************************************************
 *  Filename: StoreFeaturedProductsModel.cs
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
    public class StoreFeaturedProductsModel
    {
        public StoreLargeCapsuleModel[] LargeCapsules { get; set; }
        
        public StoreFeaturedWinModel[] FeaturedWin { get; set; }
        
        public StoreFeaturedMacModel[] FeaturedMac { get; set; }
        
        public StoreFeaturedLinuxModel[] FeaturedLinux { get; set; }
        
        public string Layout { get; set; }
        
        public int Status { get; set; }
    }
}
