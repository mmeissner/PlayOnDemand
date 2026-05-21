#region Licence
/****************************************************************
 *  Filename: IWholeDiskUsage.cs
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

using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Domain.Models.Disk
{
    /// <summary>
    /// Contains information about local disk storage usage at specific moment of time (immutable).
    /// </summary>
    public interface IWholeDiskUsage // immutable data object
    {
        /// <summary>
        /// Total amount of bytes on local disk (excluding a small, configurable safety margin).
        /// </summary>
        long TotalDiskSpace { get; }

        /// <summary>
        /// Contains data about used local disk storage space (in bytes) by different <see cref="ContentType"/>.
        /// </summary>
        IDictionary<ContentType, long> ContentUsedDiskSpace { get; }

        /// <summary>
        /// Local disk space (in bytes) used by operating system and other applications.
        /// </summary>
        long SystemUsedDiskUsage { get; }
    }
}
