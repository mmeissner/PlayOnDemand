#region Licence
/****************************************************************
 *  Filename: SchemaModel.cs
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
    public class SchemaModel
    {   
        public string ItemsGameUrl { get; set; }
        
        public SchemaQualitiesModel Qualities { get; set; }
        
        public IList<SchemaOriginNameModel> OriginNames { get; set; }
        
        public IList<SchemaItemModel> Items { get; set; }
        
        public IList<SchemaAttributeModel> Attributes { get; set; }
        
        public IList<SchemaItemSetModel> ItemSets { get; set; }
        
        public IList<SchemaAttributeControlledAttachedParticleModel> AttributeControlledAttachedParticles { get; set; }
        
        public IList<SchemaItemLevelModel> ItemLevels { get; set; }
        
        public IList<SchemaKillEaterScoreTypeModel> KillEaterScoreTypes { get; set; }
        
        public IList<SchemaStringLookupModel> StringLookups { get; set; }
    }
}
