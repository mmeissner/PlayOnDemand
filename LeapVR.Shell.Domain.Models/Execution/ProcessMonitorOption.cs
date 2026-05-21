#region Licence
/****************************************************************
 *  Filename: ProcessMonitorOption.cs
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

namespace LeapVR.Shell.Domain.Models.Execution
{
    
    [Flags]
    public enum ProcessMonitorOption
    {
        /// <summary>
        /// No Options are specified, Process will be ignored
        /// </summary>
        Ignore = 0,

        /// <summary>
        ///Kills these process on application exit
        /// </summary>
        KillOnExit = 1 << 0,

        /// <summary>
        /// The kill process on not responding
        /// </summary>
        KillProcessOnNotResponding = 1 << 1,

        /// <summary>
        /// Specifies that the process should be one of those that support the application to run. 
        /// </summary>
        IsMainExecutable = 1 << 2,
    }
}
