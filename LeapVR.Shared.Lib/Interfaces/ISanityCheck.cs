#region Licence
/****************************************************************
 *  Filename: ISanityCheck.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shared.Lib.Interfaces
{
    /// <summary>
    /// Single sanity check, that can cover multiple values and multiple rules.
    /// </summary>
    public interface ISanityCheck
    {
        /// <summary>
        /// Checks sanity of choosen data set.
        /// </summary>
        /// <param name="errorMessage">If check failed is not null, contains message explaining reason of failure.</param>
        /// <returns>Boolean indicating success/failure.</returns>
        bool Check(out string errorMessage);
    }
}
