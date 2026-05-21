#region Licence
/****************************************************************
 *  Filename: IFile.cs
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
namespace LeapVR.Shell.Managers.UsbStorage.Interfaces
{
    /// <summary>
    /// Lazy loading represents single file in file system structure.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Removeable USB storage file is located at.
        /// </summary>
        IUsbStorage UsbStorage { get; set; }

        /// <summary>
        /// Name of a file.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Full path to file.
        /// </summary>
        string AbsolutePath { get; set; }

        /// <summary>
        /// <see cref="IFolder{T}"/> that file is located at.
        /// </summary>
        object Parent { get; set; } // TODO [RM]: object becouse cannot use IFolder<IFile> here

        /// <summary>
        /// Loading state of file.
        /// </summary>
        LoadedState LoadedState { get; }

        /// <summary>
        /// Loads file, performing required reads on file content.
        /// </summary>
        /// <returns>State after loading try.</returns>
        LoadedState LoadFile();
    }

    public enum LoadedState
    {
        NotLoaded = 0,
        Success = 1,
        Failure = 2,
    }
}
