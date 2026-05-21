#region Licence
/****************************************************************
 *  Filename: NetworkListManagerEvents.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using LeapVR.Shared.Lib.Helper;
using NETWORKLIST;
using NLog;

namespace LeapVR.Shell.Modules.Network
{
    public class NetworkListManagerEvents : INetworkListManagerEvents, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
            catch(Exception exception)
            {
                Logger.Error(exception,"Exception during Disposal of ConnectionPoint");
            }

            if (disposing)
            {
                _whenConnectivityChanged.OnCompleted();
            }
        }
    }
}
