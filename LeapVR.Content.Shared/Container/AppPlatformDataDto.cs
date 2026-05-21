#region Licence
/****************************************************************
 *  Filename: AppPlatformDataDto.cs
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    public class AppPlatformDataDto : IAppPlatformDataDto
    {
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformPluginId { get; set; }
        public IEnumerable<IProcessExecutionLogicDto> ExecutionLogicInstructions { get; set; }
    }
}
