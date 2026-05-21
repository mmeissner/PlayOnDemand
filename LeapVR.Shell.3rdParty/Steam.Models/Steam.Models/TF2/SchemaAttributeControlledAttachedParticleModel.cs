#region Licence
/****************************************************************
 *  Filename: SchemaAttributeControlledAttachedParticleModel.cs
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
    public class SchemaAttributeControlledAttachedParticleModel
    {
        public string System { get; set; }
        
        public int Id { get; set; }
        
        public bool AttachToRootbone { get; set; }
    
        public string Name { get; set; }
        
        public string Attachment { get; set; }
    }
}
