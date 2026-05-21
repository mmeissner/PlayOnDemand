#region Licence
/****************************************************************
 *  Filename: MaxTimeSpanRule.cs
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
using System;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    public class MaxTimeSpanRule : SanityRule<TimeSpan>
    {
        private readonly TimeSpan _maxValue;

        public MaxTimeSpanRule(TimeSpan maxValue)
        {
            _maxValue = maxValue;
        }

        public override string RuleName => nameof(MinTimeSpanRule);
        public override bool CheckSanity(TimeSpan value)
        {
            return value <= _maxValue;
        }
    }
}
