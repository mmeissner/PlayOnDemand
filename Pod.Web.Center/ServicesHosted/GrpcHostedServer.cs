#region Licence
/****************************************************************
 *  Filename: GrpcHostedServer.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pod.Services;

namespace Pod.Web.Center.ServicesHosted
{
    /// <summary>
    /// Sends a Disconnect command to every connected station via <see cref="PublisherHub{T}"/> at host shutdown
    /// so streaming gRPC clients can drain gracefully. The gRPC services themselves are mapped via
    /// endpoint routing in <see cref="Startup"/>; this hook only owns the publisher-hub lifetime signal.
    /// </summary>
    public class GrpcHostedServer : IHostedService
    {
        private readonly ILogger<GrpcHostedServer> _logger;
        private readonly PublisherHub<ClientCommandType> _publisherHub;

        public GrpcHostedServer(ILogger<GrpcHostedServer> logger, PublisherHub<ClientCommandType> publisherHub)
        {
            _logger = logger;
            _publisherHub = publisherHub;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PublisherHub shutdown coordinator started");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Broadcasting Disconnect to streaming gRPC clients");
            await _publisherHub.ShutdownAsync(ClientCommandType.Disconnect);
        }
    }
}
