#region Licence
/****************************************************************
 *  Filename: PlatformInstallViewModel.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels
{
    public class PlatformInstallViewModel : Screen, IHandle<IUIAppInstalledEvent>, IHandle<IUIAppUninstalledEvent>
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IPlatform _platform;
        private readonly IPlatformController _platformController;
        private readonly IUIMessageBroker _uiMessageBroker;
        private readonly SynchronizationContext _uiContext;

        public Guid PlatformId => _platform.PlatformGuid;
        private bool _isUpdateing;
        private ListViewModel<PlatformAppViewModel> _uninstallApplications;
        private ListViewModel<PlatformAppViewModel> _installApplications;

        private PlatformAppViewModel _lastUninstallSelected;
        private PlatformAppViewModel _lastInstallSelected;

        public ListViewModel<PlatformAppViewModel> UninstallApplications
        {
            get => _uninstallApplications;
            set
            {
                if(Equals(value, _uninstallApplications)) return;
                _uninstallApplications = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedUninstall));
            }
        }
        public ListViewModel<PlatformAppViewModel> InstallApplications
        {
            get => _installApplications;
            set
            {
                if(Equals(value, _installApplications)) return;
                _installApplications = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedInstall));
            }
        }
        public PlatformAppViewModel SelectedUninstall => UninstallApplications.SelectedItem;
        public PlatformAppViewModel SelectedInstall => InstallApplications.SelectedItem;


        public PlatformInstallViewModel(IPlatform platform, IPlatformController platformController,IUIMessageBroker uiMessageBroker)
        {
            _platformController = platformController;
            _platform = platform;
            _uiMessageBroker = uiMessageBroker;
            _uiMessageBroker.Subscribe(this);
            UninstallApplications =  new ListViewModel<PlatformAppViewModel>();
            InstallApplications = new ListViewModel<PlatformAppViewModel>();
            InstallApplications.PropertyChanged += InstallApplicationsPropertyChanged;
            UninstallApplications.PropertyChanged += UninstallApplicationsPropertyChanged;
            _uiContext = SynchronizationContext.Current;
        }

        public bool IsUpdateing
        {
            get => _isUpdateing;
            set
            {
                if(value == _isUpdateing) return;
                _isUpdateing = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanUpdate));
                NotifyOfPropertyChange(nameof(CanInstall));
                NotifyOfPropertyChange(nameof(CanUninstall));
            }
        }

        public bool CanUpdate => !IsUpdateing;
        public void Update()
        {
            IsUpdateing = true;
            UninstallApplications.ShowLoading = true;
            InstallApplications.ShowLoading = true;
            _platform.GetPlatformApps(AddPlatformAppCallback, CompletedPlatformAppCallback,_uiContext );
        }

        public bool CanInstall => !IsUpdateing && SelectedInstall != null && SelectedInstall.IsLoaded && SelectedInstall.CanInstall;
        public void Install()
        {
            SelectedInstall.Install();
        }

        public bool CanUninstall  => !IsUpdateing && SelectedUninstall != null && SelectedUninstall.IsLoaded && SelectedUninstall.CanUninstall;
        public void Uninstall()
        {
            SelectedUninstall.Uninstall(false);
        }

        private void AddPlatformAppCallback(IAppPlatformInfo obj)
        {
            var appViewModel = new PlatformAppViewModel(obj);
            if(appViewModel.InstallationState == InstallationState.NotInstalled)
            {
                InstallApplications.Items.Add(appViewModel);
            }
            else
            {
                UninstallApplications.Items.Add(appViewModel);
            }

            if(!appViewModel.IsLoaded)
            {
                Task.Run(async ()=> await appViewModel.UpdateDisplayDataAsync());
            }
        }

        private void CompletedPlatformAppCallback()
        {
            UninstallApplications.ShowLoading = false;
            InstallApplications.ShowLoading = false;
            IsUpdateing = false;
        }

        private void UninstallApplicationsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(ListViewModel<PlatformAppViewModel>.SelectedItem)))
            {

                var selectedItem = UninstallApplications.SelectedItem;
                if(_lastUninstallSelected != null)
                {
                    _lastUninstallSelected.PropertyChanged -= LastUninstallSelectedOnPropertyChanged; 
                }

                if(selectedItem != null)
                {
                    _lastUninstallSelected = selectedItem;
                    _lastUninstallSelected.PropertyChanged += LastUninstallSelectedOnPropertyChanged;
                }
                NotifyOfPropertyChange(nameof(SelectedUninstall));
                NotifyOfPropertyChange(nameof(CanUninstall));
            }
        }

        private void LastUninstallSelectedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(PlatformAppViewModel.CanUninstall)))
            {
                NotifyOfPropertyChange(nameof(CanUninstall));
            }
        }

        private void InstallApplicationsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(ListViewModel<PlatformAppViewModel>.SelectedItem)))
            {
                var selectedItem = InstallApplications.SelectedItem;
                if(_lastInstallSelected != null)
                {
                    _lastInstallSelected.PropertyChanged -= LastInstallSelectedOnPropertyChanged; 
                }

                if(selectedItem != null)
                {
                    _lastInstallSelected = selectedItem;
                    _lastInstallSelected.PropertyChanged += LastInstallSelectedOnPropertyChanged;
                }
                NotifyOfPropertyChange(nameof(SelectedInstall));
                NotifyOfPropertyChange(nameof(CanInstall));
            }
        }
        private void LastInstallSelectedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(PlatformAppViewModel.CanInstall)))
            {
                NotifyOfPropertyChange(nameof(CanInstall));
            }
        }


        public void Handle(IUIAppInstalledEvent message)
        {
            if(!message.PlatformGuid.Equals(PlatformId))return;
            var appItem =
                    _installApplications?.Items?.FirstOrDefault(
                            x => x.ApplicationId.Equals(message.ApplicationGuid));
            if(appItem != null)
            {
                _installApplications.Items.Remove(appItem);
            }

            var appInfo = _platform.GetInstalledPlatformApp(message.ApplicationGuid);
            if(appInfo != null)_uninstallApplications.Items.Add(new PlatformAppViewModel(appInfo));
            else
            {
                Logger.Warn($"Received an AppInstalledEvent but could not get IAppPlatform Info, skipping app with Guid={message.ApplicationGuid}");
            }
        }
        public void Handle(IUIAppUninstalledEvent message)
        {
            //Only for the AppId that relates to this object
            if(!message.PlatformGuid.Equals(PlatformId))return;
            //Is object in our UninstallApplications list ?
            var appItem =
                    _uninstallApplications?.Items?.FirstOrDefault(
                            x => x.ApplicationId.Equals(message.ApplicationGuid));
            if(appItem != null)
            {
                _uninstallApplications.Items.Remove(appItem);
            }
            var appInfo = _platform.GetPlatformApp(message.ApplicationGuid);
            if(appInfo != null)_installApplications.Items.Add(new PlatformAppViewModel(appInfo));
            else
            {
                Logger.Warn($"Received an AppUninstalledEvent but could not get IAppPlatform Info, skipping app with Guid={message.ApplicationGuid}");
            }
        }
    }
}
