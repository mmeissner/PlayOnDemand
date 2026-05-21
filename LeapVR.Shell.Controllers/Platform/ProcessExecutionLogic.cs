#region Licence
/****************************************************************
 *  Filename: ProcessExecutionLogic.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Disk;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Controllers.Platform
{
    class ProcessExecutionLogic : IProcessExecutionLogic 
    {
        public Guid ApplicationGuid { get;  }
        public Guid ExecutionGuid { get; }
        public string DisplayName { get; set; }
        public Guid PlatformPluginId { get; }
        public IDiskEntity ExecutionFile { get; set; }
        public string ExecutionParameters { get; set; }
        public IProcessMonitorInstruction[] MonitorInstructions { get; set; }
        public string RelativeWorkingDirectory { get; set; }
        public string ReguiredVrModuleGuid { get; set; }
        public string[] RequiredModuleGuids { get; set; }
        public string[] OptionalModuleGuids { get; set; }

        public ProcessExecutionLogic(IEditableProcessExecutionLogic processExecution)
        {
            ApplicationGuid = processExecution.ApplicationId;
            PlatformPluginId = processExecution.PlatformPluginId;
            ExecutionGuid = processExecution.ExecutionGuid;
            DisplayName = processExecution.DisplayName;
            RelativeWorkingDirectory = processExecution.RelativeWorkingDirectory;
            ReguiredVrModuleGuid = processExecution.RequiredVrModuleGuid;
            RequiredModuleGuids = processExecution.RequiredModuleGuids;
            OptionalModuleGuids = processExecution.OptionalModuleGuids;
            MonitorInstructions =
                    new List<IProcessMonitorInstruction>(processExecution.MonitorInstructions).ToArray();
            ExecutionFile = new DiskEntity(processExecution.ExecutionFile);
        }



        public ProcessExecutionLogic(IAppPlatformData platformData)
        {
            ApplicationGuid = platformData.ApplicationGuid;
            PlatformPluginId = platformData.PlatformPluginId;
            ExecutionGuid = Guid.NewGuid();
            DisplayName = "No Name";
            ReguiredVrModuleGuid = null;
            RequiredModuleGuids = new string[0];
            OptionalModuleGuids = new string[0];
            ExecutionFile = new DiskEntity(platformData.ApplicationGuid,platformData.PlatformPluginId, DiskEntityType.Absolute);
            MonitorInstructions = new IProcessMonitorInstruction[0];
        }

        public ProcessExecutionLogic(IProcessExecutionLogicDto processExecutionLogicDto,DiskEntityType diskEntityType)
        {
            ApplicationGuid = processExecutionLogicDto.ApplicationGuid;
            ExecutionGuid = Guid.NewGuid();
            PlatformPluginId = processExecutionLogicDto.PlatformPluginId;
            DisplayName = processExecutionLogicDto.DisplayName;
            ExecutionFile = new DiskEntity(processExecutionLogicDto.ExecutionFile,processExecutionLogicDto.PlatformPluginId,diskEntityType);
            ExecutionParameters = processExecutionLogicDto.ExecutionParameters;
            MonitorInstructions = (from instruction in processExecutionLogicDto.MonitorInstructions
                                   select new ProcessMonitorInstruction(instruction) as IProcessMonitorInstruction).
                    ToArray();
            RelativeWorkingDirectory = processExecutionLogicDto.RelativeWorkingDirectory;
            ReguiredVrModuleGuid = processExecutionLogicDto.ReguiredVrModuleGuid;
            RequiredModuleGuids = processExecutionLogicDto.RequiredModuleGuids;
            OptionalModuleGuids = processExecutionLogicDto.OptionalModuleGuids;
        }
    }
}
