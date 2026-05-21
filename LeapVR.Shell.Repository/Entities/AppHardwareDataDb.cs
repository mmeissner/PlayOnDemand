#region Licence
/****************************************************************
 *  Filename: AppHardwareDataDb.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Repository.Entities
{
    
    class AppHardwareDataDb:  IAppHardwareData
    {
        public Guid Id { get; set; }
        public Guid ApplicationGuid { get; set; }
        public IEnumerable<Guid> RequiredDevices { get; set; }
        public IEnumerable<Guid> DisabledDevices { get; set; }
    }
}