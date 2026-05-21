#region Licence
/****************************************************************
 *  Filename: AppStatisticsRepository.cs
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
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Repository
{
    public class AppStatisticsRepository : IAppStatisticsRepository
    {        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public bool Delete(Guid applicationGuid)
        {
            try
            {
                var application =
                    Database.Database.QueryDatabase<AppStatisticsDataDb, AppStatisticsDataDb>(
                        col => col.FindOne(x => x.ApplicationGuid == applicationGuid));
                if (application == null) return true;
                return application.Delete<AppStatisticsDataDb>();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Error on {nameof(Delete)} of {nameof(IAppStatisticsData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }

        public IAppStatisticsData Get(Guid applicationGuid)
        {
            try
            {
                return PrivateGet(applicationGuid);
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IAppStatisticsData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }

        public IEnumerable<IAppStatisticsData> GetAll()
        {
            try
            {
                // TODO: [FH] decide what should the All stands for? Installed Apps or Available Apps or all the apps ever exist?
                return Database.Database.QueryDatabase<IEnumerable<AppStatisticsDataDb>, AppStatisticsDataDb>(collection => collection.FindAll()).ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAll)} of {nameof(IAppStatisticsData)}", exception);
            }
        }

        public bool CreateOrUpdate(IApplicationExecution applicationExecution)
        {
            try
            {
                if (applicationExecution.LogicToExecute == null || applicationExecution.LogicToExecute.ApplicationGuid == Guid.Empty)
                {
                    Logger.Error("Insufficent Data for persistence", applicationExecution);
                    throw new NotSupportedException($"ApplicationGuid for {nameof(IApplicationExecution)} must be set!");
                }
                //Check if we have already an Record
                var appStatisticsData = PrivateGet(applicationExecution.LogicToExecute.ApplicationGuid);
                if(appStatisticsData == null)
                {
                    //Create New Record
                    appStatisticsData = EntityConverter.Convert(applicationExecution);
                }
                else
                {
                    //Update Existing Record
                    var currentSessionDuration = applicationExecution.ExecutionDurationTicks();
                    appStatisticsData.TimesPlayed = appStatisticsData.TimesPlayed + 1;
                    appStatisticsData.LongestSession = Math.Max(appStatisticsData.LongestSession, currentSessionDuration);
                    appStatisticsData.TotalRuntime = appStatisticsData.TotalRuntime + currentSessionDuration;
                }
                appStatisticsData.Store();
                return true;
            }
            catch (LiteDB.LiteException exception)
            {
                Logger.Error($"Error on {nameof(CreateOrUpdate)} of {nameof(IApplicationExecution)}",applicationExecution);
                throw new RepositoryGetDbException($"Error on {nameof(CreateOrUpdate)} of {nameof(IApplicationExecution)}  with Execution = {applicationExecution}", exception);
            }
        }
        private AppStatisticsDataDb PrivateGet(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<AppStatisticsDataDb, AppStatisticsDataDb>(collection =>
                                                                                                         collection.FindOne(x => x.ApplicationGuid == applicationGuid));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IAppStatisticsData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }
    }
}
