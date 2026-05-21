#region Licence
/****************************************************************
 *  Filename: IHardwareDeviceTemplateRepository.cs
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
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Hardware;

namespace LeapVR.VBox.Modules.Interfaces.Repositories
{
    public interface IHardwareDeviceTemplateRepository
    {
        IHardwareDeviceTemplateData Get(Guid hardwareTemplateId);
        IEnumerable<IHardwareDeviceTemplateData> GetAll();
        void Store(IHardwareDeviceTemplateData dataTemplate);
        void Delete(IHardwareDeviceTemplateData dataTemplate);
    }
}