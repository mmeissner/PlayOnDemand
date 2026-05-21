#region Licence
/****************************************************************
 *  Filename: Folder.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LeapVR.Shell.Managers.Annotations;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;

namespace LeapVR.Shell.Managers.UsbStorage
{
    public class Folder<T> : INotifyPropertyChanged, IFolder<T> where T : IFile, new()
    {
        #region Properties & Fields

        public string FolderName { get; }
        public IFolder<T> Parent { get; }
        public string AbsolutePath { get; }
        public ObservableCollection<T> Files { get; } = new ObservableCollection<T>();
        public ObservableCollection<IFolder<T>> Folders { get; } = new ObservableCollection<IFolder<T>>();

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set
            {
                _isLoaded = value;
                OnPropertyChanged();
            }
        }

        private readonly UsbStorage _usbStorage;

        #endregion Properties & Fields

        #region Constructors

        internal Folder(UsbStorage usbStorage, IFolder<T> parent, string name)
        {
            if (usbStorage == null || name == null)
            {
                throw new ArgumentNullException(usbStorage == null ? nameof(usbStorage) : nameof(name));
            }

            FolderName = name;
            Parent = parent;
            _usbStorage = usbStorage;
            AbsolutePath = Path.Combine(parent?.AbsolutePath ?? "", name);
        }

        #endregion Constructors

        #region Method

        public bool LoadFolder()
        {
            // TODO [RM]: only allow one thread to execute at time (but smarter than Semaphores/lock)

            if (IsLoaded)
            {
                return true;
            }

            if (!LoadFolders() || !LoadFiles())
            {
                // TODO [RM]: handle folder is dead state
                return false;
            }

            IsLoaded = true;
            return true;
        }

        private bool LoadFolders()
        {
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(AbsolutePath);
            }
            catch
            {
                // TODO [RM]: set folder state to dead?
                _usbStorage.CheckIfDriveRemoved(true);
                return false;
            }

            foreach (var dirFullPath in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dirFullPath);
                    if (dirInfo.Attributes.HasFlag(FileAttributes.System)
                        || dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        // skip system or hidden directories
                        continue;
                    }

                    var dirName = Path.GetFileName(dirFullPath);
                    var newFolder = new Folder<T>(_usbStorage, this, dirName);
                    Folders.Add(newFolder);
                }
                catch
                {
                    if (_usbStorage.CheckIfDriveRemoved(true))
                    {
                        return false;
                    }

                    // folder specific error - just ignore
                }

            }

            return true;
        }

        private bool LoadFiles()
        {
            var searchFiltersAttribute = typeof(T)
                .GetCustomAttributes(typeof(FileSearchFiltersAttribute), true)
                .Cast<FileSearchFiltersAttribute>()
                .DefaultIfEmpty(new FileSearchFiltersAttribute(new[] { "*" }))
                .Single();

            string[] fullFilePaths;
            try
            {
                fullFilePaths = searchFiltersAttribute.SearchFilters
                    .SelectMany(searchFilter => Directory.GetFiles(AbsolutePath, searchFilter, SearchOption.TopDirectoryOnly))
                    .ToArray();
            }
            catch
            {
                // TODO [RM]: set folder state to dead?
                _usbStorage.CheckIfDriveRemoved(true);
                return false;
            }

            foreach (var fileFullPath in fullFilePaths)
            {
                try
                {
                    var fileName = Path.GetFileName(fileFullPath);
                    var newFile = new T
                    {
                        UsbStorage = _usbStorage,
                        Parent = this,
                        FileName = fileName,
                        // ReSharper disable once ArrangeThisQualifier
                        AbsolutePath = Path.Combine(this.AbsolutePath, fileName),
                    };
                    // TODO [RM]: check file read rights and decide if add
                    Files.Add(newFile);
                }
                catch
                {
                    if (_usbStorage.CheckIfDriveRemoved(true))
                    {
                        return false;
                    }

                    // file specific error - just ignore
                }
            }

            return true;
        }

        #endregion Method

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
