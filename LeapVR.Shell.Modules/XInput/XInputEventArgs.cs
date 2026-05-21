#region Licence
/****************************************************************
 *  Filename: XInputEventArgs.cs
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
using System.Windows.Input;
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;

namespace LeapVR.Shell.Modules.XInput
{
    /// <summary>
    /// Event arguments when an XInput device triggered and emulate keyboard key pressed.
    /// </summary>
    public class XInputEventArgs : KeyEventArgs
    {
        public XInputButtonState XButtonState { get; set; }
        public XInputButtons XButton { get; set; }

        public XInputEventArgs(InputManager inputManager, Key key, XInputButtonState state, XInputButtons button) :
            base(inputManager.PrimaryKeyboardDevice, inputManager.PrimaryKeyboardDevice.ActiveSource, Environment.TickCount, key)
        {
            XButtonState = state;
            XButton = button;
            RoutedEvent = state == XInputButtonState.Pressed ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent;
        }
    }

}
