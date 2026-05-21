#region Licence
/****************************************************************
 *  Filename: ShellServerViewModels.cs
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
using System.Linq;
using System.Linq.Expressions;
using Pod.Data.Models.Shell;
using Pod.ViewModels.ShellServer;

namespace Pod.ViewModels.Expressions
{

    public static class ToShellServerVm
    {
        public static readonly Func<Data.Models.Servers.ShellServer, ShellServerViewModel> FuncFromShellServer =
                FromShellServer().Compile();
        public static Expression<Func<Data.Models.Servers.ShellServer, ShellServerViewModel>> FromShellServer()
        {
            return x => new ShellServerViewModel
                        {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                PublicInterfaceVersion = x.PublicInterfaceVersion,
                                PublicHostAddress = x.PublicHostAddress,
                                PublicPort = x.PublicPort,
                                HeartbeatInterval = x.HeartbeatInterval,
                                HeartbeatTimeout = x.HeartbeatTimeout,
                                ConnectTimeout = x.ConnectTimeout,
                                IsActive = x.IsActive
                        };
        }
    }

    public static class ToShellServerDetailsVm
    {
        public static Expression<Func<Data.Models.Servers.ShellServer, ShellServerDetailsViewModel>> FromShellServer()
        {
            return x => new ShellServerDetailsViewModel
                        {
                                Id = x.Id,
                                DisplayName = x.DisplayName,
                                CreatedUtc = x.CreatedUtc,
                                IsActive = x.IsActive,
                                PublicHostAddress = x.PublicHostAddress,
                                PublicPort = x.PublicPort,
                                PublicInterfaceVersion = x.PublicInterfaceVersion,
                                HeartbeatInterval = x.HeartbeatInterval,
                                HeartbeatTimeout = x.HeartbeatTimeout,
                                ConnectTimeout = x.ConnectTimeout,
                                //Do not access Count through property as it will otherwise not work 
                                ConnectedClients = x.ConnectedClients != null ? x.ConnectedClients.Count() : 0
                        };
        }

        #region SQL Query
            //SELECT x."Id", x."DisplayName", x."CreatedUtc", x."IsActive", x."PublicHostAddress", x."PublicPort", x."PublicInterfaceVersion", x."HeartbeatInterval", x."HeartbeatTimeout", x."ConnectTimeout", CASE
            //        WHEN x."Id" IS NOT NULL
            //        THEN (
            //                SELECT COUNT(*)::INT4
            //        FROM "ConnectionStates" AS c
            //WHERE x."Id" = c."ShellServerId"
            //) ELSE 0
            //END AS "ConnectedClients"
            //FROM "Servers" AS x
        #endregion
    }

    public static class ToShellServerConnectedClientVm
    {
        public static Expression<Func<ConnectionState, ShellServerConnectedClientViewModel>> FromConnectionState()
        {
            return x => new ShellServerConnectedClientViewModel
                        {
                                StationId = x.StationId,
                                State = x.NetworkState,
                                ServerRequestOn = x.ServerRequestOnUtc,
                                ConnectedOnUtc = x.ConnectedOnUtc,
                                LastHeartBeatOnUtc = x.LastHeartbeatOnUtc,
                                DeviceIdentity = x.DeviceIdentityId,
                                ConnectionId = x.ConnectionId
                        };
        }
    }
}
