#region Licence
/****************************************************************
 *  Filename: IHmdActivityWatchdog.cs
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
    /// Comunicates with OpenVR API and notifies when <see cref="HmdActivityStatus"/> changes.
    /// </summary>
    public interface IHmdActivityWatchdog : IDisposable
    {
        /// <summary>
        /// Gets fired when OpenVR API reports that HMD activity has changed.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<IOpenVrEvent> WhenEventOccures { get; }
    }
}
