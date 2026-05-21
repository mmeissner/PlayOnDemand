#region Licence
/****************************************************************
 *  Filename: IPlatformController.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;


namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Controller handling application and platform related matters.
    /// </summary>
    public interface IPlatformController : IController, IRunLevelMsgReceiver
    {
        #region App Methods

        IEnumerable<IAppPlatformInfo> GetInstalledApplications();

        /// <summary>
        /// Gets <see cref="IAppDisplayInfo"/> for application with specified <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <param name="platformGuid"></param>
        /// <returns><see cref="IAppDisplayInfo"/></returns>
        IAppPlatformInfo GetInstalledApplication(Guid applicationGuid, Guid platformGuid);

        /// <summary>
        /// Gets collection of available to start applications, these applications needs
        /// to fullfill multiple requirements evaluated by the controller to be reported as availible
        /// </summary>
        /// <returns>collection of available to start applications' <see cref="IAppDisplayInfo"/></returns>
        IEnumerable<IAppPlatformInfo> GetAvailableApplications();

        /// <summary>
        /// Tries the get an application that must be availible and fullfill the conditions to be a listed/executable app.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <param name="platformApp">The platform application.</param>
        /// <returns></returns>
        bool TryGetAvailableApplication(Guid applicationGuid, out IAppPlatformInfo platformApp);

        /// <summary>
        /// Determines whether the application fullfills all conditions to be availible.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns>
        ///   <c>true</c> if the application is availible; otherwise, <c>false</c>.
        /// </returns>
        bool IsAvailible(Guid applicationGuid);

        /// <summary>
        /// Returns the Applications currently locked
        /// Applications are Locked during Installation, Uninstallation or Play
        /// </summary>
        /// <returns></returns>
        HashSet<Guid> GetLockedApplications();
        #endregion

        #region Execution Methods
        /// <summary>
        /// Is called to get IAppExecution info for an specific Application
        /// Returns information about possible executions for that Application
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <param name="needsFullfilledRequirements">Returns only ExecutionInfo were the system meets the requirements</param>
        /// <returns></returns>
        IAppExecutionInfo GetAppExecutionInfo(Guid applicationGuid, bool needsFullfilledRequirements);

        /// <summary>
        /// Requests the execution object for an application execution.
        /// </summary>
        /// <param name="executable">The display info.</param>
        /// <returns></returns>
        IApplicationExecution RequestExecutionObject(IExecuteable executable);
        #endregion

        #region Install/Uninstall Methods
        /// <summary>
        /// Gets <see cref="IAppInstallationData"/> for application with given <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <returns><see cref="IAppInstallationData"/> if found, null otherwise</returns>
        IAppInstallationData GetApplicationInstallationData(Guid applicationGuid);
        /// <summary>
        /// Gets collection of all installed applications' <see cref="IAppInstallationData"/>.
        /// </summary>
        /// <returns>Collection of all installed applications' <see cref="IAppInstallationData"/></returns>
        IEnumerable<IAppInstallationData> GetApplicationInstallationData(AppInstallationType type);

        /// <summary>
        /// Performs check if installation of specified <see cref="container"/> is possible.
        /// </summary>
        /// <param name="container">Container to check</param>
        /// <returns><see cref="CanInstallStatus"/></returns>
        CanInstallStatus CanInstall(IAppInstallationContainer<IContainerPackage> container);

        /// <summary>
        /// Determines whether a app with the specified Id can be install.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        CanInstallStatus CanInstall(Guid applicationGuid);

        /// <summary>
        /// Performs check if uninstallation of specified application with <see cref="applicationGuid"/> is possible.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <returns><see cref="CanUninstallStatus"/></returns>
        CanUninstallStatus CanUninstall(Guid applicationGuid);

        /// <summary>
        /// Request installation of specific <see cref="container"/>.
        /// </summary>
        /// <param name="container">Container to be installed</param>
        /// <returns><see cref="IInstallationProcessInfo"/></returns>
        void Install(IAppInstallationContainer<IContainerPackage> container);

        /// <summary>
        /// Request uninstallation of application with specified <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <param name="useMinimalDisplayInfo"></param>
        /// <returns><see cref="IUninstallationProcessInfo"/></returns>
        void Uninstall(Guid applicationGuid,Guid platformGuid, bool useMinimalDisplayInfo);

        /// <summary>
        /// Request uninstallation of platform application
        /// </summary>
        /// <param name="platformInfo">The platform display information.</param>
        /// <param name="tryFullUninstall">Try to uninstall from native platform if supported</param>
        void Uninstall(IAppPlatformInfo platformInfo,bool tryFullUninstall);
        #endregion

        #region Platforms
        IEnumerable<IPlatform> GetPlatforms();
        #endregion
    }
}