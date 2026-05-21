#region Licence
/****************************************************************
 *  Filename: TabItemStatisticsApplicationsViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Statistics;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Universal.Dialog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels
{
    public sealed class TabItemStatisticsApplicationsViewModel : TabItemStatisticsScreen
    {

        #region Fields & Properties
        public override int DisplayOrder => 9;
        public override string DisplayName
        {
            get { return Resources.System_Statistics_Applications; }
            set { /* ignore */ }
        }

        private int _installedApplicationCount;
        public int InstalledApplicationCount
        {
            get { return _installedApplicationCount; }
            set
            {
                _installedApplicationCount = value;
                NotifyOfPropertyChange();
            }
        }
        private BindableCollection<ApplicationStatisticsViewModel> _items;
        public BindableCollection<ApplicationStatisticsViewModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }
        private ApplicationStatisticsViewModel _selectedItem;
        public ApplicationStatisticsViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanResetGameStatistics));
            }
        }
        public bool CanResetGameStatistics => SelectedItem != null;

        private readonly IWindowManager _windowManager;
        private readonly IStatisticsController _statisticsController;
        private readonly ViewModelFactory _viewModelFactory;
        #endregion

        #region Constructors
        public TabItemStatisticsApplicationsViewModel(
            IUIMessageBroker messageBroker,
            IWindowManager windowManager,
            IStatisticsController statisticsController,
            IPlatformController platformController,
            ViewModelFactory viewModelFactory
            ) : base(messageBroker,"IconStatisticsApplications")
        {
            QuickLeap.AssertNotNull(
                windowManager,
                statisticsController,
                platformController,
                viewModelFactory);

            _windowManager = windowManager;
            _statisticsController = statisticsController;
            _viewModelFactory = viewModelFactory;
            _items = new BindableCollection<ApplicationStatisticsViewModel>();

        }
        
        
        #endregion

        #region Methods
        protected override void HandleLanguageChange(IUILanguageChangedEvent message){}
        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            RefreshApplicationsStatisticsData();
        }

        private void RefreshApplicationsStatisticsData()
        {
            Items.Clear();
            int count = 0;
            // TODO [FH] we display the statistics data only for the installed apps.
            foreach(IAppStatistic appStatistic in _statisticsController.GetAllAppStatistics())
            {
                var appStatisticsViewModel = new ApplicationStatisticsViewModel(appStatistic,_statisticsController)
                                             {
                                                 Number = ++count
                                             };
                Items.Add(appStatisticsViewModel);
            }
            InstalledApplicationCount = count;
        }

        public void ResetGameStatistics()
        {

            var viewModel = _viewModelFactory.Build(DialogType.ResetVrBoxStatistics);
            var result = _windowManager.ShowDialog(viewModel, null, ShellClientHelper.GetUniversalDialogSettings());
            if (result != true)
            {
                return;
            }
            SelectedItem?.ResetStatisticsData();
        }

        #endregion
    }
}
