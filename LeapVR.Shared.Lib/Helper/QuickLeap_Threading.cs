#region Licence
/****************************************************************
 *  Filename: QuickLeap_Threading.cs
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
using System.Threading;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        private const int IntFalse = 0;
        private const int IntTrue = 1;

        /// <summary>
        /// Operates on int flag using <see cref="Interlocked"/> atomic Exchange operation.
        /// Atomicaly reads current value of flag and sets new value if requested.
        /// </summary>
        /// <param name="flag">Integer flag to operate on</param>
        /// <param name="setValueTo">If not null will atomicaly exchange old value to this value</param>
        /// <param name="requiredValue">If not null will throw exception if exchanged value didn't satisfy requirement</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="requiredValue"/> is not null, but value stored in flag couldn't satisfy this requirement</exception>
        public static bool OperateInterlockedFlag(ref int flag, bool? setValueTo = null, bool? requiredValue = null)
        {
            var previousValue = _OperateInterlockedFlag(ref flag, setValueTo);

            if (requiredValue == null || requiredValue == requiredValue.Value)
            {
                return previousValue;
            }

            throw new InvalidOperationException("Value requirement was not fulfilled.");
        }

        private static bool _OperateInterlockedFlag(ref int flag, bool? setValueTo = null)
        {
            if (setValueTo == null)
            {
                return flag == IntTrue;
            }

            var previousValue = Interlocked.Exchange(ref flag, setValueTo.Value ? IntTrue : IntFalse) == IntTrue;
            return previousValue;
        }
    }
}
