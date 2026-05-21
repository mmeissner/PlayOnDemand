#region Licence
/****************************************************************
 *  Filename: IProcessExecutionLogicDto.cs
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

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Execution logic for executing single file and monitoring single process by its name.
    /// </summary>
    
    public interface IProcessExecutionLogicDto
    {
        /// <summary>
        /// Gets or sets the application unique identifier.
        /// </summary>
        /// <value>
        /// The application unique identifier.
        /// </value>
        Guid ApplicationGuid { get; }
        /// <summary>
        /// Get the display name of this instruction.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets or sets the platform plugin identifier.
        /// </summary>
        /// <value>
        /// The platform plugin identifier.
        /// </value>
        Guid PlatformPluginId { get; set; }

        /// <summary>
        /// Points to executable file that should be started when application execution requested.
        /// </summary>
        IDiskEntityDto ExecutionFile { get; }

        /// <summary>
        /// Command line parameters for <see cref="ExecutionFile"/>.
        /// </summary>
        string ExecutionParameters { get; }

        /// <summary>
        /// Descriptors for related processes running during application lifetime.
        /// </summary>
        IProcessMonitorInstructionDto[] MonitorInstructions { get; set; }

        /// <summary>
        /// Working Directory relative to application files root directory.
        /// </summary>
        string RelativeWorkingDirectory { get; }

        /// <summary>
        /// Gets or sets the reguired Vr Module unique identifier to run this process.
        /// </summary>
        /// <value>
        /// The reguired vr module unique identifier.
        /// </value>
        string ReguiredVrModuleGuid { get; set; }

        /// <summary>
        /// Gets the guids for the required Modules on the System to run this Process.
        /// </summary>
        /// <value>
        /// Array with Guids as string
        /// </value>
        string[] RequiredModuleGuids { get; }

        /// <summary>
        /// Gets the guids for the optional Modules on the System to run this Process.
        /// </summary>
        /// <value>
        /// Array with Guids as string
        /// </value>
        string[] OptionalModuleGuids { get; }
    }
}
