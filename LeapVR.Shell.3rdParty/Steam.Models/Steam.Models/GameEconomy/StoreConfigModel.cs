#region Licence
/****************************************************************
 *  Filename: StoreConfigModel.cs
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
namespace Steam.Models.GameEconomy
{
    public class StoreConfigModel
    {
        public long DropdownId { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public long DefaultSelectionId { get; set; }
    }
}