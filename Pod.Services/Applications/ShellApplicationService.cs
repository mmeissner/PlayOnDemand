#region Licence
/****************************************************************
 *  Filename: ShellApplicationService.cs
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
using Pod.Data.Models;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellApplications;
using Pod.Grpc.Messages.ShellHost;

namespace Pod.Services.Applications
{
    /// <summary>
    /// Service providing functionality to App functionality
    /// </summary>
    public class ShellApplicationService
    {
        private readonly ILogger<ShellApplicationService> _logger;
        private readonly StationResponseHub _stationResponseHub;
        private readonly PodDbContext _podContext;
        private readonly IUniqueAppFactory _uniqueAppFactory;
        public ShellApplicationService(
                ILogger<ShellApplicationService> logger, StationResponseHub stationResponseHub, PodDbContext podContext,
                IUniqueAppFactory uniqueAppFactory)
        {
            _logger = logger;
            _stationResponseHub = stationResponseHub;
            _podContext = podContext;
            _uniqueAppFactory = uniqueAppFactory;
        }


        /// <summary>
        /// Requests the current known app states from the server or just the last synchronization timestamp 
        /// </summary>
        /// <param name="clientCredentials">The credentials for an station</param>
        /// <param name="request">The app information</param>
        /// <returns>The request response</returns>
        public async Task<IResult<SyncResponse>> GetSyncState(ClientCredentials clientCredentials, SyncRequest request)
        {

            //Verify Access
            var result = await clientCredentials.VerifyCredentials<SyncResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ApplicationInvalidConnectionId);
            if(result.HasError()) return result.Add(result);
            try
            {
                var applicationRoot = await GetApplicationRoot(result, clientCredentials.StationId, connectionId, true);
                if(result.HasError()) return result;
                if(request.SendOnlyLastSyncTimestamp)
                {
                    return result.Add(new SyncResponse {LastSyncTimestamp = applicationRoot.LastSyncTimestampUtc.ToDateTimeUtcAsLong()});
                }

                var appStates =
                        await _podContext.LocalApps.
                                          Where(x => x.IsInstalled && x.ApplicationRootId == applicationRoot.Id).
                                          Select(
                                                  x => new AppDataState
                                                       {
                                                               ApplicationId = x.UniqueAppId.ToGuidAsBytes(),
                                                               InstanceVersion = x.InstanceVersion
                                                       }).
                                          AsNoTracking().
                                          ToArrayAsync();
                return result.Add(
                        new SyncResponse
                        {LastSyncTimestamp = applicationRoot.LastSyncTimestampUtc.ToDateTimeUtcAsLong(), AppStates = {appStates}});
            }
            finally
            {
                _stationResponseHub.Publish(
                        new ClientResponse(clientCredentials.StationId, ClientRequestType.AppsSyncRequest, result));
            }
        }

        /// <summary>
        /// Provides synchronization info for the server about all applications that were installed, uninstalled or update
        /// This information are based on the last known state provided by the server to the client
        /// </summary>
        /// <param name="clientCredentials"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IResult<SyncAppsResponse>> SyncStates(ClientCredentials clientCredentials, SyncAppsRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<SyncAppsResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ApplicationInvalidConnectionId);
            if(result.HasError()) return result.Add(result);
            try
            {
                var applicationRoot = await GetApplicationRoot(result, clientCredentials.StationId, connectionId, true);
                if(!result.HasError()) return result;

                var allAppIds = request.Uninstalls.Select(x => x.ApplicationId.ToGuid()).
                                        Concat(request.Installations.Select(x => x.ApplicationId.ToGuid())).
                                        Concat(request.Updates.Select(x => x.ApplicationId.ToGuid()));
                var appStates =
                        await _podContext.LocalApps.
                                          Where(
                                                  x => x.IsInstalled &&
                                                       x.ApplicationRootId == applicationRoot.Id &&
                                                       allAppIds.Contains(x.UniqueAppId)).
                                          ToDictionaryAsync(x => x.UniqueAppId);
                //Apps to Set Uninstalled
                foreach(var uninstallInfo in request.Uninstalls)
                {
                    if(appStates.TryGetValue(uninstallInfo.ApplicationId.ToGuid(), out var appToUninstall))
                    {
                        appToUninstall.SetUninstalled();
                    }
                }

                //Apps to Update
                foreach(var updateInfo in request.Updates)
                {
                    if(appStates.TryGetValue(updateInfo.ApplicationId.ToGuid(), out var appToUpdate))
                    {
                        var createAppUpdateResult = AppUpdate.FromAppUpdateInfo(updateInfo);
                        if(createAppUpdateResult.HasError()) return result.Add(createAppUpdateResult);
                        var appUpdate = createAppUpdateResult.ReturnValue;

                        var createUpdateResult = appToUpdate.Update(LocalAppUpdate.FromIAppUpdateInfo(appUpdate));
                        if(createUpdateResult.HasError()) return result.Add(createUpdateResult);
                    }
                    //App Not Found
                    else
                    {
                        return result.Add(
                                $"Could not find App to Update with AppId {updateInfo.ApplicationId.ToGuid()} in ApplicationRoot with Id {applicationRoot.Id}",
                                UserError.ApplicationNotFound);
                    }
                }

                //Apps To Install
                foreach(var installInfo in request.Installations)
                {
                    var createAppUpdateResult = AppUpdate.FromAppInstalledInfo(installInfo);
                    if(createAppUpdateResult.HasError()) return result.Add(createAppUpdateResult);
                    var appUpdate = createAppUpdateResult.ReturnValue;

                    //ReInstallation
                    if(appStates.TryGetValue(installInfo.ApplicationId.ToGuid(), out var appToInstall))
                    {
                        var createUpdateResult = appToInstall.Reinstalled(LocalAppUpdate.FromIAppUpdateInfo(appUpdate));
                        if(createUpdateResult.HasError()) return result.Add(createUpdateResult);
                    }
                    //New Installation
                    else
                    {
                        var createUniqueAppResult = await _uniqueAppFactory.GetOrCreateAsync(
                                CreatorType.Station,
                                clientCredentials.StationId.ToString(),
                                appUpdate);
                        if(createUniqueAppResult.HasError()) return result.Add(createUniqueAppResult);
                        var createLocalAppResult = LocalApp.CreateLocalApp(
                                applicationRoot,
                                createUniqueAppResult.ReturnValue,
                                appUpdate);
                        if(createLocalAppResult.HasError()) return result.Add(createLocalAppResult);
                        _podContext.Add(createLocalAppResult.ReturnValue);
                    }
                }
                applicationRoot.SetLastSyncTimestamp();
                await _podContext.SaveChangesAsync();

                return result.Add(
                        new SyncAppsResponse
                        {NewSyncTimestamp = applicationRoot.LastSyncTimestampUtc.ToDateTimeUtcAsLong()});
            }
            finally
            {
                _stationResponseHub.Publish(
                        new ClientResponse(clientCredentials.StationId, ClientRequestType.AppsSync, result));
            }
        }

        /// <summary>
        /// Sets an Application to uninstalled
        /// </summary>
        /// <param name="clientCredentials">The credentials for an station</param>
        /// <param name="request">The app information</param>
        /// <returns>The response</returns>
        public async Task<IResult<UpdateResponse>> AppUninstalled(ClientCredentials clientCredentials, AppUninstalledRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<UpdateResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ApplicationInvalidConnectionId);
            if(result.HasError()) return result;
            try
            {
                var applicationRoot = await GetApplicationRoot(
                        result,
                        clientCredentials.StationId,
                        connectionId,
                        false);
                if(!result.HasError()) return result;

                var appToUninstall = await _podContext.LocalApps.Where(
                                                               x => x.UniqueAppId ==
                                                                    request.Uninstalled.ApplicationId.ToGuidNullable() &&
                                                                    x.ApplicationRootId == applicationRoot.Id).
                                                       SingleOrDefaultAsync();

                if(!result.ValueNotNull(
                        appToUninstall,
                        nameof(appToUninstall),
                        UserError.ApplicationNotFound)) return result;

                appToUninstall.SetUninstalled();
                await _podContext.SaveChangesAsync();
                return result.Add(new UpdateResponse());
            }
            finally
            {
                _stationResponseHub.Publish(
                        new ClientResponse(clientCredentials.StationId, ClientRequestType.AppUninstalled, result));
            }
        }

        /// <summary>
        /// Sets an Application to installed
        /// </summary>
        /// <param name="clientCredentials">The credentials for an station</param>
        /// <param name="request">The app information</param>
        /// <returns>The response</returns>
        public async Task<IResult<UpdateResponse>> AppInstalled(ClientCredentials clientCredentials, AppInstalledRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<UpdateResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ApplicationInvalidConnectionId);
            if(result.HasError()) return result;
            try
            {
                var applicationRoot = await GetApplicationRoot(result, clientCredentials.StationId, connectionId, true);
                if(result.HasError()) return result;

                //Evaluate if its reinstalled
                var previousInstalled = await _podContext.LocalApps.Where(
                                                             x => x.UniqueAppId ==
                                                                  request.Installed.ApplicationId.ToGuidNullable() &&
                                                                  x.ApplicationRootId == applicationRoot.Id).
                                                     SingleOrDefaultAsync();

                //CreateUpdate
                var createAppUpdateResult = AppUpdate.FromAppInstalledInfo(request.Installed);
                if(createAppUpdateResult.HasError()) return result.Add(createAppUpdateResult);
                var appUpdate = createAppUpdateResult.ReturnValue;

                //Was already once installed
                if(previousInstalled != null)
                {
                    var createUpdateResult = previousInstalled.Reinstalled(LocalAppUpdate.FromIAppUpdateInfo(appUpdate));
                    if(createUpdateResult.HasError()) return result.Add(createUpdateResult);
                }
                //New Installation
                else
                {
                    var createUniqueAppResult = await _uniqueAppFactory.GetOrCreateAsync(
                            CreatorType.Station,
                            clientCredentials.StationId.ToString(),
                            appUpdate);
                    if(createUniqueAppResult.HasError()) return result.Add(createUniqueAppResult);
                    var createLocalAppResult = LocalApp.CreateLocalApp(
                            applicationRoot,
                            createUniqueAppResult.ReturnValue,
                            appUpdate);
                    if(createLocalAppResult.HasError()) return result.Add(createLocalAppResult);
                    _podContext.Add(createLocalAppResult.ReturnValue);
                }

                await _podContext.SaveChangesAsync();
                return result.Add(new UpdateResponse());
            }
            finally
            {
                _stationResponseHub.Publish(
                        new ClientResponse(clientCredentials.StationId, ClientRequestType.AppInstalled, result));
            }
        }

        /// <summary>
        /// Updates an Application
        /// </summary>
        /// <param name="clientCredentials">The credentials for an station</param>
        /// <param name="request">The app information</param>
        /// <returns>The response</returns>
        public async Task<IResult<UpdateResponse>> AppUpdated(ClientCredentials clientCredentials, AppUpdateRequest request)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<UpdateResponse>(_podContext);
            result.ArgNotNull(request, nameof(request), UserError.ShellClientRequestNull);
            result.ArgTrue(
                    request.ConnectionId.HasConnectionId(out var connectionId),
                    nameof(request.ConnectionId),
                    UserError.ApplicationInvalidConnectionId);
            if(result.HasError()) return result;
            try
            {
                var applicationRoot = await GetApplicationRoot(
                        result,
                        clientCredentials.StationId,
                        connectionId,
                        false);
                if(result.HasError()) return result;

                var appToUpdate = await _podContext.LocalApps.Where(
                                                             x => x.UniqueAppId ==
                                                                  request.Updated.ApplicationId.ToGuid() &&
                                                                  x.ApplicationRootId == applicationRoot.Id).
                                                     SingleOrDefaultAsync();

                //CreateUpdate
                var createAppUpdateResult = AppUpdate.FromAppUpdateInfo(request.Updated);
                if(createAppUpdateResult.HasError()) return result.Add(createAppUpdateResult);
                var appUpdate = createAppUpdateResult.ReturnValue;
                if(!result.ValueNotNull(
                        appToUpdate,
                        nameof(appToUpdate),
                        UserError.ApplicationNotFound)) return result;

                var createUpdateResult = appToUpdate.Update(LocalAppUpdate.FromIAppUpdateInfo(appUpdate));
                if(createUpdateResult.HasError()) return result.Add(createUpdateResult);
                if(createUpdateResult.ReturnValue) await _podContext.SaveChangesAsync();
                return result.Add(new UpdateResponse());
            }
            finally
            {
                _stationResponseHub.Publish(
                        new ClientResponse(clientCredentials.StationId, ClientRequestType.AppUpdated, result));
            }
        }

        private async Task<ApplicationRoot> GetApplicationRoot(
                Result result, Guid stationId, Guid connectionId, bool allowCreate)
        {
            //Get the Station
            var stationDb = await _podContext.Stations.Where(x => x.Id == stationId).
                                              Include(x => x.ConnectionState).
                                              ThenInclude(x => x.DeviceIdentity).
                                              ThenInclude(x => x.ApplicationRoots).
                                              SingleOrDefaultAsync();
            if(!result.ValueNotNull(stationDb, nameof(stationDb), UserError.StationNotFound)) return null;


            if(!result.ValueEqual(
                    stationDb?.ConnectionState?.ConnectionId,
                    nameof(stationDb.ConnectionState.ConnectionId),
                    connectionId,
                    UserError.ApplicationInvalidConnectionId)) return null;

            if(!result.ArgNotNullOrWhitespace(
                    stationDb?.ConnectionState?.DeviceIdentityId,
                    nameof(stationDb.ConnectionState.DeviceIdentityId),
                    UserError.ApplicationDeviceNotFound)) return null;

            //Get the AppRoot for the current Station/Device
            var appRootRequired =
                    stationDb.ConnectionState.DeviceIdentity.ApplicationRoots?.FirstOrDefault(
                            x => x.StationId == stationId);

            if(appRootRequired == null && allowCreate)
            {
                var appRootCreateResult = ApplicationRoot.CreateApplicationRoot(
                        stationDb,
                        stationDb.ConnectionState.DeviceIdentity);
                if(appRootCreateResult.HasError())
                {
                    result.Add(appRootCreateResult);
                    return null;
                }

                appRootRequired = appRootCreateResult.ReturnValue;
                _podContext.Add(appRootRequired);
                await _podContext.SaveChangesAsync();
                return appRootRequired;
            }

            if(!result.RefNotNull(
                    appRootRequired,
                    nameof(stationDb.ConnectionState.DeviceIdentity.ApplicationRoots),
                    UserError.ApplicationRootNotFound)) return null;

            return appRootRequired;
        }
    }
}