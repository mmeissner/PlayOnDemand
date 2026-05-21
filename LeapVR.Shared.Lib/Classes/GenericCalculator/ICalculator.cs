#region Licence
/****************************************************************
 *  Filename: ICalculator.cs
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
    /// <summary>
    /// Generic class able to perform simple calculations on generic type <see cref="T"/>, such as addition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICalculator<T>
    {
        T Zero { get; }

        T Add(T a, T b);
    }

    public static class Calculator
    {
        public static ICalculator<T> Get<T>()
        {
            var type = typeof(T);

            if (type == typeof(decimal))
            {
                return new DecimalCalculator() as ICalculator<T>;
            }

            if (type == typeof(TimeSpan))
            {
                return new TimeSpanCalculator() as ICalculator<T>;
            }

            throw new InvalidOperationException($"Unsupported type = `{type}`.");
        }
    }
}
