#region Licence
/****************************************************************
 *  Filename: RegexRule.cs
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

using System.Text.RegularExpressions;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if string satisfies specified regular expression.
    /// </summary>
    public class RegexRule : SanityRule<string>
    {
        private readonly string _pattern;

        public RegexRule(string pattern)
        {
            _pattern = pattern;
        }

        public override string RuleName => "RegexRule";
        public override bool CheckSanity(string value)
        {
            var regex = new Regex(_pattern);
            return regex.IsMatch(value);
        }
    }
}
