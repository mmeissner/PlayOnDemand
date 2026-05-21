#region Licence
/****************************************************************
 *  Filename: ConnectHostServiceGrpc.cs
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
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Pod.Grpc.Base.Server;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Grpc.Utilities;
using Pod.Services.ConnectHost;

namespace Pod.Grpc.ConnectHost.Server.Services
{
    /// <summary>
    /// Service for Clients to receive server where they can connect
    /// </summary>
    /// <remarks>
    /// Authorisation runs through <see cref="GrpcMetadataAuthenticationHandler"/>
    /// before any method body executes — see <c>docs/architecture/auth.md</c>
    /// scheme #3. The PBKDF2 password verification that used to live inside
    /// each method via <c>credentials.VerifyCredentials(...)</c> is now done
    /// once at the auth layer; <see cref="CallContextUtil.ToClientCredentials"/>
    /// returns the StationId from the authenticated principal.
    /// </remarks>
    [Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]
    public class ConnectHostServiceGrpc : ConnectHost.ConnectHostServiceGrpc.ConnectHostServiceGrpcBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ConnectHostServiceGrpc(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Allows to Request a Connection to a Shell Server and must be called to receive a ConnectionId
        /// to be able to connect to a shell server
        /// </summary>
        /// <param name="request">The client request</param>
        /// <param name="context">grpc call context</param>
        /// <returns></returns>
        public override async Task<ShellServerResponse> GetHost(ShellServerRequest request, ServerCallContext context)
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var credentials = context.ToClientCredentials();
                var connectService = scope.ServiceProvider.GetRequiredService<ConnectService>();
                var result = await connectService.RequestServer(
                        credentials,
                        request.IdentityId,
                        request.MaxInterfaceVersion,
                        request.ReconnectConnectionId.ToGuidNullable());
                if(result.IsSuccess()) return result.ReturnValue;
                throw result.ToException();
            }
        }
    }
}