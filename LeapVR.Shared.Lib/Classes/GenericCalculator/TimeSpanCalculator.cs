#region Licence
/****************************************************************
 *  Filename: TimeSpanCalculator.cs
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

namespace LeapVR.Shared.Lib.Classes.GenericCalculator
{
    public class TimeSpanCalculator : ICalculator<TimeSpan>
    {
        public TimeSpan Zero => TimeSpan.Zero;

        public TimeSpan Add(TimeSpan a, TimeSpan b) => a + b;
    }
}
