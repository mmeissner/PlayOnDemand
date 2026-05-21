#region Licence
/****************************************************************
 *  Filename: UpdateState.cs
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

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Represents state of software update process.
    /// </summary>
    public enum UpdateState
    {
        Unknown = 0,

        /// <summary>
        /// Cancellation in progress.
        /// </summary>
        Canceling = 1,

        /// <summary>
        /// RestartByLauncher process was not started.
        /// </summary>
        NotStarted = 2,

        /// <summary>
        /// RestartByLauncher process has ended with error.
        /// </summary>
        Errored = 3,

        /// <summary>
        /// Checking for new version in progress.
        /// </summary>
        CheckingNewestVersion = 4,

        /// <summary>
        /// RestartByLauncher process has ended - no new version updates available.
        /// </summary>
        NoUpdatesAvailable = 5,

        /// <summary>
        /// New version update is available, update possible.
        /// </summary>
        UpdateAvailable = 6,

        /// <summary>
        /// Downloading new version update in progress.
        /// </summary>
        Downloading = 7,

        /// <summary>
        /// New version update downloaded successfuly, ready to update.
        /// </summary>
        ReadyToUpdate = 8,

        /// <summary>
        /// RestartByLauncher process in progress.
        /// </summary>
        Updating = 9,

        /// <summary>
        /// RestartByLauncher process ended successfuly, new version will be used after restart.
        /// </summary>
        AwaitingRestart = 10,
    }
}
