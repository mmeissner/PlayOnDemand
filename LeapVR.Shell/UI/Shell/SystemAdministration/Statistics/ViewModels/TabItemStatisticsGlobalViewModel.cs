#region Licence
/****************************************************************
 *  Filename: TabItemStatisticsGlobalViewModel.cs
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
using Caliburn.Micro;
using Humanizer;
using Humanizer.Localisation;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Universal.Dialog;
using Resources = LeapVR.Shell.Language.Resources;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels
{
    public sealed class TabItemStatisticsGlobalViewModel : TabItemStatisticsScreen
    {

        #region Fields & Properties
        public override int DisplayOrder => 10;
        public override string DisplayName
        {
            get { return Resources.System_Statistics_Global; }
            set { /* ignore */ }
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
        private string _longestPlayedGame;
        public string LongestPlayedGame
        {
            get => _longestPlayedGame;
            set
            {
                _longestPlayedGame = value;
                NotifyOfPropertyChange();
            }
        }
        private string _longestGameSession;
        public string LongestGameSession
        {
            get => _longestGameSession;
            set
            {
                _longestGameSession = value;
                NotifyOfPropertyChange();
            }
        }
        private string _mostPlayedGame;
        public string MostPlayedGame
        {
            get => _mostPlayedGame;
            set
            {
                _mostPlayedGame = value;
                NotifyOfPropertyChange();
            }
        }

        private readonly IWindowManager _windowManager;
        private readonly IAppStatisticsRepository _appStatisticsRepo;
        private readonly IAppDisplayRepository _appDisplayRepo;
        //TODO Remove Statistics Controller
        private readonly IStatisticsController _statisticsController;
        private readonly ViewModelFactory _viewModelFactory;
        #endregion

        #region Constructors
        public TabItemStatisticsGlobalViewModel(
            IUIMessageBroker messageBroker,
            IWindowManager windowManager,
            IAppDisplayRepository appDisplayRepository,
            IAppStatisticsRepository appStatisticsRepo,
            IStatisticsController statisticsController,
            ViewModelFactory viewModelFactory
            ) : base(messageBroker,"IconStatisticsGlobal")
        {
            QuickLeap.AssertNotNull(
                windowManager,
                appDisplayRepository,
                appStatisticsRepo,
                statisticsController,
                viewModelFactory);


            _windowManager = windowManager;
            _appDisplayRepo = appDisplayRepository;
            _appStatisticsRepo = appStatisticsRepo;
            _statisticsController = statisticsController;
            _viewModelFactory = viewModelFactory;
        }

        #endregion

        #region Methods

        protected override void OnActivate()
        {
            RefreshGlobalStatisticsData();
            base.OnActivate();
        }

        protected override void HandleLanguageChange(IUILanguageChangedEvent message)
        {
            //Units are translated
            RefreshGlobalStatisticsData();
        }

        public void ResetVrBoxStatistics()
        {
            var viewModel = _viewModelFactory.Build(DialogType.ResetVrBoxStatistics);
            var result = _windowManager.ShowDialog(viewModel, null, ShellClientHelper.GetUniversalDialogSettings());
            if (result != true)
            {
                return;
            }
            _statisticsController.ClearStatisticsDataForAllApp();
            RefreshGlobalStatisticsData();
        }

        private void RefreshGlobalStatisticsData()
        {
            PrepareTotalRunTimeStatistics();
            PrepareLongestPlayedGameStatistics();
            PrepareLongestGameSessionStatistics();
            PrepareMostPlayedGameStatistics();
        }

        private void PrepareTotalRunTimeStatistics()
        {
            // TODO [FH] for now the total runtime is calculated by all apps ever installed and played.
            var totalRuntimeTimeSpan = _statisticsController.GetTotalRuntimeForAllApplications();
            TotalRuntime = totalRuntimeTimeSpan == TimeSpan.Zero ? Resources.System_Statistics_NoData : totalRuntimeTimeSpan.Humanize(3, maxUnit: TimeUnit.Hour, minUnit: TimeUnit.Second, countEmptyUnits: true, collectionSeparator: " ");
        }
        private void PrepareLongestPlayedGameStatistics()
        {
            var longestPlayedApp = _statisticsController.GetApplicationWithLongestTotalSession();
            LongestPlayedGame = longestPlayedApp == null
                ? Resources.System_Statistics_NoData
                : $"{longestPlayedApp.TotalSessions.Humanize(3, maxUnit: TimeUnit.Hour, minUnit: TimeUnit.Second, countEmptyUnits: true, collectionSeparator: " ")} - {longestPlayedApp.DisplayInfo.Name}";
        }
        private void PrepareLongestGameSessionStatistics()
        {
            var longestGameSessionApp = _statisticsController.GetApplicationWithLongestSession();
            LongestGameSession = longestGameSessionApp == null ? Resources.System_Statistics_NoData : $"{longestGameSessionApp.LongestSession.Humanize(3, maxUnit: TimeUnit.Hour, minUnit: TimeUnit.Second, countEmptyUnits: true, collectionSeparator: " ")} - {longestGameSessionApp.DisplayInfo.Name}";
        }
        private void PrepareMostPlayedGameStatistics()
        {
            var mostPlayedGame = _statisticsController.GetApplicationWithMostExecutedTimes();
            MostPlayedGame = mostPlayedGame == null ? Resources.System_Statistics_NoData : $"{mostPlayedGame.TimesPlayed} {Resources.System_Statistics_Global_Times} - {mostPlayedGame.DisplayInfo.Name}";
        }

        #endregion

    }
}
