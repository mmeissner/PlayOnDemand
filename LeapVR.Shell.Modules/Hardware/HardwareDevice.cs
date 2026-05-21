#region Licence
/****************************************************************
 *  Filename: HardwareDevice.cs
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

using System;
using System.Reflection;
using LeapVR.Shell.Domain.Models.Hardware;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Hardware;

namespace LeapVR.Shell.Modules.Hardware
{
    
    public class HardwareDevice : IHardwareDevice
    {
        private IHardwareDeviceModule _hardwareDeviceModule;
        private IHardwareDeviceTemplateData _templateData;
        private DeviceData _deviceData;

        public HardwareDevice(IHardwareDeviceTemplateData template,DeviceData device, IHardwareDeviceModule module)
        {
            _templateData = template;
            _deviceData = device;
            _hardwareDeviceModule = module;
        }

        public Guid HardwareDeviceTemplateGuid => _templateData.HardwareDeviceTemplateGuid;
        public string DisplayName => _templateData.DisplayName;
        public DeviceState DefaultState => _templateData.DefaultState;
        public string GenericDeviceId => _templateData.GenericDeviceId;
        public DeviceState CurrentState => _deviceData.State;
        public string SpecificId => _deviceData.DeviceId;
        public int EnableDelayMs => _templateData.EnableDelayMs;
        public int DisableDelayMs => _templateData.DisableDelayMs;
        public void Enable()
        {
            _deviceData = _hardwareDeviceModule.EnableHardware(this);
        }
        public void Disable()
        {
            _deviceData = _hardwareDeviceModule.DisableHardware(this);
        }
    }
}
