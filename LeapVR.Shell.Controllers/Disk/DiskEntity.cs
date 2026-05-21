#region Licence
/****************************************************************
 *  Filename: DiskEntity.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Controllers.Disk
{
    class DiskEntity : IDiskEntity
    {
        public Guid ApplicationGuid { get;  }
        public Guid PlatformGuid { get; }
        public DiskEntityType Type { get; set; }
        public Guid PackageGuid { get; }
        public string Path { get; set; }

        internal DiskEntity(Guid applicationGuid, Guid platformGuid, DiskEntityType type,string path = null, Guid? packageGuid = null)
        {
            ApplicationGuid = applicationGuid;
            PlatformGuid = platformGuid;
            Type = type;
            Path = path;
            PackageGuid = packageGuid ?? Guid.Empty;
        }

        internal DiskEntity(IEditableDiskEntity editableDisk)
        {
            ApplicationGuid = editableDisk.ApplicationGuid;
            PlatformGuid = editableDisk.PlatformGuid;
            Type = editableDisk.Type;
            PackageGuid = editableDisk.PackageGuid;
            Path = editableDisk.Path;
        }

        internal DiskEntity(IDiskEntityDto diskEntityDto, Guid platformGuid, DiskEntityType diskEntityType)
        {
            ApplicationGuid = diskEntityDto.ApplicationGuid;
            PlatformGuid = platformGuid;
            Type = diskEntityType;
            PackageGuid = diskEntityDto.PackageGuid;
            Path = diskEntityDto.RelativePath;
        }
    }
}
