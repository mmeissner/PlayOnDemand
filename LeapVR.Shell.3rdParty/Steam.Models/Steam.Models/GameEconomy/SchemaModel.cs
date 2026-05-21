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
using System.Collections.Generic;

namespace Steam.Models.GameEconomy
{
    public class SchemaModel
    {
        public int Status { get; set; }

        public string ItemsGameUrl { get; set; }

        public SchemaQualitiesModel Qualities { get; set; }

        public IReadOnlyCollection<SchemaOriginNameModel> OriginNames { get; set; }

        public IReadOnlyCollection<SchemaItemModel> Items { get; set; }

        public IReadOnlyCollection<SchemaAttributeModel> Attributes { get; set; }

        public IReadOnlyCollection<SchemaItemSetModel> ItemSets { get; set; }

        public IReadOnlyCollection<SchemaAttributeControlledAttachedParticleModel> AttributeControlledAttachedParticles { get; set; }

        public IReadOnlyCollection<SchemaItemLevelModel> ItemLevels { get; set; }

        public IReadOnlyCollection<SchemaKillEaterScoreTypeModel> KillEaterScoreTypes { get; set; }

        public IReadOnlyCollection<SchemaStringLookupModel> StringLookups { get; set; }
    }
}