#region Licence
/****************************************************************
 *  Filename: XButton.cs
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
namespace LeapVR.Shared.Lib.Win.VirtualKeyboard.Native
{
    /// <summary>
    /// XButton definitions for use in the MouseData property of the <see cref="Mouseinput"/> structure. (See: http://msdn.microsoft.com/en-us/library/ms646273(VS.85).aspx)
    /// </summary>
    internal enum XButton : uint
    {
        /// <summary>
        /// Set if the first X button is pressed or released.
        /// </summary>
        XButton1 = 0x0001,

        /// <summary>
        /// Set if the second X button is pressed or released.
        /// </summary>
        XButton2 = 0x0002,
    }
}
