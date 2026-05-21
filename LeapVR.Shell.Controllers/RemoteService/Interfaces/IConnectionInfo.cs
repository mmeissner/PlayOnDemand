#region Licence
/****************************************************************
 *  Filename: IConnectionInfo.cs
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
    //public interface IConnectionInfo
    //{
    //    string Host { get; }
    //    uint Port { get; }
    //    RpcConnectionStatus Status { get; }
    //    ServiceType Type { get; }
    //}

    public enum ServiceType
    {
        Unknown,
        Connect,
        ShellHost,
    }
}