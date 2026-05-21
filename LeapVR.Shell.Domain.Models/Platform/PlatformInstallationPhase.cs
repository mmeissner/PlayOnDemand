#region Licence
/****************************************************************
 *  Filename: PlatformInstallationPhase.cs
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
namespace LeapVR.Shell.Domain.Models.Platform {
    public enum PlatformInstallationPhase
    {
        /// <summary>
        /// When Installation is signaled as started
        /// </summary>
        Started,
        /// <summary>
        /// When the platform connection/client is starting
        /// </summary>
        PlatformStartup,
        /// <summary>
        /// When the Platform is downloading the application
        /// </summary>
        PlatformDownloading,
        /// <summary>
        /// When the platform is installing the application
        /// </summary>
        PlatformInstalling,
        /// <summary>
        /// When the System is Installing/Adding the application
        /// </summary>
        SystemInstalling,
        Finished,
        Error
    }
}