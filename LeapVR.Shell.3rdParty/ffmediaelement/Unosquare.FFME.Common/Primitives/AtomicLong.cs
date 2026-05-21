#region Licence
/****************************************************************
 *  Filename: AtomicLong.cs
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
namespace Unosquare.FFME.Primitives
{
    /// <summary>
    /// Fast, atomioc long combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicLong : AtomicTypeBase<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        /// <param name="initialValue">if set to <c>true</c> [initial value].</param>
        public AtomicLong(long initialValue)
            : base(initialValue)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        public AtomicLong()
            : base(0)
        {
            // placeholder
        }

        /// <summary>
        /// COnverts froma long value to the target type.
        /// </summary>
        /// <param name="backingValue">The backing value.</param>
        /// <returns>
        /// The value converted form a long value
        /// </returns>
        protected override long FromLong(long backingValue)
        {
            return backingValue;
        }

        /// <summary>
        /// Converts from the target type to a long value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The value converted to a long value
        /// </returns>
        protected override long ToLong(long value)
        {
            return value;
        }
    }
}
