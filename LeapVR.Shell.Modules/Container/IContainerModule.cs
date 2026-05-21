#region Licence
/****************************************************************
 *  Filename: IContainerModule.cs
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
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Modules.Container
{
    /// <summary>
    /// Module responsible for handling <see cref="IContainer{T}"/> based objects.
    /// </summary>
    public interface IContainerModule
    {
        /// <summary>
        /// Creates handle <see cref="IAppInstallationContainer{T}"/> object used for installing application from header and data files.
        /// </summary>
        /// <param name="headerFilePath">Path to file containing header of container</param>
        /// <returns>Handle <see cref="IAppInstallationContainer{IReadablePackage}"/> object to use when installing application.</returns>
        IAppInstallationContainer<IContainerPackage> GetAppInstallationContainer(string headerFilePath);

        /// <summary>
        /// Creates new <see cref="INewApplicationInstallationContainer"/> instance that can be used to add new packages to it and saved later as container releated (header and data) files.
        /// </summary>
        /// <param name="applicationGuid">Guid to be bound with application</param>
        /// <returns>New <see cref="INewApplicationInstallationContainer"/> object ready to add packages to</returns>
        INewApplicationInstallationContainer CreateNewApplicationInstallationContainer(Guid applicationGuid);
    }
}
