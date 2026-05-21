#region Licence
/****************************************************************
 *  Filename: PlatformDisplayDataPackage.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Disk;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    class PlatformDisplayDataPackage : IExtractablePackage, IPackageData
    {
        private const string ImageSubPath = @"images";
        private const string ImageDefaultName = "mainPicture.png";
        private readonly IAppPlatformInfo _platformInfo;
        private string _directoryPath;
        public Guid PackageGuid { get; }
        public uint PackageVersion { get; }
        public int TotalFilesCount { get; }
        public long TotalFilesSize { get; }
        public Guid ApplicationGuid { get; }
        public ContentType ContentType { get; }
        public PlatformDisplayDataPackage(IAppPlatformInfo platformInfo)
        {
            _platformInfo = platformInfo;
            ApplicationGuid = platformInfo.ApplicationGuid;
            PackageGuid = Guid.NewGuid();
            PackageVersion = 1;
            TotalFilesCount = 1;
            TotalFilesSize = platformInfo.Thumbnail.LongLength;
            ContentType = ContentType.MediaFiles;
        }

        public void ExtractToDirectory(string directoryPath)
        {
            if(String.IsNullOrEmpty(directoryPath) || !String.IsNullOrEmpty(_directoryPath))
            {
                throw new NotSupportedException(
                        $"{nameof(ExtractToDirectory)} can only be called once and only with a valid directoryPath, directoryPath={directoryPath} ");
            }

            _directoryPath = directoryPath;
            var path = Path.Combine(directoryPath, ImageSubPath);
            Directory.CreateDirectory(path);
            File.WriteAllBytes(Path.Combine(path, ImageDefaultName), _platformInfo.Thumbnail);
        }
        public IAppDisplayData GetDisplayData()
        {
            return new AppDisplayData()
                   {
                           ApplicationGuid = _platformInfo.ApplicationGuid,
                           Name = _platformInfo.Name,
                           Category = _platformInfo.Category.Identifier,
                           Description = _platformInfo.Description,
                           MainPicture = new DiskEntity(_platformInfo.ApplicationGuid,_platformInfo.PlatformGuid,DiskEntityType.Relative,$"{ImageSubPath}\\{ImageDefaultName}",PackageGuid)
                   };
        }
        public override string ToString()
        {
            return $"{nameof(ApplicationGuid)}={ApplicationGuid} {Environment.NewLine}" +
                   $"{nameof(ContentType)}={ContentType} {Environment.NewLine}" +
                   $"{nameof(PackageGuid)}={PackageGuid} {Environment.NewLine} " +
                   $"{nameof(PackageVersion)}={PackageVersion} {Environment.NewLine}" +
                   $"{nameof(TotalFilesCount)}={TotalFilesCount} {Environment.NewLine}" +
                   $"{nameof(TotalFilesSize)}={TotalFilesSize}";
        }
    }
}