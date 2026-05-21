#region Licence
/****************************************************************
 *  Filename: NoEdgeWhitespacesRule.cs
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
    /// <see cref="SanityRule{T}"/> that checks if string doesn't start or end with whitespace (Char.IsWhiteSpace) characters.
    /// </summary>
    public class NoEdgeWhitespacesRule : SanityRule<string>
    {
        public override string RuleName => "NoEdgeWhitespacesRule";
        public override bool CheckSanity(string value)
        {
            if (value.Length == 0)
            {
                return true;
            }

            var first = value.First();
            var last = value.Last();

            return !Char.IsWhiteSpace(first) && !Char.IsWhiteSpace(last);
        }
    }
}
