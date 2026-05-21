#region Licence
/****************************************************************
 *  Filename: SchemaToolModel.cs
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
    public class SchemaToolModel
    {
        public string Type { get; set; }
        
        public SchemaUsageCapabilitiesModel UsageCapabilities { get; set; }
        
        public string UseString { get; set; }
        
        public string Restriction { get; set; }
    }
}
