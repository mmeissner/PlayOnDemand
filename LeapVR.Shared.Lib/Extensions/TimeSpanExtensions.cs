#region Licence
/****************************************************************
 *  Filename: TimeSpanExtensions.cs
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
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Multiplies <see cref="TimeSpan"/> with <see cref="long"/>, producing <see cref="TimeSpan"/> as result.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="multiplier"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        public static TimeSpan Multiply(this TimeSpan self, long multiplier)
        {
            return TimeSpan.FromTicks(self.Ticks * multiplier);
        }

        /// <summary>
        /// Multiplies <see cref="TimeSpan"/> with <see cref="double"/>, producing <see cref="TimeSpan"/> as result.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="multiplier"></param>
        /// <returns><see cref="TimeSpan"/></returns>
        public static TimeSpan Multiply(this TimeSpan self, double multiplier)
        {
            return TimeSpan.FromTicks((long) (self.Ticks * multiplier));
        }

        /// <summary>
        /// Divides <see cref="TimeSpan"/> by <see cref="long"/>, producing <see cref="TimeSpan"/> as result.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        /// <exception cref="DivideByZeroException"></exception>
        public static TimeSpan Divide(this TimeSpan self, long divisor)
        {
            return TimeSpan.FromTicks(self.Ticks / divisor);
        }

        /// <summary>
        /// Divides cleanly one <see cref="TimeSpan"/> by another <see cref="TimeSpan"/>, producing <see cref="long"/> as result.
        /// Throws <see cref="InvalidOperationException"/> if values cannot be cleanly divided (i.e. Ticks, the smallest unit in <see cref="TimeSpan"/>, cannot be cleanly divided).
        /// </summary>
        /// <param name="self"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="DivideByZeroException"></exception>
        public static long DivideClean(this TimeSpan self, TimeSpan divisor)
        {

            if (self.Ticks % divisor.Ticks != 0)
            {
                throw new InvalidOperationException("Cannot do clean division; self is not multiple of divisor.");
            }

            return self.Ticks / divisor.Ticks;
        }

        /// <summary>
        /// Divides one <see cref="TimeSpan"/> by another <see cref="TimeSpan"/>, producing <see cref="decimal"/> as result.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        /// <exception cref="DivideByZeroException"></exception>
        public static decimal DivideDecimal(this TimeSpan self, TimeSpan divisor)
        {
            return (decimal) self.Ticks / divisor.Ticks;
        }

        /// <summary>
        /// Returns true if value is multiply of quant (there is such integer x that x*quant == value).
        /// </summary>
        /// <param name="self"></param>
        /// <param name="quant"></param>
        /// <returns>Boolean indicating if value is multiply of quant</returns>
        public static bool IsMultiply(this TimeSpan self, TimeSpan quant)
        {
            return self.Ticks % quant.Ticks == 0;
        }

        /// <summary>
        /// Returns remaining value after dividing value by specified divisor (TimeSpan).
        /// </summary>
        /// <param name="self"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static TimeSpan Modulo(this TimeSpan self, TimeSpan divisor)
        {
            return new TimeSpan(self.Ticks % divisor.Ticks);
        }

        /// <summary>
        /// Equates given <see cref="self"/> value to closest multiplicity of <see cref="quant"/> that is equal or bigger than <see cref="timeSpan"/>.
        /// <see cref="quant"/> must be positive, otherwise <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="quant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan Ceiling(this TimeSpan self, TimeSpan quant)
        {
            if (quant <= TimeSpan.Zero)
            {
                throw new ArgumentException($"quant must be positive TimeSpan.");
            }

            var ticksModulo = self.Ticks % quant.Ticks;
            return ticksModulo == 0
                ? self
                : TimeSpan.FromTicks(self.Ticks - ticksModulo + (ticksModulo > 0 ? quant.Ticks : 0));
        }

        /// <summary>
        /// Equates given <see cref="self"/> value to closest multiplicity of <see cref="quant"/> that is equal or lower than <see cref="timeSpan"/>.
        /// <see cref="quant"/> must be positive, otherwise <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="quant"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TimeSpan Floor(this TimeSpan self, TimeSpan quant)
        {
            if (quant <= TimeSpan.Zero)
            {
                throw new ArgumentException($"quant must be positive TimeSpan.");
            }

            var ticksModulo = self.Ticks % quant.Ticks;
            return ticksModulo == 0
                ? self
                : TimeSpan.FromTicks(self.Ticks - ticksModulo - (ticksModulo > 0 ? 0 : quant.Ticks));
        }
    }
}
