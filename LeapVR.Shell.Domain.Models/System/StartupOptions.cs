#region Licence
/****************************************************************
 *  Filename: StartupOptions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;

namespace LeapVR.Shell.Domain.Models.System
{
    [Flags]
    public enum StartupOptions : uint
    {
        /// Shift Bits for easier way for power of two instead of 0,1,2,4,8,16.....
        None = 0,
        RunInWindow = 1 << 0,
        HideTaskBar = 1 << 1,
        HideCursor = 1 << 2
    }
}
