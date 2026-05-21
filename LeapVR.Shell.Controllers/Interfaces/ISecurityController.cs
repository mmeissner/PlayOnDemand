#region Licence
/****************************************************************
 *  Filename: ISecurityController.cs
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
using LeapVR.Shell.Domain.Models.Controllers;

namespace LeapVR.Shell.Controllers.Interfaces
{
    public delegate void SecurityChanged(bool isEnabled);
    /// <summary>
    /// Controls local application security releated duties.
    /// </summary>
    public interface ISecurityController : IController
    {
        /// <summary>
        /// Identifies if System Management menu is protected by PIN.
        /// </summary>
        bool IsSecurityEnabled { get; }

        /// <summary>
        /// Switches System Management PIN protection ON/OFF.
        /// </summary>
        /// <param name="isSecurityEnabled">New state to set</param>
        void SetSecurityIsEnabled(bool isSecurityEnabled);

        /// <summary>
        /// Sets System Management PIN code.
        /// </summary>
        /// <param name="code"></param>
        void SetSecurityCode(string code);

        /// <summary>
        /// Verifies if PIN code provided is correct or not.
        /// </summary>
        /// <param name="code">PIN code to check</param>
        /// <returns>Boolean indicating if code is correct</returns>
        bool UnlockAdminAccess(string code);

        /// <summary>
        /// Specifies inactivity in System Management interval after which System Management will be locked with PIN automatically.
        /// </summary>
        TimeSpan SystemInactivityTimeout { get; }

        /// <summary>
        /// Occurs when Security changes from enabled to disabled.
        /// </summary>
        event SecurityChanged WhenSecurityChanged;
    }
}
