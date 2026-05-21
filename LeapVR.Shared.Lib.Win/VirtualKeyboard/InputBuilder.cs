#region Licence
/****************************************************************
 *  Filename: InputBuilder.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;

namespace LeapVR.Shared.Lib.Win.VirtualKeyboard
{
    /// <summary>
    /// A helper class for building a list of <see cref="WindowsInput.INPUT"/> messages ready to be sent to the native Windows API.
    /// </summary>
    internal class InputBuilder : IEnumerable<Native.Input>
    {
        /// <summary>
        /// The public list of <see cref="WindowsInput.INPUT"/> messages being built by this instance.
        /// </summary>
        private readonly List<Native.Input> _inputList;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputBuilder"/> class.
        /// </summary>
        public InputBuilder()
        {
            _inputList = new List<Native.Input>();
        }

        /// <summary>
        /// Returns the list of <see cref="WindowsInput.INPUT"/> messages as a <see cref="System.Array"/> of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <returns>The <see cref="System.Array"/> of <see cref="WindowsInput.INPUT"/> messages.</returns>
        public Native.Input[] ToArray()
        {
            return _inputList.ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<Native.Input> GetEnumerator()
        {
            return _inputList.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="WindowsInput.INPUT"/> at the specified position.
        /// </summary>
        /// <value>The <see cref="WindowsInput.INPUT"/> message at the specified position.</value>
        public Native.Input this[int position]
        {
            get
            {
                return _inputList[position];
            }
        }

        /// <summary>
        /// Determines if the <see cref="VirtualKeyCode"/> is an ExtendedKey
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <returns>true if the key code is an extended key; otherwise, false.</returns>
        /// <remarks>
        /// The extended keys consist of the ALT and CTRL keys on the right-hand side of the keyboard; the INS, DEL, HOME, END, PAGE UP, PAGE DOWN, and arrow keys in the clusters to the left of the numeric keypad; the NUM LOCK key; the BREAK (CTRL+PAUSE) key; the PRINT SCRN key; and the divide (/) and ENTER keys in the numeric keypad.
        /// 
        /// See http://msdn.microsoft.com/en-us/library/ms646267(v=vs.85).aspx Section "Extended-Key Flag"
        /// </remarks>
        public static bool IsExtendedKey(VirtualKeyCode keyCode)
        {
            if (keyCode == VirtualKeyCode.Menu ||
                keyCode == VirtualKeyCode.Lmenu ||
                keyCode == VirtualKeyCode.Rmenu ||
                keyCode == VirtualKeyCode.Control ||
                keyCode == VirtualKeyCode.Rcontrol ||
                keyCode == VirtualKeyCode.Insert ||
                keyCode == VirtualKeyCode.Delete ||
                keyCode == VirtualKeyCode.Home ||
                keyCode == VirtualKeyCode.End ||
                keyCode == VirtualKeyCode.Prior ||
                keyCode == VirtualKeyCode.Next ||
                keyCode == VirtualKeyCode.Right ||
                keyCode == VirtualKeyCode.Up ||
                keyCode == VirtualKeyCode.Left ||
                keyCode == VirtualKeyCode.Down ||
                keyCode == VirtualKeyCode.Numlock ||
                keyCode == VirtualKeyCode.Cancel ||
                keyCode == VirtualKeyCode.Snapshot ||
                keyCode == VirtualKeyCode.Divide)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a key down to the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/>.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddKeyDown(VirtualKeyCode keyCode)
        {
            var down =
                new Native.Input
                    {
                        Type = (UInt32) Native.InputType.Keyboard,
                        Data =
                            {
                                Keyboard =
                                    new Native.Keybdinput
                                        {
                                            KeyCode = (UInt16) keyCode,
                                            Scan = 0,
                                            Flags = IsExtendedKey(keyCode) ? (UInt32) Native.KeyboardFlag.ExtendedKey : 0,
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                            }
                    };

            _inputList.Add(down);
            return this;
        }

        /// <summary>
        /// Adds a key up to the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/>.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddKeyUp(VirtualKeyCode keyCode)
        {
            var up =
                new Native.Input
                    {
                        Type = (UInt32) Native.InputType.Keyboard,
                        Data =
                            {
                                Keyboard =
                                    new Native.Keybdinput
                                        {
                                            KeyCode = (UInt16) keyCode,
                                            Scan = 0,
                                            Flags = (UInt32) (IsExtendedKey(keyCode)
                                                                  ? Native.KeyboardFlag.KeyUp | Native.KeyboardFlag.ExtendedKey
                                                                  : Native.KeyboardFlag.KeyUp),
                                            Time = 0,
                                            ExtraInfo = IntPtr.Zero
                                        }
                            }
                    };

            _inputList.Add(up);
            return this;
        }

        /// <summary>
        /// Adds a key press to the list of <see cref="WindowsInput.INPUT"/> messages which is equivalent to a key down followed by a key up.
        /// </summary>
        /// <param name="keyCode">The <see cref="VirtualKeyCode"/>.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddKeyPress(VirtualKeyCode keyCode)
        {
            AddKeyDown(keyCode);
            AddKeyUp(keyCode);
            return this;
        }

        /// <summary>
        /// Adds the character to the list of <see cref="WindowsInput.INPUT"/> messages.
        /// </summary>
        /// <param name="character">The <see cref="System.Char"/> to be added to the list of <see cref="WindowsInput.INPUT"/> messages.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddCharacter(char character)
        {
            UInt16 scanCode = character;

            var down = new Native.Input
                           {
                               Type = (UInt32)Native.InputType.Keyboard,
                               Data =
                                   {
                                       Keyboard =
                                           new Native.Keybdinput
                                               {
                                                   KeyCode = 0,
                                                   Scan = scanCode,
                                                   Flags = (UInt32)Native.KeyboardFlag.Unicode,
                                                   Time = 0,
                                                   ExtraInfo = IntPtr.Zero
                                               }
                                   }
                           };

            var up = new Native.Input
                         {
                             Type = (UInt32)Native.InputType.Keyboard,
                             Data =
                                 {
                                     Keyboard =
                                         new Native.Keybdinput
                                             {
                                                 KeyCode = 0,
                                                 Scan = scanCode,
                                                 Flags =
                                                     (UInt32)(Native.KeyboardFlag.KeyUp | Native.KeyboardFlag.Unicode),
                                                 Time = 0,
                                                 ExtraInfo = IntPtr.Zero
                                             }
                                 }
                         };

            // Handle extended keys:
            // If the scan code is preceded by a prefix byte that has the value 0xE0 (224),
            // we need to include the KEYEVENTF_EXTENDEDKEY flag in the Flags property. 
            if ((scanCode & 0xFF00) == 0xE000)
            {
                down.Data.Keyboard.Flags |= (UInt32)Native.KeyboardFlag.ExtendedKey;
                up.Data.Keyboard.Flags |= (UInt32)Native.KeyboardFlag.ExtendedKey;
            }

            _inputList.Add(down);
            _inputList.Add(up);
            return this;
        }

        /// <summary>
        /// Adds all of the characters in the specified <see cref="IEnumerable{T}"/> of <see cref="char"/>.
        /// </summary>
        /// <param name="characters">The characters to add.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddCharacters(IEnumerable<char> characters)
        {
            foreach (var character in characters)
            {
                AddCharacter(character);
            }
            return this;
        }

        /// <summary>
        /// Adds the characters in the specified <see cref="string"/>.
        /// </summary>
        /// <param name="characters">The string of <see cref="char"/> to add.</param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddCharacters(string characters)
        {
            return AddCharacters(characters.ToCharArray());
        }

        /// <summary>
        /// Moves the mouse relative to its current position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddRelativeMouseMovement(int x, int y)
        {
            var movement = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)Native.MouseFlag.Move;
            movement.Data.Mouse.X = x;
            movement.Data.Mouse.Y = y;

            _inputList.Add(movement);

            return this;
        }

        /// <summary>
        /// Move the mouse to an absolute position.
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddAbsoluteMouseMovement(int absoluteX, int absoluteY)
        {
            var movement = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)(Native.MouseFlag.Move | Native.MouseFlag.Absolute);
            movement.Data.Mouse.X = absoluteX;
            movement.Data.Mouse.Y = absoluteY;

            _inputList.Add(movement);

            return this;
        }

        /// <summary>
        /// Move the mouse to the absolute position on the virtual desktop.
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddAbsoluteMouseMovementOnVirtualDesktop(int absoluteX, int absoluteY)
        {
            var movement = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            movement.Data.Mouse.Flags = (UInt32)(Native.MouseFlag.Move | Native.MouseFlag.Absolute | Native.MouseFlag.VirtualDesk);
            movement.Data.Mouse.X = absoluteX;
            movement.Data.Mouse.Y = absoluteY;

            _inputList.Add(movement);

            return this;
        }

        /// <summary>
        /// Adds a mouse button down for the specified button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseButtonDown(MouseButton button)
        {
            var buttonDown = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            buttonDown.Data.Mouse.Flags = (UInt32)ToMouseButtonDownFlag(button);

            _inputList.Add(buttonDown);

            return this;
        }

        /// <summary>
        /// Adds a mouse button down for the specified button.
        /// </summary>
        /// <param name="xButtonId"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseXButtonDown(int xButtonId)
        {
            var buttonDown = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            buttonDown.Data.Mouse.Flags = (UInt32)Native.MouseFlag.XDown;
            buttonDown.Data.Mouse.MouseData = (UInt32)xButtonId;
            _inputList.Add(buttonDown);

            return this;
        }

        /// <summary>
        /// Adds a mouse button up for the specified button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseButtonUp(MouseButton button)
        {
            var buttonUp = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            buttonUp.Data.Mouse.Flags = (UInt32)ToMouseButtonUpFlag(button);
            _inputList.Add(buttonUp);

            return this;
        }

        /// <summary>
        /// Adds a mouse button up for the specified button.
        /// </summary>
        /// <param name="xButtonId"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseXButtonUp(int xButtonId)
        {
            var buttonUp = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            buttonUp.Data.Mouse.Flags = (UInt32)Native.MouseFlag.XUp;
            buttonUp.Data.Mouse.MouseData = (UInt32)xButtonId;
            _inputList.Add(buttonUp);

            return this;
        }

        /// <summary>
        /// Adds a single click of the specified button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseButtonClick(MouseButton button)
        {
            return AddMouseButtonDown(button).AddMouseButtonUp(button);
        }

        /// <summary>
        /// Adds a single click of the specified button.
        /// </summary>
        /// <param name="xButtonId"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseXButtonClick(int xButtonId)
        {
            return AddMouseXButtonDown(xButtonId).AddMouseXButtonUp(xButtonId);
        }

        /// <summary>
        /// Adds a double click of the specified button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseButtonDoubleClick(MouseButton button)
        {
            return AddMouseButtonClick(button).AddMouseButtonClick(button);
        }

        /// <summary>
        /// Adds a double click of the specified button.
        /// </summary>
        /// <param name="xButtonId"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseXButtonDoubleClick(int xButtonId)
        {
            return AddMouseXButtonClick(xButtonId).AddMouseXButtonClick(xButtonId);
        }

        /// <summary>
        /// Scroll the vertical mouse wheel by the specified amount.
        /// </summary>
        /// <param name="scrollAmount"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseVerticalWheelScroll(int scrollAmount)
        {
            var scroll = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            scroll.Data.Mouse.Flags = (UInt32)Native.MouseFlag.VerticalWheel;
            scroll.Data.Mouse.MouseData = (UInt32)scrollAmount;

            _inputList.Add(scroll);

            return this;
        }

        /// <summary>
        /// Scroll the horizontal mouse wheel by the specified amount.
        /// </summary>
        /// <param name="scrollAmount"></param>
        /// <returns>This <see cref="InputBuilder"/> instance.</returns>
        public InputBuilder AddMouseHorizontalWheelScroll(int scrollAmount)
        {
            var scroll = new Native.Input { Type = (UInt32)Native.InputType.Mouse };
            scroll.Data.Mouse.Flags = (UInt32)Native.MouseFlag.HorizontalWheel;
            scroll.Data.Mouse.MouseData = (UInt32)scrollAmount;

            _inputList.Add(scroll);

            return this;
        }

        private static Native.MouseFlag ToMouseButtonDownFlag(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LeftButton:
                    return Native.MouseFlag.LeftDown;

                case MouseButton.MiddleButton:
                    return Native.MouseFlag.MiddleDown;

                case MouseButton.RightButton:
                    return Native.MouseFlag.RightDown;

                default:
                    return Native.MouseFlag.LeftDown;
            }
        }

        private static Native.MouseFlag ToMouseButtonUpFlag(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.LeftButton:
                    return Native.MouseFlag.LeftUp;

                case MouseButton.MiddleButton:
                    return Native.MouseFlag.MiddleUp;

                case MouseButton.RightButton:
                    return Native.MouseFlag.RightUp;

                default:
                    return Native.MouseFlag.LeftUp;
            }
        }
    }
}