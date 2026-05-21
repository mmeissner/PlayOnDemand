#region Licence
/****************************************************************
 *  Filename: CertificateRequestService.cs
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
using System.Threading;
using System.Threading.Tasks;
using Certes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pod.LetsEncrypt.Config;

namespace Pod.LetsEncrypt.Services
{
    /// <summary>
    /// Service responsible to check and renew Certificates periodically
    /// </summary>
    public class CertificateRequestService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly LetsEncryptOptions _options;
        private readonly CertificateSelector _certificateSelector;
        private readonly AccountManager _accountManager;
        private readonly IHttpChallengeResponseStore _httpChallengeResponseStore;
        private readonly IEnumerable<ILetsEncryptHook> _hooks;
        private Timer _timer;

        public CertificateRequestService(ILogger<CertificateRequestService> logger,
            IOptions<LetsEncryptOptions> options,
            CertificateSelector certificateSelector,
            AccountManager accountManager,
            IHttpChallengeResponseStore httpChallengeResponseStore,
            IEnumerable<ILetsEncryptHook> hooks)
        {
            _logger = logger;
            _options = options.Value;
            _certificateSelector = certificateSelector;
            _accountManager = accountManager;
            _httpChallengeResponseStore = httpChallengeResponseStore;
            _hooks = hooks ?? new List<ILetsEncryptHook>();
        }

        /// <summary>
        /// Starts the Service
        /// </summary>
        /// <param name="cancellationToken">unused</param>
        /// <returns>Task for Hosted Service</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LetsEncrypt request service has started");

            _timer = new Timer(RefreshCertificates, null, TimeSpan.Zero, TimeSpan.FromHours(12));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks and Orders certificates
        /// </summary>
        /// <param name="state"></param>
        private async void RefreshCertificates(object state)
        {
            var refreshErrorArgs = new LetsEncryptEventArgs()
                            {
                                    Stage = LetsEncryptStage.RefreshStarted
                            };
            try
            {
                _httpChallengeResponseStore.ClearPendingOrders();

                var domainNames = _certificateSelector.GetCertificatesAboutToExpire();

                if(domainNames.Length > 0)
                {
                    refreshErrorArgs.Stage = LetsEncryptStage.GetAccountKey;
                    var account = await _accountManager.GetAccountKey();

                    if(string.IsNullOrEmpty(account))
                    {
                        _logger.LogError("Cant get Let´s Encrypt account");
                        throw new Exception("Cant get Let´s Encrypt account");
                    }
                    
                    var listEventArgs = new List<LetsEncryptEventArgs>();
                    foreach(var domainName in domainNames)
                    {
                        var eventArg = new LetsEncryptEventArgs()
                                           {
                                                   Stage = LetsEncryptStage.CreateOrder,
                                                   DomainName = domainName
                                           };
                        try
                        {
                            var acme = new AcmeContext(_options.AcmeServer, KeyFactory.FromPem(account));

                            _logger.LogInformation($"LetsEncrypt: Creating order for domain name: {domainName}");

                            var order = await acme.NewOrder(new[] {domainName});
                            _logger.LogDebug($"LetsEncrypt: Order created for domain name: {domainName}");
                            eventArg.Stage = LetsEncryptStage.RequestChallenge;
                            _logger.LogDebug("LetsEncrypt: Requesting authorization for order");
                            var authz = (await order.Authorizations()).First();
                            _logger.LogDebug($"LetsEncrypt: Authorization Response: Location: {authz.Location}");
                            _logger.LogDebug("LetsEncrypt: Requesting challenge for order");
                            var httpChallenge = await authz.Http();
                            _logger.LogDebug($"LetsEncrypt: Challenge for order received Type:{httpChallenge.Type}, Token:{httpChallenge.Token}");
                            var orderInfo = new OrderInfo
                                            {
                                                    Order = order,
                                                    Challenge = httpChallenge,
                                                    DomainName = domainName
                                            };
                            _logger.LogDebug($"LetsEncrypt:Adding Challenge to Response Store, Type:{httpChallenge.Type}, Token: {httpChallenge.Token}");
                            _httpChallengeResponseStore.AddChallengeResponse(httpChallenge.Token, orderInfo);
                            eventArg.Stage = LetsEncryptStage.AcknowledgesChallenge;
                            _logger.LogDebug($"LetsEncrypt: Sending Acknowledge of Challenge to Server, Type:{httpChallenge.Type}, Token: {httpChallenge.Token}");
                            var result =await httpChallenge.Validate();
                            if(result != null)
                            {
                                if(result.Error != null)
                                {
                                    _logger.LogError($"Type: {result.Error.Type}, Detail:{result.Error.Detail}, Identifier:{result.Error.Identifier}");
                                }
                                else
                                {
                                    _logger.LogDebug($"Type: {result.Type}, Uri: {result.Url}, Status: {result.Status}");
                                }
                            }
                            else
                            {
                                _logger.LogError("Could not receive a challenge result from server");
                            }
                        }
                        catch(Exception e)
                        {
                            _logger.LogError(e, "Exception during certificate refresh request");
                            eventArg.Exception = e;
                        }
                        finally
                        {
                            listEventArgs.Add(eventArg);
                        }
                    }
                    //Create Notifications
                    foreach(var arg in listEventArgs)
                    {
                        await arg.SendArgs(_logger, _hooks);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception in CertificateRequestService");
                refreshErrorArgs.Exception = ex;
                await refreshErrorArgs.SendArgs(_logger, _hooks);
            }
        }

        /// <summary>
        /// Stops the Service
        /// </summary>
        /// <param name="cancellationToken">unused</param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LetsEncrypt request service is stopping");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
