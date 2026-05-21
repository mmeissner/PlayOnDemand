#region Licence
/****************************************************************
 *  Filename: GuiContextType.cs
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
namespace Unosquare.FFME.Platform
{
    /// <summary>
    /// Enumerates GUI Context Types
    /// </summary>
    public enum GuiContextType
    {
        /// <summary>
        /// An invalid GUI context (console applications)
        /// </summary>
        None,

        /// <summary>
        /// A WPF GUI context (i.e. has dispatcher and is not Windows Forms)
        /// </summary>
        WPF,

        /// <summary>
        /// A Windows Forms GUI Context
        /// </summary>
        WinForms
    }
}
