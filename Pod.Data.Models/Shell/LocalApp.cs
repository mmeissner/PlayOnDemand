#region Licence
/****************************************************************
 *  Filename: LocalApp.cs
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
using Pod.Data.Infrastructure;
using Pod.Data.Models.Interfaces;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Representation of an Application Installed on an Station/Device combo
    /// </summary>
    public class LocalApp
    {
        private HashSet<SessionRuleLocalApp> _sessionRules;
        private LocalApp() { }
        private LocalApp(
                ApplicationRoot applicationRoot, UniqueApp uniqueApp, IAppUpdate appUpdate)
        {
            ApplicationRootId = applicationRoot.Id;
            ApplicationRoot = applicationRoot;
            UniqueAppId = uniqueApp.Id;
            UniqueApp = uniqueApp;
            LocalDisplayName = appUpdate.DisplayName;
            InstanceVersion = appUpdate.InstanceVersion;
            IsEnabled = appUpdate.IsEnabled;
            IsInstalled = true;
        }

        public Guid Id { get; private set; }

        /// <summary>
        /// Version information that needs to be increased by the client
        /// each time an update to a app is done, each uninstall will reset
        /// the version and each update should contain a higher version then the previous
        /// </summary>
        public UInt32 InstanceVersion { get; private set; }
        /// <summary>
        /// Display Name used on the Station for this App
        /// </summary>
        public string LocalDisplayName { get; private set; }

        /// <summary>
        /// Indicates if this app is enabled on the station
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Indicates if this app is (still) installed on the station
        /// </summary>
        public bool IsInstalled { get; private set; }

        /// <summary>
        /// The Id of the Application root this local app belongs to
        /// </summary>
        public Guid ApplicationRootId { get; private set; }

        /// <summary>
        /// The Navigation property for the Application Root
        /// </summary>
        public ApplicationRoot ApplicationRoot { get; private set; }

        /// <summary>
        /// The Unique AppId for this Application
        /// </summary>
        public Guid UniqueAppId { get; private set; }

        /// <summary>
        /// The Navigation Property for the UniqueApp
        /// </summary>
        public UniqueApp UniqueApp { get; private set; }

        /// <summary>
        /// All session rules ever contained this LocalApp
        /// </summary>
        public IReadOnlyCollection<SessionRuleLocalApp> SessionRules => _sessionRules;

        /// <summary>
        /// Creates a new Local App
        /// </summary>
        /// <param name="applicationRoot">The Application Root this Local app is for</param>
        /// <param name="uniqueApp">The Unique App this local app is related to</param>
        /// <param name="appUpdate">The AppUpdate provided by the client containing the local app data</param>
        /// <returns>result</returns>
        public static IResult<LocalApp> CreateLocalApp(
                ApplicationRoot applicationRoot, UniqueApp uniqueApp, IAppUpdate appUpdate)
        {
            var result = new Result<LocalApp>();
            result.ArgNotNull(applicationRoot, nameof(ApplicationRoot));
            result.ArgNotNull(uniqueApp, nameof(uniqueApp));
            result.ArgNotNull(appUpdate, nameof(appUpdate));
            if(result.HasError()) return result;

            result.ArgNotLowerThen(appUpdate.InstanceVersion,nameof(appUpdate.InstanceVersion),0,"min allowed timestamp value");
            result.ArgNotNullOrWhitespace(appUpdate.DisplayName, nameof(appUpdate.DisplayName));
            result.ArgNotEqual(appUpdate.ApplicationId,nameof(appUpdate.ApplicationId),Guid.Empty);
            if(result.HasError()) return result;
            return result.Add(new LocalApp(applicationRoot, uniqueApp, appUpdate));
        }

        /// <summary>
        /// Updates this Local App instance with the data from the LocalAppUpdate
        /// </summary>
        /// <param name="appUpdate">Data source</param>
        /// <returns>result</returns>
        public IResult<bool> Update(LocalAppUpdate appUpdate)
        {
            var result = new Result<bool>();
            if(!result.ArgEqual(UniqueAppId,nameof(UniqueAppId),appUpdate.ApplicationId) ||
               !result.ArgNotNull(appUpdate, nameof(appUpdate)) ||
               !result.ArgTrue(appUpdate.IsValid(), nameof(appUpdate.IsValid))) return result;

            if(appUpdate.IsNewer(InstanceVersion))
            {
                LocalDisplayName = appUpdate.DisplayName;
                IsEnabled = appUpdate.IsEnabled;
                InstanceVersion = appUpdate.InstanceVersion;
                result.Add(true);
            }
            return result;
        }

        /// <summary>
        /// Handles the case if LocalApp was reinstalled on the client
        /// </summary>
        /// <param name="appUpdate">The LocalAppUpdate containing the information from the client</param>
        /// <returns>result</returns>
        public IResult Reinstalled(LocalAppUpdate appUpdate)
        {
            var result = new Result();
            if(!result.ArgEqual(UniqueAppId,nameof(UniqueAppId),appUpdate.ApplicationId) ||
               !result.ArgNotNull(appUpdate, nameof(appUpdate)) ||
               !result.ArgTrue(appUpdate.IsValid(), nameof(appUpdate.IsValid))) return result;
            LocalDisplayName = appUpdate.DisplayName;
            IsEnabled = appUpdate.IsEnabled;
            IsInstalled = true;
            InstanceVersion = appUpdate.InstanceVersion;
            return result;
        }

        /// <summary>
        /// Handles the case if LocalApp was uninstalled on the client
        /// </summary>
        public void SetUninstalled()
        {
            IsInstalled = false;
            IsEnabled = false;
            InstanceVersion = 0;
        }
    }
}