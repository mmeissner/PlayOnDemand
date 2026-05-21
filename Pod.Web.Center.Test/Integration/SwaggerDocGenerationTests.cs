#region Licence
/****************************************************************
 *  Filename: SwaggerDocGenerationTests.cs
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
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Pod.Web.Center.Test.Fixtures;
using Xunit;

namespace Pod.Web.Center.Test.Integration
{
    /// <summary>
    /// Pins the Swashbuckle 10 / Microsoft.OpenApi 2.x doc-generation pipeline:
    /// all custom filters (LowercaseDocumentFilter, EnumDocumentFilter,
    /// OnlyApiResponseAndRequestFilterOrdered, AuthorizationOperationFilter,
    /// AppendAuthorizeToSummaryOperationFilter) have to run without throwing,
    /// and the resulting JSON must be parseable + carry the openapi:"3.0.x" header,
    /// the v1 and v1_internal docs, and the two security schemes we registered
    /// (JWT + amx).
    ///
    /// Without these assertions a filter regression would silently 500 on every
    /// /swagger/v1/swagger.json hit, breaking the operator UI's API discovery.
    /// </summary>
    public class SwaggerDocGenerationTests : IClassFixture<PodWebApplicationFactory>
    {
        private readonly PodWebApplicationFactory _factory;
        public SwaggerDocGenerationTests(PodWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task SwaggerJson_V1_GeneratesAndParses()
        {
            using var client = _factory.CreateClient();
            using var resp = await client.GetAsync("/swagger/v1/swagger.json");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
            // The doc must parse as valid JSON.
            using var doc = JsonDocument.Parse(body);

            // OpenAPI 3 envelope.
            doc.RootElement.TryGetProperty("openapi", out var openapi).Should().BeTrue();
            openapi.GetString().Should().StartWith("3.");

            // Title set in AddSwaggerGen (SwaggerDoc("v1", new OpenApiInfo { Title = "Leap Play", ... })).
            doc.RootElement.TryGetProperty("info", out var info).Should().BeTrue();
            info.GetProperty("title").GetString().Should().Be("Leap Play");

            // Security schemes registered via AddSecurityDefinition (Bearer + amx).
            doc.RootElement.TryGetProperty("components", out var components).Should().BeTrue();
            components.TryGetProperty("securitySchemes", out var schemes).Should().BeTrue();
            schemes.TryGetProperty("Bearer", out _).Should().BeTrue("JWT scheme registered as 'Bearer'");
            schemes.TryGetProperty("amx", out _).Should().BeTrue("REST station HMAC scheme registered as 'amx'");
        }

        [Fact]
        public async Task SwaggerJson_V1Internal_GeneratesAndParses()
        {
            using var client = _factory.CreateClient();
            using var resp = await client.GetAsync("/swagger/v1_internal/swagger.json");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            doc.RootElement.GetProperty("info").GetProperty("title").GetString().Should().Be("Leap Play - Internal");
        }
    }
}
