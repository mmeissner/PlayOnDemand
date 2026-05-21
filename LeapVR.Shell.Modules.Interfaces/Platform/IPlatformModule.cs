#region Licence
/****************************************************************
 *  Filename: IPlatformModule.cs
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
using System.Reflection;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Modules.Interfaces.Platform
{
    /// <summary>
    /// A Platform Module provides Platform specific implementation used by a Platform
    /// </summary>
    public interface IPlatformModule :IBaseModule
    {

        /// <summary>
        /// Gets a value indicating whether this instance is available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        bool IsAvailable { get; }
        /// <summary>
        /// Gets the installation types supported by this Platform.
        /// </summary>
        /// <value>
        /// The supported installation types.
        /// </value>
        InstallationType SupportedInstallationTypes { get; }

        /// <summary>
        /// Gets the Account Types that are supported by this platform.
        /// Allowed combinations are None or Manually or Automatic and Manually
        /// </summary>
        /// <value>
        /// The type of the supported account.
        /// </value>
        AccountType SupportedAccountType { get; }

        /// <summary>
        /// Gets a value indicating whether this Platform requires and Account and provides Licenses.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires account]; otherwise, <c>false</c>.
        /// </value>
        bool RequiresAccount  { get; }

        /// <summary>
        /// Gets a value indicating whether the Platform supports Uninstallation for non Container Apps.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [platform uninstall supported]; otherwise, <c>false</c>.
        /// </value>
        bool PlatformUninstallSupported  { get; }

        /// <summary>
        /// Name for the Platform, can be MAX 8 ASCII Chars long
        /// Is used to generate Platform Unique Application Guids
        /// </summary>
        /// <value>
        /// The name of the platform.
        /// </value>
        string PlatformNameId { get; }

        /// <summary>
        /// Determines whether an Application of an Platform is availible by the Platform.
        /// Availible apps must be installed and runable on the Platform
        /// </summary>
        /// <param name="appId">The application identifier.</param>
        /// <returns>
        ///   <c>true</c> if the application is availible; otherwise, <c>false</c>.
        /// </returns>
        bool IsApplicationAvailable(Guid appId);

        /// <summary>
        /// Gets a value indicating whether this instance has a cache.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has cache; otherwise, <c>false</c>.
        /// </value>
        bool HasCache { get; }

        /// <summary>
        /// Gets the all apps that are installed locally on that platform.
        /// </summary>
        /// <returns>Dictionary with ApplicationId and PlatformData</returns>
        Dictionary<Guid, IAppPlatformData> GetLocalInstallations();

        /// <summary>
        /// Gets a locally installed app from that platform.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>The AppPlatform Data collected from the Platform</returns>
        IAppPlatformData GetLocalInstallation(Guid applicationId);

        /// <summary>
        /// Request the module to report if that application is locally installed
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>true if its locally available, false if not</returns>
        bool IsLocalInstalled(Guid applicationId);

        /// <summary>
        /// Clears the Platform Module Cache if any available.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Called to install an App from an Platform Online Source
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="accountAccess">The account access.</param>
        /// <param name="progressReportCallBack">Callback to report the progress of the installation.</param>
        /// <param name="installedApp">The IAppPlatformData of the newly installed application</param>
        /// <returns>True in case of success, false if failure</returns>
        bool OnlineInstallation(
                Guid applicationId, IAccountAccess accountAccess,
                Action<PlatformInstallationPhase> progressReportCallBack, out IAppPlatformData installedApp);

        /// <summary>
        /// Gets the display information from an OnlineSource.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="addImage">Specifies if a image should be downloaded</param>
        /// <returns></returns>
        Task<IAppDisplayInfo> GetOnlineDisplayInfoAsync(Guid applicationId, bool addImage);

        /// <summary>
        /// Receives the licensed applications from online by the Platform.
        /// </summary>
        /// <param name="platformAccount">The platform account.</param>
        /// <returns></returns>
        Task<HashSet<Guid>> GetApplicationsFromAccountAsync(IPlatformAccount platformAccount);

        /// <summary>
        /// Creates the object required for the application Execution.
        /// </summary>
        /// <param name="appPlatformInfo">The application display information.</param>
        /// <param name="appPlatformData">The applications platform data</param>
        /// <param name="executionLogic">The execution object that should be run by the engine.</param>
        /// <returns></returns>
        IApplicationExecution CreateExecution(IAppPlatformInfo appPlatformInfo,IAppPlatformData appPlatformData,IProcessExecutionLogic executionLogic);
    }
}