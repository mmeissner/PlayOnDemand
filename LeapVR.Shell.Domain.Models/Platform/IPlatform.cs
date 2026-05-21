#region Licence
/****************************************************************
 *  Filename: IPlatform.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Domain.Models.Platform
{
    public interface IPlatform
    {
        /// <summary>
        /// Gets the platform unique identifier provided by the Platform Module.
        /// </summary>
        /// <value>
        /// The platform unique identifier.
        /// </value>
        Guid PlatformGuid { get; }

        /// <summary>
        /// Gets the supported installation types from this platform.
        /// </summary>
        /// <value>
        /// The supported installation types.
        /// </value>
        InstallationType SupportedInstallationTypes { get; }

        /// <summary>
        /// Gets the type of the supported accounts that will also influcense the Availibility Check
        /// </summary>
        /// <value>
        /// The type of the supported account.
        /// </value>
        AccountType SupportedAccountType { get; }

        /// <summary>
        /// Gets a value indicating whether a native client App uinstallation is supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [platform uninstall supported]; otherwise, <c>false</c>.
        /// </value>
        bool PlatformUninstallSupported { get; }

        /// <summary>
        /// Gets the IAppPlatformDisplayInfo for an installed platform application
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>null if not installed</returns>
        IAppPlatformInfo GetInstalledPlatformApp(Guid applicationId);

        /// <summary>
        /// Gets an platform application, there is no check whatsoever if it
        /// really exists or is linked to an account and so on
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns></returns>
        IAppPlatformInfo GetPlatformApp(Guid applicationId);

        /// <summary>
        /// Tries to get all Platform Apps that can be known to the System, this includes installed apps
        /// apps that are locally installed for that platform, and apps that a license exists for in any
        /// linked platform account
        /// </summary>
        /// <param name="callback">The callback that provides the platform apps.</param>
        /// <param name="completed">The callback that is executed when all the callbacks are completed.</param>
        void GetPlatformApps(Action<IAppPlatformInfo> callback, Action completed,SynchronizationContext context);

        /// <summary>
        /// Gets all Platform accounts for the specified PlatformType
        /// </summary>
        /// <returns>Platform Accounts</returns>
        List<IPlatformAccount> GetPlatformAccounts();

        /// <summary>
        /// Creates the an new Platform Account.
        /// </summary>
        /// <param name="username">The Username.</param>
        /// <param name="password">The Password.</param>
        /// <param name="platformPluginId">The Platform Id.</param>
        /// <param name="platformAccount">The new platform account.</param>
        /// <returns>
        ///   <c>true</c> if [is account created]; otherwise, <c>false</c>.
        /// </returns>
        bool CreatePlatformAccount(string username,string password,Guid platformPluginId,out IPlatformAccount platformAccount);

        /// <summary>
        /// Deletes the Platform Account.
        /// </summary>
        /// <param name="platformAccount">The platform account.</param>
        /// <returns>
        ///   <c>true</c> if [is account is deleted]; otherwise, <c>false</c>.
        /// </returns>
        bool DeletePlatformAccount(IPlatformAccount platformAccount);
    }

}
