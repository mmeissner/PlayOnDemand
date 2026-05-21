#region Licence
/****************************************************************
 *  Filename: CollectionChange.cs
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

using System.Collections.Generic;
using System.Linq;
using LeapVR.Shared.Lib.Interfaces;

namespace LeapVR.Shared.Lib.Objects
{
    public class CollectionChange<T> : ICollectionChange<T>
    {
        #region Properties & Fields

        public IEnumerable<T> AddedItems { get; }
        public IEnumerable<T> RemovedItems { get; }

        #endregion Properties & Fields

        #region Constructors

        public CollectionChange(IEnumerable<T> addedItems, IEnumerable<T> removedItems)
        {
            addedItems = addedItems ?? new T[0];
            removedItems = removedItems ?? new T[0];

            AddedItems = addedItems.Skip(0); // Skip(0) is a readonly collection hack
            RemovedItems = removedItems.Skip(0); // Skip(0) is a readonly collection hack
        }

        #endregion Constructors

        #region Methods

        //

        #endregion Methods
    }
}
