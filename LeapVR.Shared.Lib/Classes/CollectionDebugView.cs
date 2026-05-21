#region Licence
/****************************************************************
 *  Filename: CollectionDebugView.cs
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
using System.Diagnostics;

namespace LeapVR.Shared.Lib.Classes
{
    public sealed class CollectionDebugView<T>
    {
        private readonly ICollection<T> m_collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            m_collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[m_collection.Count];
                m_collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}