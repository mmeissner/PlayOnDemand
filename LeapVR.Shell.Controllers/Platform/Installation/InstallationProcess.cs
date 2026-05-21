#region Licence
/****************************************************************
 *  Filename: InstallationProcess.cs
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
using System.IO;
using System.Linq;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using NLog;
using DiskEntityDto = LeapVR.Content.Shared.DiskEntityDto;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    /// <summary>
    /// Installation Process for Container based installations
    /// </summary>
    /// <seealso cref="LeapVR.Shell.Controllers.Platform.Installation.InstallationProcessBase{LeapVR.Shell.Domain.Models.Container.IAppInstallationContainer{LeapVR.Shell.Domain.Models.Container.IContainerPackage}}" />
    internal class InstallationProcess : InstallationProcessBase<IAppInstallationContainer<IContainerPackage>>
    {
        private const string MetaFileDisplayData = @"database\displayData.json";
        private const string MetaFilePlatformData = @"database\platformData.json";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private volatile bool _hasError = false;
        private Exception _exception;
        private IDiskController _diskController;
        public InstallationProcess(IAppInstallationContainer<IContainerPackage> container, IDiskController diskController)
            : base(container, diskController){}

        protected override InstallProcessData SetPostInstallData(InstallProcessData data)
        {
            //Read Meta Data
            var metadataPackage = _diskController
                .GetStoredPackages(ApplicationGuid)
                .Where(q => q.ContentType == ContentType.Metadata)
                .Single();
            var appDisplayDataDto = GetMetadataFile<IAppDisplayDataDto>(metadataPackage);
            var appPlatformDataDto = GetMetadataFile<IAppPlatformDataDto>(metadataPackage);

            data.DisplayData = DtoConverter.Convert(appDisplayDataDto,appPlatformDataDto.PlatformPluginId);
            data.PlatformData = DtoConverter.Convert(appPlatformDataDto,appDisplayDataDto.Name);

            //Delete Meta Data
            try
            {
                Directory.Delete(_diskController.GetContentDirectory(ContentType.Metadata, ApplicationGuid),true);
            }
            catch(Exception exception)
            {
                Logger.Error(exception,"Failed to delete MetaData after Install!");
            }
            return data;
        }

        protected override void InstallLogic(IDiskController diskController)
        {
            _diskController = diskController;
            foreach (var package in Packages)
            {
                CurrentPackage = package;
                if (_hasError)
                {
                    throw _exception;
                }
                using (package.WhenPackageProgressChanged.Subscribe(q => OnPackageProgressChanged(package), OnError))
                {
                    try
                    {
                        diskController.StorePackage(package);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, "Error during StorePackage was performed!");
                    }
                }
            }
        }

        private void OnError(Exception exception)
        {
            Logger.Error(exception,"Error during OnPackageProgressChanged, setting Error Flag!");
            _exception = exception;
            _hasError = true;
        }

        private TOut GetMetadataFile<TOut>(IStoredPackageData metadataPackage)
        {
            string relativePath;
            if (typeof(TOut) == typeof(IAppDisplayDataDto))
            {
                relativePath = MetaFileDisplayData;
            }
            else if (typeof(TOut) == typeof(IAppPlatformDataDto))
            {
                relativePath = MetaFilePlatformData;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported TOut type = `{typeof(TOut)}`.");
            }

            var diskEntity = new DiskEntityDto
            {
                ApplicationGuid = metadataPackage.ApplicationGuid,
                PackageGuid = metadataPackage.PackageGuid,
                RelativePath = relativePath,
            };
            var fileDirectory = _diskController.GetContentDirectory(
                    metadataPackage.ContentType,
                    metadataPackage.ApplicationGuid);
            var filePath = Path.Combine(fileDirectory, diskEntity.RelativePath);
            var appFileContent = File.ReadAllText(filePath);
            return ContainerJsonSerializer.DeserializeObject<TOut>(appFileContent);
        }
    }
}
