#region Licence
/****************************************************************
 *  Filename: SchemaAttributeModel.cs
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
    public class SchemaAttributeModel
    {
        public string Name { get; set; }
        
        public int Defindex { get; set; }
        
        public string AttributeClass { get; set; }
        
        public string DescriptionString { get; set; }
        
        public string DescriptionFormat { get; set; }
        
        public string EffectType { get; set; }
        
        public bool Hidden { get; set; }
        
        public bool StoredAsInteger { get; set; }
    }
}
