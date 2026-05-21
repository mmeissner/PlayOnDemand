#region Licence
/****************************************************************
 *  Filename: IAppPlatformInfo.cs
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
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Domain.Models.App
{
    public delegate void PlatformAppUpdatedEventHandler(IAppPlatformInfo platformInfo, PlatformAppUpdate updateType); 
    
    public interface IAppPlatformInfo: IAppDisplayInfo, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the platform unique identifier.
        /// </summary>
        /// <value>
        /// The platform unique identifier.
        /// </value>
        Guid PlatformGuid { get; }

        /// <summary>
        /// Gets Application Id on Platform.
        /// </summary>
        /// <value>
        /// The platform application identifier.
        /// </value>
        ulong PlatformAppId {get;}

        /// <summary>
        /// Gets a value indicating whether this instance is a state it can be fully visually.
        /// Represented
        /// Call <see cref="GetOrUpdateDisplayDataAsync"/> to initialize the Data
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        bool IsDisplayable { get; }

        /// <summary>
        /// Gets a value indicating whether a Update is in progress.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [update in progress]; otherwise, <c>false</c>.
        /// </value>
        bool UpdateInProgress { get; }

        /// <summary>
        /// Gets a value indicating whether a License is required
        /// </summary>
        /// <value>
        ///   <c>true</c> if [license required]; otherwise, <c>false</c>.
        /// </value>
        bool IsLicenseRequired {get;}

        bool IsEnabled { get; }

        void SetEnabled(bool enabled);

        /// <summary>
        /// Gets the install state asynchronous.
        /// </summary>
        /// <returns></returns>
        PlatformInstallState ClientInstallState();

        /// <summary>
        /// Gets the License information for that App.
        /// </summary>
        /// <returns></returns>
        IAppLicenseInfo LicenseInfo();

        /// <summary>
        /// Tries to get the Installation information.
        /// Use this method instead of <see cref="SystemInstallState"></see> if you need the additional data/>
        /// </summary>
        /// <param name="installationInfo">The installation information.</param>
        /// <returns>true if availible, false if not availible</returns>
        bool TryGetInstallationInfo(out IAppInstallationInfo installationInfo);

        /// <summary>
        /// Gets a value indicating the installion state for to the system.
        /// </summary>
        /// <value>
        /// The state of the system install.
        /// </value>
        InstallationState SystemInstallState();

        /// <summary>
        /// Initializes the Platform Properties Asynchronous.
        /// Can also be called to Update the Data
        /// </summary>
        /// <returns>True if Data was updated, false if update is already in progress or if data could not be updated</returns>
        Task<bool> GetOrUpdateDisplayDataAsync();

        /// <summary>
        /// Determines whether this App can be Installed
        /// App must be Initialized to return useable value
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can install be installed; otherwise, <c>false</c>.
        /// </returns>
        bool CanInstall();

        /// <summary>
        /// Determines whether this instance can be Uninstalled.
        /// App must be Initialized to return useable value
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can uninstall; otherwise, <c>false</c>.
        /// </returns>
        bool CanUninstall();

        /// <summary>
        /// Determines whether this Application can be uninstalled from a native client of the platform
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can platform uninstall]; otherwise, <c>false</c>.
        /// </returns>
        bool CanPlatformUninstall();

        /// <summary>
        /// Installs an Application, but Local or Online Installation needs to be supported by the platform
        /// </summary>
        void Install();

        /// <summary>
        /// Uninstalls an Application
        /// </summary>
        /// <param name="tryFullUninstall">if set to <c>true</c>will try to uninstall from native platform.</param>
        void Uninstall(bool tryFullUninstall);

        /// <summary>
        /// Determines whether this application has any platform account associated
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has platform account]; otherwise, <c>false</c>.
        /// </returns>
        bool HasPlatformAccount();

        /// <summary>
        /// Tries the get an account with availible access for application.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountAccess">The account access.</param>
        /// <returns>true if it succeeds, false if not account with access could be allocated</returns>
        bool TryGetAccountAccessForApp(out IPlatformAccount account, out IAccountAccess accountAccess);

        bool AddLicense(IPlatformAccount account);

        bool RemoveLicense(IPlatformAccount account);

        Task<FirewallState> GetFirewallStateAsync();

        void SetFirewallState(FirewallState firewallState);

        bool TryGetAppExecutableUpdate(out IAppExecutablesUpdate update);

        event PlatformAppUpdatedEventHandler PlatformAppUpdated;

    }

    /// <summary>
    /// The Application State of an app belonging to a Platform
    /// that supports a <see cref="InstallationType"/> of <see cref="InstallationType.Local"/> and/or <see cref="InstallationType.Online"/>
    /// </summary>
    public enum PlatformInstallState
    {
        /// <summary>
        /// The App is not Locally Installed and Online Unavailable
        /// </summary>
        Unavailable,
        /// <summary>
        /// The App is locally Installed
        /// </summary>
        Local,
        /// <summary>
        /// The App is locally NOT installed but online availible
        /// </summary>
        Online,
        /// <summary>
        /// The App is currently beeing installed
        /// </summary>
        Installing,
        /// <summary>
        /// An error occured during Installation or to determine Installation State
        /// </summary>
        Error
    }

    public enum PlatformAppUpdate
    {
        PlatformInstallation,
        SystemInstallation,
        Licensing
    }
}
