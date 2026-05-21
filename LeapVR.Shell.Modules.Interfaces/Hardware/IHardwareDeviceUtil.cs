#region Licence
/****************************************************************
 *  Filename: IHardwareDeviceUtil.cs
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

namespace LeapVR.Shell.Modules.Interfaces.Hardware
{
    public interface IHardwareDeviceUtil
    {
        DeviceData GetDeviceState(string deviceId);
        List<DeviceData> GetDeviceStates();
        bool DisableDevice(string deviceId);
        bool EnableDevice(string deviceId);
        bool RestartDevice(string deviceId);
    }
}