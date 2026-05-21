#region Licence
/****************************************************************
 *  Filename: ShellHostServiceGrpc.cs
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
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Models.Servers;
using Pod.Enums;
using Pod.Grpc.Base.Server;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellHost;
using Pod.Grpc.Utilities;
using Pod.Services;
using Pod.Services.ShellHost;

namespace Pod.Grpc.ShellHost.Server.Services
{
    /// <summary>
    /// Basic Service for Shell Host to handle Shell Client Requests related to Connections and Sessions as well as Notifications
    /// </summary>
    /// <remarks>
    /// Authorisation runs through <see cref="GrpcMetadataAuthenticationHandler"/>
    /// before any method body executes. <see cref="GetNotifications"/> still
    /// calls <c>credentials.VerifyCredentials(...)</c> internally — that's now
    /// a redundant defence-in-depth re-check, but it's kept so the existing
    /// flow that combines password verification with the connection-id
    /// binding check remains structurally identical to the netcoreapp2.1
    /// version (and continues to fail closed if anything in the wiring
    /// regresses).
    /// </remarks>
    [Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]
    public class ShellHostServiceGrpc : ShellHost.ShellHostServiceGrpc.ShellHostServiceGrpcBase
    {
        private readonly ILogger<ShellHostServiceGrpc> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ShellServer _serverInfo;

        public ShellHostServiceGrpc(ILogger<ShellHostServiceGrpc> logger,IServiceProvider serviceProvider, ShellServer serverInfo)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _serverInfo = serverInfo;
        }

        public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.ConnectToServer(credentials, request, _serverInfo.Id);
                if(result.IsSuccess())
                {
                    return result.ReturnValue;
                }

                throw result.ToException();
            }
        }

        public override Task<ServerSettingsResponse> GetServerSettings(
                ServerSettingsRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                    new ServerSettingsResponse()
                    {
                            HeartbeatInterval = _serverInfo.HeartbeatInterval.ToTimeSpanAsLong(),
                            HeartbeatTimeout = _serverInfo.HeartbeatTimeout.ToTimeSpanAsLong(),
                            ServerTimeUtcNow = DateTime.UtcNow.ToDateTimeUtcAsLong()
                    });
        }

        public override async Task<ClientSettingsResponse> GetClientSettings(
                ClientSettingsRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.GetClientSettings(credentials);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        public override async Task<HeartbeatResponse> SendHeartbeat(HeartbeatRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.SetHeartbeat(credentials, request, _serverInfo.Id);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        /// <summary>
        /// Provides a Stream to for a client were <see cref="ClientNotification"/> can be send 
        /// </summary>
        /// <param name="request">The Request</param>
        /// <param name="responseStream">The Stream where the Client Notifications are send by</param>
        /// <param name="context">The Context provided by Grpc</param>
        /// <returns>A Task that ends when there are no messages to send. This can be because the client disconnected or the because no messages are allowed to be send anymore</returns>
        public override async Task GetNotifications(
                NotificationRequest request, IServerStreamWriter<ClientNotification> responseStream,
                ServerCallContext context)
        {
            try
            {
                var credentials = context.ToClientCredentials();
                using(var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<PodDbContext>();
                    var verificationResult = await credentials.VerifyCredentials(dbContext);
                    //Check for Credentials and ConnectionId
                    if(verificationResult.HasError() ||
                       !verificationResult.ArgTrue(
                               request.ConnectionId.HasConnectionId(out var connectionId),
                               nameof(request.ConnectionId),
                               UserError.ShellClientConnectionIdMismatch))
                    {
                        throw verificationResult.ToException();
                    }

                    //Verify ConnectionId
                    var connection = await dbContext.Stations.Where(x => x.Id == credentials.StationId).
                                                     Select(x => x.ConnectionState.ConnectionId).
                                                     FirstOrDefaultAsync();
                    if(!verificationResult.ValueTrue(
                            connection.HasValue && connection.Value == connectionId,
                            nameof(connectionId),
                            UserError.ShellClientConnectionIdMismatch))
                    {
                        throw verificationResult.ToException();
                    }
                }


                //Initializes an the streaming of Messages to the client
                //The parameters provided are:
                //To convert the Hubs Messages to Data Transfer Objects that can be send
                //To define an Message that is the signal for the stream to end
                //To provide some messages that should be added to the send queue from the start
                var handler =  _serviceProvider.GetRequiredService<PublisherHub<ClientCommandType>>().
                                       GetHandler(
                                               credentials.StationId,
                                               responseStream,
                                               message => new ClientNotification
                                                          {Event = message.ToNotificationMessage()},
                                               message => message == ClientCommandType.Disconnect,
                                               new[]
                                               {
                                                       ClientCommandType.UpdateServerSettings,
                                                       ClientCommandType.UpdateClientSettings
                                               });

                //Handler can be null if the Message Hub is about to shutdown
                if(handler != null) await handler.ReceiveMessages(context.CancellationToken);

                //On Client Side the finish of an notification stream should be all time considered as a disconnect
            }
            catch (Exception e)
            {
                _logger.LogCritical(e,"Exception in Notification Call");
                throw;
            }
        }

        public override async Task<LoginRequestResponse> SendLoginIntention(
                LoginRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.RequestLogin(credentials, request, context.Peer);
                if(result.HasError()) throw result.ToException();
                _serviceProvider.GetService<PublisherHub<ClientCommandType>>().
                                 Publish(credentials.StationId, ClientCommandType.GetLoginRequest);
                return result.ReturnValue;
            }
        }

        public override async Task<RequestedLoginResponse> GetLoginIntention(
                RequestedLoginRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.GetLoginIntention(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        public override async Task<LoginIntentionReplyResponse> SendLoginResponse(
                LoginIntentionReplyRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.SetLoginResponse(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        public override async Task<SessionStateResponse> GetSessionState(
                SessionStateRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.GetSessionState(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        public override async Task<LogoutResponse> SendLogoutRequest(LogoutRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.LogoutSession(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        public override async Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
                var result = await shellService.Disconnect(credentials, request, _serverInfo.Id);
                if(result.HasError()) throw result.ToException();
                _serviceProvider.GetService<PublisherHub<ClientCommandType>>().
                                 Publish(credentials.StationId, ClientCommandType.Disconnect);
                return result.ReturnValue;
            }
        }
    }
}