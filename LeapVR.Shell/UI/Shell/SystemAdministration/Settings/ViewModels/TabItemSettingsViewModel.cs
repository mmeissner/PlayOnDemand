#region Licence
/****************************************************************
 *  Filename: TabItemSettingsViewModel.cs
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
using System.Reactive.Linq;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Shell.Connect.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Settings.ViewModels
{
    public sealed class TabItemSettingsViewModel : TabItemSystemScreen
    {
        #region Fields & Properties
        private readonly IStationController _stationController;
        private ModeSwitchViewModel _modeSwitchViewModel;
        private StationDetailsViewModel _stationDetailsViewModel;
        private BindableCollection<NotificationKeyValuePair<string, long>> _items;
        private bool _forceVrDriverRestart;
        private bool _vrDriverInteraction;

        public override string DisplayName
        {
            get { return Resources.System_Settings; }
            set { /* ignore */ }
        }

        public ModeSwitchViewModel ModeSwitchViewModel
        {
            get => _modeSwitchViewModel;
            set
            {
                _modeSwitchViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public StationDetailsViewModel StationDetailsViewModel
        {
            get => _stationDetailsViewModel;
            set
            {
                _stationDetailsViewModel = value;
                NotifyOfPropertyChange();
            }
        }


        public BindableCollection<NotificationKeyValuePair<string, long>> Items
        {
            get => _items;
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }

        
        public override int DisplayOrder => 9;
        protected override void HandleLanguageChange(IUILanguageChangedEvent message) {  }
        #endregion

        #region Constructors

        public TabItemSettingsViewModel(
            IUIMessageBroker messageBroker,
            IStationController stationController,

            ModeSwitchViewModel modeSwitchViewModel,
            StationDetailsViewModel stationDetailsViewModel) : base(messageBroker,"IconSettings")
        {
            QuickLeap.AssertNotNull(stationDetailsViewModel,stationController);           


            StationDetailsViewModel = stationDetailsViewModel;
            ModeSwitchViewModel = modeSwitchViewModel;
            Items = new BindableCollection<NotificationKeyValuePair<string, long>>();
            _stationController = stationController;
            _forceVrDriverRestart = stationController.ForceVrDriverRestart;
            _vrDriverInteraction = !stationController.DisableVrDriverInteraction;
        }
        #endregion

        public void OpenConnectionDialog()
        {
            _stationController.OpenConnectDialog();
        }

        public bool ForceVrDriverRestart
        {
            get => _forceVrDriverRestart;
            set
            {
                if(value == _forceVrDriverRestart) return;
                _forceVrDriverRestart = value;
                _stationController.SetRestartVrDriver(_forceVrDriverRestart);
                NotifyOfPropertyChange();
            }
        }
        public bool VrDriverInteraction
        {
            get => _vrDriverInteraction;
            set
            {
                if(value == _vrDriverInteraction) return;
                _vrDriverInteraction = value;
                _stationController.SetDisableVrDriverInteraction(!_vrDriverInteraction);
                NotifyOfPropertyChange();
            }
        }
    }
}
