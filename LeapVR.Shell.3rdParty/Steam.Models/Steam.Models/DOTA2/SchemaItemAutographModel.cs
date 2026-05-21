#region Licence
/****************************************************************
 *  Filename: SchemaItemAutographModel.cs
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
namespace Steam.Models.DOTA2
{
    public class SchemaItemAutographModel
    {
        public string DefIndex { get; set; }

        public string Name { get; set; }

        public string Autograph { get; set; }

        public long? WorkshopLink { get; set; }

        public int Language { get; set; }

        public string IconPath { get; set; }

        public string Modifier { get; set; }
    }
}