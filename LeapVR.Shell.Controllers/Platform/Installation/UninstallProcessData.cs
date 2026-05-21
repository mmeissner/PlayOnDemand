#region Licence
/****************************************************************
 *  Filename: UninstallProcessData.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    class UninstallProcessData 
    {
        internal Guid ApplicationGuid { get; }
        public Guid PlatformGuid { get; }
        public AppInstallationType Type { get; }
        public AppUninstallationType UninstallationType { get; }
        internal string ApplicationName { get; }
        internal byte[] ApplicationThumbnail { get;  }
        internal Exception Exception { get; private set; }
        internal Action<UninstallProcessData> FinalizeAction { get; private set; }

        public UninstallProcessData(IAppDisplayInfo displayInfo, IAppInstallationData installationData,AppUninstallationType uninstallationType)
        {
            UninstallationType = uninstallationType;
            ApplicationGuid = installationData.ApplicationGuid;
            PlatformGuid = installationData.PlatformPluginGuid;
            Type = installationData.Type;
            ApplicationName = displayInfo.Name;
            ApplicationThumbnail = displayInfo.Thumbnail;
        }

        public void AddException(Exception exception) { Exception = exception; }
        public void AddFinalizeAction(Action<UninstallProcessData> finalizeAction) { FinalizeAction = finalizeAction; }
    }
}