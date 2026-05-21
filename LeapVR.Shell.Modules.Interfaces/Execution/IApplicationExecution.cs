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
using System.Collections.Generic;
using System.Threading.Tasks;
using LeapVR.VBox.DataModel.Interfaces.App;

namespace LeapVR.VBox.Modules.Interfaces.Execution
{
    /// <summary>
    /// Context in which the application is executed. Notifies about <see cref="ExecutionPhase"/> changes, allows to request termination of the execution.
    /// </summary>
    public interface IApplicationExecution
    {
        /// <summary>
        /// Platform releated data connected with executing application.
        /// </summary>
        IAppPlatformData AppPlatformData { get; }

        /// <summary>
        /// Collection of <see cref="IOptionalBehavior"/> that should be executed in different moments of execution lifetime.
        /// </summary>
        IEnumerable<IOptionalBehavior> BehaviorsToExecute { get; }

        /// <summary>
        /// Fired when <see cref="ExecutionPhase"/> changes.
        /// Hot observable type with no memory (like Subject).
        /// </summary>
        IObservable<ExecutionPhase> WhenExecutionPhaseChange { get; }

        /// <summary>
        /// UTC DateTime of execution start. Can be null if execution not yet started.
        /// </summary>
        DateTime? Started { get; }

        /// <summary>
        /// UTC DateTime of execution stop. Can be null if execution not yet stoped.
        /// </summary>
        DateTime? Stopped { get; }

        /// <summary>
        /// Internal method to inform <see cref="IApplicationExecution"/> about observed changes of application <see cref="ExecutionPhase"/>.
        /// When <see cref="ExecutionPhase"/> changes all subscribers to <see cref="WhenExecutionPhaseChange"/> will be notifies, as well as all releated <see cref="BehaviorsToExecute"/> will be fired.
        /// </summary>
        /// <param name="newPhase">The desired phase</param>
        void OnExecutionPhaseChanging(ExecutionPhase newPhase); // TODO [RM]: much better if could make this internal only somehow, just implemented in concrete class ApplicationExecution

        /// <summary>
        /// 
        /// </summary>
        void Execute();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
        /// <summary>
        /// Requests termination of application execution and releated update of <see cref="ExecutionPhase"/>.
        /// </summary>
        void TerminateExecution();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task TerminateExecutionAsync();
    }
}
