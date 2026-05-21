#region Licence
/****************************************************************
 *  Filename: PureDateRule.cs
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
    public class PureDateRule : SanityRule<DateTime>
    {
        public override string RuleName => nameof(PureDateRule);
        public override bool CheckSanity(DateTime value)
        {
            return value == value.Date;
        }
    }
}
