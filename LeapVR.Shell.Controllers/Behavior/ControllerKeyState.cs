#region Licence
/****************************************************************
 *  Filename: ControllerKeyState.cs
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
using LeapVR.Shell.Domain.Models.Input.OpenVR;

namespace LeapVR.Shell.Controllers.Behavior
{
    public class ControllerKeyState : IControllerKeyState
    {
        public ButtonState State { get; set; }
        public DateTime InStateSince { get; set; }
    }
}
