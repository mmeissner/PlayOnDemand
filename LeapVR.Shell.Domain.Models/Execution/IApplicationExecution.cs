#region Licence
/****************************************************************
 *  Filename: IApplicationExecution.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-7
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Domain.Models.Execution
{
    /// <summary>
    /// Context in which the application is executed. Notifies about <see cref="ExecutionPhase"/> changes, allows to request termination of the execution.
    /// </summary>
    public interface IApplicationExecution
    {
        /// <summary>
        /// Gets the display information.
        /// </summary>
        /// <value>
        /// The display information.
        /// </value>
        IAppPlatformInfo DisplayInfo { get; }

        /// <summary>
        /// UTC DateTime of execution start. Can be null if execution not yet started.
        /// </summary>
        DateTime? Started { get; }

        /// <summary>
        /// UTC DateTime of execution stop. Can be null if execution not yet stoped.
        /// </summary>
        DateTime? Stopped { get; }

        /// <summary>
        /// Gets the Process executionlogic to execute.
        /// </summary>
        /// <value>
        /// The logic to execute.
        /// </value>
        IProcessExecutionLogic LogicToExecute { get; }

        /// <summary>
        /// Fired when <see cref="ExecutionPhase"/> changes.
        /// Hot observable type with no memory (like Subject).
        /// </summary>
        IObservable<AppExecutionMessage> WhenExecutionPhaseChange { get; }

        void Run();
        void Terminate(bool isSystemShutdown);

        /// <summary>
        /// Returns the Executions duration in ticks.
        /// </summary>
        /// <returns></returns>
        long ExecutionDurationTicks();
    }
}
