#region Licence
/****************************************************************
 *  Filename: LoginDecisionResultType.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Authentication
{
    /// <summary>
    /// Type of result to <see cref="ILoginDecision"/> made.
    /// </summary>
    public enum LoginDecisionResultType
    {
        Unknown = 0,

        /// <summary>
        /// New session was started.
        /// </summary>
        SessionStarted = 1,

        /// <summary>
        /// Server confirmed cancellation of <see cref="ILoginIntention"/>.
        /// </summary>
        Canceled = 2,

        /// <summary>
        /// <see cref="ILoginDecision"/> was ignored, because related <see cref="ILoginIntention"/> has expired.
        /// </summary>
        IntentionExpired = 3,

        /// <summary>
        /// Server denied starting new session, because session rate changed meantime.
        /// </summary>
        SessionRateChanged = 4,

        /// <summary>
        /// Server denied starting new session, because this station have already running session.
        /// </summary>
        StationHaveRunningSession = 5,

        /// <summary>
        /// Server denied starting new session, because user intending to start session have already other session running on another station.
        /// </summary>
        UserHaveRunningSession = 6,

        /// <summary>
        /// Server denied starting new session, because user have insufficiency points balance.
        /// </summary>
        NotEnoughBalance = 7,

        /// <summary>
        /// Server denied starting new session, because user account is blocked.
        /// </summary>
        UserBlocked = 8,

        /// <summary>
        /// Server denied starting new session, because user account is not activated yet.
        /// </summary>
        UserNotActive = 9,
    }
}
