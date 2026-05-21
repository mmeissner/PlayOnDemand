#region Licence
/****************************************************************
 *  Filename: EnumerableRuleSet.cs
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

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if all elements in provided <see cref="IEnumerable{T}"/> satisfies given set of <see cref="SanityRule{T}"/>.
    /// </summary>
    public class EnumerableRuleSet<TItem> : SanityRule<IEnumerable<TItem>>
    {
        private readonly IEnumerable<SanityRule<TItem>> _rules;
        public override string RuleName => "EnumerableRuleSet";

        public EnumerableRuleSet(IEnumerable<SanityRule<TItem>> rules)
        {
            _rules = rules;
        }

        public override bool CheckSanity(IEnumerable<TItem> value)
        {
            foreach (var item in value)
            {
                foreach (var rule in _rules)
                {
                    if (!rule.CheckSanity(item))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
