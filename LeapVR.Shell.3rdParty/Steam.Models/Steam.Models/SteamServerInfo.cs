#region Licence
/****************************************************************
 *  Filename: SteamServerInfo.cs
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
using Steam.Models.Utilities;
using System;

namespace Steam.Models
{
    public class SteamServerInfo
    {
        public long ServerTime { get; set; }
        public string ServerTimeString { get; set; }
        public DateTime ServerTimeDateTime { get { return ServerTime.ToDateTime(); } }
    }
}