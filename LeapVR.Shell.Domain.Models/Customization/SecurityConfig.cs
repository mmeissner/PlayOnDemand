#region Licence
/****************************************************************
 *  Filename: SecurityConfig.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Customization
{
    
    public class SecurityConfig : ConfigObject
    {
        public string SecurityCode { get; set; } = "000000";
        public TimeSpan SystemInactivityTimeout { get; } = TimeSpan.FromSeconds(30);
    }
}
