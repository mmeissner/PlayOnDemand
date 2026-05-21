#region Licence
/****************************************************************
 *  Filename: IHardwareDevice.cs
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

using LeapVR.Shell.Domain.Models.Hardware;

namespace LeapVR.Shell.Modules.Interfaces.Hardware
{
    public interface IHardwareDevice :IHardwareDeviceTemplateData
    {
        DeviceState CurrentState { get; }
        string SpecificId { get; }
        void Enable();
        void Disable();

    }
}
