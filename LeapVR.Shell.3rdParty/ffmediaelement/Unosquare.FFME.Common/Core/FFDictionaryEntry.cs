#region Licence
/****************************************************************
 *  Filename: FFDictionaryEntry.cs
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
namespace Unosquare.FFME.Core
{
    using FFmpeg.AutoGen;

    /// <summary>
    /// An AVDictionaryEntry wrapper
    /// </summary>
    internal unsafe class FFDictionaryEntry
    {
        // This ointer is generated in unmanaged code.
#pragma warning disable SA1401 // Fields must be private
        internal readonly AVDictionaryEntry* Pointer;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Initializes a new instance of the <see cref="FFDictionaryEntry"/> class.
        /// </summary>
        /// <param name="entryPointer">The entry pointer.</param>
        public FFDictionaryEntry(AVDictionaryEntry* entryPointer)
        {
            Pointer = entryPointer;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key => Pointer != null ?
                    FFInterop.PtrToStringUTF8(Pointer->key) : null;

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value => Pointer != null ?
                    FFInterop.PtrToStringUTF8(Pointer->value) : null;
    }
}
