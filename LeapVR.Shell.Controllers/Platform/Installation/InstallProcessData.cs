#region Licence
/****************************************************************
 *  Filename: InstallProcessData.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    class InstallProcessData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProcessData"/> class.
        /// </summary>
        /// <param name="appInfo">The application display information.</param>
        /// <param name="packages">Data of all packages that are to be persisted.</param>
        internal InstallProcessData(IAppPlatformInfo appInfo, List<IPackageData> packages)
        {
            InstallationData = new AppInstallationData(appInfo,packages);
        }

        internal InstallProcessData(IAppInstallationContainer<IContainerPackage> container)
        {
            InstallationData = new AppInstallationData(container);
        }

        internal IAppInstallationData InstallationData { get; set; }
        internal IAppDisplayData DisplayData { get; set; }
        internal IAppPlatformData PlatformData { get; set; }
        internal Action<InstallProcessData> FinalizeAction{get;set;}
        internal Exception Exception { get; set; }
    }
}
