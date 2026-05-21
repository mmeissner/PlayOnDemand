#region Licence
/****************************************************************
 *  Filename: AppPlatformData.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.Platform
{
    
    public class AppPlatformData : IAppPlatformData
    {
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformPluginId { get; set; }
        public string ApplicationName { get; set; }
        public IEnumerable<IProcessExecutionLogic> ExecutionLogicInstructions { get; set; }
        public bool IsEnabled { get; set; }
    }
}