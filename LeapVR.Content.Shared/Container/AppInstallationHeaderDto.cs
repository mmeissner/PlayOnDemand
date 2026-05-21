#region Licence
/****************************************************************
 *  Filename: AppInstallationHeaderDto.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2018-1-19
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
using LeapVR.Shell.Domain.Models.Container;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    [ProtoContract]

    class AppInstallationHeaderDto : IAppInstallationHeader
    {
        [ProtoMember(10)]
        public Guid ApplicationGuid { get; set; }
        [ProtoMember(20)]
        public int Version { get; set; }
        [ProtoMember(40)]
        public byte[] ThumbnailAsBytes { get; set; }
        [ProtoMember(50)]
        public int TotalFilesCount { get; set; }
        [ProtoMember(60)]
        public long TotalFilesSize { get; set; }
        [ProtoMember(70)]
        public Dictionary<IPackageData, long> PackageDataFileOffsets { get; set; }
        [ProtoMember(80)]
        public string DisplayName { get; set; }

        public AppInstallationHeaderDto(){}
        public AppInstallationHeaderDto(IAppInstallationHeader installationHeader)
        {
            ApplicationGuid = installationHeader.ApplicationGuid;
            Version = installationHeader.Version;
            ThumbnailAsBytes = installationHeader.ThumbnailAsBytes;
            TotalFilesCount = installationHeader.TotalFilesCount;
            TotalFilesSize = installationHeader.TotalFilesSize;
            DisplayName = installationHeader.DisplayName;
            if(installationHeader.PackageDataFileOffsets != null)
            {
                PackageDataFileOffsets = new Dictionary<IPackageData, long>();
                foreach(KeyValuePair<IPackageData, long> keyValuePair in installationHeader.PackageDataFileOffsets)
                {
                    PackageDataFileOffsets.Add(new PackageDataDto(keyValuePair.Key),keyValuePair.Value);
                }
            }
        }
    }
}
