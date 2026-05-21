#region Licence
/****************************************************************
 *  Filename: Program.cs
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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Pod.Data;
using Pod.LetsEncrypt.Config;
using Pod.LetsEncrypt.Services;

namespace Pod.Web.Center
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();
                using (var scope = host.Services.CreateScope())
                {
                    // Apply EF migrations + run DbSetupTasks (superuser, ShellServer, email
                    // templates) BEFORE hosted services come up. ConnectionHealthService and
                    // friends start querying the DB at host-start time; without this, they
                    // race the migration step and crash with "relation does not exist".
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<ContextInitializer>();
                    dbInitializer.Initialize();

                    // seed IP data from appsettings
                    var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
                    await ipPolicyStore.SeedAsync();
                }

                await host.RunAsync();
            }
            catch(Exception e)
            {
                logger.Fatal(e, "Exception during Main routine");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .AddJsonFile("appsettings.production.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables()
                          .AddCommandLine(args);
                })
                .ConfigureLogging(logging =>
                {
                    //Clearing as the LogLevel is decided by NLog Config
                    //The Logging configuration specified in appsettings.json overrides any call to SetMinimumLevel
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    // Kestrel binding strategy:
                    //   LetsEncrypt ENABLED  -> :80 for ACME + :443 with the LE-issued cert
                    //   DevTls PFX SET       -> :80 cleartext + :443 with the supplied dev PFX
                    //                          (for kiosk smoke-testing on private networks)
                    //   Otherwise            -> :80 HTTP/1.1+HTTP/2 cleartext only (gRPC works
                    //                          via h2c on this port, REST works as plain HTTP)
                    //
                    // The DevTls branch lets a station kiosk (which uses Grpc.Core SslCredentials
                    // and always requires TLS) connect to a local docker-compose stack without
                    // standing up a real public domain for Let's Encrypt. Set
                    // DevTlsOptions__PfxPath + DevTlsOptions__PfxPassword in .env to enable.
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var letsEncryptOptions = context.Configuration.GetSection(nameof(LetsEncryptOptions)).Get<LetsEncryptOptions>();
                        var devTlsPfxPath = context.Configuration["DevTlsOptions:PfxPath"];
                        var devTlsPfxPassword = context.Configuration["DevTlsOptions:PfxPassword"];

                        if(letsEncryptOptions != null && letsEncryptOptions.IsEnabled)
                        {
                            options.Listen(IPAddress.Any, 80);
                            options.Listen(IPAddress.Any, 443, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                listenOptions.UseHttps(httpsOptions =>
                                {
                                    httpsOptions.ServerCertificateSelector = (features, name) =>
                                    {
                                        var certSelector = ServiceLocator.GetCertificateSelector();
                                        return certSelector.Select(features, name);
                                    };
                                });
                            });
                        }
                        else if(!string.IsNullOrWhiteSpace(devTlsPfxPath) && File.Exists(devTlsPfxPath))
                        {
                            // Cleartext :80 for /health + browser
                            options.Listen(IPAddress.Any, 80, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            });
                            // TLS :443 with the supplied dev PFX for the gRPC kiosk channel.
                            // Cap at TLS 1.2 - the kiosk's Grpc.Core 2.46.6 native stack does
                            // not negotiate TLS 1.3 cleanly against ASP.NET Core 10's Kestrel.
                            options.Listen(IPAddress.Any, 443, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                listenOptions.UseHttps(httpsOptions =>
                                {
                                    httpsOptions.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                                        devTlsPfxPath,
                                        devTlsPfxPassword ?? string.Empty);
                                    httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                                });
                            });
                        }
                        else
                        {
                            // Cleartext HTTP/1.1 + HTTP/2 (h2c) on :80. Lets gRPC work without
                            // TLS for dev / behind-reverse-proxy deployments.
                            options.Listen(IPAddress.Any, 80, listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            });
                        }
                    });
                });
        }
    }
}
