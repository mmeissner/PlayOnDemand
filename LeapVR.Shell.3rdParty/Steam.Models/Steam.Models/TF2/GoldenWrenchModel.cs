#region Licence
/****************************************************************
 *  Filename: GoldenWrenchModel.cs
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
using System;

namespace Steam.Models.TF2
{
    public class GoldenWrenchModel
    {
        public object SteamId { get; set; }

        public DateTime Timestamp { get; set; }

        public int ItemId { get; set; }

        public int WrenchNumber { get; set; }
    }
}