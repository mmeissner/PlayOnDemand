#region Licence
/****************************************************************
 *  Filename: NetworkModule.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Modules.Interfaces.Network;
using NETWORKLIST;
using NLog;

namespace LeapVR.Shell.Modules.Network
{
    public class NetworkModule : INetworkModule, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private volatile NetworkConnectionStatus _networkConnectionStatus = NetworkConnectionStatus.Unknown;
        private readonly NetworkListManagerEvents _networkListManagerEvents;
        private readonly BehaviorSubject<NetworkConnectionStatus> _whenNetworkConnectionChangedSubject;
        private readonly IDisposable _onLocalConnectivityChangedSubscription;


        public IObservable<NetworkConnectionStatus> WhenNetworkConnectionChanged => _whenNetworkConnectionChangedSubject.AsObservable();
        public NetworkConnectionStatus NetworkConnectionStatus => _networkConnectionStatus;
        public NetworkModule()
        {
            _networkListManagerEvents = new NetworkListManagerEvents(new NetworkListManager());
            _whenNetworkConnectionChangedSubject = new BehaviorSubject<NetworkConnectionStatus>(NetworkConnectionStatus);
            _onLocalConnectivityChangedSubscription = _networkListManagerEvents.WhenConnectivityChanged.Subscribe(OnLocalConnectivityChanged);
        }

        private void OnLocalConnectivityChanged(NLM_CONNECTIVITY newConnectivity)
        {
            var hasFlagIPV4Internet = newConnectivity.HasFlag(NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV4_INTERNET);
            var hasFlagIPV6Internet = newConnectivity.HasFlag(NLM_CONNECTIVITY.NLM_CONNECTIVITY_IPV6_INTERNET);
            var isLocalInternetAccess = hasFlagIPV4Internet || hasFlagIPV6Internet;
            Logger.Info($"New local connectivity statu reported. newConnectivity={newConnectivity}, isLocalInternetAccess={isLocalInternetAccess}, hasFlagIPV4Internet={hasFlagIPV4Internet}, hasFlagIPV6Internet={hasFlagIPV6Internet}");

            var shouldBeStatus = isLocalInternetAccess ? NetworkConnectionStatus.Connected : NetworkConnectionStatus.Disconnected;
            if (shouldBeStatus != _networkConnectionStatus)
            {
                _networkConnectionStatus = shouldBeStatus;
                _whenNetworkConnectionChangedSubject.OnNext(shouldBeStatus);
            }
        }
        public void Dispose()
        {
            _networkListManagerEvents?.Dispose();
            _onLocalConnectivityChangedSubscription?.Dispose();
        }
    }
}
