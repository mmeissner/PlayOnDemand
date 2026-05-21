#region Licence
/****************************************************************
 *  Filename: ICategoryProvider.cs
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
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Categories
{
    public interface ICategoryProvider
    {
        IAppCategory GetOrCreateAppCategory(string identifier);
        /// <summary>
        /// Gets all known Categories.
        /// </summary>
        /// <value>
        /// Categories.
        /// </value>
        IEnumerable<IAppCategory> GetAllCategories { get; }
    }
}