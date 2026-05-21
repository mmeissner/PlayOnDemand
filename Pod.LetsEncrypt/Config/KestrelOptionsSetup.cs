#region Licence
/****************************************************************
 *  Filename: KestrelOptionsSetup.cs
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
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Pod.LetsEncrypt.Services;

namespace Pod.LetsEncrypt.Config
{
    /// <summary>
    /// Helper for the <see cref="CertificateSelector"/>
    /// </summary>
    public class KestrelOptionsSetup : IConfigureOptions<KestrelServerOptions>
    {
        private readonly CertificateSelector _certificateSelector;

        /// <summary>
        /// Builds a new instance this Helper instance
        /// </summary>
        /// <param name="certificateSelector"></param>
        public KestrelOptionsSetup(CertificateSelector certificateSelector)
        {
            _certificateSelector = certificateSelector ?? throw new ArgumentNullException(nameof(certificateSelector));
        }

        /// <summary>
        /// Called by the Asp.Net Core MVC Framework after IOC is ready
        /// and sets up the  <see cref="CertificateSelector"/> 
        /// </summary>
        /// <param name="options"></param>
        public void Configure(KestrelServerOptions options)
        {
            options.ConfigureHttpsDefaults(o =>
            {
                o.ServerCertificateSelector = _certificateSelector.Select;
            });
        }
    }

}
