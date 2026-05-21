#region Licence
/****************************************************************
 *  Filename: AppInstallationDataDb.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class AppInstallationDataDb : IEntity,IAppInstallationData
    {
        public Guid Id { get; set; }
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformPluginGuid { get; set; }
        public string DisplayName { get; set; }
        public AppInstallationType Type { get; set; }
        public DateTime InstallationDate { get; set; }
        public InstallationState InstallationState { get; set; }

        public int TotalFilesCount { get; set; }
        public long TotalFilesSize { get; set; }
        public IEnumerable<Guid> InstalledPackagesGuids { get; set; }

        public AppInstallationDataDb(){}
        public AppInstallationDataDb(IAppInstallationData appInstallationData)
        {
            ApplicationGuid = appInstallationData.ApplicationGuid;
            PlatformPluginGuid = appInstallationData.PlatformPluginGuid;
            DisplayName = appInstallationData.DisplayName;
            Type = appInstallationData.Type;
            InstallationDate = appInstallationData.InstallationDate;
            InstallationState = appInstallationData.InstallationState;
            TotalFilesCount = appInstallationData.TotalFilesCount;
            TotalFilesSize = appInstallationData.TotalFilesSize;
            InstalledPackagesGuids = appInstallationData.InstalledPackagesGuids;
        }
    }
}
