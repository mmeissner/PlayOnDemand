#region Licence
/****************************************************************
 *  Filename: ConnectionIndicatorViewModel.cs
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
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Modules.Interfaces.Network;
using LeapVR.Shell.Modules.Network;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class ConnectionIndicatorViewModel : Screen, IDisposable
    {

        #region Fields & Properties
        private readonly IDisposable _connectionSubscription;
        private readonly DispatcherTimer _timer;
        private readonly object _updateLock = new object();
        private volatile ushort _disconnectLevel = 0;
        private NetworkConnectionStatus _lastKnowNetworkConnectionStatus = NetworkConnectionStatus.Unknown;

        private ImageSource _connectionIndicator;
        public ImageSource ConnectionIndicator
        {
            get => _connectionIndicator;
            set
            {
                _connectionIndicator = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors
        public ConnectionIndicatorViewModel(INetworkModule networkModule)
        {
            QuickLeap.AssertNotNull(networkModule);;
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(2), DispatcherPriority.Normal, OnInternetDisconnected, Application.Current.Dispatcher);
            _connectionSubscription = networkModule.WhenNetworkConnectionChanged.SubscribeOnDispatcher().Subscribe(OnNetworkConnectionChanged);
        }
        #endregion

        #region Callbacks
        private void OnNetworkConnectionChanged(NetworkConnectionStatus networkConnectionStatus)
        {
            lock(_updateLock)
            {
                switch (_lastKnowNetworkConnectionStatus)
                {
                    //From Unknown
                    case NetworkConnectionStatus.Unknown:
                        if (networkConnectionStatus == NetworkConnectionStatus.Connected) ApplyStatus(StatusIndicator.GoodConnection);
                        else if (networkConnectionStatus == NetworkConnectionStatus.Disconnected) ApplyStatus(StatusIndicator.Disconnected);
                        break;
                    //From Connected
                    case NetworkConnectionStatus.Connected:
                        //To Disconnected
                        if (networkConnectionStatus == NetworkConnectionStatus.Disconnected)
                        {
                            _timer.Start();
                            _disconnectLevel = 1;
                            ApplyStatus(StatusIndicator.PoorConnection);
                        }
                        break;
                    //From Disconnected
                    case NetworkConnectionStatus.Disconnected:
                        //To Connected
                        if (networkConnectionStatus == NetworkConnectionStatus.Connected)
                        {
                            _timer.Stop();
                            _disconnectLevel = 0;
                            ApplyStatus(StatusIndicator.GoodConnection);
                        }
                        break;
                }
                _lastKnowNetworkConnectionStatus = networkConnectionStatus;
            }
        }
        private void OnInternetDisconnected(object obj, EventArgs e)
        {
            if (_lastKnowNetworkConnectionStatus == NetworkConnectionStatus.Connected) return;
            lock (_updateLock)
            {
                if (_lastKnowNetworkConnectionStatus == NetworkConnectionStatus.Connected) return;
                if (_disconnectLevel >= 3) return;
                _disconnectLevel++;
                if (_disconnectLevel == 2)
                {
                    ApplyStatus(StatusIndicator.BadConnection);
                }
                else if (_disconnectLevel == 3)
                {
                    ApplyStatus(StatusIndicator.Disconnected);
                    _timer.Stop();
                }
            }
        }
        #endregion

        #region Methods
        void ApplyStatus(StatusIndicator indicator)
        {
            switch(indicator)
            {
                case StatusIndicator.Disconnected:
                    ConnectionIndicator = Application.Current.Resources["StatusDisconnected"] as ImageSource;
                    break;
                case StatusIndicator.PoorConnection:
                    ConnectionIndicator = Application.Current.Resources["StatusDisconnectedStep1"] as ImageSource;
                    break;
                case StatusIndicator.BadConnection:
                    ConnectionIndicator = Application.Current.Resources["StatusDisconnectedStep2"] as ImageSource;
                    break;
                case StatusIndicator.GoodConnection:
                    ConnectionIndicator = Application.Current.Resources["StatusConnected"] as ImageSource;
                    break;
            }
        }

        enum StatusIndicator
        {
            Disconnected,
            PoorConnection,
            BadConnection,
            GoodConnection,
        }

        public void Dispose()
        {
            _connectionSubscription?.Dispose();
        }
        #endregion
    }
}
