#region Licence
/****************************************************************
 *  Filename: ShellApplicationServiceGrpc.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pod.Grpc.Base.Server;
using Pod.Grpc.Messages.ShellApplications;
using Pod.Grpc.ShellApplications;
using Pod.Grpc.Utilities;
using Pod.Services.Applications;
using Pod.Services.ShellHost;

namespace Pod.Grpc.ShellHost.Server.Services
{
    /// <summary>
    /// Grpc Server side implementation for Services provided to Shell Clients related to applications
    /// </summary>
    /// <remarks>
    /// Authorisation runs through <see cref="GrpcMetadataAuthenticationHandler"/>
    /// before any method body executes. The PBKDF2 password verification that
    /// used to live inside each method via <c>credentials.VerifyCredentials(...)</c>
    /// is now done once at the auth layer; method bodies still call
    /// <see cref="CallContextUtil.ToClientCredentials"/> to obtain the StationId
    /// (now sourced from the authenticated principal).
    /// </remarks>
    [Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]
    public class ShellApplicationServiceGrpc : ShellApplications.ShellApplicationServiceGrpc.ShellApplicationServiceGrpcBase
    {
        private readonly ILogger<ShellHostServiceGrpc> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ShellApplicationServiceGrpc(ILogger<ShellHostServiceGrpc> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Call from Client to signal an that an application was uninstalled
        /// </summary>
        /// <param name="request">Data about app uninstall</param>
        /// <param name="context">grpc call context</param>
        /// <returns>request result</returns>
        public override async Task<UpdateResponse> SendAppUninstalled(
                AppUninstalledRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var appService = scope.ServiceProvider.GetRequiredService<ShellApplicationService>();
                var result = await appService.AppUninstalled(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        /// <summary>
        /// Call from Client about an updated application (e.g. name or enabled state)
        /// </summary>
        /// <param name="request">The application data</param>
        /// <param name="context">grpc call context</param>
        /// <returns>request result</returns>
        public override async Task<UpdateResponse> SendAppUpdate(AppUpdateRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var appService = scope.ServiceProvider.GetRequiredService<ShellApplicationService>();
                var result = await appService.AppUpdated(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        /// <summary>
        /// Call from Client about an installation of an application
        /// </summary>
        /// <param name="request">The application data</param>
        /// <param name="context">grpc call context</param>
        /// <returns>request result</returns>
        public override async Task<UpdateResponse> SendAppInstalled(
                AppInstalledRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var appService = scope.ServiceProvider.GetRequiredService<ShellApplicationService>();
                var result = await appService.AppInstalled(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        /// <summary>
        /// Call from client to request an synchronization cycle for applications
        /// Should be called when a client comes online or when some installation/uninstall/update
        /// occured during the client was not connected
        /// </summary>
        /// <param name="request">Request that allows to specify the response required
        /// A full app status known to the server about the apps or a server based timestamp of the last full sync</param>
        /// <param name="context">grpc call context</param>
        /// <returns>request result</returns>
        public override async Task<SyncResponse> GetSyncAppStates(SyncRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var appService = scope.ServiceProvider.GetRequiredService<ShellApplicationService>();
                var result = await appService.GetSyncState(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }

        /// <summary>
        /// Call from client in the synchronization cycle for applications
        /// This call provides all changes that occured on the client since the last sync
        /// This information is calculate from the last sync timestamp or the full app list received from the request
        /// </summary>
        /// <param name="request">Provides all apps that needs to be updated on the server</param>
        /// <param name="context">grpc call context</param>
        /// <returns>request result</returns>
        public override async Task<SyncAppsResponse> SendSyncAppStates(
                SyncAppsRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var appService = scope.ServiceProvider.GetRequiredService<ShellApplicationService>();
                var result = await appService.SyncStates(credentials, request);
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }
    }
}