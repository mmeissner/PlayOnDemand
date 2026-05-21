#region Licence
/****************************************************************
 *  Filename: ListExtension.cs
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
using System.Text;

namespace LeapVR.Shared.Lib.Extensions
{
    public static class ListExtension
    {
        public static void MoveItem<T>(this List<T> list,int oldIndex, int newIndex)
        {
            T removedItem = list[oldIndex];

            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--; 
            // the actual index could have shifted due to the removal
            list.Insert(newIndex, removedItem);
        }
    }
}
