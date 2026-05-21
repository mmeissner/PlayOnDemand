#region Licence
/****************************************************************
 *  Filename: IDiskController.cs
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

using System;
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Domain.Models.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Responsible for managing application files on local disk storage.
    /// </summary>
    public interface IDiskController : IController
    {
        /// <summary>
        /// Contains <see cref="IWholeDiskUsage"/> object describing current local disk usage.
        /// </summary>
        IWholeDiskUsage DiskUsage { get; }

        /// <summary>
        /// Observable fired when <see cref="DiskUsage"/> has changed (e.g. when new application was installed or uninstalled).
        /// </summary>
        IObservable<IWholeDiskUsage> WhenDiskUsageChanged { get; }

        /// <summary>
        /// Indicates base directory for installed games.
        /// </summary>
        string StorageDirectory { get; }

        /// <summary>
        /// Gets <see cref="IStoredPackageData"/> containing information about stored on local disk <see cref="IPackageData"/>.
        /// </summary>
        /// <param name="packageGuid">Guid of <see cref="IPackageData"/></param>
        /// <returns><see cref="IStoredPackageData"/> containing information about stored on local disk <see cref="IPackageData"/></returns>
        IStoredPackageData GetStoredPackageData(Guid packageGuid);

        /// <summary>
        /// Returns collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for all installed applications.
        /// </summary>
        /// <returns>Collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for all installed applications</returns>
        IEnumerable<IStoredPackageData> GetStoredPackages();

        /// <summary>
        /// Returns collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for selected installed application (with Guid <see cref="applicationGuid"/>).
        /// </summary>
        /// <returns>Collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for selected installed application (with Guid <see cref="applicationGuid"/>)</returns>
        IEnumerable<IStoredPackageData> GetStoredPackages(Guid applicationGuid);

        /// <summary>
        /// Returns collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for selected <see cref="ContentType"/>.
        /// </summary>
        /// <returns>Collection of <see cref="IStoredPackageData"/> data objects for all stored <see cref="IPackageData"/> for selected <see cref="ContentType"/></returns>
        IEnumerable<IStoredPackageData> GetStoredPackages(ContentType contentType);

        /// <summary>
        /// Checks if storing given collection of <see cref="packages"/> is possible in current state (i.e. if disk space is enough).
        /// </summary>
        /// <param name="packages">Collection of packages to check</param>
        /// <returns>Boolean indicating possibility (or not).</returns>
        bool CanStorePackages(IEnumerable<IPackageData> packages);

        /// <summary>
        /// Stores given <see cref="package"/> to local disk storage.
        /// </summary>
        /// <param name="package">Package to store</param>
        void StorePackage<T>(T package) where T: IExtractablePackage,  IPackageData;

        /// <summary>
        /// Stores collection of given <see cref="packages"/> to local disk storage.
        /// </summary>
        /// <param name="packages">Collection of packages to store</param>
        void StorePackages<T>(IEnumerable<T> packages)where T: IExtractablePackage, IPackageData;

        //void RemovePackage(Guid packageGuid); // TODO [RM]: to be implemented later

        /// <summary>
        /// Removes all <see cref="IStoredPackageData"/> and other application related data from local disk storage.
        /// </summary>
        /// <param name="applicationGuid">Guid of application of which data will be removed</param>
        void RemoveAllApplicationData(Guid applicationGuid);

        //void UpdatePackage(IPackageData newPackageData); // TODO [RM]: to be implemented later

        //IEnumerable<IAppFile> GetAppFiles(ContentType contentType, Guid applicationGuid);

        /// <summary>
        /// Given <see cref="IDiskEntityDto"/> retrives <see cref="IAppFile"/>.
        /// </summary>
        /// <param name="diksEntity">Disk entity to retrive data based on.</param>
        /// <returns>Retrieved data.</returns>
        string GetFilePath(IDiskEntity diksEntity);

        /// <summary>
        /// Gets directory used to store content of <see cref="contentType"/> for application with <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="applicationGuid"></param>
        /// <returns></returns>
        string GetContentDirectory(ContentType contentType, Guid applicationGuid);

        /// <summary>
        /// Gets base directory where all application related files (of all <see cref="ContentType"/>) are stored.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <returns>Path to base application storage location</returns>
        string GetApplicationStorageDirectory(Guid applicationGuid);
    }
}
