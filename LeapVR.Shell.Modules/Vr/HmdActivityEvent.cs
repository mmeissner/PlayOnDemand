#region Licence
/****************************************************************
 *  Filename: HmdActivityEvent.cs
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

using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Modules.Vr
{
    public class HmdActivityEvent : IHmdActivityEvent
    {
        public HmdActivityStatus Status { get; set; }
    }
}
