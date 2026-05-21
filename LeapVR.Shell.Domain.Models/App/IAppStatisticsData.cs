#region Licence
/****************************************************************
 *  Filename: IAppStatisticsData.cs
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

namespace LeapVR.Shell.Domain.Models.App
{
    /// <summary>
    /// Contains statistics for single application.
    /// </summary>
    
    public interface IAppStatisticsData
    {
        /// <summary>
        /// Guid of application.
        /// </summary>
        Guid ApplicationGuid { get; }
        /// <summary>
        /// Gets the Appsd Platform Id.
        /// </summary>
        /// <value>
        /// The platform unique identifier.
        /// </value>
        Guid PlatformGuid { get; }

        /// <summary>
        /// Gets or sets the display name of the App.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; set; }

        /// <summary>
        /// (UTC) Last time application was executed.
        /// </summary>
        DateTime LastTimePlayed { get; }

        /// <summary>
        /// Amount of times application was executed.
        /// </summary>
        int TimesPlayed { get; }

        /// <summary>
        /// Longest session time (in TimeSpan Ticks)
        /// </summary>
        long LongestSession { get; }

        /// <summary>
        /// Total execution time (in TimeSpan Ticks)
        /// </summary>
        long TotalRuntime { get; }
    }
}
