#region Licence
/****************************************************************
 *  Filename: MediaTypeDictionary.cs
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
    using Shared;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a very simple dictionary for MediaType keys
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public sealed class MediaTypeDictionary<TValue>
        : Dictionary<MediaType, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeDictionary{TValue}"/> class.
        /// </summary>
        public MediaTypeDictionary()
            : base(Enum.GetValues(typeof(MediaType)).Length)
        {
            // placeholder
        }

        /// <summary>
        /// Gets or sets the item with the specified key.
        /// return the default value of the value type when the key does not exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item</returns>
        public new TValue this[MediaType key]
        {
            get => ContainsKey(key) == false ? default : base[key];
            internal set => base[key] = value;
        }
    }
}
