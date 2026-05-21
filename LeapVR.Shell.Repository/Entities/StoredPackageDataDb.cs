#region Licence
/****************************************************************
 *  Filename: StoredPackageDataDb.cs
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class StoredPackageDataDb : IStoredPackageData, IEntity
    {
        public Guid Id { get; set; }
        public Guid PackageGuid { get; set; }
        public uint PackageVersion { get; set; }
        public Guid ApplicationGuid { get; set; }
        public ContentType ContentType { get; set; }
        public int TotalFilesCount { get; set; }
        public long TotalFilesSize { get; set; }

        public PackageState PackageState { get; set; }

        public StoredPackageDataDb(){}

        public StoredPackageDataDb(IPackageData packageData, PackageState packageState)
        {
            PackageGuid = packageData.PackageGuid;
            PackageVersion = packageData.PackageVersion;
            ApplicationGuid = packageData.ApplicationGuid;
            ContentType = packageData.ContentType;
            TotalFilesCount = packageData.TotalFilesCount;
            TotalFilesSize = packageData.TotalFilesSize;
            PackageState = packageState;
        }
    }
}
