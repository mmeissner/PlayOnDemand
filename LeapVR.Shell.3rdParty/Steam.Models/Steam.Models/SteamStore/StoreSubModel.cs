#region Licence
/****************************************************************
 *  Filename: StoreSubModel.cs
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
    public class StoreSubModel
    {
        public string Packageid { get; set; }
        
        public string PercentSavingsText { get; set; }
        
        public int PercentSavings { get; set; }
    
        public string OptionText { get; set; }
        
        public string OptionDescription { get; set; }
        
        public string CanGetFreeLicense { get; set; }
        
        public bool IsFreeLicense { get; set; }
        
        public int PriceInCentsWithDiscount { get; set; }
    }
}
