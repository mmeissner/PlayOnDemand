#region Licence
/****************************************************************
 *  Filename: SchemaItemSetModel.cs
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
    public class SchemaItemSetModel
    {
        public string RawName { get; set; }

        public string ItemSet { get; set; }

        public string Name { get; set; }

        public IReadOnlyCollection<string> Items { get; set; }

        public IReadOnlyCollection<SchemaItemSetAttributeModel> Attributes { get; set; }

        public string StoreBundleName { get; set; }
    }
}