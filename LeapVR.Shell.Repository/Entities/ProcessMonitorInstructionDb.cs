#region Licence
/****************************************************************
 *  Filename: ProcessMonitorInstructionDb.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Repository.Entities
{
    
    class ProcessMonitorInstructionDb : IProcessMonitorInstruction
    {
        public string ExecutableRelativePathFileName { get; set; }
        public ProcessMonitorOption Instruction { get; set; }

        public ProcessMonitorInstructionDb(){}
        public ProcessMonitorInstructionDb(IProcessMonitorInstruction monitorInstruction)
        {
            ExecutableRelativePathFileName = monitorInstruction.ExecutableRelativePathFileName;
            Instruction = monitorInstruction.Instruction;
        }
    }
}
