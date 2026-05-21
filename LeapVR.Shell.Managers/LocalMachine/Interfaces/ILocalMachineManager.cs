#region Licence
/****************************************************************
 *  Filename: ILocalMachineManager.cs
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
namespace LeapVR.Shell.Managers.LocalMachine.Interfaces
{
    /// <summary>
    /// Manages local machine related software & hardware data.
    /// </summary>
    public interface ILocalMachineManager
    {
        /// <summary>
        /// Type of VBox.
        /// </summary>
        VBoxType VBoxType { get; }

        /// <summary>
        /// Unique, hardware-based fingerprint of current machine.
        /// </summary>
        string VBoxFingerprint { get; }

        /// <summary>
        /// Description of CPU installed in current machine.
        /// </summary>
        string CpuDetails { get; }

        /// <summary>
        /// Description of VGA installed in current machine.
        /// </summary>
        string VgaDetails { get; }

        /// <summary>
        /// Description of RAM memory installed in current machine.
        /// </summary>
        string RamDetails { get; }
    }
}
