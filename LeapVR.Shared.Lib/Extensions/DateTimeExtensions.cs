#region Licence
/****************************************************************
 *  Filename: DateTimeExtensions.cs
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

namespace LeapVR.Shared.Lib.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Equates given <see cref="self"/> value to closest multiplicity of <see cref="denominator"/> that is equal or lower than <see cref="self"/>.
        /// <see cref="denominator"/> must be positive, otherwise <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static DateTime Floor(this DateTime self, TimeSpan denominator)
        {
            if (denominator <= TimeSpan.Zero)
            {
                throw new ArgumentException("Denominator must be positive TimeSpan.");
            }

            var ticksModulo = self.Ticks % denominator.Ticks;
            return ticksModulo == 0
                ? self
                : new DateTime(self.Ticks - ticksModulo - (ticksModulo > 0 ? 0 : denominator.Ticks));
        }

        
        /// <summary>
        /// Equates given <see cref="self"/> value to closest multiplicity of <see cref="denominator"/> that is equal or bigger than <see cref="self"/>.
        /// <see cref="denominator"/> must be positive, otherwise <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static DateTime Ceiling(this DateTime self, TimeSpan denominator)
        {
            if (denominator <= TimeSpan.Zero)
            {
                throw new ArgumentException("Denominator must be positive TimeSpan.");
            }

            var ticksModulo = self.Ticks % denominator.Ticks;
            return ticksModulo == 0
                    ? self
                    : new DateTime(self.Ticks - ticksModulo + (ticksModulo > 0 ? denominator.Ticks : 0));
        }
    }
}
