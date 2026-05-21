#region Licence
/****************************************************************
 *  Filename: ShellService.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Services.Data;
using NLog;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Base.Client;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellHost;
using Pod.Grpc.ShellHost;
using Pod.Grpc.Utilities;
using ConnectResponse = LeapVR.Shell.Services.Data.ConnectResponse;
using DisconnectResponse = LeapVR.Shell.Services.Data.DisconnectResponse;
using HeartbeatResponse = LeapVR.Shell.Services.Data.HeartbeatResponse;
using LoginRequest = LeapVR.Shell.Services.Data.LoginRequest;
using LogoutResponse = LeapVR.Shell.Services.Data.LogoutResponse;
using SessionState = LeapVR.Shell.Services.Data.SessionState;

namespace LeapVR.Shell.Services.RpcServices
{
    internal class ShellService : BaseService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly GrpcClient<ShellHostServiceGrpc.ShellHostServiceGrpcClient> _grpcClient;
        private readonly Subject<ServerMessage> _whenServerMessage =
                new Subject<ServerMessage>();
        private readonly GuidAsBytes _connectionId;
        public ShellService(
                GrpcClient<ShellHostServiceGrpc.ShellHostServiceGrpcClient> grpcClient, Guid connectionId) : base(
                grpcClient.Handler)
        {
            _connectionId = connectionId.ToGuidAsBytes();
            _grpcClient = grpcClient;
        }
        public IObservable<ServerMessage> WhenServerMessage => _whenServerMessage.AsObservable();

        public IResult<ConnectResponse> ConnectToShellServer(CancellationToken ct)
        {
            Logger.Info("Trying to Connect to Server");
            return RpcWrapper(
                    () =>
                    {
                        var request = new ConnectRequest
                                      {
                                              ConnectionId = _connectionId,
                                      };
                        return _grpcClient.RpcClient.Connect(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ConnectResponse.FromResponse);
        }
        public async Task<IResult<ConnectResponse>> ConnectToShellServerAsync(CancellationToken ct)
        {
            Logger.Info("Trying to Connect to Server Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new ConnectRequest
                                      {
                                              ConnectionId = _connectionId,
                                      };
                        return await _grpcClient.RpcClient.ConnectAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ConnectResponse.FromResponse);
        }

        public IResult<ServerSettings> GetServerSettings(CancellationToken ct)
        {
            Logger.Info("Requesting Server Settings");
            return RpcWrapper(
                    () =>
                    {
                        var request = new ServerSettingsRequest { };
                        return _grpcClient.RpcClient.GetServerSettings(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ServerSettings.FromResponse);
        }
        public async Task<IResult<ServerSettings>> GetServerSettingsAsync(CancellationToken ct)
        {
            Logger.Info("Requesting Server Settings Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new ServerSettingsRequest { };
                        return await _grpcClient.RpcClient.GetServerSettingsAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ServerSettings.FromResponse);
        }

        public IResult<ClientSettings> GetClientSettings(CancellationToken ct)
        {
            Logger.Info("Requesting Client Settings");
            return RpcWrapper(
                    () =>
                    {
                        var request = new ClientSettingsRequest() { };
                        return _grpcClient.RpcClient.GetClientSettings(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ClientSettings.FromResponse);
        }
        public async Task<IResult<ClientSettings>> GetClientSettingsAsync(CancellationToken ct)
        {
            Logger.Info("Requesting Client Settings Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new ClientSettingsRequest() { };
                        return await _grpcClient.RpcClient.GetClientSettingsAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },ClientSettings.FromResponse);
        }
       
        public IResult<LoginRequest> SendLoginRequest(CancellationToken ct)
        {
            Logger.Info("Requesting LoginIntention");
            return RpcWrapper(
                    () =>
                    {
                        var request = new Pod.Grpc.Messages.ShellHost.LoginRequest() {ConnectionId = _connectionId};
                        return _grpcClient.RpcClient.SendLoginIntention(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },LoginRequest.FromResponse);
        }
        public async Task<IResult<LoginRequest>> SendLoginRequestAsync(CancellationToken ct)
        {
            Logger.Info("Requesting LoginIntention Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new Pod.Grpc.Messages.ShellHost.LoginRequest() {ConnectionId = _connectionId};
                        return await _grpcClient.RpcClient.SendLoginIntentionAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },LoginRequest.FromResponse);
        }

        public IResult<RequestedLogin> GetLoginIntention(CancellationToken ct)
        {
            Logger.Info("Trying to get LoginIntention");
            return RpcWrapper(
                    () =>
                    {
                        var request = new RequestedLoginRequest() {ConnectionId = _connectionId};
                        return _grpcClient.RpcClient.GetLoginIntention(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, RequestedLogin.FromResponse);
        }
        public async Task<IResult<RequestedLogin>> GetLoginIntentionAsync(CancellationToken ct)
        {
            Logger.Info("Trying to get LoginIntention Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new RequestedLoginRequest() {ConnectionId = _connectionId};
                        return await _grpcClient.RpcClient.GetLoginIntentionAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, RequestedLogin.FromResponse);
        }

        public IResult<LoginIntentionReply> SendLoginIntentionResponse(bool isAccepted, CancellationToken ct)
        {
            Logger.Info("Sending LoginIntention Response");
            return RpcWrapper(
                    () =>
                    {
                        var request = new LoginIntentionReplyRequest
                                      {ConnectionId = _connectionId, IsLoginAccepted = isAccepted};
                        return _grpcClient.RpcClient.SendLoginResponse(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, LoginIntentionReply.FromResponse);
        }
        public async Task<IResult<LoginDecisionResponse>> SendLoginIntentionResponseAsync(
                bool isAccepted, CancellationToken ct)
        {
            Logger.Info("Sending LoginIntention Response Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new LoginIntentionReplyRequest
                                      {ConnectionId = _connectionId, IsLoginAccepted = isAccepted};
                        return await _grpcClient.RpcClient.SendLoginResponseAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },LoginDecisionResponse.FromResponse);
        }

        public IResult<SessionState> GetSessionState(CancellationToken ct)
        {
            Logger.Info("Requesting Session State");
            return RpcWrapper(
                    () =>
                    {
                        var request = new SessionStateRequest {ConnectionId = _connectionId};
                        return _grpcClient.RpcClient.GetSessionState(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },SessionState.FromResponse);
        }
        public async Task<IResult<SessionState>> GetSessionStateAsync(CancellationToken ct)
        {
            Logger.Info("Requesting Session State Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new SessionStateRequest {ConnectionId = _connectionId};
                        return await _grpcClient.RpcClient.GetSessionStateAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },SessionState.FromResponse);
        }

        public IResult<LogoutResponse> SendLogoutRequest(SessionStopReason reason, CancellationToken ct)
        {
            Logger.Info("Sending Logout Request");
            return RpcWrapper(
                    () =>
                    {
                        var request = new LogoutRequest {ConnectionId = _connectionId, Reason = ConvertEnum.FromSessionStopReason(reason)};
                        return _grpcClient.RpcClient.SendLogoutRequest(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },LogoutResponse.FromResponse);
        }
        public async Task<IResult<LogoutResponse>> SendLogoutRequestAsync(SessionStopReason reason, CancellationToken ct)
        {
            Logger.Info("Sending Logout Request Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new LogoutRequest {ConnectionId = _connectionId, Reason = ConvertEnum.FromSessionStopReason(reason)};
                        return await _grpcClient.RpcClient.SendLogoutRequestAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    },LogoutResponse.FromResponse);
        }

        public IResult<HeartbeatResponse> SendHeartbeat(CancellationToken ct)
        {
            Logger.Debug("Sending Heartbeat");
            return RpcWrapper(
                    () =>
                    {
                        var request = new HeartbeatRequest() {ConnectionId = _connectionId};
                        return _grpcClient.RpcClient.SendHeartbeat(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, HeartbeatResponse.FromResponse);
        }
        public async Task<IResult<HeartbeatResponse>> SendHeartbeatAsync(CancellationToken ct)
        {
            Logger.Info("Sending Heartbeat Async");
            return await RpcWrapperAsync(
                    async () =>
                    {
                        var request = new HeartbeatRequest() {ConnectionId = _connectionId};
                        return await _grpcClient.RpcClient.SendHeartbeatAsync(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, HeartbeatResponse.FromResponse);
        }

        public IResult<DisconnectResponse> DisconnectFromShellHost(CancellationToken ct)
        {
            Logger.Info("Disconnecting from ShellHost");
            return RpcWrapper(
                    () =>
                    {
                        var request = new DisconnectRequest() {ConnectionId = _connectionId};
                        return _grpcClient.RpcClient.Disconnect(
                                request,
                                deadline: Handler.GetDeadline(),
                                cancellationToken: ct);
                    }, DisconnectResponse.FromResponse);
        }

        public async Task<IResult> GetNotificationsAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Starting to receive server messages");
            var result = new Result();
            try
            {
                var request = new NotificationRequest
                              {
                                      ConnectionId = _connectionId
                              };
                var streamingNotifications = _grpcClient.RpcClient.GetNotifications(request);
                while(await streamingNotifications.ResponseStream.MoveNext(cancellationToken))
                {
                    var message = ConvertEnum.FromClientNotificationEvent(
                            streamingNotifications.ResponseStream.Current.Event);
                    Logger.Info($"Received Server Message: {message.LogJson()}");
                    _whenServerMessage.OnNext(message);
                }
                return result;
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Warn("Task was canceled");
                    return result;
                }
                else if(e is RpcException rpcException)
                {
                    Logger.Error(e,"Server Message Task had an RPC exception");
                    _whenServerMessage.OnError(rpcException);
                    return result.Add(rpcException.ToResult());
                }
                Logger.Error(e, "Server Message Task had an unknown exception");
                return result.Add(e.Message, UserError.InternalError);
            }
            finally
            {
                Logger.Info("Server messages completed");
                _whenServerMessage.OnCompleted();
            }
        }
        public override async Task<IResult> ConnectServiceAsync() { return await _grpcClient.Connect(); }
    }
}