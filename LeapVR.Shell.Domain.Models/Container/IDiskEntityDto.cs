#region Licence
/****************************************************************
 *  Filename: IDiskEntityDto.cs
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
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Describes one single file of one application contained in one package.
    /// </summary>
    
    public interface IDiskEntityDto
    {
        /// <summary>
        /// Guid of the application the file belongs to.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Guid of the package the file belongs to.
        /// </summary>
        Guid PackageGuid { get; }

        /// <summary>
        /// Relative path to file, on top of <see cref="ApplicationGuid"/> + <see cref="PackageGuid"/> base location.
        /// </summary>
        string RelativePath { get; }
    }
}
