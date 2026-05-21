#region Licence
/****************************************************************
 *  Filename: GrpcMetadataAuthenticationExtensions.cs
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// Convenience extensions to register the gRPC station-credential
    /// authentication scheme on an <see cref="AuthenticationBuilder"/>.
    ///
    /// Pod.Web.Center wires it like:
    /// <code>
    ///     services.AddAuthentication()
    ///             .AddJwtBearer(...)
    ///             .AddScheme&lt;ApiKeySecretHandler&gt;("amx", _ =&gt; { })
    ///             .AddGrpcStationMetadata();
    /// </code>
    ///
    /// The default <see cref="IGrpcStationCredentialVerifier"/> binding
    /// (<see cref="DefaultGrpcStationCredentialVerifier"/>) requires the
    /// caller's DI to also have <c>PodDbContext</c> registered (which
    /// Pod.Web.Center already does). Tests / specialised hosts that want
    /// to substitute a fake verifier should register their own
    /// <see cref="IGrpcStationCredentialVerifier"/> *before* calling this
    /// extension — <see cref="ServiceCollectionDescriptorExtensions.TryAddScoped"/>
    /// here will see the existing registration and skip the default.
    /// </summary>
    public static class GrpcMetadataAuthenticationExtensions
    {
        /// <summary>
        /// Registers the <see cref="GrpcMetadataAuthenticationHandler"/>
        /// scheme. The scheme name is fixed to
        /// <see cref="GrpcMetadataAuthenticationHandler.SchemeName"/> so
        /// service-class <c>[Authorize]</c> attributes can refer to the
        /// constant directly.
        /// </summary>
        public static AuthenticationBuilder AddGrpcStationMetadata(
            this AuthenticationBuilder builder,
            Action<AuthenticationSchemeOptions> configure = null)
        {
            builder.Services.TryAddScoped<IGrpcStationCredentialVerifier, DefaultGrpcStationCredentialVerifier>();
            return builder.AddScheme<AuthenticationSchemeOptions, GrpcMetadataAuthenticationHandler>(
                GrpcMetadataAuthenticationHandler.SchemeName,
                displayName: null,
                configureOptions: configure ?? (_ => { }));
        }
    }
}
