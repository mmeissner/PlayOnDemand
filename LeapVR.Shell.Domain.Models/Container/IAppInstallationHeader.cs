#region Licence
/****************************************************************
 *  Filename: IAppInstallationHeader.cs
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

namespace LeapVR.Shell.Domain.Models.Container
{
    
    public interface IAppInstallationHeader : IZipContainerHeader
    {
        string DisplayName { get; }

        /// <summary>
        /// Thumbnail image stored as bytes. To be displayed e.g. near application name on list of installed applications.
        /// </summary>
        byte[] ThumbnailAsBytes { get; }
    }
}
