#region Licence
/****************************************************************
 *  Filename: IAppInstallationFile.cs
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
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Managers.UsbStorage.Interfaces
{
    /// <summary>
    /// Lazy loading representation of single file based entity containing <see cref="IAppInstallationContainer{T}"/>.
    /// </summary>
    public interface IAppInstallationFile : IFile
    {
        /// <summary>
        /// <see cref="IAppInstallationContainer{T}"/> condained in entity.
        /// </summary>
        IAppInstallationContainer<IContainerPackage> AppinstallationContainer { get; }

        /// <summary>
        /// Gets fired when loading of file finishes.
        /// Cold observable, notifies subscriber even if he subscribes after event happened (like ReplaySubject).
        /// </summary>
        IObservable<LoadedState> WhenLoadFinished { get; }
    }
}
