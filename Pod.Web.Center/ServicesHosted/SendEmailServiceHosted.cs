#region Licence
/****************************************************************
 *  Filename: SendEmailServiceHosted.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pod.Services.Email;

namespace Pod.Web.Center.ServicesHosted {
    /// <summary>
    /// Background Service sending Emails from Templates
    /// </summary>
    public class SendEmailServiceHosted : BackgroundService
    {
        private readonly ILogger<SendEmailServiceHosted> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkMailsInterval;
        public SendEmailServiceHosted(
                ILogger<SendEmailServiceHosted> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _checkMailsInterval = TimeSpan.FromMinutes(1.5);
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Hosted Email Send Service");

            try
            {
                do
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mailService =
                                scope.ServiceProvider.GetRequiredService<EMailService>();

                        var result = await mailService.SendEmailOrders(stoppingToken);
                        if(result.HasError())
                        {
                            _logger.LogError($"Error occured during sending of emails: {result.ToErrorString()}");
                        }
                    }

                    await Task.Delay(_checkMailsInterval, stoppingToken);
                } while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException)
                {
                    _logger.LogInformation("Hosted Email Send Service canceled");
                }
            }

            _logger.LogInformation("Stopped Hosted Email Send Service eHosted");
        }

    }
}