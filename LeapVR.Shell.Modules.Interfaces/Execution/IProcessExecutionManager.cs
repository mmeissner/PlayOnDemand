#region Licence
/****************************************************************
 *  Filename: IProcessExecutionManager.cs
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

namespace LeapVR.Shell.Modules.Interfaces.Execution
{
    /// <summary>
    /// Manages operating system's process execution environment.
    /// </summary>
    [Obsolete ("Do not use this interface it will be removed with the next versions")]
    public interface IProcessExecutionManager
    {
        /// <summary>
        /// Creates new <see cref="IProcessExecution"/> object with given parameters, that can be started later to start process execution.
        /// </summary>
        /// <param name="path">Full path to file requested to execute</param>
        /// <param name="parameters">Command line parameters to start process with</param>
        /// <param name="workingDirectory">Sets working directory for process execution</param>
        /// <returns><see cref="IProcessExecution"/> handler</returns>
        IProcessExecution CreateProcessExecution(string path, string parameters, string workingDirectory);

        /// <summary>
        /// Finds all processes with given <see cref="processName"/> and creates <see cref="IProcessExecution"/> wrapper for each of them.
        /// </summary>
        /// <param name="processName">Process name to find</param>
        /// <returns>Collection of <see cref="IProcessExecution"/> handlers</returns>
        IEnumerable<IProcessExecution> AttachToProcesses(string processName);
    }
}
