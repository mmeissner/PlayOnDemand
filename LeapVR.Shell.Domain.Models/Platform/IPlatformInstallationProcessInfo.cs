#region Licence
/****************************************************************
 *  Filename: IPlatformInstallationProcessInfo.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.Platform
{
    /// <summary>
    /// Defines one single application installation process from an Platform.
    /// </summary>
    public interface IPlatformInstallationProcessInfo
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
        /// Emit new instance of <see cref="IPlatformInstallationProgress"/> to inform about the current installation progress.
        /// When subscribed afterwards all notifications will be pushed to subscribers instantly. (ReplaySubject)
        /// </summary>
        IObservable<IPlatformInstallationProgress> WhenInstallationProgressChanged { get; }

        /// <summary>
        /// Not null indicates that installation process has failed.
        /// </summary>
        Exception Exception { get; }
    }
}
