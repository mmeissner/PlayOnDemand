#region Licence
/****************************************************************
 *  Filename: ShellServerViewModels.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Pod.Enums;

namespace Pod.ViewModels.ShellServer
{
    public class ShellServerViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public uint PublicInterfaceVersion { get; set; }
        public string PublicHostAddress { get; set; }
        public uint PublicPort { get; set; }
        public TimeSpan HeartbeatInterval { get; set; }
        public TimeSpan HeartbeatTimeout { get; set; }
        public TimeSpan ConnectTimeout { get; set; }
        public bool IsActive { get; set; }
    }

    public class ShellServerConnectedClientViewModel
    {
        public Guid StationId { get; set; }
        public NetworkState State { get; set; }
        public DateTime? ServerRequestOn { get;set; }
        public DateTime? ConnectedOnUtc { get; set; }
        public DateTime? LastHeartBeatOnUtc { get; set; }
        public string DeviceIdentity { get; set; }
        public Guid? ConnectionId { get; set; }
    }

    public class ShellServerDetailsViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedUtc { get; set; }
        public bool IsActive { get; set; }
        public string PublicHostAddress { get; set; }
        public uint PublicPort { get; set; }
        public uint PublicInterfaceVersion { get; set; }
        public TimeSpan HeartbeatInterval { get; set; }
        public TimeSpan HeartbeatTimeout { get; set; }
        public TimeSpan ConnectTimeout { get; set; }
        public int ConnectedClients { get; set; }
    }
}
