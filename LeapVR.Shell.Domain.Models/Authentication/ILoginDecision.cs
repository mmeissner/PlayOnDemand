#region Licence
/****************************************************************
 *  Filename: ILoginDecision.cs
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

namespace LeapVR.Shell.Domain.Models.Authentication
{
    /// <summary>
    /// Represents <see cref="Decision"/> made to specific <see cref="Intention"/>.
    /// </summary>
    public interface ILoginDecision
    {
        ///// <summary>
        ///// <see cref="ILoginIntention"/> the decision releates to.
        ///// </summary>
        //ILoginIntention Intention { get; }

        /// <summary>
        /// Decision made (see <see cref="LoginDecisionType"/>).
        /// </summary>
        LoginDecisionType Decision { get; }
    }
}
