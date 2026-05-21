#region Licence
/****************************************************************
 *  Filename: CallContextUtil.cs
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
using System.Security.Claims;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Pod.Data.Infrastructure;
using Pod.Grpc.Base.Const;

namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// Helper to extract Client Credentials from a gRPC call.
    ///
    /// In the AspNetCore-hosted world the call comes through after
    /// <see cref="GrpcMetadataAuthenticationHandler"/> has already verified
    /// the station's password and built a <see cref="ClaimsPrincipal"/> with
    /// the StationId claim. This helper now READS that claim instead of
    /// re-parsing the metadata headers itself, so service implementations
    /// keep the same one-liner style:
    ///
    /// <code>
    ///     var credentials = context.ToClientCredentials();
    /// </code>
    ///
    /// The returned <see cref="ClientCredentials"/> still carries the
    /// password (pulled out of the request headers) so legacy service code
    /// that calls <c>credentials.VerifyCredentials(...)</c> for additional
    /// in-method checks (e.g. <see cref="ShellHostServiceGrpc.GetNotifications"/>'s
    /// connection-id binding) keeps round-tripping the same data shape.
    /// Re-verification is redundant under [Authorize], but it stays safe.
    /// </summary>
    public static class CallContextUtil
    {
        /// <summary>
        /// Extracts the authenticated <see cref="ClientCredentials"/> for
        /// the calling station from a gRPC <see cref="ServerCallContext"/>.
        ///
        /// Pre-condition: the call has been routed through
        /// <see cref="GrpcMetadataAuthenticationHandler"/> (i.e. the service
        /// is decorated with <c>[Authorize(AuthenticationSchemes =
        /// GrpcMetadataAuthenticationHandler.SchemeName)]</c>). If the
        /// principal is missing the expected StationId claim — which would
        /// indicate the auth handler is mis-wired or the caller bypassed
        /// it — this method throws <c>RpcException(Internal, ...)</c>
        /// rather than silently returning a default credential.
        /// </summary>
        public static ClientCredentials ToClientCredentials(this ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var stationId = ReadStationIdClaim(httpContext?.User);

            if (stationId == Guid.Empty)
            {
                throw new RpcException(new Status(
                    StatusCode.Internal,
                    "gRPC call reached the service without a validated station identity. " +
                    "Is the service missing [Authorize(AuthenticationSchemes = " +
                    "GrpcMetadataAuthenticationHandler.SchemeName)] or is the auth " +
                    "handler not wired in Startup?"));
            }

            // Password is still pulled from headers so existing service code
            // that calls VerifyCredentials for defence-in-depth keeps working.
            var password = ReadHeaderValue(context, AuthConstants.ShellClientPasswordKey);

            return new ClientCredentials
            {
                StationId = stationId,
                Password = password,
            };
        }

        private static Guid ReadStationIdClaim(ClaimsPrincipal principal)
        {
            if (principal == null) return Guid.Empty;

            // Either claim is acceptable — the auth handler sets both.
            var raw = principal.FindFirst(GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId)?.Value
                   ?? principal.FindFirst(GrpcMetadataAuthenticationHandler.ClaimType_ApiKeyStationId)?.Value
                   ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }

        private static string ReadHeaderValue(ServerCallContext context, string key)
        {
            // Prefer the Kestrel HttpContext (cheapest path, case-insensitive
            // lookup); fall back to gRPC's RequestHeaders metadata if the
            // HttpContext isn't available (e.g. test doubles).
            var httpContext = context.GetHttpContext();
            if (httpContext != null && httpContext.Request.Headers.TryGetValue(key, out var values))
            {
                return values.FirstOrDefault();
            }

            var entry = context.RequestHeaders?
                .FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
            return entry?.Value;
        }
    }
}
