#region Licence
/****************************************************************
 *  Filename: IWindowData.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Modules.Interfaces.Utilities.WinApi
{
    public interface IWindowData
    {
        string WindowClassName { get; }

        int PosX { get; }
        int PosY { get; }

        int Width { get; }
        int Height { get; }
    }
}
