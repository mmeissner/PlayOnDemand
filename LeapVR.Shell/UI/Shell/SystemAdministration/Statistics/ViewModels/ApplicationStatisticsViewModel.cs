#region Licence
/****************************************************************
 *  Filename: ApplicationStatisticsViewModel.cs
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
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Statistics;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Base;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels
{
    public class ApplicationStatisticsViewModel : ApplicationBaseViewModel
    {

        #region Fields & Properties
        private readonly IStatisticsController _statisticsController;
        private int _number;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                NotifyOfPropertyChange();
            }
        }

        private string _lastTimePlayed;
        public string LastTimePlayed
        {
            get => _lastTimePlayed;
            set
            {
                _lastTimePlayed = value;
                NotifyOfPropertyChange();
            }
        }


        private string _timesPlayed;
        public string TimesPlayed
        {
            get => _timesPlayed;
            set
            {
                _timesPlayed = value;
                NotifyOfPropertyChange();
            }
        }

        private string _longestSession;
        public string LongestSession
        {
            get => _longestSession;
            set
            {
                _longestSession = value;
                NotifyOfPropertyChange();
            }
        }


        private string _totalRuntime;
        public string TotalRuntime
        {
            get => _totalRuntime;
            set
            {
                _totalRuntime = value;
                NotifyOfPropertyChange();
            }
        }


        #endregion

        #region Constructors
        public ApplicationStatisticsViewModel(IAppStatistic appStatistic,IStatisticsController statisticsController) : base(appStatistic.DisplayInfo)
        {
            _statisticsController = statisticsController;
            LastTimePlayed = appStatistic.LastTimePlayed.ToShortDateString();
            TimesPlayed = appStatistic.TimesPlayed <= 0 ? Resources.System_Statistics_NoData : appStatistic.TimesPlayed.ToString();

            LongestSession = appStatistic.LongestSession == TimeSpan.Zero ? Resources.System_Statistics_NoData : $"{(int)appStatistic.LongestSession.TotalHours} H {appStatistic.LongestSession.Minutes} M";
            TotalRuntime = appStatistic.TotalSessions == TimeSpan.Zero ? Resources.System_Statistics_NoData : $"{(int)appStatistic.TotalSessions.TotalHours} H {appStatistic.TotalSessions.Minutes} M";
        }
        #endregion

        #region Methods

        public void ResetStatisticsData()
        {
            _statisticsController.ClearStatisticsDataForSpecificApp(ApplicationGuid);
            ResetStatisticsDataForView();

        }

        private void ResetStatisticsDataForView()
        {
            LastTimePlayed = Resources.System_Statistics_NoData;
            TimesPlayed = Resources.System_Statistics_NoData;
            LongestSession = Resources.System_Statistics_NoData;
            TotalRuntime = Resources.System_Statistics_NoData;
        }

        #endregion

    }
}
