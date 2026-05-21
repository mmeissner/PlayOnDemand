#region Licence
/****************************************************************
 *  Filename: TabItemPlatformViewModel.cs
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
using System.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.ViewModels;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels
{
    public sealed class TabItemPlatformViewModel : TabItemAppManagementScreen
    {

        private readonly Dictionary<Guid,PlatformInstallViewModel> _platformInstallViewModels = new Dictionary<Guid, PlatformInstallViewModel>();
        private readonly IPlatformController _platformController;
        private PlatformInstallViewModel _platformInstaller;
        private bool _showNoPlatformSelected;



        #region Fields & Properties
        public override int DisplayOrder => 30;
        public override string DisplayName
        {
            get { return Resources.System_Platform; }
            set
            {
                /* ignore */
            }
        }
        #endregion
        public PlatformSelectorViewModel PlatformSelector { get; }
        public PlatformInstallViewModel PlatformInstaller   
        {
            get => _platformInstaller;
            set
            {
                if(Equals(value, _platformInstaller)) return;
                _platformInstaller = value;
                NotifyOfPropertyChange();
            }
        }
        public bool ShowNoPlatformSelected
        {
            get => _showNoPlatformSelected;
            set
            {
                if(value == _showNoPlatformSelected) return;
                _showNoPlatformSelected = value;
                NotifyOfPropertyChange();
            }
        }

        #region Constructors
        public TabItemPlatformViewModel(
                IUIMessageBroker messageBroker,
                ViewModelFactory viewModelFactory,
                IPlatformController platformController
        ) : base(messageBroker, "IconPlatforms")
        {

            _platformController = platformController;
            ShowNoPlatformSelected = true;
            PlatformSelector = viewModelFactory.BuildPlatformSelector();
            PlatformSelector.EnableSelect = true;
            PlatformSelector.PropertyChanged += PlatformSelector_PropertyChanged;
        }

        private void PlatformSelector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName== nameof(PlatformSelectorViewModel.SelectedPlatform))
            {
                if(PlatformSelector.SelectedPlatform == null)
                {
                    PlatformInstaller = null;
                    ShowNoPlatformSelected = true;
                }
                else
                {
                    PlatformSelected(PlatformSelector.SelectedPlatform.Platform);
                }
            }
        }
        #endregion


        private void PlatformSelected(IPlatform selectedPlatform)
        {
            if(_platformInstallViewModels.ContainsKey(selectedPlatform.PlatformGuid))
            {
                PlatformInstaller = _platformInstallViewModels[selectedPlatform.PlatformGuid];
            }
            else
            {
                var platformInstallerViewModel = new PlatformInstallViewModel(selectedPlatform,_platformController,MessageBroker);
                platformInstallerViewModel.Update();
                _platformInstallViewModels.Add(platformInstallerViewModel.PlatformId,platformInstallerViewModel);
                PlatformInstaller = platformInstallerViewModel;
            }
            ShowNoPlatformSelected = false;
        }

        protected override void OnActivate()
        {
            if(PlatformSelector.SelectedPlatform != null)
            {
                PlatformSelected(PlatformSelector.SelectedPlatform.Platform);
            }
            base.OnActivate();
        }

        protected override void HandleLanguageChange(IUILanguageChangedEvent message) {  }
    }
}