#region Licence
/****************************************************************
 *  Filename: ApplicationExecutableViewModel.cs
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
using System.Linq;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Universal.Dialog;
using LeapVR.Shell.UI.Universal.Dialog.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public class ApplicationExecutableViewModel : ApplicationBaseViewModel
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IWindowManager _windowManager;
        private readonly IStationController _stationController;
        private readonly IPlatformController _platformController;
        private readonly ViewModelFactory _viewModelFactory;
        #endregion

        #region Constructors

        public ApplicationExecutableViewModel(
            IAppPlatformInfo appPlatformInfo,
            IPlatformController platformController,
            IStationController stationController,
            IWindowManager windowManager,
            ViewModelFactory viewModelFactory) : base(appPlatformInfo)
        {
            _platformController = platformController;
            _stationController = stationController;
            _windowManager = windowManager;
            _viewModelFactory = viewModelFactory;
        }

        #endregion

        #region Methods
        protected override void OnActivate()
        {
            base.OnActivate();
            NotifyOfPropertyChange(() => IsActive);
            Logger.Debug($"Application '{Name}' got activated.");
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            NotifyOfPropertyChange(() => IsActive);
            Logger.Debug($"Application '{Name}' got deactivated.");
        }

        public void Execute()
        {

            //TODO This Code needs to be rewritten: Build appExecutionSelectViewModel with Execution Options as Constructor parameter
            Logger.Info($"Try to perform start application '{Name}'.");

            //ConfirmationDialogViewModel viewModel = null;

            bool requireConfirmation = false;
            var appExecutionInfo = _platformController.GetAppExecutionInfo(ApplicationGuid, true);

            var appExecutionSelectViewModel = _viewModelFactory.Build();
            try
            {
                //There is no execution available
                if (appExecutionInfo.ExecutionCandidates.Length <= 0)
                {
                    _windowManager.ShowDialog(_viewModelFactory.Build(DialogType.NoSuitableExecution), null, ShellClientHelper.GetUniversalDialogSettings());
                    return;
                }

                IExecuteable executionDisplayToRun = null;
                if (appExecutionInfo.ExecutionCandidates.Length == 1)
                {
                    executionDisplayToRun = appExecutionInfo.ExecutionCandidates.First();
                }
                else
                {

                    appExecutionSelectViewModel.ExecutionCandidates.AddRange(from raw in appExecutionInfo.ExecutionCandidates select new AppExecutionInfoResultViewModel(raw));
                    //ScreenExtensions.TryDeactivate(this, false);
                    try
                    {
                        if (_windowManager.ShowDialog(appExecutionSelectViewModel, null, ShellClientHelper.GetUniversalDialogSettings()) != true)
                        {
                            return;
                        }
                    }
                    finally
                    {
                        //ScreenExtensions.TryActivate(this);
                        Logger.Debug($"Re-activate {this} after the popup window.");
                    }
                    executionDisplayToRun = appExecutionSelectViewModel.SelectedItem.Executeable;
                }

                var requireVr = executionDisplayToRun.IsVirtualRealityRequired;

                ConfirmationDialogViewModel viewModel = null;
                switch (_stationController.Mode)
                {
                    case StationMode.Screen when requireVr:
                        viewModel = _viewModelFactory.Build(DialogType.StartVrGameInScreenMode);
                        Logger.Info( $"Confirmation dialog pops up before {Name} starts in {_stationController.Mode} mode.");
                        requireConfirmation = true;
                        break;
                    case StationMode.VirtualReality when !requireVr:
                        viewModel = _viewModelFactory.Build(DialogType.StartScreenGameInVrMode);
                        Logger.Info( $"Confirmation dialog pops up before {Name} starts in {_stationController.Mode} mode.");
                        requireConfirmation = true;
                        break;
                }

                if (requireConfirmation)
                {
                    //ScreenExtensions.TryDeactivate(this, false);
                    Logger.Debug($"Deactivate {this} before the popup window.");
                    bool? proceedToContinue;
                    try
                    {
                        proceedToContinue = _windowManager.ShowDialog(viewModel, null, ShellClientHelper.GetUniversalDialogSettings());
                    }
                    finally
                    {
                        //ScreenExtensions.TryActivate(this);
                        Logger.Debug($"Re-activate {this} after the popup window.");
                    }
                    if (proceedToContinue == false)
                    {
                        Logger.Info( $"User actively cancel to start {Name} in {_stationController.Mode} mode.");
                        return;
                    }
                }

                if(!_stationController.RequestExecution(executionDisplayToRun))
                {
                    Logger.Warn($"Failed to start {Name}");
                }
            }
            finally
            {
                appExecutionSelectViewModel.Dispose();
            }
           
        }

        #endregion

    }
}
