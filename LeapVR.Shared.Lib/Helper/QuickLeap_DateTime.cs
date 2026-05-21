#region Licence
/****************************************************************
 *  Filename: QuickLeap_DateTime.cs
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
using LeapVR.Shared.Lib.Classes.SanityRules;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        /// <summary>
        /// Returns <see cref="DateTime"/> that is earler than another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DateTime Earlier(DateTime a, DateTime b)
        {
            return a < b ? a : b;
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> that is later than another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DateTime Later(DateTime a, DateTime b)
        {
            return a > b ? a : b;
        }

        /// <summary>
        /// Returns <see cref="TimeSpan"/> that is smaller than another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static TimeSpan Min(TimeSpan a, TimeSpan b)
        {
            return a < b ? a : b;
        }

        /// <summary>
        /// Returns <see cref="TimeSpan"/> that is bigger than another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static TimeSpan Max(TimeSpan a, TimeSpan b)
        {
            return a > b ? a : b;
        }

        public static void AssertCorrectDateRange(DateTime fromDate, DateTime toDate, int minDays, int maxDays)
        {
            if (fromDate > toDate)
            {
                throw new InvalidOperationException($"fromDate = `{fromDate}` cannot be later than toDate = `{toDate}`.");
            }

            if (minDays > maxDays)
            {
                throw new InvalidOperationException($"`minDays` cannot be greater than `maxDays`.");
            }

            if (minDays < 0 || maxDays < 0)
            {
                throw new InvalidOperationException($"`minDays` and `maxDays` cannot be smaller than 0.");
            }

            if (fromDate.Date != fromDate || toDate.Date != toDate)
            {
                throw new InvalidOperationException($"fromDate = `{fromDate}` or toDate = `{toDate}` is not pure date. Pure date have only year, month and day set; hours, minutes, seconds, etc. are not set (e.g. 2001-06-01 00:00:00).");
            }

            var days = (toDate - fromDate).TotalDays;

            if (days > maxDays)
            {
                throw new InvalidOperationException($"Provided date range is longer than maxDays limit (days = `{days}`, maxDays = `{maxDays}`).");
            }

            if (days < minDays)
            {
                throw new InvalidOperationException($"Provided date range is shorter than min limit (days = `{days}`, maxDays = `{maxDays}`).");
            }
        }

        public static void AssertCorrectDateTimeRange(DateTime fromDateTime, DateTime toDateTime, TimeSpan minLength, TimeSpan maxLength)
        {
            if (fromDateTime > toDateTime)
            {
                throw new InvalidOperationException($"fromDateTime = `{fromDateTime}` cannot be later than toDateTime = `{toDateTime}`.");
            }

            if (minLength > maxLength)
            {
                throw new InvalidOperationException($"`minLength` cannot be longer than `maxLength`.");
            }

            if (minLength < TimeSpan.Zero || maxLength < TimeSpan.Zero)
            {
                throw new InvalidOperationException($"`minLength` and `maxLength` cannot be shorter than `00:00:00`.");
            }

            var length = toDateTime - fromDateTime;

            if (length > maxLength)
            {
                throw new InvalidOperationException($"Provided dateTime range is longer than maxLength limit (length = `{length}`, maxDays = `{maxLength}`).");
            }

            if (length < minLength)
            {
                throw new InvalidOperationException($"Provided dateTime range is shorter than minLength limit (length = `{length}`, maxDays = `{minLength}`).");
            }
        }

        /// <summary>
        /// Gets collection of dates (without hours, minutes, seconds, etc. parts) in specified dates range.
        /// </summary>
        /// <param name="dateFrom">Date from (included in output).</param>
        /// <param name="dateTo">Date to (included in output).</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetDatesInRange(DateTime dateFrom, DateTime dateTo)
        {
            AssertSanity(dateFrom, new PureDateRule());
            AssertSanity(dateTo, new PureDateRule());

            var days = (int)(dateTo - dateFrom).TotalDays;
            if (days < 0)
            {
                throw new ArgumentException($"dateFrom = `{dateFrom}` > dateTo = `{dateTo}`.");
            }

            return Enumerable
                .Range(0, days+1)
                .Select(q => dateFrom + TimeSpan.FromDays(q))
                .ToArray();
        }
    }
}
