#region Licence
/****************************************************************
 *  Filename: InstallationFolderViewModel.cs
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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules.Container;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class InstallationFolderViewModel : Screen
    {
        #region Fields & Properties
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                NotifyOfPropertyChange();
            }
        }

        public IFolder<AppInstallationFile> Folder { get; }
        public FolderType FolderType { get; }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        public InstallationFolderViewModel(IFolder<AppInstallationFile> folder, FolderType folderType = FolderType.Folder)
        {
            QuickLeap.AssertNotNull(folder);
            Folder = folder;
            FolderType = folderType;
            switch (folderType)
            {
                case FolderType.Folder:
                    Thumbnail = (BitmapImage)Application.Current.Resources["IconFolder"];
                    Title = folder.FolderName;
                    break;
                case FolderType.ParentFolder:
                    Thumbnail = (BitmapImage)Application.Current.Resources["IconParentFolder"];
                    Title = "..";
                    break;
                case FolderType.RootFolder:
                    Thumbnail = (BitmapImage)Application.Current.Resources["IconParentFolder"];
                    Title = "/";
                    break;
            }
            IsLoaded = true;
        }

        #endregion

        #region Methods

        #endregion
    }

    public enum FolderType
    {
        None = 0,
        Folder = 1,
        ParentFolder = 2,
        RootFolder = 3,
    }
}
