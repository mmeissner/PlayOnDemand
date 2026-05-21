#region Licence
/****************************************************************
 *  Filename: IControllerKeyAction.cs
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
    /// Represents single controller button trigger action.
    /// For example: Left controller trigger pressed for more than 2 seconds.
    /// </summary>
    public interface IControllerKeyAction
    {
        /// <summary>
        /// Represents Which controller: Left/Right should trigger action.
        /// Values are related to ETrackedControllerRole OpenVR enum.
        /// </summary>
        int ControlerRole { get; }

        /// <summary>
        /// Represents which button on controller should trigger action.
        /// Values are related EVRButtonId to OpenVR enum.
        /// </summary>
        int ButtonId { get; }

        /// <summary>
        /// Represents in which state button should stay to trigger action.
        /// </summary>
        ButtonState State { get; }

        /// <summary>
        /// Represents minimal time for button to stay in <see cref="State"/> to trigger action.
        /// </summary>
        TimeSpan TriggerTime { get; }
    }
}
