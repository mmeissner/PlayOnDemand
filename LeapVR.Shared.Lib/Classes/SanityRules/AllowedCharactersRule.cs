#region Licence
/****************************************************************
 *  Filename: AllowedCharactersRule.cs
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
using System.Collections.Generic;
using System.Linq;
using LeapVR.Shared.Lib.Helper;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if string contains only specified allowed characters.
    /// </summary>
    public class AllowedCharactersRule : SanityRule<string>
    {
        private readonly Func<char, bool> _predicate;

        public AllowedCharactersRule(Func<char, bool> predicate)
        {
            QuickLeap.AssertNotNull(predicate);
            _predicate = predicate;
        }

        public AllowedCharactersRule(IEnumerable<char> allowedCharacters)
        {
            QuickLeap.AssertNotNull(allowedCharacters);
            _predicate = allowedCharacters.Contains;
        }

        public override string RuleName => "AllowedCharactersRule";
        public override bool CheckSanity(string value)
        {
            return value.All(_predicate);
        }
    }
}
