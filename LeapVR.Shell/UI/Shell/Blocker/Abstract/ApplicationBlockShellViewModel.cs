#region Licence
/****************************************************************
 *  Filename: ApplicationBlockShellViewModel.cs
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
using System.Linq;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.Blocker.Abstract
{
    /// <summary>
    /// Representing a modal view that blocks the shell with an application information attached including <see cref="AppThumbnail"/> and <see cref="ErrorInfo"/> This derived from <see cref="BlockShellBaseViewModel"/>.
    /// </summary>
    public abstract class ApplicationBlockShellViewModel : BlockShellBaseViewModel
    {
        #region Fields & Properties
        private string _appName;
        private bool _isEnded;
        private bool _hasError;
        private string _errorInfo;
        private ApplicationThumbnailViewModel _appThumbnail;

        public Guid ApplicationGuid { get; }

        /// <summary>
        /// Get or set the thumbnail of the application.
        /// </summary>
        public ApplicationThumbnailViewModel AppThumbnail
        {
            get => _appThumbnail;
            set
            {
                _appThumbnail = value;
                NotifyOfPropertyChange(() => AppThumbnail);
            }
        }

        /// <summary>
        /// Get or set the name of the application
        /// </summary>
        public string AppName
        {
            get => _appName;
            set
            {
                _appName = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Get or set the indicator ended or not.
        /// </summary>
        public bool IsEnded
        {
            get => _isEnded;
            set
            {
                _isEnded = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Get or set the indicator of whether there is error or not.
        /// </summary>
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Get or set the error information of the application.
        /// </summary>
        public string ErrorInfo
        {
            get => _errorInfo;
            set
            {
                _errorInfo = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors
        protected ApplicationBlockShellViewModel(IAppDisplayInfo appDisplayInfo,IViewInputHandler inputHandler):base(inputHandler)
        {
            AppName = appDisplayInfo.Name;
            AppThumbnail = new ApplicationThumbnailViewModel(UIHelper.BytesToImageSource(appDisplayInfo.Thumbnail),appDisplayInfo.IsSupportScreen,appDisplayInfo.IsSupportVirtualReality);
        }

        protected ApplicationBlockShellViewModel(IInstallationProcessInfo installationProcessInfo, IViewInputHandler inputHandler) : base(inputHandler)
        {
            AppName = installationProcessInfo.ApplicationName;
            if (installationProcessInfo.ApplicationThumbnail != null && installationProcessInfo.ApplicationThumbnail.Any())
                AppThumbnail = new ApplicationThumbnailViewModel(
                    UIHelper.BytesToImageSource(installationProcessInfo.ApplicationThumbnail));
            else AppThumbnail = new ApplicationThumbnailViewModel(null);
            ApplicationGuid = installationProcessInfo.ApplicationGuid;

        }

        protected ApplicationBlockShellViewModel(IUninstallationProcessInfo uninstallationProcessInfo, IViewInputHandler inputHandler) : base(inputHandler)
        {
            AppName = uninstallationProcessInfo.ApplicationName;

            //Broken installations might not have an Thumbnail
            if (uninstallationProcessInfo.ApplicationThumbnail != null && uninstallationProcessInfo.ApplicationThumbnail.Any())
                AppThumbnail = new ApplicationThumbnailViewModel(
                    UIHelper.BytesToImageSource(uninstallationProcessInfo.ApplicationThumbnail));
            else AppThumbnail = new ApplicationThumbnailViewModel(null);
            ApplicationGuid = uninstallationProcessInfo.ApplicationGuid;
        }
        #endregion

    }
}
