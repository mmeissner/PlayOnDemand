#region Licence
/****************************************************************
 *  Filename: XInputGesture.cs
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

using System.Windows.Input;
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;

namespace LeapVR.Shell.Modules.XInput
{
    public class XInputGesture : InputGesture
    {
        private readonly XInputButtons _button;

        public XInputGesture(XInputButtons button)
        {
            _button = button;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is XInputEventArgs args)
            {
                return args.XButtonState == XInputButtonState.Pressed && args.XButton == _button;
            }
            return false;
        }
    }


}
