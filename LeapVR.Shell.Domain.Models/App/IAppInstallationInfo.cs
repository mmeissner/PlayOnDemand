#region Licence
/****************************************************************
 *  Filename: IAppInstallationInfo.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container.Installation;

namespace LeapVR.Shell.Domain.Models.App {
    /// <summary>
    /// Consist data related to single installed application.
    /// </summary>
    
    public interface IAppInstallationInfo
    {
        
        /// <summary>
        /// Provides the Type of Installation.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        AppInstallationType Type { get; }

        /// <summary>
        /// (UTC) Date and time application was installed to system.
        /// </summary>
        DateTime InstallationDate { get; }

        /// <summary>
        /// <see cref="InstallationState"/> representing current state of installation.
        /// </summary>
        InstallationState InstallationState { get; set; }
    }
}