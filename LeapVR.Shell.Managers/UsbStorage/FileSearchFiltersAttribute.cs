#region Licence
/****************************************************************
 *  Filename: FileSearchFiltersAttribute.cs
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

namespace LeapVR.Shell.Managers.UsbStorage
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FileSearchFiltersAttribute : Attribute
    {
        public string[] SearchFilters { get; }

        public FileSearchFiltersAttribute(string[] searchFilters)
        {
            SearchFilters = searchFilters;
        }
    }
}
