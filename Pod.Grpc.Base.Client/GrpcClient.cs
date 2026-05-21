#region Licence
/****************************************************************
 *  Filename: GrpcClient.cs
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
using System.Threading.Tasks;
using Grpc.Core;
using NLog;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Utilities;

namespace Pod.Grpc.Base.Client
{
 public class GrpcClient<T> where T : ClientBase<T>
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IRpcHostDetails HostDetails { get; }

        public GrpcClient(
            IRpcHostDetails hostDetails,
            GrpcChannelCredentialsHandler channelCredentialsHandler,
            uint grpcCallTimeout)
        {
            HostDetails = hostDetails;
            Handler = new GrpcConnectionHandler(
                new GrpcConnectionHandler.ConnectionSettings(
                    hostDetails,
                    channelCredentialsHandler,
                    grpcCallTimeout));
            RpcClient = CreateGenericInstance(Handler.GrpChannel);
        }

        /// <summary>
        /// Can be used to work with two clients that share the same Connection/Channel.
        /// </summary>
        /// <param name="connectionHandler">The connection handler.</param>
        public GrpcClient(GrpcConnectionHandler connectionHandler)
        {
            HostDetails = connectionHandler.Settings.HostDetails;
            Handler = connectionHandler;
            RpcClient = CreateGenericInstance(connectionHandler.GrpChannel);
        }
        public T RpcClient { get; private set; }

        public GrpcConnectionHandler Handler { get; }

        private T CreateGenericInstance(Channel channel)
        {
            T retval = null;
            try
            {
                retval = (T)Activator.CreateInstance(typeof(T), channel);
            }
            catch(Exception exception)
            {
                _logger.Error(exception, "Error during creation of Rpc Client Instance!");
            }
            return retval;
        }

        public async Task<IResult> Connect(uint timeoutMs = 10000, [CallerMemberName] string caller = "Unknown")
        {
            var result = new Result();
            try
            {
                await Handler.GrpChannel.ConnectAsync(DateTime.UtcNow.AddMilliseconds(timeoutMs));
                return result;
            }
            catch(RpcException exception)
            {
                _logger.Warn(exception, $"Connect from {caller} failed!");
                result.Add(exception.ToResult());
                return result;
            }
            catch(Exception e)
            {
                _logger.Warn(e, $"Connect from {caller} failed!");
                return result.Add(e.Message, UserError.InternalError);
            }
        }
    }
}
