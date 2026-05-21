#region Licence
/****************************************************************
 *  Filename: EntityConverter.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Repository.Entities
{
    /// <summary>
    /// Converts Data Model Entities to Database Entities
    /// </summary>
    static class EntityConverter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static AppPlatformDataDb Convert(IAppPlatformData platformData)
        {
            if(platformData is AppPlatformDataDb platformDataDb)
            {
                return platformDataDb;
            }
            return new AppPlatformDataDb(platformData);
        }

        public static MultimediaPlaylistDataDb Convert(IMultimediaPlaylistData playlistData)
        {
            if(playlistData is MultimediaPlaylistDataDb playlistDataDb)
            {
                return playlistDataDb;
            }
            return new MultimediaPlaylistDataDb(playlistData);
        }

        public static List<ProcessExecutionLogicDb> Convert(IEnumerable<IProcessExecutionLogic> processExecutionLogics)
        {
            return processExecutionLogics.Select(instruction => new ProcessExecutionLogicDb(instruction)).ToList();
        }

        public static AppDisplayDataDb Convert(IAppDisplayData displayData)
        {
            if(displayData is AppDisplayDataDb displayDataDb)
            {
                return displayDataDb;
            }
            return new AppDisplayDataDb(displayData);
        }

        public static AppInstallationDataDb Convert(IAppInstallationData appInstallationData)
        {
            if(appInstallationData is AppInstallationDataDb installationDataDb)
            {
                return installationDataDb;
            }
            return new AppInstallationDataDb(appInstallationData);
        }

        public static PlatformAccountDataDb Convert(IPlatformAccountData appPlatformAccountData)
        {
            if(appPlatformAccountData is PlatformAccountDataDb accountDataDb)
            {
                return accountDataDb;
            }
            return new PlatformAccountDataDb(appPlatformAccountData);
        }

        public static AppStatisticsDataDb Convert(IApplicationExecution execution)
        {
            var retval = new AppStatisticsDataDb();
            retval.DisplayName = execution.DisplayInfo.Name;
            retval.ApplicationGuid = execution.LogicToExecute.ApplicationGuid;
            retval.PlatformGuid = execution.LogicToExecute.PlatformPluginId;
            if(execution.Stopped.HasValue) retval.LastTimePlayed = execution.Stopped.Value;
            else if(execution.Started.HasValue) retval.LastTimePlayed = execution.Started.Value;
            else
            {
                Logger.Error("Can not Convert to AppStatisticsDataDb object without an Start or Stop Date!",execution);
                throw new ArgumentNullException(nameof(execution),@"Can not covert AppStatisticsDataDb without Start or Stop Date");
            }
            retval.LongestSession = execution.ExecutionDurationTicks();
            retval.TotalRuntime = retval.LongestSession;
            retval.TimesPlayed = 1;
            return retval;
        }
    }
}
