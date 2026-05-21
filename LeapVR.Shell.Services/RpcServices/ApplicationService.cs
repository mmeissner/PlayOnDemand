#region Licence
/****************************************************************
 *  Filename: ApplicationService.cs
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
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Services.Data;
using Pod.Data.Infrastructure;
using Pod.Grpc.Base.Client;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellApplications;
using Pod.Grpc.ShellApplications;
using SyncAppsResponse = LeapVR.Shell.Services.Data.SyncAppsResponse;
using SyncResponse = LeapVR.Shell.Services.Data.SyncResponse;

namespace LeapVR.Shell.Services.RpcServices
{
    internal class ApplicationService :BaseService
    {
        private readonly GrpcClient<ShellApplicationServiceGrpc.ShellApplicationServiceGrpcClient> _grpcClient;
        private readonly GuidAsBytes _connectionId;

        public ApplicationService(
                GrpcClient<ShellApplicationServiceGrpc.ShellApplicationServiceGrpcClient> grpcClient, Guid connectionId):  base(grpcClient.Handler)
        {
            _connectionId = connectionId.ToGuidAsBytes();
            _grpcClient = grpcClient;
        }

        public IResult<ISyncResponse> GetSyncAppStates(bool onlyTimestamp = false)
        {
            return RpcWrapper(
                    () =>
                    {
                        var request = new SyncRequest()
                                      {
                                              ConnectionId = _connectionId,
                                              SendOnlyLastSyncTimestamp = onlyTimestamp
                                      };
                        return _grpcClient.RpcClient.GetSyncAppStates(request);
                    },SyncResponse.FromResponse);
        }

        public IResult<ISyncAppsResponse> SendSyncAppStates(TimeSpan timeSkew,
                IEnumerable<IAppUpdateInfo> updated = null,
                IEnumerable<IAppInstallInfo> installed = null,
                IEnumerable<IAppUninstallInfo> uninstalled = null)
        {

            return RpcWrapper(
                    () => {var request = new SyncAppsRequest
                                         {
                                                 ConnectionId = _connectionId
                                         };
                        if(updated != null) request.Updates.AddRange(updated.Select(AppInfoConverter.ToAppUpdateInfo));
                        if(installed != null) request.Installations.AddRange(installed.Select(AppInfoConverter.ToAppInstallInfo));
                        if(uninstalled != null) request.Uninstalls.AddRange(uninstalled.Select(x=>AppInfoConverter.ToAppUninstallInfo(timeSkew,x)));
                        return _grpcClient.RpcClient.SendSyncAppStates(request); },SyncAppsResponse.FromResponse);
        }

        public IResult<IAppUpdateResponse> SendAppInstalled(IAppInstallInfo installedApp)
        {
            return RpcWrapper(
                    () =>
                    { var request = new AppInstalledRequest()
                                    {
                                            ConnectionId = _connectionId,
                                            Installed = AppInfoConverter.ToAppInstallInfo(installedApp)
                                    };
                        return _grpcClient.RpcClient.SendAppInstalled(request);
                    },AppUpdateResponse.FromResponse);
        }

        public IResult<IAppUpdateResponse> SendAppUninstalled(AppUninstallInfo uninstalledApp)
        {

            return RpcWrapper(
                    () =>
                    {
                        var request = new AppUninstalledRequest()
                                      {
                                              ConnectionId = _connectionId,
                                              Uninstalled = uninstalledApp
                                      };
                        return _grpcClient.RpcClient.SendAppUninstalled(request);
                    },AppUpdateResponse.FromResponse);
        }

        public IResult<IAppUpdateResponse> SendAppUpdated(AppUpdateInfo updateInfo)
        {

            return RpcWrapper(
                    () =>
                    {
                        var request = new AppUpdateRequest
                                      {
                                              ConnectionId = _connectionId,
                                              Updated = updateInfo
                                      };
                        return _grpcClient.RpcClient.SendAppUpdate(request);
                    },AppUpdateResponse.FromResponse);
        }
        public override async Task<IResult> ConnectServiceAsync() { return await _grpcClient.Connect(); }
    }
}
