#region Licence
/****************************************************************
 *  Filename: GrpcMetadataAuthenticationHandler.cs
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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pod.Data.Infrastructure;
using Pod.Grpc.Base.Const;

namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// ASP.NET Core authentication handler for the gRPC station scheme
    /// (kiosks identifying themselves with the <c>identity</c> + <c>password</c>
    /// metadata headers, see <c>docs/architecture/auth.md</c> Scheme #3).
    ///
    /// On a successful authentication the resulting <see cref="ClaimsPrincipal"/>
    /// carries:
    ///   • <see cref="ClaimTypes.NameIdentifier"/>                    = StationId (Guid)
    ///   • <see cref="ClaimTypes.Name"/>                              = StationId (Guid)
    ///   • <see cref="ClaimType_GrpcStationId"/>                       = StationId (Guid)
    ///   • <see cref="ClaimType_ApiKeyStationId"/>                     = StationId (Guid)
    ///     so that downstream code (<c>StationController</c> etc.) which already
    ///     resolves the calling station via <c>PodClaimsTypes.ApiKeyStationId</c>
    ///     keeps working unchanged when called via gRPC.
    ///
    /// On any failure path <see cref="HandleAuthenticateAsync"/> returns
    /// <see cref="AuthenticateResult.Fail(string)"/>; the gRPC pipeline then
    /// translates that into <c>Status(Unauthenticated, ...)</c> on the wire.
    /// </summary>
    public sealed class GrpcMetadataAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// The name of this authentication scheme. Wire it in
        /// <c>Pod.Web.Center/Startup.cs</c> via
        /// <c>AddAuthentication().AddScheme&lt;..., GrpcMetadataAuthenticationHandler&gt;(SchemeName, ...)</c>
        /// (or the convenience <c>AddGrpcStationMetadata()</c> extension)
        /// and reference it from service classes via
        /// <c>[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]</c>.
        /// </summary>
        public const string SchemeName = "grpc-station";

        /// <summary>
        /// Claim type carrying the StationId for callers authenticated by
        /// this handler. Distinct from the REST <c>amx</c> scheme's claim
        /// (<c>ApiKeyStationId</c>) so service code can tell which channel
        /// the caller arrived on.
        /// </summary>
        public const string ClaimType_GrpcStationId = "GrpcStationId";

        /// <summary>
        /// Claim type also set on the principal so that controllers / helpers
        /// already keyed on <c>PodClaimsTypes.ApiKeyStationId</c> work
        /// unchanged when called via the gRPC channel. Mirrored intentionally —
        /// kept as a string literal here to avoid coupling Pod.Grpc.Base.Server
        /// to Pod.Services.Authentication.PodClaimsTypes.
        /// </summary>
        public const string ClaimType_ApiKeyStationId = "ApiKeyStationId";

        private readonly IGrpcStationCredentialVerifier _verifier;

        public GrpcMetadataAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IGrpcStationCredentialVerifier verifier)
            : base(options, logger, encoder)
        {
            _verifier = verifier;
        }

        /// <inheritdoc />
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // gRPC normalises HTTP/2 header names to lowercase; AuthConstants
            // are defined lowercase too. ASP.NET Core's IHeaderDictionary is
            // case-insensitive, so this works either way.
            var identityHeader = ReadSingleHeader(AuthConstants.ShellClientIdentityKey);
            var passwordHeader = ReadSingleHeader(AuthConstants.ShellClientPasswordKey);

            if (string.IsNullOrWhiteSpace(identityHeader) ||
                string.IsNullOrWhiteSpace(passwordHeader))
            {
                // No headers at all -> NoResult so other schemes (if any)
                // can have a go. There aren't any other gRPC schemes today
                // but this stays the polite contract.
                return AuthenticateResult.NoResult();
            }

            if (!Guid.TryParse(identityHeader, out var stationId) || stationId == Guid.Empty)
            {
                return AuthenticateResult.Fail("Malformed station identity header.");
            }

            var credentials = new ClientCredentials
            {
                StationId = stationId,
                Password = passwordHeader,
            };

            var verifyResult = await _verifier.VerifyAsync(credentials).ConfigureAwait(false);
            if (verifyResult == null || verifyResult.HasError())
            {
                return AuthenticateResult.Fail("Invalid station credentials.");
            }

            var stationIdString = stationId.ToString();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, stationIdString),
                new Claim(ClaimTypes.Name, stationIdString),
                new Claim(ClaimType_GrpcStationId, stationIdString),
                new Claim(ClaimType_ApiKeyStationId, stationIdString),
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return AuthenticateResult.Success(ticket);
        }

        private string ReadSingleHeader(string headerName)
        {
            // gRPC metadata translates to repeated HTTP/2 headers; if a client
            // (incorrectly) sent more than one we take the first non-empty.
            return Context.Request.Headers
                .TryGetValue(headerName, out var values)
                ? values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))
                : null;
        }
    }
}
