#region Licence
/****************************************************************
 *  Filename: Extensions.cs
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
using System.Text;
using Pod.Grpc.Messages.Shared;
using Pod.Services;

namespace Pod.Grpc.ShellHost.Server
{
    public static class Extensions
    {
        public static ClientNotificationEvent ToNotificationMessage(this ClientCommandType clientCommandType)
        {
            switch(clientCommandType)
            {
                case ClientCommandType.UpdateServerSettings:
                    return ClientNotificationEvent.UpdateServerSettings;
                case ClientCommandType.UpdateClientSettings:
                    return ClientNotificationEvent.UpdateClientSettings;
                case ClientCommandType.GetLoginRequest:
                    return ClientNotificationEvent.CheckLoginRequest;
                case ClientCommandType.UpdateSession:
                    return ClientNotificationEvent.UpdateSessionState;
                case ClientCommandType.Unset:
                    return ClientNotificationEvent.Unset;
                case ClientCommandType.SendHeartbeat:
                    return ClientNotificationEvent.SendHeartbeat;;
                default:
                    throw new ArgumentOutOfRangeException(nameof(clientCommandType), clientCommandType, null);
            }
        }
    }
}
