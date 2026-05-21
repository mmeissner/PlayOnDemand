#region Licence
/****************************************************************
 *  Filename: UninstallationAppViewModel.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Base;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class UninstallationAppViewModel : ApplicationBaseViewModel
    {
        #region Fields & Properties
        private long _occupiedSpaceSize;
        private bool _isInstallationBroken;
        private IAppInstallationData _installationData;
        private ImageSource _healthIcon;
        private CanUninstallStatus _uninstallableStatus;


        public IAppInstallationData InstallationData
        {
            get => _installationData;
            set
            {
                _installationData = value;
                NotifyOfPropertyChange();
            }
        }
        public long OccupiedSpaceSize
        {
            get => _occupiedSpaceSize;
            set
            {
                _occupiedSpaceSize = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsInstallationBroken
        {
            get => _isInstallationBroken;
            private set
            {
                _isInstallationBroken = value;
                NotifyOfPropertyChange();

                HealthIcon = _isInstallationBroken
                    ? (ImageSource)Application.Current.Resources["IconAppContainerBroken"]
                    : (ImageSource)Application.Current.Resources["IconAppAlreadyInstalled"];

            }
        }
        public ImageSource HealthIcon
        {
            get => _healthIcon;
            private set
            {
                _healthIcon = value;
                NotifyOfPropertyChange();
            }
        }
        public CanUninstallStatus UninstallableStatus
        {
            get => _uninstallableStatus;
            set
            {
                _uninstallableStatus = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors

        public UninstallationAppViewModel(
            IUIMessageBroker messageBroker,
            IAppPlatformInfo appPlatformInfo,
            IAppInstallationData installationData, 
            CanUninstallStatus state) : base(appPlatformInfo)
        {
            QuickLeap.AssertNotNull(appPlatformInfo, installationData, messageBroker);
            Initialize(installationData, state);
        }
        public UninstallationAppViewModel(
            IUIMessageBroker messageBroker,
            IAppInstallationData installationData,
            CanUninstallStatus state ) : base(installationData)
        {
            QuickLeap.AssertNotNull(installationData,  messageBroker);
            Initialize(installationData, state);
        }

        private void Initialize(IAppInstallationData installationData, CanUninstallStatus state)
        {
            _installationData = installationData;
            OccupiedSpaceSize = _installationData.TotalFilesSize;
            UninstallableStatus = state;
            UpdateUninstallableState(state);
        }
        #endregion

        public void UpdateUninstallableState(CanUninstallStatus status)
        {
            switch (status)
            {
                case CanUninstallStatus.Unknown:
                case CanUninstallStatus.BrokenCanUninstall:
                case CanUninstallStatus.BrokenCannotUninstall:
                    IsInstallationBroken = true;
                    break;
                default:
                    IsInstallationBroken = false;
                    break;
            }
        }

    }
}
