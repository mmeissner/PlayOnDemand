#region Licence
/****************************************************************
 *  Filename: PackageProgress.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-27
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Represents a progress with suffient information when reading or writing a package.
    /// </summary>
    public class PackageProgress
    {
        public PackagePhases CurrentPhase { get; set; }
        /// <summary>
        /// Returns the package name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Return how many entries are read.
        /// </summary>
        public int EntriesRead { get; set; }
        /// <summary>
        /// Return how many entries to be read in total.
        /// </summary>
        public int EntriesTotal { get; set; }
        /// <summary>
        /// The number of bytes read or written so far for this entry.
        /// </summary>
        public long BytesTransferred { get; set; }
        /// <summary>
        /// Total number of bytes that will be read or written for this entry. This number will be -1 if the value cannot be determined.
        /// </summary>
        public long TotalBytesToTransfer { get; set; }
        /// <summary>
        /// Return Type of content stored in this package. See <see cref="ContentType"/>.
        /// </summary>
        public ContentType ContentType { get; set; }
    }
}
