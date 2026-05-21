#region Licence
/****************************************************************
 *  Filename: IZipContainerHeader.cs
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
using System.Collections.Generic;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Header describing container of ZIP type. Serialized to file.
    /// ZIP type container consists of two files. One being (this) header serialized, another is data file containing all container packages that contains all application files.
    /// Data file consists of appended ZIP archives, each representing one package. Each archive contains folder structure with files that when installing application will be extracted to different locations.
    /// </summary>
    
    public interface IZipContainerHeader
    {
        /// <summary>
        /// Globally unique identifier of application.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Header version. Reserved for further use.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Look-up dictionary containing package data (see <see cref="IPackageData"/>) as key and file offset as value.
        /// File offset points to offset in data file where ZIP archive preceded by 8 byte number length is located.
        /// </summary>
        Dictionary<IPackageData, long> PackageDataFileOffsets { get; }

        /// <summary>
        /// Total amount of files stored in all packages in container, sumed.
        /// </summary>
        int TotalFilesCount { get; set; }

        /// <summary>
        /// Combined size of all files contained in all packages, sumed.
        /// </summary>
        long TotalFilesSize { get; set; }
    }
}
