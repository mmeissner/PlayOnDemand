#region Licence
/****************************************************************
 *  Filename: MOUSEKEYBDHARDWAREINPUT.cs
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
using System.Runtime.InteropServices;

namespace LeapVR.Shared.Lib.Win.VirtualKeyboard.Native
{
#pragma warning disable 649
    /// <summary>
    /// The combined/overlayed structure that includes Mouse, Keyboard and Hardware Input message data (see: http://msdn.microsoft.com/en-us/library/ms646270(VS.85).aspx)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Mousekeybdhardwareinput
    {
        /// <summary>
        /// The <see cref="Mouseinput"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public Mouseinput Mouse;

        /// <summary>
        /// The <see cref="Keybdinput"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public Keybdinput Keyboard;

        /// <summary>
        /// The <see cref="Hardwareinput"/> definition.
        /// </summary>
        [FieldOffset(0)]
        public Hardwareinput Hardware;
    }
#pragma warning restore 649
}
