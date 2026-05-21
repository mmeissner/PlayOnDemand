#region Licence
/****************************************************************
 *  Filename: IPackageData.cs
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
using System.ComponentModel;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Interface defining package releated data. Package is part of Container (see <see cref="IContainer"/>).
    /// </summary>
    
    public interface IPackageData
    {
        /// <summary>
        /// Uniquely, globally identifies package.
        /// </summary>
        Guid PackageGuid { get; }

        /// <summary>
        /// Version of package.
        /// </summary>
        uint PackageVersion { get; }

        /// <summary>
        /// Total amount of files stored in current package.
        /// </summary>
        int TotalFilesCount { get; }

        /// <summary>
        /// Combined size of all files contained in package.
        /// </summary>
        long TotalFilesSize { get; }

        /// <summary>
        /// Identifies application that this package (and wider, container this package belongs to) is part of.
        /// </summary>
        Guid ApplicationGuid { get; } // TODO [RM]: are always packages aplication-relative? If no need to split interface to IPackage / IApplicationPackage

        /// <summary>
        /// Type of content stored in package. See <see cref="ContentType"/>.
        /// </summary>
        ContentType ContentType { get; }
    }
}
