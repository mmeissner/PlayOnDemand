#region Licence
/****************************************************************
 *  Filename: StoreLargeCapsuleModel.cs
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
    public class StoreLargeCapsuleModel
    {
        public int Id { get; set; }
        
        public int Type { get; set; }
        
        public string Name { get; set; }
        
        public bool Discounted { get; set; }
        
        public int DiscountPercent { get; set; }
        
        public int OriginalPrice { get; set; }
        
        public int FinalPrice { get; set; }
        
        public string Currency { get; set; }
        
        public string LargeCapsuleImage { get; set; }
        
        public string SmallCapsuleImage { get; set; }
        
        public bool WindowsAvailable { get; set; }
        
        public bool MacAvailable { get; set; }
        
        public bool LinuxAvailable { get; set; }
        
        public bool StreamingvideoAvailable { get; set; }
        
        public string HeaderImage { get; set; }
        
        public string ControllerSupport { get; set; }
        
        public string Headline { get; set; }
    }
}
