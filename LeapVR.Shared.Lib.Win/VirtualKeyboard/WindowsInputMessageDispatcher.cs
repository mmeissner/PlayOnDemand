#region Licence
/****************************************************************
 *  Filename: WindowsInputMessageDispatcher.cs
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
using System.Runtime.InteropServices;
using LeapVR.Shared.Lib.Win.VirtualKeyboard.Native;
using LeapVR.Shared.Lib.Win.WinApi.Win32;

namespace LeapVR.Shared.Lib.Win.VirtualKeyboard
{
    /// <summary>
    /// Implements the <see cref="IInputMessageDispatcher"/> by calling <see cref="System.Runtime.InteropServices.NativeMethods.SendInput"/>.
    /// </summary>
    internal class WindowsInputMessageDispatcher : IInputMessageDispatcher
    {
        /// <summary>
        /// Dispatches the specified list of <see cref="Input"/> messages in their specified order by issuing a single called to <see cref="System.Runtime.InteropServices.NativeMethods.SendInput"/>.
        /// </summary>
        /// <param name="inputs">The list of <see cref="Input"/> messages to be dispatched.</param>
        /// <exception cref="ArgumentException">If the <paramref name="inputs"/> array is empty.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="inputs"/> array is null.</exception>
        /// <exception cref="Exception">If the any of the commands in the <paramref name="inputs"/> array could not be sent successfully.</exception>
        public void DispatchInput(Native.Input[] inputs)
        {
            if (inputs == null) throw new ArgumentNullException("inputs");
            if (inputs.Length == 0) throw new ArgumentException("The input array was empty", "inputs");
            var successful = User32.SendInput((UInt32)inputs.Length, inputs, Marshal.SizeOf(typeof (Native.Input)));
            if (successful != inputs.Length)
                throw new Exception("Some simulated input commands were not sent successfully. The most common reason for this happening are the security features of Windows including User Interface Privacy Isolation (UIPI). Your application can only send commands to applications of the same or lower elevation. Similarly certain commands are restricted to Accessibility/UIAutomation applications. Refer to the project home page and the code samples for more information.");
        }
    }
}