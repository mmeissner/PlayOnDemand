#region Licence
/****************************************************************
 *  Filename: IInstallationProcessInfo.cs
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

using System;

namespace LeapVR.Shell.Domain.Models.Container.Installation
{
    /// <summary>
    /// Defines one single application installation process from an Container.
    /// </summary>
    public interface IInstallationProcessInfo
    {
        /// <summary>
        /// Guid of application being installed.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Display name of application being installed.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Small thumbnail of application being installed.
        /// Image as bytes.
        /// </summary>
        byte[] ApplicationThumbnail { get; }

        /// <summary>
        /// Emit new instance of <see cref="InstallationProgress"/> when installation get started, in progress and ended.
        /// When subscribed afterwards all notifications will be pushed to subscribers instantly. (ReplaySubject)
        /// </summary>
        IObservable<InstallationProgress> WhenInstallationProgressChanged { get; }

        /// <summary>
        /// Indicates progress of installation process.
        /// </summary>
        int PercentageDone { get; }

        /// <summary>
        /// Not null indicates that installation process has failed.
        /// </summary>
        Exception Exception { get; }
    }
}
