#region Licence
/****************************************************************
 *  Filename: PlatformAppViewModel.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Universal.Platform.ViewModels
{
    public class PlatformAppViewModel : Screen
    {
        #region Private Fields
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IAppPlatformInfo _info;
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1,1);
        private ApplicationThumbnailViewModel _thumbnail;
        private bool _isLoaded;
        private bool _isLoadingError;
        #endregion

        #region Public Properties
        public Guid ApplicationId => _info.ApplicationGuid;
        public ApplicationThumbnailViewModel Thumbnail
        {
            get => _thumbnail;
            set
            {
                if(Equals(value, _thumbnail)) return;
                _thumbnail = value;
                NotifyOfPropertyChange();
            }
        }
        public string Title => _info.Name;
        public PlatformInstallState PlatformInstallationState => _info.ClientInstallState();
        public License LicenseState => GetLicenseState();
        public InstallationState InstallationState => _info.SystemInstallState();
        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if(value == _isLoaded) return;
                _isLoaded = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanInstall));
                NotifyOfPropertyChange(nameof(CanUninstall));
            }
        }
        public bool IsLoadingError
        {
            get => _isLoadingError;
            set
            {
                if(value == _isLoadingError) return;
                _isLoadingError = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructor
        public PlatformAppViewModel(IAppPlatformInfo appPlatformInfo)
        {
            _info = appPlatformInfo;
            _info.PlatformAppUpdated += _whenPlatformAppUpdated;
            if(_info.IsDisplayable)
            {
                UpdateViewModelData(_info, true);
            }
        }

        private void _whenPlatformAppUpdated(IAppPlatformInfo platformInfo, PlatformAppUpdate updateType)
        {
            switch(updateType)
            {
                case PlatformAppUpdate.PlatformInstallation:
                    NotifyOfPropertyChange(nameof(PlatformInstallationState));
                    break;
                case PlatformAppUpdate.SystemInstallation:
                    NotifyOfPropertyChange(nameof(InstallationState));
                    break;
                case PlatformAppUpdate.Licensing:
                    NotifyOfPropertyChange(nameof(LicenseState));
                    break;
            }
        }
        #endregion

        #region Public Methods
        public bool HasPlatformAccount() => _info.HasPlatformAccount();
        public bool CanInstall => _info.CanInstall();
        public void Install() { _info.Install(); }
        public bool CanUninstall => _info.CanUninstall();
        public void Uninstall(bool tryFullUninstall) { _info.Uninstall(tryFullUninstall); }
        public async Task UpdateDisplayDataAsync()
        {
            Logger.Debug($"Update for App with Id={_info.ApplicationGuid}, " +
                         $"with {nameof(_info.UpdateInProgress)}={_info.UpdateInProgress}");

            //Only one Update a Time
            if(_updateSemaphore.CurrentCount == 0)
            {
                Logger.Trace("Skipping UpdateDisplayData as there seems to be an update already runnig!");
                return;
            }

            try
            {
                //Try to keep only one Update a Time
                if(_updateSemaphore.CurrentCount == 0)
                {
                    Logger.Trace("Skipping UpdateDisplayData as there seems to be an update already runnig!");
                    return;
                }
                Logger.Debug("Waiting to update DisplayData!");
                await _updateSemaphore.WaitAsync();
                try
                {
                    if(_info.UpdateInProgress) return;
                    IsLoadingError = false;
                    Logger.Info($"Update Task for AppId={_info.ApplicationGuid} started");
                    var result = await _info.GetOrUpdateDisplayDataAsync();
                    Logger.Info($"Received Update for AppId={_info.ApplicationGuid} with Success={result}");
                    UpdateViewModelData(_info, result);
                }
                catch(Exception exception)
                {
                    Logger.Error(exception);
                }
                finally
                {
                    _updateSemaphore.Release();
                }
            }
            finally
            {
                Logger.Info($"Update Task for AppId={_info.ApplicationGuid} ended");
            }
        }

        public void AddLicense(IPlatformAccount account) { _info.AddLicense(account); }

        public void RemoveLicense(IPlatformAccount account) { _info.RemoveLicense(account); }
        #endregion

        #region Private Methods
        private void UpdateViewModelData(IAppPlatformInfo appPlatformInfo, bool isLoadingSuccess)
        {
            if(!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(
                        () => UpdateViewModelData(appPlatformInfo, isLoadingSuccess));
                return;
            }

            if(!isLoadingSuccess)
            {
                IsLoadingError = true;
            }
            else
            {
                UpdateThumbnailViewModel(appPlatformInfo);
                NotifyOfPropertyChange(nameof(Title));
                NotifyOfPropertyChange(nameof(PlatformInstallationState));
                NotifyOfPropertyChange(nameof(LicenseState));

                IsLoaded = true;
                IsLoadingError = false;
            }
        }
        private void UpdateThumbnailViewModel(IAppPlatformInfo info)
        {
            if(info.Thumbnail != null)
            {
                var image = info.Thumbnail.ToImageSource();
                if(image.CanFreeze) image.Freeze();
                Thumbnail = new ApplicationThumbnailViewModel(
                        image,
                        info.IsSupportScreen,
                        info.IsSupportVirtualReality);
            }
        }
        private License GetLicenseState()
        {
            if(_info.IsLicenseRequired)
            {
                var licenseInfo = _info.LicenseInfo();
                return licenseInfo.CurrentLicenseCount > 0 ? License.Available :
                        License.Missing;
            }

            return License.Free;
        }
        #endregion

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(!IsLoaded)
            {
                Task.Run(async ()=> await UpdateDisplayDataAsync());
            }
        }

        public enum License
        {
            Missing,
            Available,
            Free
        }
    }
}