#region Licence
/****************************************************************
 *  Filename: SessionState.cs
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
    /// The State of an Session
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// A new Session was requested
        /// </summary>
        Requested = 10,
        /// <summary>
        /// The Session was picked up by a Station Client
        /// </summary>
        Delivered = 20,
        /// <summary>
        /// The Session was Started on an Client
        /// </summary>
        Started = 30,
        /// <summary>
        /// The Session Ended on an Client
        /// </summary>
        Ended = 100,
        /// <summary>
        /// The Session was not accepted by the Client/User
        /// </summary>
        Canceled = 110,
        /// <summary>
        /// The Session could not be picked up by a client in time
        /// </summary>
        DeliveryTimeout = 120,
        /// <summary>
        /// A Response for an session accept/deny was not send in time by the client
        /// </summary>
        ResponseTimeout = 130,
    }

    public enum ConnectionRequestResult
    {
        Success,
        ServerMismatch,
        ConnectionIdMismatch,
        NetworkStateMismatch,
        ConnectionTimedOut,
        ConnectionStillAlive,
        OtherDeviceIdentityConnected,

    }
}
