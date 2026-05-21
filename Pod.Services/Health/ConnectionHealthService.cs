#region Licence
/****************************************************************
 *  Filename: ConnectionHealthService.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Models.Shell;
using Pod.Enums;


namespace Pod.Services.Health
{
    /// <summary>
    /// Service class that is supposed to be used in an scope. It checks open connects if they were dropped by a client without being properly closed 
    /// </summary>
    public class ConnectionHealthService
    {
        private readonly ILogger<ConnectionHealthService> _logger;
        private readonly PublisherHub<ClientCommandType> _publisher;
        private readonly PodDbContext _podContext;
        public ConnectionHealthService(ILogger<ConnectionHealthService> logger,PublisherHub<ClientCommandType> publisher, PodDbContext podContext)
        {
            _logger = logger;
            _publisher = publisher;
            _podContext = podContext;
        }

        /// <summary>
        /// Checks Connections if they were abandoned by the client without being properly closed
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CheckForOrphanedConnections(CancellationToken cancellationToken)
        {
            try
            {
                var states = await _podContext.ConnectionStates.
                                               Where(x => x.NetworkState != NetworkState.Disconnected).
                                               Include(x => x.ShellServer).
                                               Include(x => x.Station).
                                               ThenInclude(x => x.SessionDetails).
                                               ThenInclude(x => x.Session).
                                               ToArrayAsync(cancellationToken: cancellationToken);
                foreach (var state in states)
                {
                    if(EvaluateConnection(state))
                    {
                        //In some cases when the Client failed to send a Heartbeat and we detected that its
                        //Connection is dead, the Notification Stream could be still open, here we need to send
                        //A Disconnect to ensure the Stream gets closed Server side and the client recognize the Disconnect
                        _publisher.Publish(state.StationId, ClientCommandType.Disconnect);
                        await _podContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch(Exception exception)
            {
                if(exception is TaskCanceledException)
                {
                    _logger.LogWarning(exception, "CheckForOrphanedConnections was canceled");
                }
                else
                {
                    _logger.LogError(exception, "Error during CheckForOrphanedConnections");
                }
            }

        }

        private bool EvaluateConnection(ConnectionState connectionState)
        {
            var result = connectionState.RequestTimeout();

            if(result.IsSuccess() && result.ReturnValue.Result != ConnectionRequestResult.ConnectionStillAlive)
            {
                var sessionDetails = connectionState.Station?.SessionDetails;
                if (sessionDetails != null)
                {
                    return sessionDetails.HandleConnectResponse(closedConnection => _podContext.Add(closedConnection), result.ReturnValue);
                }
            }
            return false;
        }
    }
}