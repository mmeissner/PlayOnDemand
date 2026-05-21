#region Licence
/****************************************************************
 *  Filename: ExecutionPhase.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Execution
{
    /// <summary>
    /// Specifies in which phase execution of application is.
    /// </summary>
    public enum ExecutionPhase
    {
        /// <summary>
        /// The App is not started
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Signal before the App is going to be started by the Platform
        /// </summary>
        BeforeStart = 10,
        /// <summary>
        /// The Phase the Platform Executes Start Operation
        /// </summary>
        OnPlatformStart = 20,
        /// <summary>
        /// Signal after the App was detected running
        /// </summary> 
        AfterStart = 30,
        /// <summary>
        /// Signal before Platform gets Termination Command
        /// </summary>
        BeforeExit = 40,
        /// <summary>
        /// The Phase the Platform Executes Termination
        /// </summary>
        OnPlatformEnd = 50,
        /// <summary>
        /// Signal after the App was detected exited
        /// </summary>
        AfterExit = 60,
        /// <summary>
        /// Signal after the System is completely done with ExecutionPhase handling
        /// </summary>
        OnFinished = 70,
        /// <summary>
        /// Signal after the System is completely done with ExecutionPhase handling
        /// </summary>
        OnDone = 1000,
    }

}
