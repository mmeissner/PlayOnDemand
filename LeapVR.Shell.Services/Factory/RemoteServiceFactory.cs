#region Licence
/****************************************************************
 *  Filename: RemoteServiceFactory.cs
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
using System.IO;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Services.Data;
using LeapVR.Shell.Services.RpcClient;
using LeapVR.Shell.Services.RpcServices;
using Pod.Grpc.Base.Client;
using Pod.Grpc.ConnectHost;
using Pod.Grpc.ShellApplications;
using Pod.Grpc.ShellHost;


namespace LeapVR.Shell.Services.Factory
{
    /// <summary>
    /// Assembles Grpc Remote Services ready to use
    /// </summary>
    public class RemoteServiceFactory
    {
        private readonly RpcClientConfig _rpcClientConfig;
        private readonly IServerConfig _serverConfig;
        private readonly GrpcSslCredentials _grpcSslCredentials;
        private readonly ILocalMachine _localMachine;
        public RemoteServiceFactory(
                RpcClientConfig rpcClientConfig,
                IGlobalConfiguration globalConfiguration,
                IServerConfig serverConfig,
                ILocalMachine localMachine)
        {
            QuickLeap.AssertNotNull(globalConfiguration, serverConfig, localMachine);
            _rpcClientConfig = rpcClientConfig;
            _serverConfig = serverConfig;
            _localMachine = localMachine;
            _grpcSslCredentials = GrpcSslCredentials.Create(globalConfiguration.PersistentDirectory, serverConfig);
        }

        /// <summary>
        /// Creates a Remote Service object providing access to Remote Service Calls
        /// </summary>
        /// <returns></returns>
        public IRemoteServiceSet GetStationServices() { return new RemoteServicesSet(this); }

        /// <summary>
        /// Creates a Remote Service object providing access to an Connect Service
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal ConnectService GetConnectService(string stationId, string password)
        {
            return new ConnectService(
                    new GrpcClient<ConnectHostServiceGrpc.ConnectHostServiceGrpcClient>(
                            RpcHostDetails.Create(
                                    _serverConfig.ConnectServerHost,
                                    _serverConfig.ConnectServerPort),
                            GetHandler(stationId, password),
                            _rpcClientConfig.GrpcCallTimeout),
                    _localMachine);
        }

        /// <summary>
        /// Allows a Remote Service Set to create the Clients for the Services it provides
        /// </summary>
        /// <param name="shellServer">The ShellServer the Client should connect to</param>
        /// <param name="stationId">The Station Id for Call Credentials</param>
        /// <param name="password">The Password for the Station</param>
        /// <param name="shellService">The created Shell Service</param>
        /// <param name="applicationService">The created Application Service</param>
        internal void GetStationService(
                ShellServer shellServer, string stationId, string password, out ShellService shellService,
                out ApplicationService applicationService)
        {
            var hostDetails = RpcHostDetails.Create(shellServer.HostAddress, shellServer.Port);
            var shellGrpcClient = new GrpcClient<ShellHostServiceGrpc.ShellHostServiceGrpcClient>(
                    hostDetails,
                    GetHandler(stationId, password),
                    _rpcClientConfig.GrpcCallTimeout);
            var appGrpcClient = new GrpcClient<ShellApplicationServiceGrpc.ShellApplicationServiceGrpcClient>(
                    shellGrpcClient.Handler);
            shellService = new ShellService(shellGrpcClient, shellServer.ConnectionId);
            applicationService = new ApplicationService(appGrpcClient, shellServer.ConnectionId);
        }

        /// <summary>
        /// Creates an GrpcChannelCredentials Handler for an Station with that supports Call Credentials
        /// </summary>
        /// <param name="stationId">Station Id for Call Credentials</param>
        /// <param name="password">Password for Call Credentials</param>
        /// <returns></returns>
        private GrpcChannelCredentialsHandler GetHandler(string stationId, string password)
        {
            var handler = GrpcChannelCredentialsHandler.Create(_grpcSslCredentials.ServerRootCert);
            handler.SetChannelCredentials(stationId, password);
            return handler;
        }
    }
}