#region Licence
/****************************************************************
 *  Filename: CallContextUtilTest.cs
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
using System.Security.Claims;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.AspNetCore.Http;
using Pod.Grpc.Base.Const;
using Pod.Grpc.Base.Server;
using Xunit;

namespace Pod.Grpc.Base.Server.Test
{
    /// <summary>
    /// Behaviour tests for <see cref="CallContextUtil.ToClientCredentials"/>.
    /// In the AspNetCore-hosted world the StationId is now read from the
    /// authenticated <see cref="ClaimsPrincipal"/>, NOT from the metadata
    /// headers. The password continues to come out of the request headers
    /// so legacy <c>VerifyCredentials(...)</c> re-checks keep working.
    /// </summary>
    public class CallContextUtilTest
    {
        private static readonly Guid TestStationId =
            Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        [Fact]
        public void ToClientCredentials_principalHasGrpcStationIdClaim_returnsThatStationId()
        {
            var httpContext = BuildHttpContextWith(
                principalClaims: new[]
                {
                    new Claim(GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId, TestStationId.ToString()),
                },
                headerPassword: "secret");
            var serverCallContext = BuildServerCallContext(httpContext);

            var creds = serverCallContext.ToClientCredentials();

            Assert.Equal(TestStationId, creds.StationId);
            Assert.Equal("secret", creds.Password);
        }

        [Fact]
        public void ToClientCredentials_principalHasOnlyApiKeyStationIdClaim_alsoReturnsStationId()
        {
            // The auth handler sets BOTH GrpcStationId and ApiKeyStationId
            // claims; if for some reason only ApiKeyStationId is present
            // (e.g. a future caller wired an HMAC scheme that fronts gRPC)
            // we still want to honour it.
            var httpContext = BuildHttpContextWith(
                principalClaims: new[]
                {
                    new Claim(GrpcMetadataAuthenticationHandler.ClaimType_ApiKeyStationId, TestStationId.ToString()),
                },
                headerPassword: null);
            var serverCallContext = BuildServerCallContext(httpContext);

            var creds = serverCallContext.ToClientCredentials();

            Assert.Equal(TestStationId, creds.StationId);
        }

        [Fact]
        public void ToClientCredentials_principalHasNoStationIdClaim_throwsRpcExceptionInternal()
        {
            // Reaching the service without a validated station identity =
            // misconfiguration (missing [Authorize] / unwired auth handler),
            // not a bad client. Surface it as Internal rather than
            // Unauthenticated so it shows up in monitoring as a server bug.
            var httpContext = BuildHttpContextWith(
                principalClaims: Array.Empty<Claim>(),
                headerPassword: "secret");
            var serverCallContext = BuildServerCallContext(httpContext);

            var ex = Assert.Throws<RpcException>(() => serverCallContext.ToClientCredentials());
            Assert.Equal(StatusCode.Internal, ex.StatusCode);
            Assert.Contains("validated station identity", ex.Status.Detail);
        }

        [Fact]
        public void ToClientCredentials_principalCarriesEmptyGuidClaim_throwsRpcExceptionInternal()
        {
            // Defensive: a Guid.Empty claim value should be treated the same
            // as no claim at all.
            var httpContext = BuildHttpContextWith(
                principalClaims: new[]
                {
                    new Claim(GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId, Guid.Empty.ToString()),
                },
                headerPassword: "secret");
            var serverCallContext = BuildServerCallContext(httpContext);

            var ex = Assert.Throws<RpcException>(() => serverCallContext.ToClientCredentials());
            Assert.Equal(StatusCode.Internal, ex.StatusCode);
        }

        [Fact]
        public void ToClientCredentials_passwordHeaderMissing_returnsCredentialsWithNullPassword()
        {
            // Auth handler has already validated the password; downstream
            // service code that re-checks via VerifyCredentials(...) is
            // doing defence-in-depth. If the password header somehow isn't
            // there at this point we don't fail — we just return a
            // ClientCredentials whose Password is null and let the
            // re-check (if any) decide.
            var httpContext = BuildHttpContextWith(
                principalClaims: new[]
                {
                    new Claim(GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId, TestStationId.ToString()),
                },
                headerPassword: null);
            var serverCallContext = BuildServerCallContext(httpContext);

            var creds = serverCallContext.ToClientCredentials();

            Assert.Equal(TestStationId, creds.StationId);
            Assert.Null(creds.Password);
        }

        // ---------------------------------------------------------------- helpers

        private static HttpContext BuildHttpContextWith(Claim[] principalClaims, string headerPassword)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(principalClaims, GrpcMetadataAuthenticationHandler.SchemeName));
            if (headerPassword != null)
            {
                httpContext.Request.Headers[AuthConstants.ShellClientPasswordKey] = headerPassword;
            }
            return httpContext;
        }

        private static ServerCallContext BuildServerCallContext(HttpContext httpContext)
        {
            // TestServerCallContext.Create uses the gRPC test helpers; we
            // attach the HttpContext via UserState so context.GetHttpContext()
            // returns our DefaultHttpContext instance — which is the path
            // CallContextUtil uses to read both User and Request.Headers.
            var serverCallContext = TestServerCallContext.Create(
                method: "test",
                host: null,
                deadline: DateTime.UtcNow.AddSeconds(5),
                requestHeaders: new Metadata(),
                cancellationToken: default,
                peer: "ip:127.0.0.1",
                authContext: null,
                contextPropagationToken: null,
                writeHeadersFunc: _ => System.Threading.Tasks.Task.CompletedTask,
                writeOptionsGetter: () => new WriteOptions(),
                writeOptionsSetter: _ => { });

            // Grpc.AspNetCore-style: GetHttpContext() looks for "__HttpContext"
            // in UserState. Setting it directly is the supported test hook.
            serverCallContext.UserState["__HttpContext"] = httpContext;
            return serverCallContext;
        }
    }
}
