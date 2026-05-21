#region Licence
/****************************************************************
 *  Filename: CertificateSelector.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Pod.LetsEncrypt.Config;

namespace Pod.LetsEncrypt.Services
{
    /// <summary>
    /// Provides a SSL Certificate to Kestrel and allows to Add or Update one
    /// This class is thread safe
    /// </summary>
    public class CertificateSelector
    {
        private readonly ConcurrentDictionary<string, X509Certificate2> _certs;
        private readonly ILogger<CertificateSelector> _logger;
        private readonly LetsEncryptOptions _options;
        private readonly string _cacheFolder;

        public CertificateSelector(ILogger<CertificateSelector> logger,LetsEncryptOptions options, IHostingEnvironment env)
        {
            _logger = logger;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if(!String.IsNullOrWhiteSpace(options.CacheFolder))
            {
                _cacheFolder = Path.Combine(env.ContentRootPath, options.CacheFolder);
                EnsureStoreDirectory(_cacheFolder);
            }
            _certs = AddConfiguredHosts(options);
        }

        /// <summary>
        /// Returns domains where the certificates are about to expire based on the options (days before) provided
        /// </summary>
        /// <returns>Array with domains to expire</returns>
        public string[] GetCertificatesAboutToExpire()
        {
            var certs = _certs.ToArray();
            var result = new List<string>();

            foreach (var cert in certs)
            {

                bool mustRequest;
                if (cert.Value == null)
                {
                    mustRequest = true;
                }
                else
                {
                    mustRequest = DateTime.UtcNow.AddDays(_options.DaysBefore) > cert.Value.NotAfter;
                }

                if (mustRequest)
                {
                    result.Add(cert.Key);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Register a domain name and a certificate 
        /// </summary>
        /// <param name="domainName">The domain name</param>
        /// <param name="certificate">The SSL Certificate</param>
        public void Use(string domainName, X509Certificate2 certificate)
        {
            _certs.AddOrUpdate(domainName, certificate, (_, __) => certificate);
        }

        /// <summary>
        /// Returns the SSL Certificate for the domain name
        /// </summary>
        /// <param name="features"></param>
        /// <param name="hostName"></param>
        /// <returns>null if non is found</returns>
        public X509Certificate2 Select(ConnectionContext features, string hostName)
        {
            if (!_certs.TryGetValue(hostName, out var retVal))
            {
                return null;
            }

            return retVal;
        }

        private ConcurrentDictionary<string, X509Certificate2> AddConfiguredHosts(LetsEncryptOptions options)
        {
            var hosts = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
            foreach (var host in options.ConfiguredHosts)
            {
                var cert = host.FallBackCertificate;
                if (host.FallBackCertificate == null && !string.IsNullOrEmpty(_cacheFolder))
                {
                    var fileName = Path.Combine(_cacheFolder, host.HostName + ".pfx");
                    if (File.Exists(fileName))
                    {
                        cert = new X509Certificate2(fileName, options.EncryptionPassword);
                    }
                }
                hosts.TryAdd(host.HostName, cert);
            }
            return hosts;
        }
        private void EnsureStoreDirectory(string storePath)
        {
            if (!Directory.Exists(storePath))
            {
                Directory.CreateDirectory(storePath);
            }
        }
    }
}
