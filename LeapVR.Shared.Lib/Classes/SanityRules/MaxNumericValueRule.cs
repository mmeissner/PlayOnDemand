#region Licence
/****************************************************************
 *  Filename: MaxNumericValueRule.cs
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
    /// <see cref="SanityRule{T}"/> that checks if numeric signed value up to long.MaxValue is no bigger than specified limit.
    /// </summary>
    public class MaxNumericValueRule : SanityRule<long>
    {
        private readonly long _maxValue;
        public override string RuleName => "MaxNumericValueRule";

        public MaxNumericValueRule(long maxValue)
        {
            _maxValue = maxValue;
        }

        public override bool CheckSanity(long value)
        {
            return value <= _maxValue;
        }
    }
}
