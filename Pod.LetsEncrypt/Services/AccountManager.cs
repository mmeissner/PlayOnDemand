#region Licence
/****************************************************************
 *  Filename: AccountManager.cs
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
    /// Handles Lets Encrypt Accounts
    /// </summary>
    public class AccountManager
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private readonly string _accountKey;
        private readonly string _keyFile;

        /// <summary>
        /// Creates a new LetsEncrypt Account Manager
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="hostingEnv"></param>
        public AccountManager(ILogger<CertificateRequestService> logger, IOptions<LetsEncryptOptions> options, IHostingEnvironment hostingEnv)
        {
            _logger = logger;
            _options = options.Value;

            if (string.IsNullOrEmpty(options.Value.AccountKey))
            {
                if (!string.IsNullOrEmpty(options.Value.CacheFolder))
                {
                    var keyDirectory = Path.Combine(hostingEnv.ContentRootPath, options.Value.CacheFolder);
                    EnsureStoreDirectory(keyDirectory);
                    _keyFile = Path.Combine(keyDirectory, "account");

                    if (File.Exists(_keyFile))
                    {
                        _accountKey = File.ReadAllText(_keyFile);
                    }
                }
            }
            else
            {
                _accountKey = options.Value.AccountKey;
            }
        }

        /// <summary>
        /// Creates an new LetsEncrypt Account or returns a stored one
        /// </summary>
        /// <returns>Key for Account</returns>
        public async Task<string> GetAccountKey()
        {
            if (!string.IsNullOrEmpty(_accountKey))
            {
                return _accountKey;
            }

            _logger.LogInformation("Getting a new account key");

            var acme = new AcmeContext(_options.AcmeServer);
            IAccountContext account;

            try
            {
                account = await acme.NewAccount(_options.EmailAddress, true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return null;
            }

            var pemKey = acme.AccountKey.ToPem();

            if (!string.IsNullOrEmpty(_keyFile))
            {
                File.WriteAllText(_keyFile, pemKey);
            }

            return pemKey;
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
