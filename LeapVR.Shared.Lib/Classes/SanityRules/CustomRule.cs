#region Licence
/****************************************************************
 *  Filename: CustomRule.cs
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

using System;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that can check any custom condition given its predicate.
    /// </summary>
    public class CustomRule<T> : SanityRule<T>
    {
        private readonly Func<T, bool> _predicate;
        public override string RuleName { get; }

        public CustomRule(Func<T, bool> predicate, string ruleName = null)
        {
            _predicate = predicate;
            RuleName = $"CustomRule{(ruleName != null ? $"-{ruleName}" : string.Empty)}";
        }

        public override bool CheckSanity(T value)
        {
            return _predicate(value);
        }
    }
}
