#region Licence
/****************************************************************
 *  Filename: IControllerKeyState.cs
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
    /// Represents single controller button state in single moment of time.
    /// </summary>
    public interface IControllerKeyState
    {
        /// <summary>
        /// State of the button.
        /// </summary>
        ButtonState State { get; }

        /// <summary>
        /// Time (UTC) when button entered it's <see cref="State"/>.
        /// </summary>
        DateTime InStateSince { get; }
    }
}
