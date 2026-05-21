#region Licence
/****************************************************************
 *  Filename: ShellServer.cs
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
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;

namespace Pod.Data.Models.Servers
{
    /// <summary>
    /// Server handling Shell Client requests
    /// </summary>
    public class ShellServer
    {
        public static readonly TimeSpan DefaultHeartbeatTimeout = TimeSpan.FromMinutes(17);
        public static readonly TimeSpan DefaultHeartbeatInterval = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(5);

        public static readonly TimeSpan MinimumHeartbeatTimeout = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan MaximumHeartbeatTimeout = TimeSpan.FromMinutes(65);
        public static readonly TimeSpan MinimumHeartbeatInterval = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan MaximumHeartbeatInterval = TimeSpan.FromMinutes(60);

        public static readonly TimeSpan MinimumConnectTimeout = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan MaximumConnectTimeout = TimeSpan.FromSeconds(15);

        private HashSet<ConnectionState> _connectedClients;
        private HashSet<ClosedConnection> _connectionHistory;

        private ShellServer() { }
        private ShellServer(
                string displayName,
                string publicHostAddress,
                uint publicPort,
                uint publicInterfaceVersion)
        {
            CreatedUtc = DateTime.UtcNow;
            DisplayName = displayName;
            PublicHostAddress = publicHostAddress;
            PublicPort = publicPort;
            PublicInterfaceVersion = publicInterfaceVersion;
            IsActive = false;
            HeartbeatInterval = DefaultHeartbeatInterval;
            HeartbeatTimeout = DefaultHeartbeatTimeout;
            ConnectTimeout = DefaultConnectTimeout;
        }

        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// DateTime this instance was created
        /// </summary>
        public DateTime CreatedUtc { get; private set; }

        /// <summary>
        /// Display name of this instance
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// If active this server can accept new Client connections
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// The Host Address the server is reachable
        /// </summary>
        public string PublicHostAddress { get; private set; }

        /// <summary>
        /// The Port for new connections
        /// </summary>
        public uint PublicPort { get; private set; }

        /// <summary>
        /// The Interface version supported by this server
        /// </summary>
        public uint PublicInterfaceVersion { get; private set; }

        /// <summary>
        /// The Heartbeat interval for Shell Clients in that they should send a Heartbeat
        /// </summary>
        public TimeSpan HeartbeatInterval { get; private set; }

        /// <summary>
        /// The Interval after that a Shell Client Connection is considered timeout
        /// </summary>
        public TimeSpan HeartbeatTimeout { get; private set; }

        /// <summary>
        /// The Interval in that a client needs to connect after it announced a request for an connection at a connection server
        /// </summary>
        public TimeSpan ConnectTimeout { get; private set; }

        /// <summary>
        /// The Clients currently considered Online
        /// </summary>
        public IReadOnlyCollection<ConnectionState> ConnectedClients => _connectedClients;

        /// <summary>
        /// Logs of Connections to this Server
        /// </summary>
        public IReadOnlyCollection<ClosedConnection> ConnectionHistory => _connectionHistory;

        /// <summary>
        /// Creates a new Shell Server
        /// </summary>
        /// <param name="displayName">The Name</param>
        /// <param name="hostAddress">The Address to connect to</param>
        /// <param name="port">The Port to connect to</param>
        /// <param name="publicInterfaceVersion">The highest supported Interface version</param>
        /// <returns></returns>
        public static IResult<ShellServer> Create(
                string displayName,
                string hostAddress,
                uint port,
                uint publicInterfaceVersion)
        {
            var result = new Result<ShellServer>();

            result.ArgNotNullOrWhitespace(displayName,nameof(displayName),UserError.ShellServerInvalidDisplayName);
            result.ArgNotNullOrWhitespace(hostAddress,nameof(hostAddress),UserError.ShellServerInvalidHostAddress);          
            result.ArgOutOfRange(port,nameof(port),1,65535,UserError.ShellServerInvalidPort);
            if(result.HasError()) return result;
            return result.Add(new ShellServer(displayName, hostAddress, port,publicInterfaceVersion));
        }

        /// <summary>
        /// Toggles active state
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active) { IsActive = active; }

        /// <summary>
        /// Sets the DisplayName
        /// </summary>
        /// <param name="displayName">The name to set</param>
        /// <returns>result</returns>
        public IResult SetDisplayName(string displayName)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(displayName,nameof(displayName),UserError.ShellServerInvalidDisplayName);
            if(result.IsSuccess())DisplayName = displayName;
            return result;
        }

        /// <summary>
        /// Changes Heartbeat intervals
        /// </summary>
        /// <param name="heartbeatTimeout">The Timeout interval</param>
        /// <param name="heartbeatInterval">The interval for the heartbeat</param>
        /// <returns></returns>
        public IResult SetHeartbeatValues(TimeSpan heartbeatTimeout, TimeSpan heartbeatInterval)
        {
            var result = new Result();
            //Ranges
            var durationHeartBeatTimeout = heartbeatTimeout.Duration();
            var durationHeartbeatInterval = heartbeatInterval.Duration();

            //Upper and lower Range
            
            result.ArgNotLowerThen(
                    (TimeSpan)durationHeartBeatTimeout,
                    nameof(heartbeatTimeout),
                    MinimumHeartbeatTimeout,
                    nameof(MinimumHeartbeatTimeout),
                    UserError.ShellServerInvalidHeartbeatValue);
            result.ArgNotHigherThen(
                    durationHeartBeatTimeout,
                    nameof(heartbeatTimeout),
                    MaximumHeartbeatTimeout,
                    nameof(MaximumHeartbeatTimeout),
                    UserError.ShellServerInvalidHeartbeatValue);
            result.ArgNotLowerThen(
                    durationHeartbeatInterval,
                    nameof(heartbeatInterval),
                    MinimumHeartbeatInterval,
                    nameof(MinimumHeartbeatInterval),
                    UserError.ShellServerInvalidHeartbeatValue);
            result.ArgNotHigherThen(
                    durationHeartbeatInterval,
                    nameof(heartbeatInterval),
                    MaximumHeartbeatInterval,
                    nameof(MaximumHeartbeatInterval),
                    UserError.ShellServerInvalidHeartbeatValue);

            //Logic checks
            result.ArgNotHigherThen(
                    durationHeartbeatInterval,
                    nameof(heartbeatInterval),
                    durationHeartBeatTimeout,
                    nameof(durationHeartBeatTimeout),
                    UserError.ShellServerInvalidHeartbeatValue);

            if(result.HasError()) return result;

            HeartbeatTimeout = durationHeartBeatTimeout;
            HeartbeatInterval = durationHeartbeatInterval;
            return result;
        }

        /// <summary>
        /// Sets the time interval in that a client needs to establish a connection
        /// after having one requested at a Connect Server
        /// </summary>
        /// <param name="connectTimeout">The interval</param>
        /// <returns>result</returns>
        public IResult SetConnectTimeout(TimeSpan connectTimeout)
        {
            var result = new Result();
            //Ranges
            var duration = connectTimeout.Duration();

            //Upper and lower Range

            result.ArgNotLowerThen(
                    duration,
                    nameof(connectTimeout),
                    MinimumConnectTimeout,
                    nameof(MinimumConnectTimeout),
                    UserError.ShellServerInvalidTimeoutValue);
            result.ArgNotHigherThen(
                    duration,
                    nameof(connectTimeout),
                    MaximumConnectTimeout,
                    nameof(MaximumConnectTimeout),
                    UserError.ShellServerInvalidTimeoutValue);

            if(result.IsSuccess())ConnectTimeout = duration;
            return result;

        }

        /// <summary>
        /// Sets the Connection Properties
        /// </summary>
        /// <param name="hostAddress">The Address to connect to</param>
        /// <param name="port">The port to connect to</param>
        /// <param name="publicInterfaceVersion">The highest available interface version</param>
        /// <returns></returns>
        public IResult SetHostDetails(
                string hostAddress,
                uint port,
                uint publicInterfaceVersion)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(hostAddress,nameof(hostAddress),UserError.ShellServerInvalidHostAddress);
            result.ArgOutOfRange(port,nameof(port),1,65535,UserError.ShellServerInvalidPort);
            if(result.HasError()) return result;
            PublicHostAddress = hostAddress;
            PublicPort = port;
            PublicInterfaceVersion = publicInterfaceVersion;
            return result;
        }
    }
}