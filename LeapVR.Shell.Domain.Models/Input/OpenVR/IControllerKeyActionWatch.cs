#region Licence
/****************************************************************
 *  Filename: IControllerKeyActionWatch.cs
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

namespace LeapVR.Shell.Domain.Models.Input.OpenVR
{
    /// <summary>
    /// Represents logic observing user input on controllers and matching user's actions to fulfil <see cref="IControllerKeyAction"/> condition.
    /// </summary>
    public interface IControllerKeyActionWatch : IDisposable
    {
        /// <summary>
        /// Related <see cref="IControllerKeyAction"/>.
        /// </summary>
        IControllerKeyAction KeyAction { get; }

        /// <summary>
        /// Represents in which <see cref="IControllerKeyState"/> is currently observer button.
        /// </summary>
        IControllerKeyState State { get; }

        /// <summary>
        /// Fired when new <see cref="State"/> is changed.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<IControllerKeyState> WhenStateChanged { get; }

        /// <summary>
        /// Indicates if condition of <see cref="KeyAction"/> is currently satisfied or not.
        /// </summary>
        bool IsSatisfied { get; }

        /// <summary>
        /// Fired when new <see cref="IsSatisfied"/> is changed.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<bool> WhenIsSatisfiedChanged { get; }
    }
}
