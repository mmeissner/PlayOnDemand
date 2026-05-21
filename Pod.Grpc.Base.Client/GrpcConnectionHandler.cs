#region Licence
/****************************************************************
 *  Filename: GrpcConnectionHandler.cs
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
using System.Runtime.CompilerServices;
using Grpc.Core;
using NLog;
using Pod.Grpc.Base.Const;

namespace Pod.Grpc.Base.Client
{
    public class GrpcConnectionHandler : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Channel _channel;
        private readonly ConnectionSettings _connectionSettings;


        public ConnectionSettings Settings => _connectionSettings;
        public GrpcConnectionHandler(ConnectionSettings settings)
        {
            _connectionSettings = settings;
            _channel = new Channel(
                    settings.HostDetails.ServerHost,
                    Convert.ToInt32(settings.HostDetails.ServerPort),
                    settings.ChannelCredentialsHandler.GetCredentials(),
                    PodChannelOptions.DefaultClientOptions());
        }

        public Channel GrpChannel => _channel;

        public DateTime GetDeadline([CallerMemberName] string callername = "Unknown Caller")
        {
            //No Timeout, means we will wait a good year
            if(_connectionSettings.RpcTimeoutMs == 0) return DateTime.UtcNow.AddYears(1);
            var retval= DateTime.UtcNow.AddMilliseconds(_connectionSettings.RpcTimeoutMs);
            _logger.Debug($"Return Deadline:{retval} , to Caller={callername}");
            return retval;
        }

        public void Dispose()
        {
            _channel?.ShutdownAsync();
        }

        public class ConnectionSettings
        {
            private readonly uint _defaultRpcCallDeadlineMs;
            public ConnectionSettings(IRpcHostDetails hostDetails, GrpcChannelCredentialsHandler channelCredentialsHandler, uint defaultRpcCallDeadlineMs = 3000)
            {
                HostDetails = hostDetails;
                ChannelCredentialsHandler = channelCredentialsHandler;
                _defaultRpcCallDeadlineMs = defaultRpcCallDeadlineMs;
                RpcTimeoutMs = _defaultRpcCallDeadlineMs;
            }
            public IRpcHostDetails HostDetails { get; }
            public GrpcChannelCredentialsHandler ChannelCredentialsHandler { get; }
            public uint RpcTimeoutMs { get; set; } 

            public void ResetDefaultTimeoutValues()
            {
                RpcTimeoutMs = _defaultRpcCallDeadlineMs;
            }
        }
    }
}
