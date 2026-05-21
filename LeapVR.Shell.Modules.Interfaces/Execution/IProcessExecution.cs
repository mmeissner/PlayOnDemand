#region Licence
/****************************************************************
 *  Filename: IProcessExecution.cs
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
using LeapVR.Shell.Modules.Interfaces.Utilities.WinApi;

namespace LeapVR.Shell.Modules.Interfaces.Execution
{
    /// <summary>
    /// Context of single process running on operating system.
    /// Can be used to learn about process status, as well as manipulate this status (<see cref="Start"/>, <see cref="Kill"/>).
    /// </summary>
    public interface IProcessExecution
    {
        /// <summary>
        /// Get the collection of states that contain all states to uniquely identify a process.
        /// </summary>
        ProcessIdentifier Identifier { get; }
        ///// <summary>
        ///// Numeric process ID from underlying operating system. Null if not yet known (due to execution not yet started, for example).
        ///// </summary>
        //int? Id { get; }
        ///// <summary>
        ///// Numeric process parent ID from underlying operating system.
        ///// </summary>
        //int? ParentProcessId { get; }
        ///// <summary>
        ///// Get the time when the process get started.
        ///// </summary>
        //DateTime StartTime { get; }
        /// <summary>
        /// Process name. Null if not yet known (due to execution not yet started, for example).
        /// </summary>
        string ProcessName { get; }
        /// <summary>
        /// Get process handle
        /// </summary>
        IntPtr ProcessHandle { get; }
        /// <summary>
        /// The full path to process being executed.
        /// </summary>
        string ExecutablePath { get; }

        /// <summary>
        /// Parameters that the process was executed with.
        /// </summary>
        string ExecutableParams { get; }

        /// <summary>
        /// Working directory of process executable.
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        /// Indicates if execution of process has been already started (not necessary is still running, though).
        /// </summary>
        bool HasStarted { get; }

        /// <summary>
        /// Indicates if execution of process has ended.
        /// </summary>
        bool HasEnded { get; }

        /// <summary>
        /// Indicates if end of process execution was due to termination by calling <see cref="Kill"/> method.
        /// </summary>
        bool WasTerminated { get; }

        /// <summary>
        /// Gets fired when process execution ends.
        /// Cold observable, notifies subscriber even if he subscribes after event happened (like ReplaySubject).
        /// </summary>
        IObservable<IProcessExecution> WhenEnded { get; }

        /// <summary>
        /// Holds instance of <see cref="IWindowHook"/> releated to current process.
        /// </summary>
        IWindowHook WindowHook { get; }

        /// <summary>
        /// Indicates if process main window is responsive (is processing message loop messages).
        /// Appliable only to UI processes.
        /// </summary>
        bool IsResponding { get; }

        /// <summary>
        /// Gets fired when <see cref="IsResponding"/> changes.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<bool> WhenIsRespondingChanged { get; }

        /// <summary>
        /// Starts the execution of process if it has not been started yet.
        /// Fills process <see cref="Id"/> if successful.
        /// Throws exception if not successful.
        /// </summary>
        void Start();

        /// <summary>
        /// Terminates the process.
        /// </summary>
        void Kill();
    }
}
