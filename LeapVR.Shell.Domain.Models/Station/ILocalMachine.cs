#region Licence
/****************************************************************
 *  Filename: ILocalMachine.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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

namespace LeapVR.Shell.Domain.Models.Station
{
    /// <summary>
    /// Manages local machine related software & hardware data.
    /// </summary>
    public interface ILocalMachine
    {
        /// <summary>
        /// Gets the software version.
        /// </summary>
        /// <value>
        /// The software version.
        /// </value>
        Version SoftwareVersion { get; }

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
