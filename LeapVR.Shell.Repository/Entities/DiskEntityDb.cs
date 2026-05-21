#region Licence
/****************************************************************
 *  Filename: DiskEntityDb.cs
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
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Repository.Entities
{
    
    class DiskEntityDb : IDiskEntity
    {
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformGuid { get; set; }
        public DiskEntityType Type { get; set; }
        public Guid PackageGuid { get; set; }
        public string Path { get; set; }

        public DiskEntityDb(){}
        public DiskEntityDb(IDiskEntity diskEntity)
        {
            ApplicationGuid = diskEntity.ApplicationGuid;
            PlatformGuid = diskEntity.PlatformGuid;
            Type = diskEntity.Type;
            PackageGuid = diskEntity.PackageGuid;
            Path = diskEntity.Path;
        }
    }
}
