#region Licence
/****************************************************************
 *  Filename: StopReason.cs
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
namespace Pod.Enums {
    /// <summary>
    /// Reason why session has stopped.
    /// </summary>
    public enum StopReason
    {
        Unknown = 0,

        /// <summary>
        /// User explicitly used the `Logout` button to end the session.
        /// </summary>
        UserLogout = 10,

        /// <summary>
        /// Api Request to `Logout` and to end the session.
        /// </summary>
        RemoteLogout = 20,

        /// <summary>
        /// Station detected that HMD was inactive for specified amount of time, therefore session has ended automatically.
        /// </summary>
        Inactivity = 30,

        /// <summary>
        /// Session has been stopped because Station informed server that it is shutting down.
        /// </summary>
        StationShutdown = 50,

        /// <summary>
        /// Session has been stopped because the session limit was reached
        /// </summary>
        LimitReached = 100,

        /// <summary>
        /// Session was not ended explicitly in any way and Station stopped sending Session Heartbeat requests. After given timeout time session is considered as abandoned, treated effectively as ended.
        /// </summary>
        ConnectionLoss = 110,

        /// <summary>
        /// Session has been stopped because subscription has ended during session.
        /// </summary>
        SubscriptionEnded = 120
    }
}