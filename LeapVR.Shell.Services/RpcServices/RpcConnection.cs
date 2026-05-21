#region Licence
/****************************************************************
 *  Filename: RpcConnection.cs
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
using LeapVR.Shell.Controllers.RemoteService.Interfaces;

namespace LeapVR.Shell.Services.RpcServices
{
    internal class RpcConnection : IRpcConnection
    {
        public RpcConnection(string host, uint port)
        {
            Host = host;
            Port = port;
            State = RpcConnectionStatus.Disconnected;
        }

        RpcConnection(string host, uint port, RpcConnectionStatus state)
        {
            Host = host;
            Port = port;
            State = state;
        }
        public string Host { get; }
        public uint Port { get; }
        public RpcConnectionStatus State { get; }

        public RpcConnection Clone(RpcConnectionStatus newState)
        {
            return new RpcConnection(Host, Port, newState);
        }

        public override string ToString() { return $"RpcConnection: Host={Host}, Port={Port}, State={State}"; }
    }
}
