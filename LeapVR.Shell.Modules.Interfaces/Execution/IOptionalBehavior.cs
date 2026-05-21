#region Licence
/****************************************************************
 *  Filename: IOptionalBehavior.cs
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

namespace LeapVR.VBox.Modules.Interfaces.Execution
{
    /// <summary>
    /// Attachable to <see cref="IApplicationExecution"/>, holds behavior that will react to changes of <see cref="ExecutionPhase"/>.
    /// </summary>
    public interface IOptionalBehavior
    {
        /// <summary>
        /// Will be called when <see cref="ExecutionPhase"/> has changed.
        /// </summary>
        /// <param name="newPhase">Newly set <see cref="ExecutionPhase"/></param>
        void OnPhaseChanged(ExecutionPhase newPhase);

        /// <summary>
        /// Will be called when execution has fiinished (due to error or successfuly).
        /// </summary>
        void Cleanup();
    }
}
