#region Licence
/****************************************************************
 *  Filename: QuickLeap_InputQuality.cs
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
using System.Collections;
using System.Linq;
using LeapVR.Shared.Lib.Classes.SanityRules;
using static System.String;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        /// <summary>
        /// Asserts that no parameter is null.
        /// Throws <see cref="ArgumentNullException"/> if any argument is null.
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AssertNotNull(params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException($"values[{i}]", $"Value (at i = {i}) is null, while expected not to be null.");
                }
            }
        }

        /// <summary>
        /// Asserts that no parameter is not null or whitespace.
        /// Throws <see cref="ArgumentNullException"/> if any argument is null.
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AssertNotNull(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (IsNullOrWhiteSpace(values[i]))
                {
                    throw new ArgumentNullException($"values[{i}]", $"Value (at i = {i}) is null or empty, while expected not to be not.");
                }
            }
        }

        /// <summary>
        /// Asserts the Timespan to be not zero.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AssertNotZero(params TimeSpan[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == TimeSpan.Zero)
                {
                    throw new ArgumentNullException($"values[{i}]", $"Timespan Value (at i = {i}) is Zero, while expected not to be.");
                }
            }
        }
        /// <summary>
        /// Asserts that no parameter is null, nor any value on any collection specified as parameter is null either.
        /// Throws <see cref="ArgumentNullException"/> if any argument or value on any collection specified as parameter is null.
        /// </summary>
        /// <param name="values"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AssertNotNullEx(params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                var current = values[i];
                if (current == null)
                {
                    throw new ArgumentNullException($"values[{i}]", $"Value (at i = {i}) is null, while expected not to be null.");
                }

                if (current is IEnumerable enumerable)
                {
                    var castedEnumerable = enumerable.Cast<object>().ToList();
                    for (int j = 0; j < castedEnumerable.Count; j++)
                    {
                        if (castedEnumerable.ElementAt(j) == null)
                        {
                            throw new ArgumentNullException($"values[{i}]", $"IEnumerable (at i = {i}) have at least one null object (at j = {j}) while expected to be all not-null.");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks if all <see cref="string"/> values specified as parameters are not null nor empty.
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Boolean indicating if any value is null or empty</returns>
        public static bool CheckNotNullOrEmpty(params string[] values)
        {
            return !values.Any(IsNullOrEmpty);
        }

        /// <summary>
        /// Checks if provided <see cref="value"/> passes check with provided <see cref="SanityRule{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Value to check sanity of</param>
        /// <param name="rule">Rule to use when checking sanity of <see cref="value"/></param>
        public static void AssertSanity<T>(T value, SanityRule<T> rule)
        {
            AssertNotNull(rule);

            if (!rule.CheckSanity(value))
            {
                throw new InvalidOperationException($"Cannot assert sanity of value `{value?.ToString() ?? "<null>"}` (of type `{value?.GetType()}`); Sanity rule `{rule.RuleName}` check faled.");
            }
        }
    }
}
