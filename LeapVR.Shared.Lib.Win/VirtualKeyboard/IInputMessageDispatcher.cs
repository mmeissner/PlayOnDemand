#region Licence
/****************************************************************
 *  Filename: IInputMessageDispatcher.cs
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
namespace LeapVR.Shared.Lib.Win.VirtualKeyboard
{
    /// <summary>
    /// The contract for a service that dispatches <see cref="WindowsInput.INPUT"/> messages to the appropriate destination.
    /// </summary>
    internal interface IInputMessageDispatcher
    {
        /// <summary>
        /// Dispatches the specified list of <see cref="WindowsInput.INPUT"/> messages in their specified order.
        /// </summary>
        /// <param name="inputs">The list of <see cref="WindowsInput.INPUT"/> messages to be dispatched.</param>
        /// <exception cref="ArgumentException">If the <paramref name="inputs"/> array is empty.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="inputs"/> array is null.</exception>
        /// <exception cref="Exception">If the any of the commands in the <paramref name="inputs"/> array could not be sent successfully.</exception>
        void DispatchInput(Native.Input[] inputs);
    }
}
