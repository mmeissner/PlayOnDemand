#region Licence
/****************************************************************
 *  Filename: StationDetailsViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-22
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
using System.Globalization;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Humanizer;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using NLog;

namespace LeapVR.Shell.UI.Universal.StationDetails.ViewModels
{
    
    public class StationDetailsViewModel : Screen
        , IHandle<UILanguageChangedEvent>
        , IHandle<IUIStationModeChangedEvent>
        , IHandle<IUIClientInfoChangedEvent>
        , IDisposable
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private IShellClientInfo _lastShellClientInfo;
        private ImageSource _stationModeIndicator;
        private string _stationName;
        private string _licenseKey;
        private string _stationModeString;

        public string StationName
        {
            get => string.IsNullOrEmpty(_stationName) ? Resources.Global_Unset : _stationName;
            set
            {
                _stationName = value;
                NotifyOfPropertyChange();
            }
        }
        public string LicenseKey
        {
            get => string.IsNullOrEmpty(_licenseKey) ? Resources.License_Inactive : _licenseKey;
            set
            {
                _licenseKey = value;
                NotifyOfPropertyChange();
            }
        }
        public ImageSource StationModeIndicator
        {
            get { return _stationModeIndicator; }
            set
            {
                _stationModeIndicator = value;
                NotifyOfPropertyChange(() => StationModeIndicator);
            }
        }
        public string StationModeString
        {
            get { return _stationModeString; }
            set
            {
                if (value == _stationModeString) return;
                _stationModeString = value;
                NotifyOfPropertyChange(() => StationModeString);
            }
        }

        #endregion

        #region Constructors

        public StationDetailsViewModel(
            IUIMessageBroker messageBroker,
            IStationController stationController,
            IRemoteServiceController remoteServiceController)
        {
            QuickLeap.AssertNotNull(messageBroker);
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            SetStationMode(stationController.Mode);
            OnStationDetailsUpdated(remoteServiceController.LatestShellClientInfo);
        }
        #endregion

        #region Methods
        private void OnStationDetailsUpdated(IShellClientInfo shellClientInfo)
        {
            if (string.IsNullOrEmpty(shellClientInfo?.StationDisplayName))
            {
                StationName = Resources.Global_Unset;
            }
            else
            {
                StationName = shellClientInfo.StationDisplayName;
            }

            if (string.IsNullOrEmpty(shellClientInfo?.SerialNumber))
            {
                LicenseKey = Resources.License_Inactive;
            }
            else
            {
                LicenseKey = shellClientInfo.SerialNumber;
            }
            _lastShellClientInfo = shellClientInfo;
        }

        private ImageSource SetStationMode(StationMode mode)
        {
            StationModeString = mode.Humanize().Transform(To.TitleCase);
            switch (mode)
            {
                case StationMode.Screen:
                    return Application.Current.Resources["IconGamepad"] as ImageSource;
                case StationMode.VirtualReality:
                    return Application.Current.Resources["IconHeadset"] as ImageSource;
                default:
                    return null;
            }
        }

        public void Handle(IUIStationModeChangedEvent stationModeChangedEvent)
        {
            Logger.Info($"Reacting on {nameof(IUIStationModeChangedEvent)}. Changing the icon for mode indicator.");
            StationModeIndicator = SetStationMode(stationModeChangedEvent.NewMode);
        }
        #endregion

        public void Handle(UILanguageChangedEvent message)
        {
            OnStationDetailsUpdated(_lastShellClientInfo);
        }

        public void Handle(IUIClientInfoChangedEvent message)
        {
            OnStationDetailsUpdated(message.ClientInfo);
        }

        public void Dispose()
        {
            _messageBroker.Unsubscribe(this);
        }
    }
}
