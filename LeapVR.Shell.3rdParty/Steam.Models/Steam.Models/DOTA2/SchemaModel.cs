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

namespace Steam.Models.DOTA2
{
    public class SchemaModel
    {
        public SchemaGameInfoModel GameInfo { get; set; }

        public IList<SchemaRarityModel> Rarities { get; set; }

        public IList<SchemaQualityModel> Qualities { get; set; }

        public IList<SchemaColorModel> Colors { get; set; }

        public IList<SchemaPrefabModel> Prefabs { get; set; }

        public IList<SchemaItemModel> Items { get; set; }

        public IList<SchemaItemSetModel> ItemSets { get; set; }

        public IList<SchemaItemAutographModel> ItemAutographs { get; set; }
    }
}