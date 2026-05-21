#region Licence
/****************************************************************
 *  Filename: HardwareDeviceModule.cs
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
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Hardware;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Hardware;

namespace LeapVR.Shell.Modules.Hardware
{
    public class HardwareDeviceModule : IHardwareDeviceModule
    {
        private readonly IHardwareDeviceUtil _hardwareDeviceUtil;
        private readonly IHardwareDeviceTemplateRepository _deviceTemplateRepository;

        public HardwareDeviceModule(IHardwareDeviceUtil hardwareDeviceUtil, IHardwareDeviceTemplateRepository deviceTemplateRepository)
        {
            _hardwareDeviceUtil = hardwareDeviceUtil;
            _deviceTemplateRepository = deviceTemplateRepository;
        }
        public Guid ModuleId => Guid.Parse("edb3ed53-b4b6-4097-b2b7-a1e44115a946");
        public string ModuleName => "HardwareDevice Module";

        public IEnumerable<IHardwareDevice> GetHardware()
        {
            return AssignHardwareToTemplate(_hardwareDeviceUtil.GetDeviceStates(), _deviceTemplateRepository.GetAll());
        }
        public DeviceData EnableHardware(IHardwareDevice hardwareDeviceData)
        {
            throw new NotImplementedException();
        }
        public DeviceData DisableHardware(IHardwareDevice hardwareDeviceGuids)
        {
            throw new NotImplementedException();
        }

        private List<IHardwareDevice> AssignHardwareToTemplate(List<DeviceData> realDevices,IEnumerable<IHardwareDeviceTemplateData> templates)
        {
            throw new NotImplementedException();
        }
    }
}
