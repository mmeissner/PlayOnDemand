#region Licence
/****************************************************************
 *  Filename: StringExtensions.cs
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
using LeapVR.Shared.Lib.Helper;

namespace LeapVR.Shared.Lib.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims provided <see cref="trimString"/> from the end of string.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trimString"></param>
        /// <returns></returns>
        public static string TrimEnd(this string self, string trimString)
        {
            QuickLeap.AssertNotNull(self, trimString);

            if (!self.EndsWith(trimString))
            {
                return self;
            }

            return self.Remove(self.LastIndexOf(trimString, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns null when string is empty, otherwise returns original value.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string NullIfEmpty(this string self)
        {
            return self == string.Empty ? null : self;
        }

        /// <summary>
        /// Returns empty string when string is null, otherwise returns original value.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string EmptyIfNull(this string self)
        {
            return self ?? string.Empty;
        }
    }
}
