#region Licence
/****************************************************************
 *  Filename: IProcessMonitorInstructionDto.cs
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
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Represents a descriptor how the process will be executed during application runtime.
    /// </summary>
    
    public interface IProcessMonitorInstructionDto
    {
        /// <summary>
        /// Get process name
        /// </summary>
        string ExecutableRelativePathFileName { get; }
        /// <summary>
        /// Options that are applied to the process.
        /// </summary>
        ProcessMonitorOption Instruction { get; }
    }
}
