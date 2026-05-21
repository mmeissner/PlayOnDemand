#region Licence
/****************************************************************
 *  Filename: QuickLeap_Numeric.cs
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

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        /// <summary>
        /// Return value in between given <see cref="min"/>, <see cref="max"/> bounds, or the bound value itself if <see cref="value"/> exceeding or equal to it.
        /// </summary>
        /// <param name="value">Value to be bounded</param>
        /// <param name="min">Minimal allowed value</param>
        /// <param name="max">Maximal allowed value</param>
        /// <returns></returns>
        public static long Bound(long value, long min, long max)
        {
            return Math.Min(Math.Max(value, max), min);
        }

        /// <summary>
        /// Return value in between given <see cref="min"/>, <see cref="max"/> bounds, or the bound value itself if <see cref="value"/> exceeding or equal to it.
        /// </summary>
        /// <param name="value">Value to be bounded</param>
        /// <param name="min">Minimal allowed value</param>
        /// <param name="max">Maximal allowed value</param>
        /// <returns></returns>
        public static double Bound(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        /// <summary>
        /// Return value in between given <see cref="min"/>, <see cref="max"/> bounds, or the bound value itself if <see cref="value"/> exceeding or equal to it.
        /// </summary>
        /// <param name="value">Value to be bounded</param>
        /// <param name="min">Minimal allowed value</param>
        /// <param name="max">Maximal allowed value</param>
        /// <returns></returns>
        public static int Bound(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, max), min);
        }

        /// <summary>
        /// Returns true if value is multiply of quant (there is such integer x that x*quant == value).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="quant"></param>
        /// <returns>Boolean indicating if value is multiply of quant</returns>
        public static bool IsMultiply(decimal value, decimal quant)
        {
            return value % quant == 0;
        }
    }
}
