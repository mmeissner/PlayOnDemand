#region Licence
/****************************************************************
 *  Filename: IUsbStorageAccess.cs
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
using System.IO;

namespace LeapVR.Shell.Managers.UsbStorage.Interfaces
 {
    /// <summary>
    /// Provides access to specific type of files (defined by type parameter <see cref="T"/>) stored on drive.
    /// </summary>
    /// <typeparam name="T">Defines type of files on drive to get access to. Must implement <see cref="IFile"/>.</typeparam>
    public interface IUsbStorageAccess<T> where T : IFile, new()
    {
        /// <summary>
        /// Contains details about drive.
        /// </summary>
        DriveInfo DriveInfo { get; }

        /// <summary>
        /// Instance of <see cref="IFolder{T}"/> representing root directory of drive.
        /// </summary>
        IFolder<T> RootFolder { get; }
    }
}
