#region Licence
/****************************************************************
 *  Filename: AssetClassDescriptionModel.cs
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
namespace Steam.Models.SteamEconomy
{
    public class AssetClassDescriptionModel
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public string Color { get; set; }

        public string AppData { get; set; }
    }
}