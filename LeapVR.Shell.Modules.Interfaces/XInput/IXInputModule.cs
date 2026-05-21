#region Licence
/****************************************************************
 *  Filename: IXInputModule.cs
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
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;

namespace LeapVR.Shell.Modules.Interfaces.XInput
{
    /// <summary>
    /// A module takes in xinput.
    /// </summary>
    public interface IXInputModule : IBaseModule
    {
        IObservable<XInputButtonArgs> WhenXInputButtonStateChanged { get; }
        bool Enabled { get; set; }
        Action RequestAllAppTermination { set; }
        Action ResetStationState { set; }
    }
}
