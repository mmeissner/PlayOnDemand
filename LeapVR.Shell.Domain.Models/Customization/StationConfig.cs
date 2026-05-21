#region Licence
/****************************************************************
 *  Filename: StationConfig.cs
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

using System.Reflection;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.Customization
{
    
    public class StationConfig : ConfigObject
    {
        public StationMode DefaultStationMode { get; set; } = StationMode.Screen;
        public bool ForceVrDriverRestart { get; set; } = true;
        public bool DisableVrDriverInteraction { get; set; } = true;
    }
}
