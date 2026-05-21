#region Licence
/****************************************************************
 *  Filename: ICollectionChange.cs
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

namespace LeapVR.Shared.Lib.Interfaces
{
    /// <summary>
    /// Contains difference between collection state in different moments of time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICollectionChange<T>
    {
        /// <summary>
        /// Collection of items that were added to the collection.
        /// </summary>
        IEnumerable<T> AddedItems { get; }

        /// <summary>
        /// Collection of items that were removed from the collection.
        /// </summary>
        IEnumerable<T> RemovedItems { get; }
    }
}
