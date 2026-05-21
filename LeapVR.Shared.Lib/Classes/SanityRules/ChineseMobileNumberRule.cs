#region Licence
/****************************************************************
 *  Filename: ChineseMobileNumberRule.cs
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
using System.Linq;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if string is valid Chinese mobile phone number
    /// </summary>
    public class ChineseMobileNumberRule : SanityRule<string>
    {
        public override string RuleName => "ChineseMobileNumberRule";
        public override bool CheckSanity(string value)
        {
            return value.Length == 11
                   && value.All(Char.IsDigit);
        }
    }
}
