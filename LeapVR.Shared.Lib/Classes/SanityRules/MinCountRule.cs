#region Licence
/****************************************************************
 *  Filename: MinCountRule.cs
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
    /// <see cref="SanityRule{T}"/> that checks if <see cref="IEnumerable{T}"/> contains number of elements not lower than specified limit.
    /// </summary>
    public class MinCountRule<TItem> : SanityRule<IEnumerable<TItem>>
    {
        private readonly int _minCount;
        public override string RuleName => "MinCountRule";

        public MinCountRule(int minCount)
        {
            _minCount = minCount;
        }

        public override bool CheckSanity(IEnumerable<TItem> value)
        {
            return value.Count() >= _minCount;
        }
    }
}
