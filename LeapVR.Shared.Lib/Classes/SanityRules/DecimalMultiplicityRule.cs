#region Licence
/****************************************************************
 *  Filename: DecimalMultiplicityRule.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-9-22
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

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    public class DecimalMultiplicityRule : SanityRule<decimal>
    {
        private readonly decimal _minQuant;

        public DecimalMultiplicityRule(decimal minQuant)
        {
            if (minQuant == 0)
            {
                throw new InvalidOperationException($"minQuant value cannot be 0.");
            }

            _minQuant = minQuant;
        }

        public override string RuleName => nameof(DecimalMultiplicityRule);
        public override bool CheckSanity(decimal value)
        {
            return value % _minQuant == 0;
        }
    }
}
