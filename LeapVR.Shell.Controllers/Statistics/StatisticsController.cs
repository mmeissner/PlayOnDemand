#region Licence
/****************************************************************
 *  Filename: StatisticsController.cs
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
using System.Linq;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Statistics;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Controllers.Statistics
{
    public class StatisticsController : IStatisticsController
    {

        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAppStatisticsRepository _statisticsRepo;
        private readonly IPlatformController _platformController;

        #endregion

        #region Constructors

        public StatisticsController(IAppStatisticsRepository statisticsRepo,IPlatformController platformController)
        {
            _statisticsRepo = statisticsRepo;
            _platformController = platformController;
        }
        #endregion

        #region Methods
        public IAppStatistic GetApplicationWithLongestTotalSession()
        {
            return (from statisticsData in _statisticsRepo.GetAll().OrderByDescending(item => item.TotalRuntime)
                    let displayData = _platformController.GetInstalledApplication(statisticsData.ApplicationGuid,statisticsData.PlatformGuid)
                    where displayData != null
                    select new AppStatistic(displayData, statisticsData)).FirstOrDefault();
        }

        public IAppStatistic GetApplicationWithLongestSession()
        {
            return (from statisticsData in _statisticsRepo.GetAll().OrderByDescending(item => item.LongestSession)
                    let displayData = _platformController.GetInstalledApplication(statisticsData.ApplicationGuid,statisticsData.PlatformGuid)
                    where displayData != null
                    select new AppStatistic(displayData, statisticsData)).FirstOrDefault();
        }

        public IEnumerable<IAppStatistic> GetAllAppStatistics()
        {
            return  from data in _statisticsRepo.GetAll()
                    let displayInfo = _platformController.GetInstalledApplication(data.ApplicationGuid,data.PlatformGuid)
                    where displayInfo != null
                    select new AppStatistic(displayInfo, data);
        }

        public IAppStatistic GetApplicationWithMostExecutedTimes()
        {
            return (from statisticsData in _statisticsRepo.GetAll().OrderByDescending(item => item.TimesPlayed)
                    let displayData = _platformController.GetInstalledApplication(statisticsData.ApplicationGuid,statisticsData.PlatformGuid)
                    where displayData != null
                    select new AppStatistic(displayData, statisticsData)).FirstOrDefault();
        }

        public TimeSpan GetTotalRuntimeForAllApplications()
        {
            return TimeSpan.FromTicks(_statisticsRepo.GetAll().Sum(item => item.TotalRuntime));
        }

        public void ClearStatisticsDataForSpecificApp(Guid applicationGuid)
        {
            _statisticsRepo.Delete(applicationGuid);
        }

        public void ClearStatisticsDataForAllApp()
        {
            foreach (var appData in _statisticsRepo.GetAll().ToList())
            {
                _statisticsRepo.Delete(appData.ApplicationGuid);
            }
        }

        public void OnExecutionMessage(AppExecutionMessage message)
        {
            if (message.Phase != ExecutionPhase.AfterExit) return;
            UpdateStatisticsData(message);
        }

        #endregion
        private void UpdateStatisticsData(AppExecutionMessage executionMessage)
        {
            if (executionMessage.AppExecutionData?.Started == null || executionMessage.AppExecutionData.Stopped == null)
            {
                Logger.Error($"{nameof(IApplicationExecution.Started)} or {nameof(IApplicationExecution.Stopped)} is null.");
                return;
            }
           
            _statisticsRepo.CreateOrUpdate(executionMessage.AppExecutionData);
        }
    }
}
