#region Licence
/****************************************************************
 *  Filename: MinDecimalValueRule.cs
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
    /// <see cref="SanityRule{T}"/> that checks if deciaml value is no lower than specified limit.
    /// </summary>
    public class MinDecimalValueRule : SanityRule<decimal>
    {
        private readonly decimal _minValue;
        public override string RuleName => "MinDecimalValueRule";

        public MinDecimalValueRule(decimal minValue)
        {
            _minValue = minValue;
        }

        public override bool CheckSanity(decimal value)
        {
            return value >= _minValue;
        }
    }
}
