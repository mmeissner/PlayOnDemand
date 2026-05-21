#region Licence
/****************************************************************
 *  Filename: StorePackageGroupModel.cs
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
    public class StorePackageGroupModel
    {
        public string Name { get; set; }
        
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string SelectionText { get; set; }
        
        public string SaveText { get; set; }
        
        public int DisplayType { get; set; }
        
        public string IsRecurringSubscription { get; set; }
        
        public StoreSubModel[] Subs { get; set; }
    }
}
