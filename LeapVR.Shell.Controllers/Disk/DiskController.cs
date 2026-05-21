#region Licence
/****************************************************************
 *  Filename: DiskController.cs
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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Controllers.Disk
{
    //<inheritdoc />
    public class DiskController : IDiskController
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IWholeDiskUsage DiskUsage { get; private set; }
        private readonly Subject<IWholeDiskUsage> _whenDiskUsageChangedSubject;
        public IObservable<IWholeDiskUsage> WhenDiskUsageChanged { get; }

        public string StorageDirectory { get; }

        private readonly IEnumerable<ContentType> _allPossibleContentTypes;

        private readonly DiskConfig _diskConfig;
        private readonly IStoredPackageRepository _storedPackagesRepository;

        #endregion Properties & Fields

        #region Constructors

        public DiskController(IConfigFileRepository<DiskConfig> configRepository, IStoredPackageRepository storedPackagesRepository)
        {
            QuickLeap.AssertNotNull(configRepository, storedPackagesRepository);
            _diskConfig = configRepository.Get();
            _storedPackagesRepository = storedPackagesRepository;

            StorageDirectory = _diskConfig.StorageBaseDir;

            _allPossibleContentTypes = _diskConfig.ContentRelativeDirs.Keys.ToList();

            _whenDiskUsageChangedSubject = new Subject<IWholeDiskUsage>();
            WhenDiskUsageChanged = _whenDiskUsageChangedSubject.AsObservable();
            RecalculateDiskUsage();
        }

        #endregion Constructors

        #region Methods

        public bool CanStorePackages(IEnumerable<IPackageData> packages)
        {
            QuickLeap.AssertNotNull(packages);

            var contentSpaceNeeded = packages.Sum(q => q.TotalFilesSize);
            var contentSpaceFree = DiskUsage.TotalDiskSpace - DiskUsage.SystemUsedDiskUsage - DiskUsage.ContentUsedDiskSpace.Sum(q => q.Value);

            return contentSpaceNeeded <= contentSpaceFree;
        }

        public string GetFilePath(IDiskEntity diskEntity)
        {
            if(diskEntity.Type == DiskEntityType.Relative)
            {
                var package = GetStoredPackageData(diskEntity.PackageGuid);
                if (package == null)
                {
                    throw new InvalidOperationException($"{nameof(package)} == null");
                }

                if (package.PackageState != PackageState.Stored)
                {
                    throw new InvalidOperationException($"{nameof(package.PackageState)} != {nameof(PackageState.Stored)}");
                }
                var baseDir = GetContentDirectory(package.ContentType,package.ApplicationGuid);
                var fullPath = Path.Combine(baseDir, diskEntity.Path);
                return fullPath;
            }

            return diskEntity.Path;
        }

        public IStoredPackageData GetStoredPackageData(Guid packageGuid)
        {
            return _storedPackagesRepository.Get(packageGuid);
        }

        public IEnumerable<IStoredPackageData> GetStoredPackages()
        {
            return _storedPackagesRepository.GetAll();
        }

        public IEnumerable<IStoredPackageData> GetStoredPackages(Guid applicationGuid)
        {
            return _storedPackagesRepository.GetAll(applicationGuid);
        }

        public IEnumerable<IStoredPackageData> GetStoredPackages(ContentType contentType)
        {
            return _storedPackagesRepository.GetAll(contentType);
        }

        public void StorePackage<T>(T package) where T: IExtractablePackage, IPackageData
        {
            QuickLeap.AssertNotNull(package);

            var storedPackage = _storedPackagesRepository.Get(package.PackageGuid);
            if (storedPackage != null)
            {
                throw new InvalidOperationException($"Package ({nameof(package.PackageGuid)} = `{package.PackageGuid}`) is already stored ({nameof(storedPackage.PackageState)} == {storedPackage.PackageState}). Cannot store it again.");
            }

            try
            {
                StorePackageLogic(package);
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception during StorePackage occured");
            }
            finally
            {
                RecalculateDiskUsage();
            }
        }

        public void StorePackages<T>(IEnumerable<T> packages)where T: IExtractablePackage, IPackageData
        {
            packages = packages?.ToList();
            QuickLeap.AssertNotNullEx(packages);
            var packageGuids = packages.Select(q => q.PackageGuid).ToList();

            var alreadyStoredPackages = _storedPackagesRepository.GetAll().Where(q => packageGuids.Contains(q.PackageGuid)).ToList();
            if (alreadyStoredPackages.Any())
            {
                throw new InvalidOperationException($"Some packags (with GUIDs: {QuickLeap.EnumerableToString(alreadyStoredPackages.Select(q => q.PackageGuid))}) are already stored. Cannot store them again.");
            }

            try
            {
                foreach (var package in packages)
                {
                    StorePackageLogic(package);
                }
            }
            finally
            {
                RecalculateDiskUsage();
            }
        }

        public void RemoveAllApplicationData(Guid applicationGuid)
        {
            var storedPackages = GetStoredPackages(applicationGuid).ToList();
            foreach (var storedPackage in storedPackages)
            {
                storedPackage.PackageState = PackageState.RemovingInProgress;
                _storedPackagesRepository.Store(storedPackage);
            }

            var isRemoveAllSuccess = true;
            try
            {
                var appBaseDir = GetApplicationStorageDirectory(applicationGuid);
                if (Directory.Exists(appBaseDir))
                {
                    Directory.Delete(appBaseDir, true);
                }
            }
            catch
            {
                isRemoveAllSuccess = false;
                throw;
            }
            finally
            {
                foreach (var storedPackage in storedPackages)
                {
                    if (isRemoveAllSuccess)
                    {
                        _storedPackagesRepository.Delete(storedPackage.PackageGuid);
                    }
                    else
                    {
                        storedPackage.PackageState = PackageState.RemovingFailed;
                        _storedPackagesRepository.Store(storedPackage);
                    }
                }

                RecalculateDiskUsage();
            }
        }

        private void StorePackageLogic<T>(T package) where T: IExtractablePackage, IPackageData
        {
            var contentType = package.ContentType;
            var applicationGuid = package.ApplicationGuid;
            var basePath = GetContentDirectory(contentType, applicationGuid);

            bool isStoringSuccess = true;
            if(!_storedPackagesRepository.Store(package, PackageState.StoringInProgress, out var storedPackage))
            {
                Logger.Error("Error on Storing Package Data",package);
                throw new Exception("Could not persist Package data!");
            };
            try
            {
                package.ExtractToDirectory(basePath);
            }
            catch(Exception exception)
            {
                isStoringSuccess = false;
                Logger.Error(exception,"Error during StorePackageLogic occured!");
                throw;
            }
            finally
            {
                storedPackage.PackageState = isStoringSuccess ? PackageState.Stored : PackageState.StoringFailed;
                _storedPackagesRepository.Store(storedPackage);
            }
        }

        private void RecalculateDiskUsage()
        {
            WholeDiskUsage diskUsage = null;
            try
            {
                if(_diskConfig.SystemDrives == null)return;
                var systemDriveNames = _diskConfig.SystemDrives.ToList();

                var drives = DriveInfo.GetDrives().Where(driveInfo => systemDriveNames.Contains(driveInfo.Name)).ToList();

                if (_diskConfig.ReservedDiskSpaceRatio < 0 || _diskConfig.ReservedDiskSpaceRatio > 1)
                {
                    throw new InvalidOperationException($"Invalid value of {nameof(_diskConfig.ReservedDiskSpaceRatio)} = `{_diskConfig.ReservedDiskSpaceRatio}`. Value must be in range 0-1.");
                }

                var realTotalDiskBytes = drives.Sum(q => q.TotalSize);
                var totalDiskBytes = (long)(realTotalDiskBytes * (1 - _diskConfig.ReservedDiskSpaceRatio));

                var totalUsedDiskBytes = Math.Min(realTotalDiskBytes - drives.Sum(q => q.AvailableFreeSpace), totalDiskBytes);
                var contentUsedDiskSpace = GetContentUsedDiskSpace();
                var contentUsedDiskBytes = Math.Min(contentUsedDiskSpace.Sum(q => q.Value), totalDiskBytes);
                var systemUsedDiskBytes = totalUsedDiskBytes - contentUsedDiskBytes;

                diskUsage = new WholeDiskUsage
                {
                    TotalDiskSpace = totalDiskBytes,
                    ContentUsedDiskSpace = contentUsedDiskSpace,
                    SystemUsedDiskUsage = systemUsedDiskBytes,
                };
            }
            catch (Exception exception)
            {
                Logger.Error(exception,"Error during RecalcuateDiskUsage occured!");
            }
            if (diskUsage == null)
            {
                Logger.Warn("Skipping update of disk usage due to previous error");

            }
            else
            {
                DiskUsage = diskUsage;
                try
                {
                    _whenDiskUsageChangedSubject.OnNext(diskUsage);
                }
                catch (Exception exception)
                {
                    Logger.Fatal(exception,"Exception during notification of subscribers, some might not got notified!");
                }
            }
        }

        private IDictionary<ContentType, long> GetContentUsedDiskSpace()
        {
            var allStoredPackages = _storedPackagesRepository.GetAll();
            var groupedPackages = allStoredPackages.GroupBy(q => q.ContentType);

            Dictionary<ContentType, long> contentUsedDiskSpace = _allPossibleContentTypes.ToDictionary(q => q, q => (long)0);
            foreach (var group in groupedPackages)
            {
                var contentType = group.Key;
                var contentBytesUsed = group.Aggregate<IPackageData, long>(0, (current, package) => current + package.TotalFilesSize);

                if (!contentUsedDiskSpace.ContainsKey(contentType))
                {
                    throw new InvalidOperationException($"{nameof(contentType)} = `{contentType}` is not present in {nameof(_allPossibleContentTypes)} collection.");
                }
                contentUsedDiskSpace[contentType] = contentBytesUsed;
            }

            return contentUsedDiskSpace;
        }

        public string GetContentDirectory(ContentType contentType, Guid applicationGuid)
        {
            string contentRelativeDir;
            if (!_diskConfig.ContentRelativeDirs.TryGetValue(contentType, out contentRelativeDir))
            {
                throw new ArgumentException($"Cannot get content relative dir for {nameof(contentType)} == `{contentType}`.");
            }
            var appBaseDir = GetApplicationStorageDirectory(applicationGuid);
            return Path.Combine(appBaseDir, contentRelativeDir);
        }

        public string GetApplicationStorageDirectory(Guid applicationGuid)
        {
            // only possible when all contentType app data is stored inside one app base dir
            if(StorageDirectory == null) return null;
            return Path.Combine(StorageDirectory, applicationGuid.ToString());
        }

        #endregion Methods
    }
}
