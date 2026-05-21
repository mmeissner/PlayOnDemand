#region Licence
/****************************************************************
 *  Filename: IDiskEntity.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Domain.Models.Disk
{
    
    public interface IDiskEntity
    {
        DiskEntityType Type { get; }

        Guid ApplicationGuid { get; }
        Guid PlatformGuid { get; }
        /// <summary>
        /// Guid of the package the file belongs to.
        /// </summary>
        Guid PackageGuid { get; }

        /// <summary>
        /// Relative path to file
        /// </summary>
        string Path { get; }
    }

    
    public enum DiskEntityType
    {
        Relative,
        Absolute,
        PlatformResolve
    }
}