#region Licence
/****************************************************************
 *  Filename: ConnectService.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Exceptions;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Grpc.Messages.Shared;
using Pod.Services.Accountant;
using SessionState = Pod.Enums.SessionState;


namespace Pod.Services.ConnectHost
{
    /// <summary>
    /// Provides functionality for Shell Clients to required to connect to a Shell Service
    /// </summary>
    public class ConnectService
    {
        private readonly ILogger<ConnectService> _logger;

        private readonly PodDbContext _podContext;
        public ConnectService(ILogger<ConnectService> logger, PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        /// <summary>
        /// Requests a Server to establish an Connection to
        /// </summary>
        /// <param name="clientCredentials">The Client Credentials</param>
        /// <param name="strDeviceIdentity">The physical Client Id</param>
        /// <param name="maxInterfaceVersion">The maximal supported Interface Version of the Client</param>
        /// <param name="reconnectConnectionId">ConnectionId in case of reconnect on connection loss</param>
        /// <returns>ShellHost information for connection</returns>
        public async Task<IResult<ShellServerResponse>> RequestServer(
                ClientCredentials clientCredentials, string strDeviceIdentity, uint maxInterfaceVersion, Guid? reconnectConnectionId = null)
        {
            //Verify Access
            var result = await clientCredentials.VerifyCredentials<ShellServerResponse>(_podContext);
            result.ArgNotNullOrWhitespace(
                    strDeviceIdentity,
                    nameof(strDeviceIdentity),
                    UserError.ShellClientInvalidDeviceIdentity);
            if(result.HasError()) return result;

            //Get the Station
            var stationDb = await _podContext.Stations.Where(x => x.Id == clientCredentials.StationId).
                                              Include(x => x.ConnectionState).
                                              ThenInclude(x=> x.ShellServer).
                                              Include(x => x.SessionDetails).
                                              ThenInclude(x => x.Session).
                                              FirstOrDefaultAsync();

            //Get a Server for the Station
            var serverForStation =
                    await _podContext.Servers.Where(x => x.IsActive && x.PublicInterfaceVersion <= maxInterfaceVersion).
                                      FirstOrDefaultAsync();

            //Return if there is no Server available 
            if(!result.RefNotNull(
                    serverForStation,
                    nameof(serverForStation),
                    UserError.ShellClientNoServerAvailable)) return result;

            //Get the Clients Identity
            var deviceIdentity = GetOrCreateIdentity(strDeviceIdentity);

            //Try to set the Requested State
            var connectingRequestResult =
                    stationDb.ConnectionState.RequestConnecting(serverForStation.Id, deviceIdentity.Id,reconnectConnectionId);
            if(connectingRequestResult.HasError()) return result.Add(connectingRequestResult);

            //Handle the Response and persist only if there is a new state
            if(stationDb.SessionDetails.HandleConnectResponse(closedConnection => _podContext.Add(closedConnection), connectingRequestResult.ReturnValue) ||
               result.IsConnectionResponseSuccess(connectingRequestResult.ReturnValue.Result))
            {
                await _podContext.SaveChangesAsync();
            }

            if(result.HasError()) return result;
            return result.Add(
                    new ShellServerResponse()
                    {
                            ConnectionId = stationDb.ConnectionState.ConnectionId.ToGuidAsBytes(),
                            HostAddress = serverForStation.PublicHostAddress,
                            Port = serverForStation.PublicPort,
                            RequiredInterfaceVersion = serverForStation.PublicInterfaceVersion
                    });
        }

        private DeviceIdentity GetOrCreateIdentity(string identity)
        {
            var deviceIdentity = _podContext.DeviceIdentities.Find(identity);
            if(deviceIdentity == null)
            {
                deviceIdentity = DeviceIdentity.Create(identity);
                _podContext.Add(deviceIdentity);
            }

            return deviceIdentity;
        }
    }
}