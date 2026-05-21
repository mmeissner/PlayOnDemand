#region Licence
/****************************************************************
 *  Filename: IUsbStorage.cs
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
    /// Represents single removeable storage drive connected to Station via USB.
    /// </summary>
    public interface IUsbStorage
    {
        /// <summary>
        /// Contains details about drive.
        /// </summary>
        DriveInfo DriveInfo { get; }

        /// <summary>
        /// Requests instance of <see cref="IUsbStorageAccess{T}"/> with specific type parameter <see cref="T"/>.
        /// Grants access to specific type of files (defined by type parameter <see cref="T"/>) stored on drive.
        /// </summary>
        /// <typeparam name="T">Defines type of files on drive to get access to.</typeparam>
        /// <returns><see cref="IUsbStorageAccess{T}"/> </returns>
        IUsbStorageAccess<T> GetStorageAccess<T>() where T : IFile, new();
    }
}
