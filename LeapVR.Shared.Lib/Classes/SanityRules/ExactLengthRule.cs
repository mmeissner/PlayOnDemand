#region Licence
/****************************************************************
 *  Filename: ExactLengthRule.cs
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
    /// <see cref="SanityRule{T}"/> that checks if string is of exact length.
    /// </summary>
    class ExactLengthRule : SanityRule<string>
    {
        private readonly int _length;

        public ExactLengthRule(int length)
        {
            _length = length;
        }

        public override string RuleName => "ExactLengthRule";
        public override bool CheckSanity(string value)
        {
            return value.Length == _length;
        }
    }
}
