#region Licence
/****************************************************************
 *  Filename: ApplicationViewModel.cs
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
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.Edit.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.ViewModels
{
    /// <inheritdoc />
    /// <summary>
    /// View model for a single app in management scope.
    /// </summary>
    public class ApplicationViewModel : ApplicationBaseViewModel
    {
        #region Fields & Properties
        private readonly Guid _applicationGuid;
        private readonly ICategoryProvider _categoryProvider;
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// Get or set is enabled of a sinlge app
        /// </summary>
        public bool IsEnabled
        {
            get => ((IAppPlatformInfo)AppDisplayInfo).IsEnabled;
            set
            {
                ((IAppPlatformInfo)AppDisplayInfo).SetEnabled(value);
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }

        private bool _isTrafficAllowed;
        /// <summary>
        /// Get or set if a singel app is allowed to cross windows firewall or not.
        /// </summary>
        public bool IsTrafficAllowed
        {
            get => _isTrafficAllowed;
            set
            {
                if (value == _isTrafficAllowed)
                {
                    return;
                }
                _isTrafficAllowed = value;

                ((IAppPlatformInfo)AppDisplayInfo).SetFirewallState(_isTrafficAllowed ? FirewallState.AllTrafficAllowed : FirewallState.NoTrafficAllowed);

                NotifyOfPropertyChange(() => IsTrafficAllowed);
            }
        }

        private bool _isBusy = true;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }
        public ImageSource PlatformIcon { get; }
        public bool IsContainerApp { get; private set; }
        #endregion

        #region Constructors
        public ApplicationViewModel(
                ICategoryProvider categoryProvider,
                IWindowManager windowManager,
                IAppPlatformInfo appPlatformInfo,
                ImageSource platformIcon) : base(appPlatformInfo)
        {
            _categoryProvider = categoryProvider;
            _windowManager = windowManager;
            _applicationGuid = appPlatformInfo.ApplicationGuid;
            PlatformIcon = platformIcon;
        }

        #endregion

        #region Methods
        public void EditApp()
        {
            var updateViewModel = new EditAppViewModel(_categoryProvider.GetAllCategories, AppDisplayData.GetAppDisplayUpdate());
            var retval = _windowManager.ShowDialog(updateViewModel,null,ShellClientHelper.GetUniversalDialogSettings());
            if(retval.HasValue && retval.Value)
            {
                updateViewModel.AppUpdate.ApplyChanges();
            }
        }

        public void AdvanceEditApp()
        {
            if(AppDisplayInfo is IAppPlatformInfo appPlatformInfo && appPlatformInfo.TryGetAppExecutableUpdate(out var exeUpdate))
            {
                var updateViewModel = new AdvanceEditAppViewModel(_windowManager,exeUpdate);
                _windowManager.ShowDialog(updateViewModel,null,ShellClientHelper.GetUniversalDialogSettings());
            }
        }
        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            IsBusy = true;

            if(AppDisplayData is IAppPlatformInfo appDisplayInfo &&
               appDisplayInfo.TryGetInstallationInfo(out var installationInfo) &&
               installationInfo.Type == AppInstallationType.Container &&
               installationInfo.InstallationState == InstallationState.Installed)
            {
                IsContainerApp = true;
                var getFirewallStateTask = ((IAppPlatformInfo)AppDisplayInfo).GetFirewallStateAsync();
                var firewallState = await getFirewallStateTask;
                _isTrafficAllowed = firewallState == FirewallState.AllTrafficAllowed;
            }
            else
            {
                IsContainerApp = false;
            }

            NotifyOfPropertyChange(() => IsContainerApp);
            NotifyOfPropertyChange(() => IsEnabled);
            NotifyOfPropertyChange(() => IsTrafficAllowed);

            IsBusy = false;
        }
        #endregion
    }
}
