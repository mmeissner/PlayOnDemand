#region Licence
/****************************************************************
 *  Filename: ConnectionHealthServiceHosted.cs
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
using Pod.Data.Models.Shell;
using Pod.Services.Health;

namespace Pod.Web.Center.ServicesHosted
{

    /// <summary>
    /// Background service that checks periodically for Connections/Sessions that were not properly closed
    /// and handles them accordingly
    /// </summary>
    public class ConnectionHealthServiceHosted : BackgroundService
    {
        private readonly ILogger<ConnectionHealthServiceHosted> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval;
        public ConnectionHealthServiceHosted(
                ILogger<ConnectionHealthServiceHosted> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _checkInterval = TimeSpan.FromMinutes(5);
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Method for Background Service to perform work
        /// Will scan periodically for orphaned connections and clean their status.
        /// This is mainly needed for stations that closed their connection ungracefully and
        /// never come online afterwards
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Hosted Connection Health Service");

            try
            {
                do
                {

                    using(var scope = _serviceProvider.CreateScope())
                    {
                        var healthService =
                                scope.ServiceProvider.GetRequiredService<ConnectionHealthService>();

                        await healthService.CheckForOrphanedConnections(stoppingToken);
                    }

                    await Task.Delay(_checkInterval, stoppingToken);
                } while(!stoppingToken.IsCancellationRequested);
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    _logger.LogInformation("Hosted Connection Health Service was canceled");
                }
            }

            _logger.LogInformation("Stopped Hosted Connection Health Service");
        }
    }
}