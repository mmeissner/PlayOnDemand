#region Licence
/****************************************************************
 *  Filename: InputProcessor.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-15
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shared.Lib.Win.VirtualKeyboard
{
    public static class InputProcessor
    {
        private static InputSimulator _inputSimulator = new InputSimulator();

        public static IKeyboardSimulator KeyboardInput()
        {
            return _inputSimulator.Keyboard;
        }

        public static IMouseSimulator MouseInput()
        {
            return _inputSimulator.Mouse;
        }
    }
}
