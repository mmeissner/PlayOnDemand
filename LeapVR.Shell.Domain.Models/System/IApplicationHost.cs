#region Licence
/****************************************************************
 *  Filename: IApplicationHost.cs
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

using System.Globalization;

namespace LeapVR.Shell.Domain.Models.System
{
    public interface IApplicationHost
    {
        void ShowGUI();
        void Restart();
        void Shutdown();
        void RequestPoweroff();
    }
}
