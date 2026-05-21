#region Licence
/****************************************************************
 *  Filename: TaskAction.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;

namespace LeapVR.Utilities.Windows.TaskScheduler
{

    public static partial class TaskSchedulerUtil
    {
        public class ExecuteAppAction : ITaskSchedulerAction
        {
            public ExecuteAppAction(string executable, string arguments = null, string workingDirectory = null)
            {
                Executable = executable;
                WorkingDirectory = workingDirectory;
                Arguments = arguments;
            }

            public string Executable { get; }
            public string WorkingDirectory { get; }
            public string Arguments { get; }
            internal ExecAction ToExecAction()
            {
                return new ExecAction(Executable, Arguments, WorkingDirectory);
            }
        }
        public interface ITaskSchedulerAction
        {
        }
    }
}
