#region Licence
/****************************************************************
 *  Filename: InternalFileInfo.cs
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

using System.IO;

namespace LeapVR.Shell.Modules.Container
{
    internal class InternalFileInfo
    {
        public string FullFilePath { get; set; }
        public string ArchiveRelativeFilePath { get; set; }
        public string ArchiveRelativeDirectory { get; set; }
        public FileInfo FileInfo { get; set; }
    }
}
