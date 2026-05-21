#region Licence
/****************************************************************
 *  Filename: GrpcServiceMappingTests.cs
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
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Base.Server;
using Pod.Grpc.ConnectHost;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Web.Center.Test.Fixtures;
using Xunit;

namespace Pod.Web.Center.Test.Integration
{
    /// <summary>
    /// Confirms <c>endpoints.MapGrpcService&lt;ConnectHostServiceGrpc&gt;()</c> from
    /// Startup.Configure is actually serving on the same Kestrel pipeline as REST.
    /// We hit the unauthenticated end of the kiosk's wire contract through the real
    /// Grpc.Net.Client - same generated stub the kiosk uses - to prove the dispatch
    /// works end-to-end. Without valid station credentials in the call metadata,
    /// <see cref="GrpcMetadataAuthenticationHandler"/> rejects the call with
    /// <c>StatusCode.Unauthenticated</c>; the assertion below pins that exact failure
    /// mode, which is itself a positive signal the pipeline was reached.
    /// </summary>
    public class GrpcServiceMappingTests : IClassFixture<PodWebApplicationFactory>
    {
        private readonly PodWebApplicationFactory _factory;
        public GrpcServiceMappingTests(PodWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task ConnectHost_AnonymousCall_RejectedWithUnauthenticated()
        {
            // Reuse the in-memory HTTP handler the WebApplicationFactory creates for
            // HTTP/2 multiplexed gRPC traffic. WebApplicationFactory's test server
            // exposes Server.CreateHandler() which dispatches in-process, no real
            // socket. GrpcChannel.ForAddress takes a base URL; the handler's job is
            // to route every request back into the test pipeline.
            var handler = _factory.Server.CreateHandler();
            var channel = GrpcChannel.ForAddress(_factory.Server.BaseAddress, new GrpcChannelOptions
            {
                HttpHandler = handler,
            });
            var client = new ConnectHostServiceGrpc.ConnectHostServiceGrpcClient(channel);

            Func<Task> act = async () => await client.GetHostAsync(new ShellServerRequest
            {
                IdentityId = "smoke-test-device",
                MaxInterfaceVersion = 0,
            });

            var ex = await act.Should().ThrowAsync<RpcException>();
            // The handler returns Unauthenticated when no (identity, password) metadata is present.
            ex.Which.StatusCode.Should().Be(StatusCode.Unauthenticated);
        }

        [Fact]
        public async Task ConnectHost_ValidCredentialsViaStubVerifier_ReachesService()
        {
            // Spin a fixture that swaps the production credential verifier for a stub
            // that accepts the known-good password "smoke-pass". This lets the test
            // exercise the full auth -> service -> business-logic path without seeding
            // a Station + Identity user in the InMemory DB. The negative path (above)
            // already proves the auth handler is wired; this proves the dispatch
            // reaches ConnectHostServiceGrpc once auth succeeds.
            using var factory = new StubVerifierFactory();
            var handler = factory.Server.CreateHandler();
            var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
            {
                HttpHandler = handler,
            });
            var client = new ConnectHostServiceGrpc.ConnectHostServiceGrpcClient(channel);

            var metadata = new Metadata
            {
                { "identity", Guid.NewGuid().ToString() },
                { "password", "smoke-pass" },
            };

            Func<Task> act = async () => await client.GetHostAsync(
                new ShellServerRequest { IdentityId = "smoke-device", MaxInterfaceVersion = 0 },
                headers: metadata);

            // ConnectHostServiceGrpc.GetHost will run, ConnectService.RequestServer
            // will be invoked, and likely fail with a NotFound/business error because
            // we didn't seed a matching Station. The important assertion is that we
            // got PAST auth: any RpcException whose StatusCode is *not* Unauthenticated
            // proves the auth gate let us through and the service method was reached.
            // (A plain successful call would require seeding the full Station + ShellServer
            // graph, which is the EnvVarBindingTests/StartupSmokeTests focus, not this one.)
            try
            {
                await act();
                // No exception thrown — the service returned a successful response.
                // That's also a valid "we got through auth" signal.
            }
            catch (RpcException rpc)
            {
                rpc.StatusCode.Should().NotBe(StatusCode.Unauthenticated,
                    "auth was supposed to succeed with the stub verifier");
            }
        }
    }

    /// <summary>
    /// Variant of <see cref="PodWebApplicationFactory"/> that replaces the production
    /// <see cref="IGrpcStationCredentialVerifier"/> with a stub accepting password
    /// "smoke-pass" for any station id. Lets the gRPC dispatch test exercise the
    /// auth -> service path without seeding a full Station + Identity user.
    /// </summary>
    file sealed class StubVerifierFactory : PodWebApplicationFactory
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                var existing = services.Where(d => d.ServiceType == typeof(IGrpcStationCredentialVerifier)).ToList();
                foreach (var d in existing) services.Remove(d);
                services.AddScoped<IGrpcStationCredentialVerifier, AcceptSmokePassVerifier>();
            });
        }

        private sealed class AcceptSmokePassVerifier : IGrpcStationCredentialVerifier
        {
            public Task<Result> VerifyAsync(ClientCredentials credentials)
            {
                var result = new Result();
                if (credentials.Password != "smoke-pass")
                {
                    result.Add("credential mismatch", UserError.InternalError);
                }
                return Task.FromResult(result);
            }
        }
    }
}
