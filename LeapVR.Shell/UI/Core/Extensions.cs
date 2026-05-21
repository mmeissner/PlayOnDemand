#region Licence
/****************************************************************
 *  Filename: Extensions.cs
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
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Core
{
    public static class Extensions
    {
        public static int CompareTo(ITabItemScreen tav, object other)
        {
            var myCategory = tav;
            var comparedToCategory = (ITabItemScreen)other;
            //Check by Type Order
            if (comparedToCategory.DisplayOrder > myCategory.DisplayOrder)
            {
                return -1;
            }
            return comparedToCategory.DisplayOrder < myCategory.DisplayOrder ? 1 : string.CompareOrdinal(myCategory.DisplayName, comparedToCategory.DisplayName);
            //Check now by name
        }
    }
}
