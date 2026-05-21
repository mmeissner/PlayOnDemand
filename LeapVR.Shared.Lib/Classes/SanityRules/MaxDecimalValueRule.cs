#region Licence
/****************************************************************
 *  Filename: MaxDecimalValueRule.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    public class MaxDecimalValueRule : SanityRule<decimal>
    {
        private readonly decimal _maxValue;
        public override string RuleName => "MaxDecimalValueRule";

        public MaxDecimalValueRule(decimal maxValue)
        {
            _maxValue = maxValue;
        }

        public override bool CheckSanity(decimal value)
        {
            return value <= _maxValue;
        }
    }
}
