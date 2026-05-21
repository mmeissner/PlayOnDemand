#region Licence
/****************************************************************
 *  Filename: ITaskInfo.cs
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

namespace LeapVR.Utilities.Windows.TaskScheduler
{
    public interface ITaskInfo
    {
        bool Enabled { get; }
        bool IsActive { get; }
        DateTime LastRunTime { get; }
        int LastTaskResult { get; }
        string Name { get; }
        DateTime NextRunTime { get; }
        int NumberOfMissedRuns { get; }
        string Path { get; }
        bool ReadOnly { get; }
    }
}