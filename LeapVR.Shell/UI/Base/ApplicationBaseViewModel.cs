#region Licence
/****************************************************************
 *  Filename: ApplicationBaseViewModel.cs
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
using System.ComponentModel;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Base
{
    /// <summary>
    /// Representing a view that mainly provides <see cref="IAppDisplayInfo"/> of an application.
    /// </summary>
    public abstract class ApplicationBaseViewModel : Screen
    {
        #region Fields & Properties
        protected readonly IAppDisplayInfo AppDisplayInfo;
        private ApplicationThumbnailViewModel _thumbnail;

        protected IAppDisplayInfo AppDisplayData => AppDisplayInfo;
        public Guid ApplicationGuid => AppDisplayInfo.ApplicationGuid;
        public string Name => AppDisplayInfo.Name;
        public string Description => AppDisplayInfo.Description;
        public IAppCategory Category => AppDisplayInfo.Category;
        public ApplicationThumbnailViewModel Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        #endregion

        #region Constructors

        protected ApplicationBaseViewModel(IAppPlatformInfo appDisplayInfo)
        {
            QuickLeap.AssertNotNull(appDisplayInfo);
            AppDisplayInfo = appDisplayInfo;
            AppDisplayInfo.PropertyChanged += _appDisplayInfo_PropertyChanged;

            Thumbnail = GetThumbnaimViewModel(AppDisplayInfo.Thumbnail,AppDisplayInfo.IsSupportScreen,AppDisplayInfo.IsSupportVirtualReality);
        }

        private void _appDisplayInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(AppDisplayInfo.Name):
                    NotifyOfPropertyChange(nameof(Name));
                    break;
                case nameof(AppDisplayInfo.Description):
                    NotifyOfPropertyChange(nameof(Description));
                    break;
                case nameof(AppDisplayInfo.Category):
                    NotifyOfPropertyChange(nameof(Category));
                    break;
                case nameof(AppDisplayInfo.IsSupportScreen):
                    if(Thumbnail != null) Thumbnail.IsScreenModeSupported = AppDisplayInfo.IsSupportScreen;
                    break;
                case nameof(AppDisplayInfo.IsSupportVirtualReality):
                    if(Thumbnail != null) Thumbnail.IsVrModeSupported = AppDisplayInfo.IsSupportVirtualReality;
                    break;
            }
        }

        protected ApplicationBaseViewModel(IAppInstallationData installationData)
        {
            AppDisplayInfo = new InstallationDataDisplayInfo(installationData);
            Thumbnail = GetThumbnaimViewModel();
        }
        #endregion

        #region Methods
        private ApplicationThumbnailViewModel GetThumbnaimViewModel(byte[] thumbnail = null,bool showScreenModeSupported = false, bool showVRModeSupported = false)
        {
            ImageSource thumbnailImage = null;
            if (thumbnail != null) thumbnailImage = UIHelper.BytesToImageSource(AppDisplayInfo.Thumbnail);
            return new ApplicationThumbnailViewModel(thumbnailImage, showScreenModeSupported,showVRModeSupported);
        }
        #endregion

        class InstallationDataDisplayInfo :IAppDisplayInfo
        {
            public InstallationDataDisplayInfo(IAppInstallationData installationData)
            {
                ApplicationGuid = installationData.ApplicationGuid;
                Name = String.IsNullOrEmpty(installationData.DisplayName) ? installationData.ApplicationGuid.ToString() : installationData.DisplayName;
                Tags = null;
                Category = null;
                Description = null;
                Thumbnail = null;
            }
            public Guid ApplicationGuid { get; }
            public string Name { get;  }
            public IAppCategory Category { get; }
            public string[] Tags { get; }
            public string Description { get; }
            public byte[] Thumbnail { get; }
            public bool IsSupportScreen { get; } = false;
            public bool IsSupportVirtualReality { get; } = false;

            public event PropertyChangedEventHandler PropertyChanged
            {
                add {}
                remove {}
            }
            public IAppDisplayUpdate GetAppDisplayUpdate() { throw new NotImplementedException(); }
        }
    }
}
