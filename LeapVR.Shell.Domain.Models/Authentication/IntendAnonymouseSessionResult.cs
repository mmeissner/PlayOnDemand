#region Licence
/****************************************************************
 *  Filename: IntendAnonymouseSessionResult.cs
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
using LeapVR.Shell.Domain.Models.Billing;

namespace LeapVR.Shell.Domain.Models.Authentication
{
    /// <summary>
    /// Defines result of notifying the server that Station requests to start new <see cref="INoBillingAnonymousSession"/>.
    /// </summary>
    public enum IntendAnonymousSessionResult
    {
        Unknown = 0,

        /// <summary>
        /// Anonymous session intention was succesfuly delivered to the server and server accepted it.
        /// Server will shortly notify station about new <see cref="ILoginIntention"/> arriving.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Station have already running session.
        /// </summary>
        StationHaveRunningSession = 2,

        /// <summary>
        /// Station have already active login intention with not decision made yet.
        /// </summary>
        StationHaveActiveIntention = 3,

        /// <summary>
        /// This station is not allowed to start anonymous session.
        /// </summary>
        AnonymousSessionsNotAccepted = 4,
    }
}
