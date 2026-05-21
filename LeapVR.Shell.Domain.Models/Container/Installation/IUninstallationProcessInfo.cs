#region Licence
/****************************************************************
 *  Filename: IUninstallationProcessInfo.cs
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
using LeapVR.Shared.Lib;

namespace LeapVR.Shell.Domain.Models.Container.Installation
{
    /// <summary>
    /// Provides information for one single application uninstallation process.
    /// </summary>
    public interface IUninstallationProcessInfo
    {
        /// <summary>
        /// Guid of application being uninstalled.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Display name of application being uninstalled.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Small thumbnail of application being uninstalled.
        /// Image as bytes.
        /// </summary>
        byte[] ApplicationThumbnail { get; }

        /// <summary>
        /// Indicates if uninstallation process has ended (successfuly or not).
        /// </summary>
        bool IsEnded { get; }

        /// <summary>
        /// Not null indicates that uninstallation process has failed.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Gets fired when uninstallation process ends.
        /// Cold observable, notifies subscriber even if he subscribes after event happened (like ReplaySubject).
        /// </summary>
        IObservable<Empty> WhenUninstallationEnded { get; }
    }
}
