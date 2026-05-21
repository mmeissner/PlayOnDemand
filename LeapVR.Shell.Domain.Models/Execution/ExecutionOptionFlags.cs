#region Licence
/****************************************************************
 *  Filename: ExecutionOptionFlags.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-12
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

namespace LeapVR.Shell.Domain.Models.Execution
{
    [Flags]
    public enum ExecutionOptionFlags
    {
        /// <summary>
        /// Specifies that no option flags are applied
        /// </summary>
        Default = 0,
        /// <summary>
        /// Specifies that the shell should track the application's child processes during lifetime. it will ignore the main process nam as the recognition of an application and it will silghtly slows down the performance.
        /// </summary>
        EnableTrackingChildProcesses = 1,
        /// <summary>
        /// Specifies that the shell should detect whether the application is responding or not during lifetime.
        /// </summary>
        EnableNotRespondingDetection = 2,
    }
}
