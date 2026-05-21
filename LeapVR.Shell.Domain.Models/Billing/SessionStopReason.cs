#region Licence
/****************************************************************
 *  Filename: SessionStopReason.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Domain.Models.Billing
{
    /// <summary>
    /// Defines the reason of <see cref="IUISession"/> stop.
    /// </summary>
    public enum SessionStopReason
    {
        Unknown = 0,

        /// <summary>
        /// User requested logout clicking Logout button on the Station itself.
        /// </summary>
        StationLogout = 10,
        /// <summary>
        /// Station decided to logout user due to his inactivity.
        /// </summary>
        StationInactivity = 11,

        /// <summary>
        /// Server stoped the session after station abaddoned it for defined time (e.g. due to internet/power outage).
        /// </summary>
        AbandonedSession = 20,
        /// <summary>
        /// User has been blocked while having Session running.
        /// </summary>
        UserBlocked = 21,

        /// <summary>
        /// Session has been stoped becouse Station notified server that it is shutting down.
        /// </summary>
        StationShutdown = 22,

        /// <summary>
        /// One of the sessions set limit was reached.
        /// </summary>
        SessionLimitReached = 30,
    }

}
