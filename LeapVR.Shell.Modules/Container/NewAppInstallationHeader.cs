#region Licence
/****************************************************************
 *  Filename: NewAppInstallationHeader.cs
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
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Modules.Container
{
    
    public class NewAppInstallationHeader : IAppInstallationHeader
    {
        public Guid ApplicationGuid { get; set; }
        public int Version { get; set; }

        public string DisplayName { get; set; }
        public byte[] ThumbnailAsBytes { get; set; }
        public int TotalFilesCount { get; set; }
        public long TotalFilesSize { get; set; }

        public Dictionary<IPackageData, long> PackageDataFileOffsets { get; set; }
    }
}
