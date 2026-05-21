#region Licence
/****************************************************************
 *  Filename: PackageDataDto.cs
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
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    [ProtoContract]

    class PackageDataDto : IPackageData
    {
        [ProtoMember(1)]
        public Guid PackageGuid { get; set; }
        [ProtoMember(2)]
        public uint PackageVersion { get; set; }
        [ProtoMember(3)]
        public Guid ApplicationGuid { get; set; }
        [ProtoMember(4)]
        public ContentType ContentType { get; set; }
        [ProtoMember(5)]
        public int TotalFilesCount { get; set; }
        [ProtoMember(6)]
        public long TotalFilesSize { get; set; }
        public PackageDataDto(){}
        public PackageDataDto(IPackageData packageData)
        {
            PackageGuid = packageData.PackageGuid;
            PackageVersion = packageData.PackageVersion;
            ApplicationGuid = packageData.ApplicationGuid;
            ContentType = packageData.ContentType;
            TotalFilesCount = packageData.TotalFilesCount;
            TotalFilesSize = packageData.TotalFilesSize;
        }
    }
}
