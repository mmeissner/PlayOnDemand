#region Licence
/****************************************************************
 *  Filename: GrpcMetadataAuthenticationHandlerTest.cs
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
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Base.Const;
using Pod.Grpc.Base.Server;
using Xunit;

namespace Pod.Grpc.Base.Server.Test
{
    /// <summary>
    /// Unit tests for <see cref="GrpcMetadataAuthenticationHandler"/>. The
    /// handler is exercised against an in-process <see cref="HttpContext"/>
    /// (DefaultHttpContext) with a stub <see cref="IGrpcStationCredentialVerifier"/>;
    /// no HTTP listener and no database are involved.
    /// </summary>
    public class GrpcMetadataAuthenticationHandlerTest
    {
        private static readonly Guid TestStationId =
            Guid.Parse("11111111-2222-3333-4444-555555555555");
        private const string TestPassword = "correct horse battery staple";

        // ---------------------------------------------------------------- Success

        [Fact]
        public async Task HandleAuthenticateAsync_validHeaders_returnsSuccessWithExpectedClaims()
        {
            var verifier = new StubVerifier(_ => new Result());
            var sut = await CreateHandlerAsync(
                verifier,
                identity: TestStationId.ToString(),
                password: TestPassword);

            var result = await sut.AuthenticateAsync();

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Principal);

            var claims = result.Principal.Claims;
            AssertClaim(claims, ClaimTypes.NameIdentifier, TestStationId.ToString());
            AssertClaim(claims, ClaimTypes.Name, TestStationId.ToString());
            AssertClaim(claims, GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId, TestStationId.ToString());
            AssertClaim(claims, GrpcMetadataAuthenticationHandler.ClaimType_ApiKeyStationId, TestStationId.ToString());

            // Ticket is stamped with the scheme name so [Authorize(AuthenticationSchemes=...)] matches.
            Assert.Equal(GrpcMetadataAuthenticationHandler.SchemeName, result.Ticket.AuthenticationScheme);

            // The verifier saw exactly the credentials carried on the wire.
            Assert.Equal(TestStationId, verifier.LastCredentials.StationId);
            Assert.Equal(TestPassword, verifier.LastCredentials.Password);
        }

        // ---------------------------------------------------------------- NoResult

        [Fact]
        public async Task HandleAuthenticateAsync_noHeadersAtAll_returnsNoResult()
        {
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: null, password: null);

            var result = await sut.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.True(result.None, "missing-headers should be NoResult so other auth handlers can attempt the call");
            Assert.Null(result.Failure);
            Assert.Equal(0, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_missingPasswordHeader_returnsNoResult()
        {
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: TestStationId.ToString(), password: null);

            var result = await sut.AuthenticateAsync();

            Assert.True(result.None);
            Assert.Equal(0, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_missingIdentityHeader_returnsNoResult()
        {
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: null, password: TestPassword);

            var result = await sut.AuthenticateAsync();

            Assert.True(result.None);
            Assert.Equal(0, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_whitespaceHeaders_returnsNoResult()
        {
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: "   ", password: "\t");

            var result = await sut.AuthenticateAsync();

            Assert.True(result.None);
            Assert.Equal(0, verifier.CallCount);
        }

        // ---------------------------------------------------------------- Fail

        [Fact]
        public async Task HandleAuthenticateAsync_malformedIdentityHeader_failsWithoutCallingVerifier()
        {
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: "not-a-guid", password: TestPassword);

            var result = await sut.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Contains("Malformed station identity", result.Failure.Message);
            Assert.Equal(0, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_emptyGuidIdentity_failsWithoutCallingVerifier()
        {
            // Guid.Empty parses successfully but is treated as malformed by
            // the handler — every legitimate StationId is non-empty.
            var verifier = new StubVerifier(_ => throw new InvalidOperationException("verifier should not be called"));
            var sut = await CreateHandlerAsync(verifier, identity: Guid.Empty.ToString(), password: TestPassword);

            var result = await sut.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Contains("Malformed station identity", result.Failure.Message);
            Assert.Equal(0, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_verifierReturnsErrorResult_failsAuthentication()
        {
            var verifier = new StubVerifier(_ =>
            {
                var r = new Result();
                r.Add("bad password", UserError.ShellClientInvalidPassword);
                return r;
            });
            var sut = await CreateHandlerAsync(verifier, identity: TestStationId.ToString(), password: "wrong");

            var result = await sut.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Contains("Invalid station credentials", result.Failure.Message);
            Assert.Equal(1, verifier.CallCount);
        }

        [Fact]
        public async Task HandleAuthenticateAsync_verifierReturnsNull_failsAuthentication()
        {
            // Defensive: even though the contract says non-null, a buggy
            // verifier must not crash the handler.
            var verifier = new StubVerifier(_ => null);
            var sut = await CreateHandlerAsync(verifier, identity: TestStationId.ToString(), password: TestPassword);

            var result = await sut.AuthenticateAsync();

            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
            Assert.Contains("Invalid station credentials", result.Failure.Message);
        }

        // ---------------------------------------------------------------- Header conventions

        [Fact]
        public async Task HandleAuthenticateAsync_usesLowercaseAuthConstantsHeaders()
        {
            // Sanity check that the auth handler is reading the same header
            // names that AuthConstants documents, lowercase per gRPC's HTTP/2
            // header normalisation rules.
            Assert.Equal("identity", AuthConstants.ShellClientIdentityKey);
            Assert.Equal("password", AuthConstants.ShellClientPasswordKey);

            var verifier = new StubVerifier(_ => new Result());
            var sut = await CreateHandlerAsync(verifier, identity: TestStationId.ToString(), password: TestPassword);

            var result = await sut.AuthenticateAsync();
            Assert.True(result.Succeeded);
        }

        // ---------------------------------------------------------------- Test helpers

        private static async Task<GrpcMetadataAuthenticationHandler> CreateHandlerAsync(
            IGrpcStationCredentialVerifier verifier,
            string identity,
            string password)
        {
            var optionsMonitor = new TestOptionsMonitor();
            var sut = new GrpcMetadataAuthenticationHandler(
                optionsMonitor,
                NullLoggerFactory.Instance,
                UrlEncoder.Default,
                verifier);

            var scheme = new AuthenticationScheme(
                GrpcMetadataAuthenticationHandler.SchemeName,
                displayName: null,
                handlerType: typeof(GrpcMetadataAuthenticationHandler));

            var httpContext = new DefaultHttpContext();
            if (identity != null)
            {
                httpContext.Request.Headers[AuthConstants.ShellClientIdentityKey] = identity;
            }
            if (password != null)
            {
                httpContext.Request.Headers[AuthConstants.ShellClientPasswordKey] = password;
            }

            await sut.InitializeAsync(scheme, httpContext);
            return sut;
        }

        private static void AssertClaim(IEnumerable<Claim> claims, string type, string expectedValue)
        {
            foreach (var c in claims)
            {
                if (c.Type == type)
                {
                    Assert.Equal(expectedValue, c.Value);
                    return;
                }
            }
            Assert.Fail($"No claim of type '{type}' on principal.");
        }

        private sealed class StubVerifier : IGrpcStationCredentialVerifier
        {
            private readonly Func<ClientCredentials, Result> _verify;

            public StubVerifier(Func<ClientCredentials, Result> verify)
            {
                _verify = verify;
            }

            public ClientCredentials LastCredentials { get; private set; }
            public int CallCount { get; private set; }

            public Task<Result> VerifyAsync(ClientCredentials credentials)
            {
                LastCredentials = credentials;
                CallCount++;
                return Task.FromResult(_verify(credentials));
            }
        }

        private sealed class TestOptionsMonitor : IOptionsMonitor<AuthenticationSchemeOptions>
        {
            public AuthenticationSchemeOptions CurrentValue { get; } = new AuthenticationSchemeOptions();

            public AuthenticationSchemeOptions Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<AuthenticationSchemeOptions, string> listener) => new Noop();

            private sealed class Noop : IDisposable
            {
                public void Dispose() { }
            }
        }
    }
}
