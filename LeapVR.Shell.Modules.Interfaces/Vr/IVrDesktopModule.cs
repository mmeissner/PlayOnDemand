#region Licence
/****************************************************************
 *  Filename: IVrDesktopModule.cs
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

using System;

namespace LeapVR.Shell.Modules.Interfaces.Vr
{
    /// <summary>
    /// General interface for VR Desktop like application
    /// </summary>
    public interface IVrDesktopModule
    {
        /// <summary>
        /// Specifies if VR desktop should be running right now.
        /// </summary>
        bool ShouldBeRunning { get; }

        /// <summary>
        /// Changes <see cref="ShouldBeRunning"/>.
        /// </summary>
        /// <param name="newValue">New value</param>
        void ChangeShouldBeRunning(bool newValue);

        /// <summary>
        /// Closes any vr desktop process, restarts a new process and force should be running to true.
        /// </summary>
        void RestartVrDesktopModule();
    }
}
