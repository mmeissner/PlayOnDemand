#region Licence
/****************************************************************
 *  Filename: ChineseIdNumberRule.cs
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

using System.Linq;
using System.Text.RegularExpressions;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if string is valid Chinese ID card number (18 characters only supported).
    /// </summary>
    public class ChineseIdNumberRule : SanityRule<string>
    {
        public override string RuleName => "ChineseIdNumberRule";
        public override bool CheckSanity(string value)
        {
            var regex = new Regex(@"^\d{17}[\dXx]$");
            if (!regex.IsMatch(value))
            {
                return false;
            }

            var digits = value
                .Take(17)
                .Reverse()
                .Select(q => byte.Parse(q.ToString()))
                .ToArray();

            var checksumChar = value.Last();
            var expectedChecksum = new[] {'x', 'X'}.Contains(checksumChar)
                ? 10
                : int.Parse(checksumChar.ToString());

            int checksum = 0;
            for (int i = 0; i <= digits.Length - 1; i++)
            {
                var digit = digits[i];
                var weight = (2 << i) % 11;

                checksum += digit * weight;
            }

            checksum = (12 - checksum % 11) % 11;
            return checksum == expectedChecksum;
        }
    }
}
