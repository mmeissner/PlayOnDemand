#region Licence
/****************************************************************
 *  Filename: ShellClientIntegrationTest.cs
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
using Pod.Services.ServerManager;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellHost;
using Pod.Services.ConnectHost;
using Pod.Services.Customer;
using Pod.Services.ShellHost;
using Pod.Services.Station;
using Pod.ViewModels.Customer;
using Pod.ViewModels.ShellServer;
using SessionState = Pod.Grpc.Messages.Shared.SessionState;

namespace Pod.Services.Test
{
    public class ShellClientIntegrationTest : IClassFixture<ServicesFixture>
    {
        private readonly IServiceProvider _container;
        private readonly ServicesFixture _servicesFixture;
        public ShellClientIntegrationTest(ServicesFixture servicesFixture)
        {
            _servicesFixture = servicesFixture;
            _servicesFixture.EnsureClearDb();
            _container = servicesFixture.GetServiceProvider();
        }
        [Fact(Skip = "Depends on ServicesFixture (real Postgres). To re-enable: rewrite the fixture to use the InMemory pattern from Pod.Web.Center.Test, or move this end-to-end flow into a new Pod.Web.Center.Test/Integration/ test that drives the full pipeline through PodWebApplicationFactory.")]
        public async Task SimpleConnectAndCreateSessionTest()
        {
            //Create Server
            ShellServerViewModel createdShellServer;
            using(var scope = _container.CreateScope())
            {
                var serverManager = scope.ServiceProvider.GetRequiredService<ServerManagerService>();
                var createServerResult = await serverManager.CreateNewServer("TestServer", "shellServer.example.com",50061,0);
                Assert.True(createServerResult.IsSuccess());
                createdShellServer = createServerResult.ReturnValue;
                Assert.True(createdShellServer.Id != Guid.Empty);
                Assert.Equal("TestServer", createdShellServer.DisplayName);
                Assert.Equal("shellServer.example.com", createdShellServer.PublicHostAddress);

                //Activate Server
                var activateResult = await serverManager.SetActiveState(createdShellServer.Id, true);
                Assert.True(activateResult.IsSuccess());
            }

            //Create User
            var user = await _servicesFixture.CreateTestUser();

            //Create Station
            StationSettingsViewModel station;
            using(var scope = _container.CreateScope())
            {
                //Create Station
                var customerStationService = scope.ServiceProvider.GetRequiredService<StationService>();
                var stationResult = await customerStationService.CreateNewStation(
                        user.Id,
                        "MyFirstStation",
                        "Password-1234");
                Assert.True(stationResult.IsSuccess());
                station = stationResult.ReturnValue;
                Assert.True(station.StationId != Guid.Empty);
                Assert.True(station.ControlMode != StationControlMode.Undefined);
            }

            //Create Client Credentials
            var clientCredentials = new ClientCredentials
                                    {StationId = station.StationId, Password = "Password-1234"};
            string deviceId = "myUniqueDeviceId";


            //Request Server for Station
            ShellServerResponse connectResponse;
            using(var scope = _container.CreateScope())
            {
                var connectService = scope.ServiceProvider.GetRequiredService<ConnectService>();
                var connectResult = await connectService.RequestServer(clientCredentials, deviceId, 0);
                Assert.True(connectResult.IsSuccess());
                connectResponse = connectResult.ReturnValue;
                Assert.True(connectResponse.ConnectionId.ToGuidNullable().HasValue);
            }

            //Connect to Server
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var connectResult = await shellService.ConnectToServer(
                        clientCredentials,
                        new ConnectRequest() {ConnectionId = connectResponse.ConnectionId},
                        //Use the created server
                        createdShellServer.Id);
                Assert.True(connectResult.IsSuccess());
            }

            //Receive Client Settings
            ClientSettingsResponse clientSettingsResponse;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var clientSettings = await shellService.GetClientSettings(
                        clientCredentials);
                Assert.True(clientSettings.IsSuccess());
                clientSettingsResponse = clientSettings.ReturnValue;
                Assert.Equal(station.DisplayName, clientSettingsResponse.DisplayName);
                Assert.Equal(station.ControlMode.ToGrpcControlMode(), clientSettingsResponse.Mode);
            }

            //Client Request new Session
            LoginRequestResponse requestLoginResponse;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var sessionRequestResponse = await shellService.RequestLogin(
                        clientCredentials,
                        new LoginRequest() {ConnectionId = connectResponse.ConnectionId},
                        "SomePeerInformationToStore");
                Assert.True(sessionRequestResponse.IsSuccess());
                requestLoginResponse = sessionRequestResponse.ReturnValue;
                Assert.Equal(SessionState.LoginRequested, requestLoginResponse.SessionDetails.SessionState);
                Assert.True(requestLoginResponse.SessionDetails.DeadlineUtcForPickUp.HasValue);
                Assert.True(requestLoginResponse.SessionDetails.SessionId.ToGuid() != Guid.Empty);
            }

            //Client Picks up new Session
            RequestedLoginResponse receivedLoginIntention;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var sessionRequestResponse = await shellService.GetLoginIntention(
                        clientCredentials,
                        new RequestedLoginRequest() {ConnectionId = connectResponse.ConnectionId});
                Assert.True(sessionRequestResponse.IsSuccess());
                receivedLoginIntention = sessionRequestResponse.ReturnValue;
                Assert.Equal(SessionState.AwaitingConfirmation, receivedLoginIntention.SessionDetails.SessionState);
                Assert.True(receivedLoginIntention.SessionDetails.DeadlineUtcForConfirmation.HasValue);
                Assert.True(receivedLoginIntention.SessionDetails.MaxTimeForConfirmationDecision.HasValue);
                Assert.True(receivedLoginIntention.SessionDetails.SessionId.ToGuid() != Guid.Empty);
            }

            //Client Confirms new Session
            LoginIntentionReplyResponse loginIntentionReplyResponse;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var loginResponseResult = await shellService.SetLoginResponse(
                        clientCredentials,
                        new LoginIntentionReplyRequest()
                        {
                                ConnectionId = connectResponse.ConnectionId,
                                IsLoginAccepted = true,
                                SessionId = receivedLoginIntention.SessionDetails.SessionId
                        });
                Assert.True(loginResponseResult.IsSuccess());
                loginIntentionReplyResponse = loginResponseResult.ReturnValue;
                Assert.Equal(SessionState.Running, loginIntentionReplyResponse.Session.SessionState);
                Assert.True(loginIntentionReplyResponse.Session.StartTimeUtc.HasValue);
                Assert.True(loginIntentionReplyResponse.Session.Conditions == null && !loginIntentionReplyResponse.Session.EffectiveDuration.HasValue);
                Assert.True(loginIntentionReplyResponse.Session.SessionId.ToGuid() != Guid.Empty);
            }

            //Check through service for State
            using(var scope = _container.CreateScope())
            {
                var customerStationService = scope.ServiceProvider.GetService<StationService>();
                var stationStatesResult = await customerStationService.GetStationsCurrentState(user.Id);
                Assert.True(stationStatesResult.IsSuccess());
                var stationStates = stationStatesResult.ReturnValue;
                var states = stationStates.ToList();
                Assert.True(1 == states.Count);
                var state = states.First();
                Assert.True(state.ControlMode != StationControlMode.Undefined);
                Assert.NotNull(state.NetworkState);
                Assert.Equal(NetworkState.Connected,state.NetworkState);
                Assert.NotNull(state.Session);
                Assert.Equal(Pod.Enums.SessionState.Started,state.Session.State);
                Assert.Null(state.Session.MaxDurationLimit);
                Assert.Null(state.Session.StartDuration);
                Assert.NotNull(state.Session.StartedOnUtc);
            }

            //Client Tries to get Session State
            SessionStateResponse sessionStateResponse;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var sessionStateRequestResult = await shellService.GetSessionState(
                        clientCredentials,
                        new SessionStateRequest() {ConnectionId = connectResponse.ConnectionId});
                Assert.True(sessionStateRequestResult.IsSuccess());
                sessionStateResponse = sessionStateRequestResult.ReturnValue;
                Assert.Equal(SessionState.Running, sessionStateResponse.Session.SessionState);
                Assert.True(sessionStateResponse.Session.StartTimeUtc.HasValue);
                Assert.True(sessionStateResponse.Session.Conditions == null);
                Assert.True(sessionStateResponse.Session.SessionId.ToGuid() != Guid.Empty);
            }

            //Client Tries to Stop Session
            LogoutResponse logoutResponse;
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var logoutSessionRequestResult = await shellService.LogoutSession(
                        clientCredentials,
                        new LogoutRequest()
                        {
                                ConnectionId = connectResponse.ConnectionId,
                                Reason = LogoutReason.UserLogout
                        });
                Assert.True(logoutSessionRequestResult.IsSuccess());
                logoutResponse = logoutSessionRequestResult.ReturnValue;
            }

            //Client Tries to get Session State
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var sessionStateRequestResult = await shellService.GetSessionState(
                        clientCredentials,
                        new SessionStateRequest() {ConnectionId = connectResponse.ConnectionId});
                Assert.True(sessionStateRequestResult.IsSuccess());
                sessionStateResponse = sessionStateRequestResult.ReturnValue;
                Assert.True(sessionStateResponse.Session == null);
            }

            //Client Disconnects
            using(var scope = _container.CreateScope())
            {
                var shellService = scope.ServiceProvider.GetService<ShellService>();
                var disconnectRequestResponse = await shellService.Disconnect(
                        clientCredentials,
                        new DisconnectRequest {ConnectionId = connectResponse.ConnectionId},
                        createdShellServer.Id);
                Assert.True(disconnectRequestResponse.IsSuccess());
            }

            //Check through service for State
            using(var scope = _container.CreateScope())
            {
                var customerStationService = scope.ServiceProvider.GetService<StationService>();
                var stationStatesResult = await customerStationService.GetStationsCurrentState(user.Id);
                Assert.True(stationStatesResult.IsSuccess());
                var stationStates = stationStatesResult.ReturnValue;
                var states = stationStates.ToList();
                Assert.True(1 == states.Count());
                var state = states.First();
                Assert.True(state.ControlMode != StationControlMode.Undefined);
                Assert.NotNull(state.NetworkState);
                Assert.Equal(NetworkState.Disconnected,state.NetworkState);
                Assert.Null(state.Session);
            }
        }
    }
}