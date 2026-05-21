#region Licence
/****************************************************************
 *  Filename: XInputModuleConfig.cs
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

using System.Reflection;
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Shell.Modules.FileConfig
{
    
    public class XInputModuleConfig : ConfigObject
    {
        public int XInputDevicePollingPerSecond { get; set; } = 25;
        public int XInputDeviceResendDelay { get; set; } = 700;
        public int XInputDeviceResendEachMs { get; set; } = 200;
        public string XInputCompositeButtonsForForceQuitingApps { get; set; } = "Start,Back";
        public int MillisecondsToHoldBeforeCompositeButtonsTakeEffect { get; set; } = 6000;
        public int MillisecondsToWaitBeforeCompositeButtonsThrottleReOpen { get; set; } = 5000;
    }
}
