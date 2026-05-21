#region Licence
/****************************************************************
 *  Filename: ModeSwitchViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-21
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Settings.ViewModels
{
    /// <summary>
    /// ViewModel for feature of switching station mode.
    /// </summary>
    public class ModeSwitchViewModel : Screen, IHandle<IUILanguageChangedEvent>, IHandle<IUIStationModeChangedEvent>
    {
        #region Fields & Properties

        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IStationController _stationController;
        private Dictionary<StationMode, string> _supportedModes;
        private StationMode _selectedMode;
        private bool _canSelectMode;

        public Dictionary<StationMode, string> SupportedModes
        {
            get => _supportedModes;
            set
            {
                _supportedModes = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanSelectMode
        {
            get { return _canSelectMode; }
            set
            {
                if(value == _canSelectMode) return;
                _canSelectMode = value;
                NotifyOfPropertyChange();
            }
        }
        public StationMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors

        public ModeSwitchViewModel(
            IUIMessageBroker messageBroker,
            IStationController stationController
            )
        {
            QuickLeap.AssertNotNull(messageBroker, stationController);
            _stationController = stationController;
            PrepareDynamicElements(stationController.Mode);
            messageBroker.Subscribe(this);
            CanSelectMode = true;
        }
        #endregion

        #region Methods
        public async void OnSelectedModeChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count <= 0)
            {
                Logger.Debug($"Failed to switch mode. Event args of {nameof(OnSelectedModeChanged)} is not correct.");
                e.Handled = true;
                return;
            }
            Logger.Info($"Try to change station mode to '{SelectedMode}'.");
            CanSelectMode = false;
            await _stationController.SetStationModeAsync(SelectedMode);
            CanSelectMode = true;
        }

        private void PrepareDynamicElements(StationMode currentMode)
        {
            var supportedModes = new Dictionary<StationMode, string>();

            //Show only availible Options
            foreach(StationMode mode in _stationController.GetAvailableModes())
            {
                switch(mode)
                {
                    case StationMode.Screen:
                        supportedModes.Add(StationMode.Screen, Resources.StationMode_Screen);
                        break;
                    case StationMode.VirtualReality:
                        supportedModes.Add(StationMode.VirtualReality, Resources.StationMode_VirtualReality);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SupportedModes = supportedModes;
            _selectedMode = currentMode;
            NotifyOfPropertyChange(() => SelectedMode);
        }
        #endregion


        public void Handle(IUILanguageChangedEvent message)
        {
            PrepareDynamicElements(_selectedMode);
        }

        public void Handle(IUIStationModeChangedEvent message)
        {
            PrepareDynamicElements(message.NewMode);
        }
    }
}
