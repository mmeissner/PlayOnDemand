#region Licence
/****************************************************************
 *  Filename: HardwareDeviceTemplate.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Hardware
{
    
    public class HardwareDeviceTemplate : IHardwareDeviceTemplateData
    {
        public Guid HardwareDeviceTemplateGuid { get; set; }
        public string DisplayName { get; set; }
        public DeviceState DefaultState { get; set; }
        public string GenericDeviceId { get; set; }
        public int EnableDelayMs { get; set; }
        public int DisableDelayMs { get; set; }
    }
}