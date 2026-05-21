#region Licence
/****************************************************************
 *  Filename: DateTimeExtensions.cs
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

namespace SteamWebAPI2.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this long unixTimeStamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(unixTimeStamp);
        }

        public static long ToUnixTimeStamp(this DateTime dateTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            TimeSpan timeSpanSinceOrigin = dateTime.Subtract(origin);

            return Convert.ToInt64(timeSpanSinceOrigin.TotalSeconds);
        }
    }
}