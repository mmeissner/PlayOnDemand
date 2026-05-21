#region Licence
/****************************************************************
 *  Filename: IAppInstallationData.cs
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
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.App
{
    /// <summary>
    /// Consist data related to single installed application.
    /// </summary>
    public interface IAppInstallationData : IAppInstallationInfo
    {
        /// <summary>
        /// Guid of application.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Gets or sets the platform plugin unique identifier.
        /// </summary>
        /// <value>
        /// The platform plugin unique identifier.
        /// </value>
        Guid PlatformPluginGuid { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; }

        /// <summary>
        /// Provides the Type of Installation.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        new AppInstallationType Type { get; }

        /// <summary>
        /// (UTC) Date and time application was installed to system.
        /// </summary>
        new DateTime InstallationDate { get; }

        /// <summary>
        /// <see cref="InstallationState"/> representing current state of installation.
        /// </summary>
        new InstallationState InstallationState { get; set; }

        /// <summary>
        /// Sumed amount of all files from all packages that are stored for this application.
        /// </summary>
        int TotalFilesCount { get; set; }

        /// <summary>
        /// Sumed size of all files from all packages that are stored for this application.
        /// </summary>
        long TotalFilesSize { get; set; }

        /// <summary>
        /// Colection of Guids of all packages that are installed and creates one application installation.
        /// </summary>
        IEnumerable<Guid> InstalledPackagesGuids { get; }
    }


    /// <summary>
    /// Defines Types how an App was added to the System
    /// </summary>
    public enum AppInstallationType
    {
        /// <summary>
        /// An App was installed by an VBox Container with Packages Info
        /// </summary>
        Container,
        /// <summary>
        /// An App was installed by an Platform e.g. Steam or Origin
        /// </summary>
        Platform
    }

    
    public enum AppUninstallationType
    {
        /// <summary>
        /// Remove the Application but leaves the App installed on the native Client
        /// In case of Container Install it will be handled like Uninstall
        /// </summary>
        PlatformRemove,
        /// <summary>
        /// Removes the Application and Uninstall from native Client
        /// </summary>
        PlatformUninstall
    }
}
