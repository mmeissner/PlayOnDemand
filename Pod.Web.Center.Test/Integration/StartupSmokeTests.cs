#region Licence
/****************************************************************
 *  Filename: StartupSmokeTests.cs
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
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Pod.Web.Center.Test.Fixtures;
using Xunit;

namespace Pod.Web.Center.Test.Integration
{
    /// <summary>
    /// Boots the entire Pod.Web.Center pipeline in-process and hits a few canary endpoints.
    /// Confirms that the net10 host wiring (Startup + endpoint routing + gRPC mapping +
    /// auth chain + Swashbuckle) doesn't blow up at start, and that anonymous endpoints
    /// route as expected.
    /// </summary>
    public class StartupSmokeTests : IClassFixture<PodWebApplicationFactory>
    {
        private readonly PodWebApplicationFactory _factory;

        public StartupSmokeTests(PodWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_EmptyBody_RouteResolvesWithoutAuth()
        {
            using var client = _factory.CreateClient();
            using var resp = await client.PostAsync("/api/v1/auth/login", new System.Net.Http.StringContent(""));
            // Endpoint must exist (not 404) and not require auth (not 401).
            // Anything else (400, 415, 500 from validation/binding) confirms the pipeline reached it.
            resp.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
            resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AuthorizedEndpoint_AnonymousRequest_Returns401()
        {
            using var client = _factory.CreateClient();
            using var resp = await client.GetAsync("/api/v1/stations");
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Health_AnonymousRequest_Returns200()
        {
            using var client = _factory.CreateClient();
            using var resp = await client.GetAsync("/health");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
