#region Licence
/****************************************************************
 *  Filename: IFolder.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LeapVR.Shell.Managers.UsbStorage.Interfaces
{
    /// <summary>
    /// Lazy loading representation of single directory in directory structure.
    /// Loaded after call to <see cref="LoadFolder"/>.
    /// </summary>
    /// <typeparam name="T">Type of files access to is desired</typeparam>
    public interface IFolder<T> : INotifyPropertyChanged where T : IFile, new()
    {
        /// <summary>
        /// Name of a folder.
        /// </summary>
        string FolderName { get; }

        /// <summary>
        /// Parent <see cref="IFolder{T}"/> object, where current folder is located.
        /// Null if this is root folder of its drive, or access to higher folders is not alowed.
        /// </summary>
        IFolder<T> Parent { get; }

        /// <summary>
        /// Absolute path to this folder.
        /// </summary>
        string AbsolutePath { get; }

        /// <summary>
        /// Folders located inside of this folder.
        /// </summary>
        /// TODO [FH]: type here should IObservableCollection not directly referencing the Caliburn.Micro.
        ObservableCollection<IFolder<T>> Folders { get; }

        /// <summary>
        /// Files located inside of this folder.
        /// </summary>
        /// TODO [FH]: type here should IObservableCollection not directly referencing the Caliburn.Micro.
        ObservableCollection<T> Files { get; }

        /// <summary>
        /// Indicates of folder has been loaded using <see cref="LoadFolder"/>.
        /// No access to <see cref="Folders"/>, <see cref="Files"/> is possible if folder is not loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Scans folder on drive to reveal it's content and place it in <see cref="Folders"/> and <see cref="Files"/> collections.
        /// </summary>
        /// <returns>Boolean indicating if loading was successful.</returns>
        bool LoadFolder();
    }
}
