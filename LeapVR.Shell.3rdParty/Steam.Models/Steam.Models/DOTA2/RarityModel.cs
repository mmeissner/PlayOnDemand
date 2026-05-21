#region Licence
/****************************************************************
 *  Filename: RarityModel.cs
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
    public class RarityModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Order { get; set; }
        public string Color { get; set; }
        public string LocalizedName { get; set; }
    }
}