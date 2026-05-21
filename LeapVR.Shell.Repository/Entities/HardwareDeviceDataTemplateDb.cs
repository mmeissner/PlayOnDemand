#region Licence
/****************************************************************
 *  Filename: HardwareDeviceDataTemplateDb.cs
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
using System.Runtime.Serialization;
using LeapVR.Shell.Domain.Models.Hardware;

namespace LeapVR.Shell.Repository.Entities
{
    [DataContract]
    
    class HardwareDeviceDataTemplateDb :IHardwareDeviceTemplateData
    {
        public HardwareDeviceDataTemplateDb()
        {
            DefaultState = DeviceState.Unknown;
        }
        [DataMember]
        public Guid HardwareDeviceTemplateGuid { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public DeviceState DefaultState { get; set; }
        [DataMember]
        public string GenericDeviceId { get; set; }
        [DataMember]
        public int EnableDelayMs { get; set; }
        [DataMember]
        public int DisableDelayMs { get; set; }
    }
}
