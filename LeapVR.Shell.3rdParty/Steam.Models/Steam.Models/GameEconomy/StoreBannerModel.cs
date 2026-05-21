#region Licence
/****************************************************************
 *  Filename: StoreBannerModel.cs
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
    public class StoreBannerModel
    {
        public string BaseFileName { get; set; }

        public string Action { get; set; }

        public string Placement { get; set; }

        public string ActionParam { get; set; }
    }
}