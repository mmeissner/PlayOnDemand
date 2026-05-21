#region Licence
/****************************************************************
 *  Filename: SchemaGameInfoModel.cs
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
    public class SchemaGameInfoModel
    {
        public string FirstValidClass { get; set; }

        public string LastValidClass { get; set; }

        public string FirstValidItemSlot { get; set; }

        public string LastValidItemSlot { get; set; }

        public string ItemPresetCount { get; set; }
    }
}