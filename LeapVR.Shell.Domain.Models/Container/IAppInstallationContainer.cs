#region Licence
/****************************************************************
 *  Filename: IAppInstallationContainer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Application container to use in application installation process. (see <see cref="IContainer{T}"/>). Contains all data required to install application to LeapVR system.
    /// </summary>
    /// <typeparam name="T">Type of package implementing <see cref="IPackageData"/>.</typeparam>
    public interface IAppInstallationContainer<T> : IContainer<T> where T : IPackageData
    {
        /// <summary>
        /// Thumbnail image stored as bytes. To be displayed e.g. near application name on list of installed applications.
        /// </summary>
        byte[] ThumbnailAsBytes { get; }

        /// <summary>
        /// Display name of Application. To be displayed e.g. on list of installed applications.
        /// </summary>
        string DisplayName { get; }
    }
}
