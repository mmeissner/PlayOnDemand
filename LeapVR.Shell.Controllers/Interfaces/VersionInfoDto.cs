#region Licence
/****************************************************************
 *  Filename: VersionInfoDto.cs
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

using System.Reflection;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Holds information about the newest Shell version available to download in <see cref="NewestVersion"/>,
    /// as well as holds URL location where newest version can be downloaded from (<see cref="NewestVersionDownloadUrl"/>).
    /// Is serialized as JSON and located on WWW server.
    /// </summary>
    
    public class VersionInfoDto
    {
        public string NewestVersion { get; set; }
        public string NewestVersionDownloadUrl { get; set; }
    }
}
