#region Licence
/****************************************************************
 *  Filename: SanityRule.cs
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

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// Base class for validation of input sanity.
    /// </summary>
    public abstract class SanityRule<T>
    {
        public abstract string RuleName { get; }
        public abstract bool CheckSanity(T value);
    }
}
