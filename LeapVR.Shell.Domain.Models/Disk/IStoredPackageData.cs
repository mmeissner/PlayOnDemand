#region Licence
/****************************************************************
 *  Filename: IStoredPackageData.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Domain.Models.Disk
{
    /// <summary>
    /// Contains information about single package being stored on local disk.
    /// Extends <see cref="IPackageData"/>.
    /// </summary>
    
    public interface IStoredPackageData : IPackageData
    {
        /// <summary>
        /// Contains the status of package stored on disk (see <see cref="PackageState"/>).
        /// </summary>
        PackageState PackageState { get; set; }
    }
}
