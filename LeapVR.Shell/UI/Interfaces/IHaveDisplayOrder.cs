#region Licence
/****************************************************************
 *  Filename: IHaveDisplayOrder.cs
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
namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Representing a view model that holds a display order.
    /// </summary>
    public interface IHaveDisplayOrder
    {
        /// <summary>
        /// Get the display order of a view model.
        /// </summary>
        int DisplayOrder { get; }
    }
}
