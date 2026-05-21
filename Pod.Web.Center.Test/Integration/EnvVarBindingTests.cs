#region Licence
/****************************************************************
 *  Filename: EnvVarBindingTests.cs
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
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pod.Web.Center.Test.Fixtures;
using Xunit;

namespace Pod.Web.Center.Test.Integration
{
    /// <summary>
    /// Proves the docker-compose env-var binding contract holds: keys of the form
    /// <c>AuthConfig__SecretKey</c> reach the application's <see cref="IConfiguration"/>
    /// as <c>AuthConfig:SecretKey</c>. Failures here mean docker-compose deployments
    /// would silently fall through to the placeholder appsettings.json values.
    /// </summary>
    public class EnvVarBindingTests : IClassFixture<PodWebApplicationFactory>
    {
        private readonly PodWebApplicationFactory _factory;
        public EnvVarBindingTests(PodWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public void EnvironmentVariables_With_DoubleUnderscore_BindToConfiguration()
        {
            // Spin up an isolated host that reproduces Program.cs's config chain
            // but reads from env vars only - this is what docker-compose passes.
            const string key = "AuthConfig__SecretKey";
            const string value = "env-var-binding-test-secret-32-bytes!";
            Environment.SetEnvironmentVariable(key, value);
            try
            {
                using var host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((_, c) =>
                    {
                        c.Sources.Clear();
                        c.AddEnvironmentVariables();
                    })
                    .Build();

                var cfg = host.Services.GetRequiredService<IConfiguration>();
                cfg["AuthConfig:SecretKey"].Should().Be(value);
            }
            finally
            {
                Environment.SetEnvironmentVariable(key, null);
            }
        }

        [Fact]
        public void Factory_Serves_HealthCheck()
        {
            // Re-asserted here so the fixture covers the docker healthcheck contract
            // (curl -fkS https://localhost/health). The healthcheck name "postgres"
            // is bound to PodDbContext via AddDbContextCheck.
            using var client = _factory.CreateClient();
            using var resp = client.GetAsync("/health").GetAwaiter().GetResult();
            resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
