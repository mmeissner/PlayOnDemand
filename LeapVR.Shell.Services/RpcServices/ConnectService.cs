#region Licence
/****************************************************************
 *  Filename: ConnectService.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Services.Data;
using NLog;
using Pod.Data.Infrastructure;
using Pod.Grpc.Base.Client;
using Pod.Grpc.ConnectHost;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Utilities;

namespace LeapVR.Shell.Services.RpcServices
{
    internal class ConnectService : BaseService
    {
        private readonly GrpcClient<ConnectHostServiceGrpc.ConnectHostServiceGrpcClient> _grpcClient;
        private readonly ILocalMachine _localMachine;

        public ConnectService(
                GrpcClient<ConnectHostServiceGrpc.ConnectHostServiceGrpcClient> grpcClient, ILocalMachine localMachine)
                : base(grpcClient.Handler)
        {
            _grpcClient = grpcClient;
            _localMachine = localMachine;
        }

        public async Task<IResult<ShellServer>> GetShellHostAsync(
                CancellationToken ct, Guid? reconnectConnectionId = null)
        {
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new ShellServerRequest
                                      {
                                              IdentityId = _localMachine.VBoxFingerprint,
                                              MaxInterfaceVersion = 0,
                                              ReconnectConnectionId = reconnectConnectionId.ToGuidAsBytes()
                                      };
                        return await _grpcClient.RpcClient.GetHostAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, ShellServer.FromResponse);
        }

        public IResult<ShellServer> GetShellHost(CancellationToken ct, Guid? reconnectConnectionId = null)
        {
            return RpcWrapper(
                    () =>
                    {
                        var request = new ShellServerRequest
                                      {
                                              IdentityId = _localMachine.VBoxFingerprint,
                                              MaxInterfaceVersion = 0,
                                              ReconnectConnectionId = reconnectConnectionId.ToGuidAsBytes()
                                      };
                        return _grpcClient.RpcClient.GetHost(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ShellServer.FromResponse);
        }

        public override async Task<IResult> ConnectServiceAsync() { return await _grpcClient.Connect(); }
    }
}