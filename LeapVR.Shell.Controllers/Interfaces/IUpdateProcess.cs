#region Licence
/****************************************************************
 *  Filename: IUpdateProcess.cs
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
using System.Threading.Tasks;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Responsible for controlling and monitoring state of update process of the application.
    /// </summary>
    public interface IUpdateProcess
    {
        /// <summary>
        /// Current version of application.
        /// </summary>
        Version CurrentVersion { get; }

        /// <summary>
        /// Current <see cref="UpdateState"/> of update process.
        /// Value changes when update operations (<see cref="CheckNewestVersionAsync"/>, <see cref="DownloadNewestVersionAsync"/>, <see cref="PerformUpdateAsync"/>, <see cref="CancelAsync"/>) are performed.
        /// </summary>
        UpdateState State { get; }

        /// <summary>
        /// Gets fired when <see cref="State"/> changes.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<UpdateState> WhenStateChanged { get; }

        /// <summary>
        /// Holds the newest available software version for download fetched from update server at <see cref="CheckedAt"/> time. Null if not yet checked.
        /// </summary>
        Version NewestVersion { get; }

        /// <summary>
        /// Holds <see cref="DateTime"/> of last successful <see cref="CheckNewestVersionAsync"/> operation, or null if never succedeed on it.
        /// </summary>
        DateTime? CheckedAt { get; }

        /// <summary>
        /// Gets fired when <see cref="DownloadProgressPercent"/> and <see cref="DownloadSpeed"/> value changes.
        /// Hot observable, notifies subscriber only real-time, no memory (like Subject).
        /// </summary>
        IObservable<Shared.Lib.Empty> WhenDownloadProgressChanged { get; }

        /// <summary>
        /// Holds value in range of 0.0 to 100.0 indicating current progress of downloading new version archive file, or null.
        /// Null means progress is not known.
        /// </summary>
        double? DownloadProgressPercent { get; }

        /// <summary>
        /// Holds value indicating current speed of download process in bytes/second.
        /// </summary>
        double DownloadSpeed { get; }

        /// <summary>
        /// Contacts the server and checks what is the newest version of software available for download and update. Can be cancelled by calling <see cref="CancelAsync"/> method.
        /// Can be called when <see cref="State"/> holds one of folowing values: `NotStarted`, `NoUpdatesAvailable`, `UpdateAvailable`, `ReadyToUpdate`.
        /// </summary>
        /// <returns>Task that completes when success and throws exception when failed for some reason (e.g. internet problems)</returns>
        Task<bool> CheckNewestVersionAsync();

        /// <summary>
        /// Downloads archive containing newest version files from the server. Updates <see cref="DownloadProgressPercent"/> and <see cref="DownloadSpeed"/> periodicaly. Can be cancelled by calling <see cref="CancelAsync"/> method.
        /// Can be called when <see cref="State"/> holds `UpdateAvailable` value.
        /// </summary>
        /// <returns>Task that completes when success and throws exception when failed for some reason (e.g. internet problems)</returns>
        Task<bool> DownloadNewestVersionAsync();

        /// <summary>
        /// Uses downloaded archive to perform update of the application to newer version. Cannot be cancelled.
        /// Can be called when <see cref="State"/> holds `ReadyToUpdate` value.
        /// </summary>
        /// <returns>Task that completes when success and throws exception when failed for some reason.</returns>
        Task<bool> PerformUpdateAsync();

        /// <summary>
        /// Cancels <see cref="CheckNewestVersionAsync"/> or <see cref="DownloadNewestVersionAsync"/> processes.
        /// Can be called when <see cref="State"/> holds one of folowing values: `CheckingNewestVersion`, `Downloading`.
        /// </summary>
        /// <returns>Task that completes when success and throws exception when failed for some reason.</returns>
        Task<bool> CancelAsync();
    }
}
