#region Licence
/****************************************************************
 *  Filename: NotEmptyRule.cs
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

using System.Collections.Generic;
using System.Linq;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if <see cref="IEnumerable{T}"/> is not empty.
    /// Providing null will cause exception.
    /// </summary>
    public class NotEmptyRule<TItem> : SanityRule<IEnumerable<TItem>>
    {
        public override string RuleName => "NotEmptyRule";
        public override bool CheckSanity(IEnumerable<TItem> value)
        {
            return value.Any();
        }
    }
}
