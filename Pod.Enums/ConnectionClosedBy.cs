#region Licence
/****************************************************************
 *  Filename: ConnectionClosedBy.cs
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
namespace Pod.Enums 
{
    /// <summary>
    /// Describes the cause of an Disconnect
    /// </summary>
    public enum ConnectionClosedBy
    {
        /// <summary>
        /// Invalid Value
        /// </summary>
        Undefined =0,
        /// <summary>
        /// The Client was disconnecting gracefully
        /// </summary>
        Client = 10,
        /// <summary>
        /// The Connection TimedOut as the client did not send a keep alive in time
        /// </summary>
        Timeout = 20,
        /// <summary>
        /// The Client Reconnected and the old connection was closed
        /// </summary>
        Reconnect = 30,
        /// <summary>
        /// The Client was Disconnected without notifying the server and tried to connect again before a timeout occured
        /// </summary>
        UngracefulDisconnect = 40,
    }
}