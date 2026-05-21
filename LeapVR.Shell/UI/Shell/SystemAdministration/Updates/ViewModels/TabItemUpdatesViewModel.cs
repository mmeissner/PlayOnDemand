#region Licence
/****************************************************************
 *  Filename: TabItemUpdatesViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-8-14
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

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    /// <summary>
    /// Root view model class for auto-update functionality.
    /// </summary>
    [Obsolete("Is outdated as updates have to be downloaded and installed by installer")]
    public class TabItemUpdatesViewModel 
    {

        //#region Fields & Properties

        //private readonly IUpdateController _updateController;
        //private readonly IUpdateProcess _updateProcess;
        //private readonly IDisposable _whenUpdateProcessStateChangedSubscription;
        //private readonly IStationController _stationController;

        //public override int DisplayOrder => 2;
        //public override string DisplayName
        //{
        //    get { return Resources.System_Updates; }
        //    set { /* ignore */ }
        //}

        //private string _currentVersion;
        ///// <summary>
        ///// Get or set the current version of shell
        ///// </summary>
        //public string CurrentVersion
        //{
        //    get { return _currentVersion; }
        //    set
        //    {
        //        _currentVersion = value;
        //        NotifyOfPropertyChange();
        //    }
        //}
        //private string _newestVersion;
        ///// <summary>
        ///// Get or set the newest version of shell from remote
        ///// </summary>
        //public string NewestVersion
        //{
        //    get { return _newestVersion; }
        //    set
        //    {
        //        _newestVersion = value;
        //        NotifyOfPropertyChange();
        //    }
        //}

        //public bool IsNewVersionAvailable => _updateProcess?.NewestVersion != null;

        //private UIShellClientUpdateStates _shellClientUpdateState;
        ///// <summary>
        ///// Get or set update state.
        ///// </summary>
        //public UIShellClientUpdateStates ShellClientUpdateState
        //{
        //    get { return _shellClientUpdateState; }
        //    set
        //    {
        //        _shellClientUpdateState = value;
        //        ApplyCorrespondingScreen(_shellClientUpdateState);
        //        NotifyOfPropertyChange();
        //    }
        //}

        //#endregion

        //#region Constructors
        //public TabItemUpdatesViewModel(IUIMessageBroker messageBroker,
        //    IUpdateController updateController, IStationController stationController) : base(messageBroker,"IconUpdates")
        //{
        //    _stationController = stationController;
        //    _updateController = updateController;
        //    _updateProcess = updateController.UpdateProcess;
        //    _whenUpdateProcessStateChangedSubscription = _updateProcess.WhenStateChanged.ObserveOnDispatcher().Subscribe(OnUpdateProcessStateChanged);

        //    _currentVersion = _updateController.CurrentVersion.ToString();
        //}

        //#endregion

        //#region Methods
        //protected override void OnActivate()
        //{
        //    base.OnActivate();
        //    ShellClientUpdateState = InterpreteUpdatesFrom(_updateController.UpdateProcess.State);
        //}

        //private void OnUpdateProcessStateChanged(UpdateState state)
        //{
        //    ShellClientUpdateState = InterpreteUpdatesFrom(state);
        //    NewestVersion = _updateProcess?.NewestVersion?.ToString();
        //    NotifyOfPropertyChange(() => IsNewVersionAvailable);
        //}

        //private void ApplyCorrespondingScreen(UIShellClientUpdateStates state)
        //{
        //    switch (state)
        //    {
        //        case UIShellClientUpdateStates.NotStarted:
        //            var notStartedViewModel = new CheckUpdatesViewModel(_updateController);
        //            notStartedViewModel.Information = string.Empty;
        //            notStartedViewModel.CheckUpdateStr = Resources.System_Updates_CheckUpdates;
        //            ActivateItem(notStartedViewModel);
        //            break;
        //        case UIShellClientUpdateStates.Unknown:
        //            break;
        //        case UIShellClientUpdateStates.Cancelling:
        //            var cancellingViewModel = new MessageDisplayViewModel();
        //            cancellingViewModel.Title = Resources.System_Updates_Announcement_Cancelling;
        //            ActivateItem(cancellingViewModel);
        //            break;

        //        case UIShellClientUpdateStates.CheckingNewestVersion:
        //            var checkingUpdatesViewModel = new ProcessLoadingViewModel();
        //            checkingUpdatesViewModel.Title = Resources.Global_Loading;
        //            checkingUpdatesViewModel.Description = Resources.System_Updates_Announcement_CheckingNewestVersion;
        //            ActivateItem(checkingUpdatesViewModel);
        //            break;
        //        case UIShellClientUpdateStates.UpToDate:
        //            var upToDateViewModel = new MessageDisplayViewModel();
        //            upToDateViewModel.Title = Resources.System_Updates_Announcement_UpToDate;
        //            ActivateItem(upToDateViewModel);
        //            break;
        //        case UIShellClientUpdateStates.UpdateAvailable:
        //            var updateAvailableViewModel = new UpdateAvailableViewModel(_updateController);
        //            ActivateItem(updateAvailableViewModel);
        //            break;
        //        case UIShellClientUpdateStates.Downloading:
        //            var updateDownloadingViewModel = new UpdateDownloadingViewModel(_updateController);
        //            ActivateItem(updateDownloadingViewModel);
        //            break;
        //        case UIShellClientUpdateStates.ReadyToUpdate:
        //            var readyToUpdateViewModel = new ReadyToUpdateViewModel(_updateController);
        //            ActivateItem(readyToUpdateViewModel);
        //            break;
        //        case UIShellClientUpdateStates.Upgrading:
        //            var upgradingViewModel = new ProcessLoadingViewModel();
        //            upgradingViewModel.Title = Resources.Global_Loading;
        //            upgradingViewModel.Description = Resources.System_Updates_Announcement_Updating;
        //            ActivateItem(upgradingViewModel);
        //            break;
        //        case UIShellClientUpdateStates.AwaitToRestart:
        //            var awaitToRestartViewModel = new AwaitToRestartViewModel(_stationController,_updateController);
        //            ActivateItem(awaitToRestartViewModel);
        //            break;
        //        case UIShellClientUpdateStates.Errored:
        //            var erroredViewModel = new CheckUpdatesViewModel(_updateController);
        //            erroredViewModel.Information = Resources.System_Updates_Announcement_Errored;
        //            erroredViewModel.CheckUpdateStr = Resources.System_Updates_TryAgain;
        //            ActivateItem(erroredViewModel);
        //            break;
        //    }
        //}

        ///// <summary>
        ///// Interprete from <see cref="UpdatesStates"/> to corresponding <see cref="UIShellClientUpdateStates"/>.
        ///// </summary>
        ///// <param name="originalState">incoming <see cref="UpdatesStates"/></param>
        ///// <returns></returns>
        //private static UIShellClientUpdateStates InterpreteUpdatesFrom(UpdateState originalState)
        //{
        //    UIShellClientUpdateStates returnedState;

        //    switch (originalState)
        //    {
        //        case UpdateState.Unknown:
        //            returnedState = UIShellClientUpdateStates.Unknown;
        //            break;
        //        case UpdateState.NotStarted:
        //            returnedState = UIShellClientUpdateStates.NotStarted;
        //            break;
        //        case UpdateState.CheckingNewestVersion:
        //            returnedState = UIShellClientUpdateStates.CheckingNewestVersion;
        //            break;
        //        case UpdateState.NoUpdatesAvailable:
        //            returnedState = UIShellClientUpdateStates.UpToDate;
        //            break;
        //        case UpdateState.UpdateAvailable:
        //            returnedState = UIShellClientUpdateStates.UpdateAvailable;
        //            break;
        //        case UpdateState.Downloading:
        //            returnedState = UIShellClientUpdateStates.Downloading;
        //            break;
        //        case UpdateState.ReadyToUpdate:
        //            returnedState = UIShellClientUpdateStates.ReadyToUpdate;
        //            break;
        //        case UpdateState.Updating:
        //            returnedState = UIShellClientUpdateStates.Upgrading;
        //            break;
        //        case UpdateState.AwaitingRestart:
        //            returnedState = UIShellClientUpdateStates.AwaitToRestart;
        //            break;
        //        case UpdateState.Errored:
        //            returnedState = UIShellClientUpdateStates.Errored;
        //            break;
        //        case UpdateState.Canceling:
        //            returnedState = UIShellClientUpdateStates.Cancelling;
        //            break;
        //        default:
        //            returnedState = UIShellClientUpdateStates.Unknown;
        //            break;
        //    }

        //    return returnedState;
        //}
        //#endregion

        //public new void Dispose()
        //{
        //    _whenUpdateProcessStateChangedSubscription?.Dispose();
        //    base.Dispose();
        //}
    }
}
