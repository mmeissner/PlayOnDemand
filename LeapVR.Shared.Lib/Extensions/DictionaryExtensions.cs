#region Licence
/****************************************************************
 *  Filename: DictionaryExtensions.cs
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

namespace LeapVR.Shared.Lib.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Generates reverse lookup dictionary (Dictionary with keys/values swaped).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="self"/> contains duplicated values.</exception>
        public static IDictionary<TValue, TKey> ReverseLookup<TKey, TValue>(this IDictionary<TKey, TValue> self)
        {
            var result = new Dictionary<TValue, TKey>();
            foreach (var kv in self)
            {
                if (result.ContainsKey(kv.Value))
                {
                    throw new InvalidOperationException($"Source dictionary contains duplicated values; It's impossible to generate ReverseLookup dictionary.");
                }
                result.Add(kv.Value, kv.Key);
            }
            return result;
        }
    }
}
