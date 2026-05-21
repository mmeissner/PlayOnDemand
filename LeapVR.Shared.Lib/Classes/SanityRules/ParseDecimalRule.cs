#region Licence
/****************************************************************
 *  Filename: ParseDecimalRule.cs
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
using System.Globalization;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if string can be parsed as <see cref="decimal"/> using <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public class ParseDecimalRule : SanityRule<string>
    {
        private NumberStyles? _numberStyles;

        public ParseDecimalRule(NumberStyles? numberStyles = null)
        {
            _numberStyles = numberStyles;
        }

        public override string RuleName => "ParseDecimalRule";
        public override bool CheckSanity(string value)
        {
            decimal result;

            return _numberStyles.HasValue
                ? Decimal.TryParse(value, _numberStyles.Value, CultureInfo.InvariantCulture, out result)
                : Decimal.TryParse(value, out result);
        }
    }
}
