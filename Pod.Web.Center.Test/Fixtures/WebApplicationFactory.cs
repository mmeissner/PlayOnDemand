#region Licence
/****************************************************************
 *  Filename: WebApplicationFactory.cs
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
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Servers;

namespace Pod.Web.Center.Test.Fixtures
{
    /// <summary>
    /// Boots the full Pod.Web.Center pipeline in-process for integration tests.
    ///
    /// - Injects a self-contained test configuration (no appsettings.json copy needed).
    /// - Swaps the production Postgres DbContext for EF Core InMemory.
    /// - Replaces the design-time DbContext factory so ContextInitializer hits the same store.
    /// - Pre-seeds a ShellServer row so the singleton resolution in Startup doesn't throw.
    ///
    /// Usage:
    ///     public class MyApiTests : IClassFixture&lt;PodWebApplicationFactory&gt;
    ///     {
    ///         private readonly PodWebApplicationFactory _factory;
    ///         public MyApiTests(PodWebApplicationFactory factory) => _factory = factory;
    ///     }
    /// </summary>
    public class PodWebApplicationFactory : WebApplicationFactory<Pod.Web.Center.Program>
    {
        private readonly string _dbName = $"PodWebTest_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(TestConfiguration());
            });

            builder.ConfigureServices(services =>
            {
                // Strip out the production Postgres DbContext registration so InMemory wins.
                var efDescriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<PodDbContext>) ||
                    d.ServiceType == typeof(PodDbContext) ||
                    d.ServiceType == typeof(PodDbContextFactory) ||
                    d.ServiceType == typeof(IDesignTimeDbContextFactory<PodDbContext>) ||
                    d.ServiceType.FullName?.Contains("Npgsql") == true).ToList();
                foreach (var d in efDescriptors)
                    services.Remove(d);

                // PodDbContext has an internal constructor, so AddDbContext<>() can't
                // bind it. Use the reflection-based factory for every scope.
                services.AddScoped<PodDbContext>(_ => InMemoryDbContextFactory.Create(_dbName));
                services.AddSingleton<IDesignTimeDbContextFactory<PodDbContext>>(
                    new InMemoryDesignTimeDbContextFactory(_dbName));

                // Pre-seed: production Startup eagerly resolves ShellServer via
                // dbContext.Servers.FirstOrDefault() and throws if missing.
                using var db = InMemoryDbContextFactory.Create(_dbName);
                if (!db.Servers.Any())
                {
                    var createResult = ShellServer.Create(
                        displayName: "Test ShellServer",
                        hostAddress: "localhost",
                        port: 50061,
                        publicInterfaceVersion: 0);
                    if (createResult.IsSuccess())
                    {
                        db.Servers.Add(createResult.ReturnValue);
                        db.SaveChanges();
                    }
                }
            });
        }

        private static IEnumerable<KeyValuePair<string, string>> TestConfiguration()
        {
            return new Dictionary<string, string>
            {
                ["AllowedHosts"] = "*",
                ["LetsEncryptOptions:IsEnabled"] = "false",
                ["LetsEncryptOptions:Hosts:0"] = "localhost",
                ["LetsEncryptOptions:EmailAddress"] = "test@example.com",
                ["LetsEncryptOptions:AcceptTermsOfService"] = "true",
                ["LetsEncryptOptions:EncryptionPassword"] = "test",
                ["LetsEncryptOptions:CacheFolder"] = "certs",

                ["ConnectionStrings:PodApiContext"] = "InMemory",

                ["DbContextFactoryConfig:ConnectionStringName"] = "PodApiContext",
                ["DbContextFactoryConfig:LogEntityFramework"] = "false",
                ["DbContextFactoryConfig:LogConcurrencyExceptionDetails"] = "false",
                ["DbContextFactoryConfig:LogSensitiveData"] = "false",

                ["AuthConfig:SecretKey"] = "TestSecretKey-DoNotUseInProduction-1234567890",

                ["JwtIssuerOptionsConfig:Issuer"] = "TestIssuer",
                ["JwtIssuerOptionsConfig:Audience"] = "TestAudience",
                ["JwtIssuerOptionsConfig:ValidFor"] = "02:00:00",

                ["PasswordResetTokenProviderOptions:Name"] = "PoD_ResetPasswordToken",
                ["PasswordResetTokenProviderOptions:TokenLifespan"] = "03:00:00",
                ["EmailConfirmationTokenProviderOptions:Name"] = "PoD_EmailConfirmationToken",
                ["EmailConfirmationTokenProviderOptions:TokenLifespan"] = "168:00:00",
                ["RefreshAccessTokenProviderOptions:Name"] = "PoD_RefreshToken",
                ["RefreshAccessTokenProviderOptions:TokenLifespan"] = "7300:00:00:00",

                ["ConfigSuperuser:Username"] = "superuser",
                ["ConfigSuperuser:Email"] = "superuser@example.com",
                ["ConfigSuperuser:Password"] = "Password-1234",
                ["ConfigSuperuser:StationPassword"] = "Password_1234",

                ["ConfigShellServer:DisplayName"] = "Test ShellServer",
                ["ConfigShellServer:HostAddress"] = "localhost",
                ["ConfigShellServer:Port"] = "50061",
                ["ConfigShellServer:InterfaceVersion"] = "0",

                ["WebAppConfig:WebAppHostingRoot"] = "http://localhost:5000/",

                ["GrpcServerConfig:GrpcHost"] = "0.0.0.0",
                ["GrpcServerConfig:GrpcPort"] = "50061",
                ["GrpcServerConfig:GrpcRequestCallTokensPerCompletionQueue"] = "32768",
                ["GrpcServerConfig:LogGrpc"] = "false",
                ["GrpcServerConfig:ForceClientCertificate"] = "false",
                ["GrpcServerConfig:SslCredentialFiles:CertificateChainFile"] = "ssl_credentials/server.crt",
                ["GrpcServerConfig:SslCredentialFiles:PrivateKeyFile"] = "ssl_credentials/server.key",
                ["GrpcServerConfig:SslCredentialFiles:RootClientCertificateFile"] = "ssl_credentials/ca.crt",

                ["IpRateLimiting:EnableEndpointRateLimiting"] = "false",
                ["IpRateLimiting:HttpStatusCode"] = "429",
                ["IpRateLimiting:GeneralRules:0:Endpoint"] = "*",
                ["IpRateLimiting:GeneralRules:0:Period"] = "1d",
                ["IpRateLimiting:GeneralRules:0:Limit"] = "100000",
            };
        }

        /// <summary>
        /// Design-time DbContext factory that returns InMemory-backed contexts so
        /// ContextInitializer hits the same store as application requests.
        /// </summary>
        private sealed class InMemoryDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PodDbContext>
        {
            private readonly string _dbName;
            public InMemoryDesignTimeDbContextFactory(string dbName) => _dbName = dbName;
            public PodDbContext CreateDbContext(string[] args) => InMemoryDbContextFactory.Create(_dbName);
        }
    }
}
