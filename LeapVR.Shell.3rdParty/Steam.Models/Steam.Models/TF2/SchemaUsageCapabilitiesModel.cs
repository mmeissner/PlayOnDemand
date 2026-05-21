#region Licence
/****************************************************************
 *  Filename: SchemaUsageCapabilitiesModel.cs
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

namespace Steam.Models.TF2
{
    public class SchemaUsageCapabilitiesModel
    {
        public bool Nameable { get; set; }
        
        public bool? Decodable { get; set; }
        
        public bool? Paintable { get; set; }
        
        public bool? CanCustomizeTexture { get; set; }
        
        public bool? CanGiftWrap { get; set; }
        
        public bool? PaintableTeamColors { get; set; }
        
        public bool? CanStrangify { get; set; }
        
        public bool? CanKillstreakify { get; set; }
        
        public bool? DuckUpgradable { get; set; }
        
        public bool? StrangeParts { get; set; }
        
        public bool? CanCardUpgrade { get; set; }
        
        public bool? CanSpellPage { get; set; }
        
        public bool? CanConsume { get; set; }
    }
}
