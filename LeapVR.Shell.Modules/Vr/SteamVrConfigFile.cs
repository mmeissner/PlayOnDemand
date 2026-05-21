#region Licence
/****************************************************************
 *  Filename: SteamVrConfigFile.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Collections.Generic;
using System.Reflection;

namespace LeapVR.Shell.Modules.Vr
{
    
    public class SteamVrConfigFile // TODO [RM]: move to infrastructure layer
    {
        public List<string> config { get; set; }
        public List<string> external_drivers { get; set; }
        public List<string> log { get; set; }
        public List<string> runtime { get; set; }
    }
}
