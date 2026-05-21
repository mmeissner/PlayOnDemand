#region Licence
/****************************************************************
 *  Filename: XInputButtonArgs.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-10
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Diagnostics;

namespace LeapVR.VBox.Modules.Interfaces.XInput
{
    public class XInputButtonArgs
    {
        public XInputButtons XButton { get; }
        public XInputButtonState XButtonState { get;  }
        public Stopwatch PressedWatch { get; }

        public XInputButtonArgs(XInputButtons xbutton)
        {
            XButton = xbutton;
            XButtonState = XInputButtonState.Released;
            PressedWatch = new Stopwatch();
        }

        public XInputButtonArgs(XInputButtons xbutton, XInputButtonState state)
        {
            XButton = xbutton;
            XButtonState = state;
            PressedWatch = new Stopwatch();
        }

        public XInputButtonArgs(XInputButtons xbutton, XInputButtonState state, Stopwatch stopwatch)
        {
            XButton = xbutton;
            XButtonState = state;
            PressedWatch = stopwatch;
        }

    }
}
