#region Licence
/****************************************************************
 *  Filename: ProcessExecutionLogicDb.cs
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
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Repository.Entities
{
    
    class ProcessExecutionLogicDb :IProcessExecutionLogic
    {
        public Guid ApplicationGuid { get; set; }
        public Guid ExecutionGuid { get; set; }
        public string DisplayName { get; set; }
        public Guid PlatformPluginId { get; set; }
        public IDiskEntity ExecutionFile { get; set; }
        public string ExecutionParameters { get; set; }
        public IProcessMonitorInstruction[] MonitorInstructions { get; set; }
        public string RelativeWorkingDirectory { get; set; }
        public string ReguiredVrModuleGuid { get; set; }
        public string[] RequiredModuleGuids { get; set; }
        public string[] OptionalModuleGuids { get; set; }

        public ProcessExecutionLogicDb(){}
        public ProcessExecutionLogicDb(IProcessExecutionLogic instruction)
        {
            ApplicationGuid = instruction.ApplicationGuid;
            ExecutionGuid = instruction.ExecutionGuid;
            DisplayName = instruction.DisplayName;
            PlatformPluginId = instruction.PlatformPluginId;
            if(instruction.ExecutionFile != null )ExecutionFile = new DiskEntityDb(instruction.ExecutionFile);
            ExecutionParameters = instruction.ExecutionParameters;
            if(instruction.MonitorInstructions != null)
            {
                var monitorInstructions = new List<IProcessMonitorInstruction>();
                foreach(var monitorInstruction in instruction.MonitorInstructions)
                {
                    monitorInstructions.Add(new ProcessMonitorInstructionDb(monitorInstruction));
                }
                MonitorInstructions = monitorInstructions.ToArray();
            }
            RelativeWorkingDirectory = instruction.RelativeWorkingDirectory;
            ReguiredVrModuleGuid = instruction.ReguiredVrModuleGuid;
            RequiredModuleGuids = instruction.RequiredModuleGuids;
            OptionalModuleGuids = instruction.OptionalModuleGuids;
        }
    }
}
