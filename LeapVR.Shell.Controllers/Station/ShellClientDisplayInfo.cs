#region Licence
/****************************************************************
 *  Filename: ShellClientDisplayInfo.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-11-16
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.VBox.Controllers.Interfaces.Station;

namespace LeapVR.VBox.Controllers.Station
{
    public class ShellClientDisplayInfo : IShellClientDisplayInfo
    {
        public string StationDisplayName { get; set; }
        public string LocationDisplayName { get; set; }
        public string PlatformDisplayName { get; set; }
        public string DeviceSerialNumber { get; set; }
    }
}
