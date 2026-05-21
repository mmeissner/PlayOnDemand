#region Licence
/****************************************************************
 *  Filename: CertificateBuilderService.cs
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
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pod.LetsEncrypt.Config;

namespace Pod.LetsEncrypt.Services
{
    /// <summary>
    /// Receives the new Certificate and places it in the Certificate Store
    /// </summary>
    public class CertificateBuilderService
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private readonly CertificateSelector _certificateSelector;
        private readonly IEnumerable<ILetsEncryptHook> _hooks;
        private readonly string _cacheDirectory = null;

        public CertificateBuilderService(ILogger<CertificateBuilderService> logger,
                                         IHostingEnvironment hostingEnv,
                                         IOptions<LetsEncryptOptions> options,
                                         CertificateSelector certificateSelector,
                                         IEnumerable<ILetsEncryptHook> hooks)
        {
            _logger = logger;
            _options = options.Value;
            _certificateSelector = certificateSelector;
            _hooks = hooks ?? new List<ILetsEncryptHook>();
            if(!string.IsNullOrWhiteSpace(_options.CacheFolder))
            {
                _cacheDirectory = Path.Combine(hostingEnv.ContentRootPath,_options.CacheFolder);
                logger.LogDebug($"Checking directory for Certificate Builder: {_cacheDirectory}");
                EnsureStoreDirectory(_cacheDirectory);
            }
        }

        /// <summary>
        /// Downloads and creates an Certificate from an <see cref="IOrderContext"/>
        /// </summary>
        /// <param name="order">The order containing the certificate</param>
        /// <param name="domainName">The domain name the certificate is for</param>
        /// <returns></returns>
        public async Task DownloadAndCreateCertificate(IOrderContext order, string domainName)
        {
            var eventArgs = new LetsEncryptEventArgs()
                            {
                                    DomainName = domainName,
                                    Stage = LetsEncryptStage.BuildCertificate
                            };
            try
            {
                var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

                var cert = await order.Generate(
                        new CsrInfo
                        {
                                CommonName = domainName
                        },
                        privateKey);

                var pfxBuilder = cert.ToPfx(privateKey);
                var pfx = pfxBuilder.Build(domainName, _options.EncryptionPassword);

                var x509Cert = new X509Certificate2(pfx, _options.EncryptionPassword);

                _certificateSelector.Use(domainName, x509Cert);

                if(!string.IsNullOrEmpty(_cacheDirectory))
                {
                    eventArgs.Stage = LetsEncryptStage.PersistCertificate;
                    var fileName = Path.Combine(_cacheDirectory, domainName + ".pfx");

                    if(File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    File.WriteAllBytes(fileName, pfx);
                }

                eventArgs.Stage = LetsEncryptStage.CertificateRenewed;
                _logger.LogInformation($"New certificate generated for {domainName}");
            }
            catch(Exception e)
            {
                _logger.LogError(e.ToString());
                eventArgs.Exception = e;
            }
            finally
            {
                await eventArgs.SendArgs(_logger, _hooks);
            }
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
