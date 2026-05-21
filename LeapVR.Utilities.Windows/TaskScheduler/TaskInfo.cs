#region Licence
/****************************************************************
 *  Filename: TaskInfo.cs
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
using Microsoft.Win32.TaskScheduler;

namespace LeapVR.Utilities.Windows.TaskScheduler {
    public class TaskInfo : ITaskInfo
    {
        private readonly Task _task;
        internal TaskInfo(Task task)
        {
            _task = task;
        }
        //
        // Summary:
        //     Gets the name of the registered task.
        public string Name => _task.Name;
        //
        // Summary:
        //     Gets a value indicating whether this task is read only. Only available if Microsoft.Win32.TaskScheduler.TaskService.AllowReadOnlyTasks
        //     is true.
        public bool ReadOnly => _task.ReadOnly;
        //
        // Summary:
        //     Gets the path to where the registered task is stored.
        public string Path => _task.Path;
        //
        // Summary:
        //     Gets the number of times the registered task has missed a scheduled run.
        //
        // Exceptions:
        //   T:Microsoft.Win32.TaskScheduler.NotV1SupportedException:
        //     Not supported under Task Scheduler 1.0.
        public int NumberOfMissedRuns => _task.NumberOfMissedRuns;
        //
        // Summary:
        //     Gets the time when the registered task is next scheduled to run.
        //
        // Remarks:
        //     Potentially breaking change in release 1.8.2. For Task Scheduler 2.0, the return
        //     value prior to 1.8.2 would be Dec 30, 1899 if there were no future run times.
        //     For 1.0, that value would have been DateTime.MinValue. In release 1.8.2 and later,
        //     all versions will return DateTime.MinValue if there are no future run times.
        //     While this is different from the native 2.0 library, it was deemed more appropriate
        //     to have consistency between the two libraries and with other .NET libraries.
        public DateTime NextRunTime => _task.NextRunTime;
        //
        // Summary:
        //     Gets the results that were returned the last time the registered task was run.
        //
        // Remarks:
        //     The value returned is the last exit code of the last program run via an Microsoft.Win32.TaskScheduler.ExecAction.
        public int LastTaskResult => _task.LastTaskResult;
        //
        // Summary:
        //     Gets the time the registered task was last run.
        public DateTime LastRunTime => _task.LastRunTime;
        //
        // Summary:
        //     Gets a value indicating whether this task instance is active.
        public bool IsActive => _task.IsActive;
        //
        // Summary:
        //     Gets or sets a Boolean value that indicates if the registered task is enabled.
        //
        // Remarks:
        //     As of version 1.8.1, under V1 systems (prior to Vista), this property will immediately
        //     update the Disabled state and re-save the current task. If changes have been
        //     made to the Microsoft.Win32.TaskScheduler.TaskDefinition, then those changes
        //     will be saved.
        public bool Enabled => _task.Enabled;
    }
}