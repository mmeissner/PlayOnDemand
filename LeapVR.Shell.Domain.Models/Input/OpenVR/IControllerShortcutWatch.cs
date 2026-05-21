#region Licence
/****************************************************************
 *  Filename: IControllerShortcutWatch.cs
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
using System.Collections.Generic;

namespace LeapVR.Shell.Domain.Models.Input.OpenVR
{
    /// <summary>
    /// Represents watch that detects when multiple <see cref="IControllerKeyAction"/>s are met at the same time, as well as <see cref="ConditionScope"/> is met and notify listeneres about that.
    /// </summary>
    public interface IControllerShortcutWatch : IDisposable
    {
        /// <summary>
        /// <see cref="ConditionScope"/> necessary to be met to satisfy the condition.
        /// </summary>
        ConditionScope ConditionScope { get; }

        /// <summary>
        /// <see cref="IControllerKeyAction"/>s necessary to be met to satisfy the condition.
        /// </summary>
        IEnumerable<IControllerKeyAction> KeyActions { get; }

        /// <summary>
        /// Indicates if both <see cref="ConditionScope"/> and <see cref="KeyActions"/> are currently satisfied or not.
        /// </summary>
        bool IsSatisfied { get; }

        /// <summary>
        /// Fired when new <see cref="IsSatisfied"/> is changed.
        /// Cold observable, when subscribes notifies subscriber about last set value instantly (like BehaviorSubject).
        /// </summary>
        IObservable<bool> WhenIsSatisfiedChanged { get; }
    }
}
