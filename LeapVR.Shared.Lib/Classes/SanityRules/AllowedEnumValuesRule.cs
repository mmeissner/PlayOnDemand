#region Licence
/****************************************************************
 *  Filename: AllowedEnumValuesRule.cs
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

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    /// <summary>
    /// <see cref="SanityRule{T}"/> that checks if Enum value is one of specified allowed values.
    /// </summary>
    public class AllowedEnumValuesRule : SanityRule<Enum>
    {
        private readonly IEnumerable<Enum> _allowedValues;

        public override string RuleName => "AllowedEnumValuesRule";

        public AllowedEnumValuesRule(IEnumerable<Enum> allowedValues)
        {
            _allowedValues = allowedValues;
        }

        public override bool CheckSanity(Enum value)
        {
            return _allowedValues
                .Select(Convert.ToInt64)
                .Contains(Convert.ToInt64(value));
        }
    }
}
