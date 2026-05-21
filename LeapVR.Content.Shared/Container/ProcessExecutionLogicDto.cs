#region Licence
/****************************************************************
 *  Filename: ProcessExecutionLogicDto.cs
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    public class ProcessExecutionLogicDto : IProcessExecutionLogicDto
    {
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformPluginId { get; set; }
        public string DisplayName { get; set; }
        public IDiskEntityDto ExecutionFile { get; set; }
        public string ExecutionParameters { get; set; }
        public string RelativeWorkingDirectory { get; set; }
        public IProcessMonitorInstructionDto[] MonitorInstructions { get; set; }
        public string ReguiredVrModuleGuid { get; set; }
        public string[] RequiredModuleGuids { get; set; }
        public string[] OptionalModuleGuids { get; set; }
    }
}
