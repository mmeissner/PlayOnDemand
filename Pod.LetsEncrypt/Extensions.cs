#region Licence
/****************************************************************
 *  Filename: Extensions.cs
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
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pod.LetsEncrypt.Config;
using Pod.LetsEncrypt.Middleware;
using Pod.LetsEncrypt.Services;

namespace Pod.LetsEncrypt
{
    /// <summary>
    /// Asp.Net MVC Extensions to Register LetsEncrypt 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers Middleware for Lets Encrypt Challenge Response
        /// </summary>
        /// <param name="builder"></param>
        public static void UseLetsEncrypt(this IApplicationBuilder builder)
        {
            builder.Map(LetsEncryptConst.ChallengePath, mapped =>
            {
                mapped.UseMiddleware<HttpChallengeResponseMiddleware>();
            });
        }


        /// <summary>
        /// Register required Services to IOC Container
        /// </summary>
        /// <param name="services"></param>
        public static void AddLetsEncrypt(this IServiceCollection services)
        {
            services.AddSingleton<CertificateSelector>(
                    x =>
                    {
                        var selector = new CertificateSelector(
                                x.GetRequiredService<ILogger<CertificateSelector>>(),
                                x.GetRequiredService<LetsEncryptOptions>(),
                                x.GetRequiredService<IHostingEnvironment>());
                        ServiceLocator.SetCertificateSelector(selector);
                        return selector;
                    });
            services.AddSingleton<AccountManager>();
            services.AddSingleton<HttpChallengeResponseMiddleware>();
            services.AddSingleton<IHttpChallengeResponseStore, InMemoryHttpChallengeResponseStore>();

            services.AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelOptionsSetup>();
            services.AddTransient<CertificateBuilderService>();

            services.AddHostedService<CertificateRequestService>();
        }
    }
}
