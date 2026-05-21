#region Licence
/****************************************************************
 *  Filename: ILoginDecisionResult.cs
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

using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    /// <summary>
    /// Result of <see cref="ILoginDecision"/> made.
    /// </summary>
    public interface ILoginDecisionResult
    {
        /// <summary>
        /// <see cref="LoginDecisionResultType"/> indicating if session was started, or not.
        /// </summary>
        LoginDecisionResultType Result { get; }

        /// <summary>
        /// <see cref="ISessionState"/> of newly started session, if <see cref="Result"/> is <see cref="LoginDecisionResultType.SessionStarted"/>, null otherwise.
        /// </summary>
        IUISession Session { get; }
    }

}
