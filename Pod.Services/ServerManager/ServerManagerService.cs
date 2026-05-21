#region Licence
/****************************************************************
 *  Filename: ServerManagerService.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Exceptions;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Servers;
using Pod.Enums;
using Pod.Services.Accountant;
using Pod.ViewModels.Expressions;
using Pod.ViewModels.ShellServer;

namespace Pod.Services.ServerManager
{
    /// <summary>
    /// Service providing ShellHost/Server related functionality
    /// </summary>
    public class ServerManagerService
    {
        private readonly ILogger<ServerManagerService> _logger;

        private readonly PodDbContext _podContext;
        public ServerManagerService(ILogger<ServerManagerService> logger, PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        /// <summary>
        /// Get all Shell Servers 
        /// </summary>
        /// <returns>Collection with Shell Servers</returns>
        public async Task<IResult<ICollection<ShellServerDetailsViewModel>>> GetAllServers()
        {
            var result = new Result<ICollection<ShellServerDetailsViewModel>>();
            return result.Add(await _podContext.Servers.Select(ToShellServerDetailsVm.FromShellServer()).ToArrayAsync());
        }

        /// <summary>
        /// Get all Shell Servers 
        /// </summary>
        /// <returns>Collection with Shell Servers</returns>
        public async Task<IResult<ShellServerDetailsViewModel>> GetServer(Guid serverId)
        {
            var result = new Result<ShellServerDetailsViewModel>();
            result.ArgNotEqual(serverId, nameof(serverId), 0, UserError.ShellServerInvalidId);
            if (result.HasError()) return result;
            var server = await _podContext.Servers.Where(x => x.Id == serverId).
                                           Select(ToShellServerDetailsVm.FromShellServer()).
                                           FirstOrDefaultAsync();
            result.ArgNotNull(server, nameof(server), UserError.ShellServerInvalidId);
            if(result.IsSuccess())
            {
                result.Add(server);
            }
            return result;
        }

        /// <summary>
        /// Creates a new Shell Server
        /// </summary>
        /// <returns>The created Shell Server</returns>
        public async Task<IResult<ShellServerViewModel>> CreateNewServer(string displayName, string hostAddress, uint hostPort,uint interfaceVersion)
        {
            var result = new Result<ShellServerViewModel>();
            var newServer = ShellServer.Create(
                    displayName,
                    hostAddress,
                    hostPort,
                    interfaceVersion);
            if(newServer.HasError()) return result.Add(newServer);
            _podContext.Servers.Add(newServer.ReturnValue);
            await _podContext.SaveChangesAsync();
            return result.Add(ToShellServerVm.FuncFromShellServer(newServer.ReturnValue));
        }

        public async Task<IResult<ShellServerViewModel>> SetDisplayName(Guid serverId, string displayName)
        {
            var result = new Result<ShellServerViewModel>();
            result.ArgNotEqual(serverId, nameof(serverId), 0, UserError.ShellServerInvalidId);
            if (result.HasError()) return result;

            var shellServerDb = await _podContext.Servers.FindAsync(serverId);
            result.ArgNotNull(shellServerDb, nameof(shellServerDb), UserError.ShellServerNotFound);
            if (result.HasError()) return result;

            result.Add(shellServerDb.SetDisplayName(displayName));
            if(result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
            }
            return result.Add(ToShellServerVm.FuncFromShellServer(shellServerDb));
        }

        public async Task<IResult<ShellServerViewModel>> SetTimeSettings(
                Guid serverId, TimeSpan? heartbeatInterval, TimeSpan? heartbeatTimeout, TimeSpan? connectTimeout)
        {
            var result = new Result<ShellServerViewModel>();
            result.ArgNotEqual(serverId, nameof(serverId), 0, UserError.ShellServerInvalidId);
            if (result.HasError()) return result;

            var shellServerDb = await _podContext.Servers.FindAsync(serverId);
            result.ArgNotNull(shellServerDb, nameof(shellServerDb), UserError.ShellServerNotFound);
            if (result.HasError()) return result;

            if(!heartbeatInterval.HasValue)
            {
                heartbeatInterval = shellServerDb.HeartbeatInterval;
            }

            if(!heartbeatTimeout.HasValue)
            {
                heartbeatTimeout = shellServerDb.HeartbeatTimeout;
            }
            result.Add(shellServerDb.SetHeartbeatValues(heartbeatTimeout.Value, heartbeatInterval.Value));
            if(connectTimeout.HasValue)
            {
                result.Add(shellServerDb.SetConnectTimeout(connectTimeout.Value));
            }

            if(result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
            }

            return result.Add(ToShellServerVm.FuncFromShellServer(shellServerDb));
        }

        public async Task<IResult<ShellServerViewModel>> SetConnectionSettings(Guid serverId, string hostAddress, uint port, uint interfaceVersion)
        {
            var result = new Result<ShellServerViewModel>();
            result.ArgNotEqual(serverId, nameof(serverId), 0, UserError.ShellServerInvalidId);
            if (result.HasError()) return result;

            var shellServerDb = await _podContext.Servers.FindAsync(serverId);
            result.ArgNotNull(shellServerDb, nameof(shellServerDb), UserError.ShellServerNotFound);
            if (result.HasError()) return result;

            result.Add(shellServerDb.SetHostDetails(hostAddress,port,interfaceVersion));
            if (result.IsSuccess())
            {
                await _podContext.SaveChangesAsync();
            }
            return result.Add(ToShellServerVm.FuncFromShellServer(shellServerDb));
        }

        /// <summary>
        /// Sets the Enabled State of an Shell Server
        /// </summary>
        /// <param name="serverId">The Server Id</param>
        /// <param name="activeState">The state</param>
        /// <returns>The Shell Server</returns>
        public async Task<IResult<ShellServerViewModel>> SetActiveState(Guid serverId, bool activeState)
        {
            var result = new Result<ShellServerViewModel>();
            result.ArgNotEqual(serverId, nameof(serverId), 0, UserError.ShellServerInvalidId);
            if(result.HasError()) return result;

            var shellServerDb = await _podContext.Servers.FindAsync(serverId);
            result.ArgNotNull(shellServerDb, nameof(shellServerDb), UserError.ShellServerNotFound);
            if(result.HasError()) return result;
            shellServerDb.SetActive(activeState);
            await _podContext.SaveChangesAsync();
            return result.Add(ToShellServerVm.FuncFromShellServer(shellServerDb));
        }

        /// <summary>
        /// Returns all connected Clients to a Shell Server
        /// </summary>
        /// <param name="serverId">The Shell Server Id</param>
        /// <returns>Collection with all Connected Clients</returns>
        public async Task<Result<ICollection<ShellServerConnectedClientViewModel>>> ViewConnectedStations(Guid serverId)
        {
            var result = new Result<ICollection<ShellServerConnectedClientViewModel>>();
            var connectedClients = await _podContext.ConnectionStates.Where(x => x.ShellServerId == serverId).
                                                  Select(ToShellServerConnectedClientVm.FromConnectionState()).
                                                  ToArrayAsync();
            return connectedClients == null ? result.Add(new List<ShellServerConnectedClientViewModel>()) : result.Add(connectedClients);
        }


    }
}