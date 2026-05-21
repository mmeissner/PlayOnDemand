#region Licence
/****************************************************************
 *  Filename: AppInstallationData.cs
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
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    
    public class AppInstallationData : IAppInstallationData
    {
        public AppInstallationData(IAppPlatformInfo platformInfo, List<IPackageData> packages)
        {
           
            ApplicationGuid = platformInfo.ApplicationGuid;
            PlatformPluginGuid = platformInfo.PlatformGuid;
            DisplayName = platformInfo.Name;
            Type = AppInstallationType.Platform;
            InstallationDate = DateTime.UtcNow;
            IsEnabled = true;
            InstallationState = InstallationState.InstallationInProgress;
            TotalFilesCount = packages.Sum(x=> x.TotalFilesCount);
            TotalFilesSize = packages.Sum(x=>x.TotalFilesSize);
            InstalledPackagesGuids = packages.Select(x=>x.PackageGuid);
        }

        public AppInstallationData(IAppInstallationContainer<IContainerPackage> container) 
        {
            ApplicationGuid = container.ApplicationGuid;
            //PlatformPluginGuid = platformPluginGuid;
            DisplayName = container.DisplayName;
            InstallationDate = DateTime.UtcNow;
            InstallationState = InstallationState.InstallationInProgress;
            IsEnabled = true;
            TotalFilesCount = container.TotalFilesCount;
            TotalFilesSize = container.TotalFilesSize;
            InstalledPackagesGuids = container.GetPackages().Select(q => q.PackageGuid).ToList();
            Type = AppInstallationType.Container;
        }

        public Guid ApplicationGuid { get;  }
        public Guid PlatformPluginGuid { get; set; }
        public string DisplayName { get; }
        public AppInstallationType Type { get; set; }
        public DateTime InstallationDate { get; set; }
        public bool IsEnabled { get; set; }
        public InstallationState InstallationState { get; set; }

        public int TotalFilesCount { get; set; }
        public long TotalFilesSize { get; set; }
        public IEnumerable<Guid> InstalledPackagesGuids { get; set; }
    }
}
