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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Messages.ShellHost;

namespace Pod.Services.ShellHost
{
    /// <summary>
    /// Service for Shell Clients
    /// </summary>
    public class ShellService
    {
        private readonly ILogger<ShellService> _logger;
        private readonly StationResponseHub _stationResponseHub;

        private readonly PodDbContext _podContext;
        public ShellService(ILogger<ShellService> logger,StationResponseHub stationResponseHub, PodDbContext podContext)
        {
            _logger = logger;
            _stationResponseHub = stationResponseHub;
            _podContext = podContext;
        }

        /// <summary>
        /// Informs the Server to set the Client to Connected State
        /// </summary>
        /// <param name="clientCredentials">The Clients Credentials</param>
        /// <param name="request">The Connect Request</param>
        /// <param name="serverId">The Server Id the connect occurs to</param>
        /// <returns></returns>
        public async Task<IResult<ConnectResponse>> ConnectToServer(
                ClientCredentials clientCredentials, ConnectRequest request, Guid serverId)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<ConnectResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;
            try
            {
                //Get the Station
                var stationDb = await _podContext.Stations.Where(x => x.Id == clientCredentials.StationId).
                                                  Include(x => x.ConnectionState).
                                                  Include(x => x.SessionDetails).
                                                  ThenInclude(x => x.Session).
                                                  FirstOrDefaultAsync();

                //Request Connected State
                var connectedResult = stationDb.ConnectionState.RequestConnected(serverId, connectionId);
                if(connectedResult.HasError()) return result.Add(connectedResult);

                //Handle the Response and persist only if there is a new state
                if(stationDb.SessionDetails.HandleConnectResponse(closedConnection => _podContext.Add(closedConnection), connectedResult.ReturnValue) ||
                   result.IsConnectionResponseSuccess(connectedResult.ReturnValue.Result))
                {
                    await _podContext.SaveChangesAsync();
                }

                if(result.HasError()) return result;
                return result.Add(new ConnectResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.RequestConnect,result));
            }
        }

        /// <summary>
        /// Requests the Client Settings from the Server
        /// </summary>
        /// <param name="clientCredentials">The Clients Credentials</param>
        /// <returns>The Client Settings</returns>
        public async Task<IResult<ClientSettingsResponse>> GetClientSettings(ClientCredentials clientCredentials)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<ClientSettingsResponse>(_podContext);
            if(result.HasError()) return result;

            try
            {
                var displaySettings = await _podContext.StationSettings.
                                                        Where(x => x.StationId == clientCredentials.StationId).
                                                        FirstOrDefaultAsync();
                if(!result.ValueNotNull(displaySettings, nameof(displaySettings))) return result;
                return result.Add(
                        new ClientSettingsResponse
                        {
                                DisplayName = displaySettings.DisplayName,
                                Mode = displaySettings.ControlMode.ToGrpcControlMode(),
                                QrCode = displaySettings.QRCode ?? ""
                        });
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.GetClientSettings,result));
            }

        }

        /// <summary>
        /// Sends a Keep Alive/Heartbeat signal to the Server
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="request">The Heartbeat request</param>
        /// <param name="serverId">The Server Id the request is send to</param>
        /// <returns>Result</returns>
        public async Task<IResult<HeartbeatResponse>> SetHeartbeat(
                ClientCredentials clientCredentials, HeartbeatRequest request, Guid serverId)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<HeartbeatResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                //Get the Station
                var stationDb = await _podContext.Stations.Where(x => x.Id == clientCredentials.StationId).
                                                  Include(x => x.ConnectionState).
                                                  ThenInclude(x=> x.ShellServer).
                                                  Include(x=>x.SessionDetails).
                                                  ThenInclude(x=> x.Session).
                                                  FirstOrDefaultAsync();

                //Request to set the Heartbeat
                var heartbeatResult = stationDb.ConnectionState.RequestHeartbeat(serverId, connectionId);
                if(heartbeatResult.HasError()) return result.Add(heartbeatResult);

                //Handle the Response, it might include closed connections, ended sessions
                stationDb.SessionDetails.HandleConnectResponse(closedConnection => _podContext.Add(closedConnection), heartbeatResult.ReturnValue);

                //Add an Error to result in case it was not a successful heartbeat
                result.IsConnectionResponseSuccess(heartbeatResult.ReturnValue.Result);

                //Persist as we do have changes at this point, either new heartbeat timestamp, a closed connection, an ended session
                await _podContext.SaveChangesAsync();

                if(result.HasError()) return result;
                return result.Add(new HeartbeatResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.SetHeartbeat,result));
            }
        }

        /// <summary>
        /// Sends a Request for a new Session to the Server
        /// </summary>
        /// <param name="clientCredentials">The Clients Credentials</param>
        /// <param name="request">The Request</param>
        /// <param name="requestPeer">The peer information where the request is send from</param>
        /// <returns>The Result</returns>
        public async Task<IResult<LoginRequestResponse>> RequestLogin(
                ClientCredentials clientCredentials, LoginRequest request, string requestPeer)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<LoginRequestResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                var station = await _podContext.Stations.Where(x => x.Id == clientCredentials.StationId).
                                                Include(x => x.SessionDetails).
                                                ThenInclude(x => x.Session).
                                                Include(x => x.ConnectionState).
                                                FirstAsync();
                if(!station.ConnectionState.ConnectionId.HasValue ||
                   station.ConnectionState.ConnectionId.Value != connectionId)
                {
                    result.Add(
                            "There was no ConnectionId or the provided Id does not match",
                            UserError.ShellClientConnectionIdMismatch);
                    return result;
                }

                var resultRequestSession = station.SessionDetails.RequestSession(RequestSource.ShellClient, requestPeer);
                if(resultRequestSession.HasError()) return result.Add(resultRequestSession);
                if(!result.IsSessionResponseSuccess(resultRequestSession.ReturnValue)) return result;
                await _podContext.SaveChangesAsync();
                return result.Add(station.SessionDetails.ToGrpcLoginRequestResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.RequestClientLogin,result));
            }
        }

        /// <summary>
        /// Requests a Session that is intended and waiting for pickup by the Client
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="request">The Request</param>
        /// <returns>The Session information</returns>
        public async Task<IResult<RequestedLoginResponse>> GetLoginIntention(
                ClientCredentials clientCredentials, RequestedLoginRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<RequestedLoginResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                var sessionDetails = await _podContext.SessionDetails.
                                                       Where(x => x.StationId == clientCredentials.StationId).
                                                       Include(x => x.Session).
                                                       ThenInclude(x => x.SessionRule).
                                                       Include(x => x.Station).
                                                       FirstAsync();
                var resultRequestDelivery = sessionDetails.RequestDelivery(connectionId);

                //Handle the Response and check for errors
                if(resultRequestDelivery.HasError()) return result.Add(resultRequestDelivery);
                await _podContext.SaveChangesAsync();

                //Check if we succeed to set the state to delivered
                if(!result.IsSessionResponseSuccess(resultRequestDelivery.ReturnValue)) return result;

                //Succeed Case
                return result.Add(sessionDetails.ToGrpcRequestedLoginResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.GetLoginIntention,result));
            }

        }

        /// <summary>
        /// Sends the Response for a picked up/intended Session
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="request">The request</param>
        /// <returns>The result for the response</returns>
        public async Task<IResult<LoginIntentionReplyResponse>> SetLoginResponse(
                ClientCredentials clientCredentials, LoginIntentionReplyRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<LoginIntentionReplyResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                var sessionDetails = _podContext.SessionDetails.Where(x => x.StationId == clientCredentials.StationId).
                                                 Include(x => x.Session).
                                                 ThenInclude(x => x.SessionRule).
                                                 Include(x => x.Station).
                                                 First();
                var resultRequestResponse = sessionDetails.SetResponse(connectionId, request.IsLoginAccepted);
                //Handle the Response and check for errors
                if(resultRequestResponse.HasError()) return result.Add(resultRequestResponse);

                await _podContext.SaveChangesAsync();
                if(!result.IsSessionResponseSuccess(resultRequestResponse.ReturnValue)) return result;

                //No Session to respond if successful declined
                if(!request.IsLoginAccepted)
                {
                    return result.Add(new LoginIntentionReplyResponse());
                }

                return result.Add(
                        new LoginIntentionReplyResponse()
                        {
                                Session = sessionDetails.ToGrpcSessionDetails()
                        });
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.SetLoginResponse,result));
            }

        }

        /// <summary>
        /// Requests the current remote session state
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="request">The request</param>
        /// <returns>The current remote session state</returns>
        public async Task<IResult<SessionStateResponse>> GetSessionState(
                ClientCredentials clientCredentials, SessionStateRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<SessionStateResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                var sessionDetails = _podContext.SessionDetails.Where(x => x.StationId == clientCredentials.StationId).
                                                 Include(x => x.Session).
                                                 ThenInclude(x => x.SessionRule).
                                                 Include(x => x.Station).
                                                 First();

                if(sessionDetails.Session == null) return result.Add(new SessionStateResponse());
                return result.Add(
                        new SessionStateResponse()
                        {
                                Session = sessionDetails.ToGrpcSessionDetails()
                        });
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.GetSessionState,result));
            }
        }

        /// <summary>
        /// Requests the end of a session
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="request">The request</param>
        /// <returns>The Result</returns>
        public async Task<IResult<LogoutResponse>> LogoutSession(
                ClientCredentials clientCredentials, LogoutRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<LogoutResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;
            try
            {
                var sessionDetails = _podContext.SessionDetails.Where(x => x.StationId == clientCredentials.StationId).
                                                 Include(x => x.Session).
                                                 Include(x => x.Station).
                                                 First();
                var resultRequestResponse = sessionDetails.EndSession(connectionId, request.Reason.ToStopReason());
                //Handle the Response and check for errors
                if(resultRequestResponse.HasError()) return result.Add(resultRequestResponse);
                if(!result.IsSessionResponseSuccess(resultRequestResponse.ReturnValue)) return result;
                await _podContext.SaveChangesAsync();
                return result.Add(new LogoutResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.RequestLogout,result));
            }

        }

        /// <summary>
        /// Informs the Server about an Disconnect and allows for a graceful shutdown of streams
        /// </summary>
        /// <param name="clientCredentials">The Clients Credentials</param>
        /// <param name="request">The Request</param>
        /// <param name="serverId">The Server Id the request is send to</param>
        /// <returns></returns>
        public async Task<IResult<DisconnectResponse>> Disconnect(
                ClientCredentials clientCredentials, DisconnectRequest request, Guid serverId)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<DisconnectResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ShellClientConnectionIdMismatch);
            if(result.HasError()) return result;

            try
            {
                //Get the Station
                var stationDb = await _podContext.Stations.Where(x => x.Id == clientCredentials.StationId).
                                                  Include(x => x.ConnectionState).
                                                  Include(x => x.SessionDetails).
                                                  ThenInclude(x => x.Session).
                                                  FirstOrDefaultAsync();
                var resultRequestResponse = stationDb.ConnectionState.RequestDisconnected(serverId, connectionId);
                //Handle the Response and check for errors
                if(resultRequestResponse.HasError()) return result.Add(resultRequestResponse);

                //Handle the Response and persist only if there is a new state
                if(stationDb.SessionDetails.HandleConnectResponse(closedConnection => _podContext.Add(closedConnection), resultRequestResponse.ReturnValue) ||
                   result.IsConnectionResponseSuccess(resultRequestResponse.ReturnValue.Result))
                {
                    await _podContext.SaveChangesAsync();
                }

                if(result.HasError()) return result;
                return result.Add(new DisconnectResponse());
            }
            finally
            {
                _stationResponseHub.Publish(new ClientResponse(clientCredentials.StationId,ClientRequestType.RequestDisconnect,result));
            }
        }
    }
}