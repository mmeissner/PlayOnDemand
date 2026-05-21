#region Licence
/****************************************************************
 *  Filename: IHaveIcon.cs
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
    /// Representing a view model that holds resource key of its icon.
    /// </summary>
    public interface IHaveIcon
    {
        /// <summary>
        /// Get icon resource key.
        /// </summary>
        string IconKey { get; }
    }
}
