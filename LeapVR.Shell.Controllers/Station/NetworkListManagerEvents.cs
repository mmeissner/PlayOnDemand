#region Licence
/****************************************************************
 *  Filename: NetworkListManagerEvents.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using LeapVR.Shared.NetStandard;
using NETWORKLIST;

namespace LeapVR.VBox.Controllers.Station
{
    internal class NetworkListManagerEvents : INetworkListManagerEvents, IDisposable
    {
        private readonly INetworkListManager _networkListManager;
        private readonly IConnectionPoint _connectionPoint;
        private readonly int _connectionCookie;

        private int _isDisposed; //0 = false, 1 = true

        private readonly BehaviorSubject<NLM_CONNECTIVITY> _whenConnectivityChanged;
        public IObservable<NLM_CONNECTIVITY> WhenConnectivityChanged { get; }

        public NetworkListManagerEvents(INetworkListManager networkListManager)
        {
            _whenConnectivityChanged = new BehaviorSubject<NLM_CONNECTIVITY>(networkListManager.GetConnectivity());
            WhenConnectivityChanged = _whenConnectivityChanged.AsObservable();

            _networkListManager = networkListManager;
            var guid = typeof(INetworkListManagerEvents).GUID;
            var casted = (IConnectionPointContainer)networkListManager;
            casted.FindConnectionPoint(ref guid, out _connectionPoint);
            _connectionPoint.Advise(this, out _connectionCookie);
        }

        ~NetworkListManagerEvents()
        {
            Dispose(false);
        }

        public void ConnectivityChanged(NLM_CONNECTIVITY newConnectivity)
        {
            _whenConnectivityChanged.OnNext(newConnectivity);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            var wasDisposed = QuickLeap.OperateInterlockedFlag(ref _isDisposed);
            if (wasDisposed)
            {
                return;
            }

            try
            {
                _connectionPoint.Unadvise(_connectionCookie);
            }
            catch
            {
                // swallow
            }

            if (disposing)
            {
                _whenConnectivityChanged.OnCompleted();
            }
        }
    }
}
