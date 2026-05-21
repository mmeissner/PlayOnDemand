#region Licence
/****************************************************************
 *  Filename: IRpcConnection.cs
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
namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    public interface IRpcConnection
    {
        string Host { get; }
        uint Port { get; }
        RpcConnectionStatus State { get; }
    }

    /// <summary>
    /// Specifies status of network connection of the Station.
    /// https://github.com/grpc/grpc/blob/master/doc/connectivity-semantics-and-api.md
    /// </summary>
    public enum RpcConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Broken,
    }
}