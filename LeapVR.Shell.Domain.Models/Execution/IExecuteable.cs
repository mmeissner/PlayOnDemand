#region Licence
/****************************************************************
 *  Filename: IExecuteable.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Execution
{
    public interface IExecuteable
    {
        /// <summary>
        /// Get the display name of this instruction.
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Get if vr mode is required.
        /// </summary>
        bool IsVirtualRealityRequired { get; }
        /// <summary>
        /// Get if the requirements are fulfilled.
        /// </summary>
        bool IsVrRequirementFullfiled { get; }
        /// <summary>
        /// Get if screen mode is supported.
        /// </summary>
        bool IsScreenModeSupported { get; }
    }
}
