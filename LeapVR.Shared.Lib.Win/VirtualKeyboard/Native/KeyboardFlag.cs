#region Licence
/****************************************************************
 *  Filename: KeyboardFlag.cs
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

namespace LeapVR.Shared.Lib.Win.VirtualKeyboard.Native
{
    /// <summary>
    /// Specifies various aspects of a keystroke. This member can be certain combinations of the following values.
    /// </summary>
    [Flags]
    internal enum KeyboardFlag : uint // UInt32
    {
        /// <summary>
        /// KEYEVENTF_EXTENDEDKEY = 0x0001 (If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).)
        /// </summary>
        ExtendedKey = 0x0001,

        /// <summary>
        /// KEYEVENTF_KEYUP = 0x0002 (If specified, the key is being released. If not specified, the key is being pressed.)
        /// </summary>
        KeyUp = 0x0002,

        /// <summary>
        /// KEYEVENTF_UNICODE = 0x0004 (If specified, wScan identifies the key and wVk is ignored.)
        /// </summary>
        Unicode = 0x0004,

        /// <summary>
        /// KEYEVENTF_SCANCODE = 0x0008 (Windows 2000/XP: If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero. This flag can only be combined with the KEYEVENTF_KEYUP flag. For more information, see the Remarks section.)
        /// </summary>
        ScanCode = 0x0008,
    }
}
