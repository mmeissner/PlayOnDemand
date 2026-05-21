#region Licence
/****************************************************************
 *  Filename: AssetTagsModel.cs
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
    public class AssetTagsModel
    {
        public string Cosmetics { get; set; }

        public string Tools { get; set; }

        public string Weapons { get; set; }

        public string Taunts { get; set; }

        public string Bundles { get; set; }

        public string Limited { get; set; }

        public string New { get; set; }

        public string Maps { get; set; }

        public string Halloween { get; set; }
    }
}