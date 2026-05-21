#region Licence
/****************************************************************
 *  Filename: EnumerableExtensions.cs
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
using System.Collections.Generic;
using System.Linq;

namespace LeapVR.Shared.Lib.Extensions
{
    public static class EnumerableExtensions
    {
        public static TimeSpan Sum<T>(this IEnumerable<T> self, Func<T, TimeSpan> selector)
        {
            return self.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }
    }
}
