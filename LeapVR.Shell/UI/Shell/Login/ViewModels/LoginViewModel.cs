#region Licence
/****************************************************************
 *  Filename: LoginViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-16
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
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Login.Views;
using LeapVR.Shell.UI.Universal.MediaPlayer.ViewModels;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    public class LoginViewModel : Conductor<ILoginPageViewModel>.Collection.OneActive, ILoginViewModel
        , IHandle<IUISessionSetupChangedEvent>
        , IHandle<IUIClientInfoChangedEvent>
        , IHandle<IUISessionStartedEvent>
        , IHandle<IUISessionStopedEvent>
        , IHandle<IUILoginIntendedEvent>
        , IHandleWithTask<IUILoginDecisionResultEvent>
        , IHandle<IUILoginIntentionExpiredEvent>
        , IHandle<IUINetworkStateChanged>
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IUIMessageBroker _messageBroker;
        private readonly IStationController _stationController;
        private readonly UiConfig _uiConfig;
        private readonly ViewModelFactory _viewModelFactory;
        private bool _isPanelBottomVisible;
        private IShellClientInfo _lastShellClientInfo;

        private LoginMessageViewModel _loginMessage;
        private ILoginModeViewModel _loginMode;
        private ILoginIntentionViewModel _loginIntention;
        private StationDetailsViewModel _stationDetails;
        private DisplayState _beforeErrorDisplayState;
        private DisplayState _currentDisplayState;
        private bool _isSettingsButtonVisible = true;
        private bool _mediaPlayerPaused = false;


        private DisplayState CurrentDisplayState
        {
            get => _currentDisplayState;
            set
            {
                if (_currentDisplayState.Equals(value)) return;
                _currentDisplayState = value;
                //Catch any State before we got disconnected so that we can go back to it
                if(_currentDisplayState== DisplayState.ConnectionError)return;
                _beforeErrorDisplayState = _currentDisplayState;
            }
        }
        private DisplayState BeforeErrorDisplayState => _beforeErrorDisplayState;

        #region Fields & Properties
        public UiConfig UiConfig => _uiConfig;
        public bool IsPanelBottomVisible
        {
            get => _isPanelBottomVisible;
            set
            {
                if(_isPanelBottomVisible.Equals(value))return;
                _isPanelBottomVisible = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsSettingsButtonVisible
        {
            get => _isSettingsButtonVisible;
            set
            {
                if(value == _isSettingsButtonVisible) return;
                _isSettingsButtonVisible = value;
                NotifyOfPropertyChange();
            }
        }
        public MediaPlayerViewModel VideoPlayer { get; set; }
        public LoginMessageViewModel LoginMessage
        {
            get => _loginMessage;
            set
            {
                if(_loginMessage.Equals(value))return;
                _loginMessage?.Dispose();
                _loginMessage = value;
                NotifyOfPropertyChange();
            }
        }
        public ILoginModeViewModel LoginMode
        {
            get => _loginMode;
            set
            {
                _loginMode = value;
                NotifyOfPropertyChange();
            }
        }
        public ILoginIntentionViewModel LoginIntention
        {
            get => _loginIntention;
            set
            {
                _loginIntention = value;
                NotifyOfPropertyChange();
            }
        }
        public StationDetailsViewModel StationDetails
        {
            get => _stationDetails;
            set
            {
                _stationDetails = value;
                NotifyOfPropertyChange();
            }
        }
        public Image BackgroundButtomImage { get; private set; }
        public LoginView View { get; private set; }
        #endregion

        #region Constructors
        public LoginViewModel(
            IUIMessageBroker uiMessageBroker,
            UiConfig uiConfig,
            StationDetailsViewModel stationDetailsViewModel,
            IStationController stationController,
            IMultimediaProvider multimediaProvider,
            ViewModelFactory viewModelFactory)
        {
            QuickLeap.AssertNotNull(
                uiMessageBroker,
                uiConfig,
                stationController,
                stationDetailsViewModel,
                viewModelFactory,
                multimediaProvider
                );
            _stationController = stationController;
            _messageBroker = uiMessageBroker;
            _messageBroker.Subscribe(this);
            _viewModelFactory = viewModelFactory;
            _uiConfig = uiConfig;

            _loginMessage = _viewModelFactory.Build(MessageType.Connecting);
            _loginMode = null;
            _loginIntention = null;

            //Setup the Items One Active Collection
            IsPanelBottomVisible = true;
            Items.Add(LoginMessage);
            Items.Add(LoginMode);
            StationDetails = stationDetailsViewModel;
            VideoPlayer = new MediaPlayerViewModel(multimediaProvider.GetMultimediaSet(GlobalConfig.GetGlobalConfiguration().BackgroundPlayerId));
        }
        #endregion

        #region Methods
        public void Settings()
        {
            _stationController.RequestAdminAccess();
        }
        #endregion

        #region Message Handlers
        public async void Handle(IUISessionStartedEvent message)
        {
            IsPanelBottomVisible = false;
            if(VideoPlayer != null && VideoPlayer.MultimediaModule.MultiMediaElement.IsPlaying)
            {
                _mediaPlayerPaused = true;
                await VideoPlayer.MultimediaModule.PauseAsync();
            }
        }

        public async void Handle(IUISessionStopedEvent message)
        {
            IsPanelBottomVisible = true;
            if(VideoPlayer != null && _mediaPlayerPaused)
            {
                await VideoPlayer.MultimediaModule.PlayAsync();
                _mediaPlayerPaused = false;
            }
        }

        public async Task Handle(IUILoginDecisionResultEvent loginDecisionResultEvent)
        {
            //Remove the LoginIntention
            LoginIntention = null;
            //In case of an failure we will Show a message for short with an delay and then go back to DisplayType
            ReplaceLoginMessage(_viewModelFactory.Build(loginDecisionResultEvent.Decision), true);
            if (loginDecisionResultEvent.Decision != LoginDecisionResultType.Canceled)
            {
                await Task.Delay(_uiConfig.MillisecondsToStayBeforeAutoCloseViewsClosing);
            }
            switch(CurrentDisplayState)
            {
                case DisplayState.Initialization:
                    ReplaceLoginMessage(_viewModelFactory.Build(MessageType.Connecting), true);
                    IsPanelBottomVisible = true;
                    break;
                case DisplayState.LoginView:
                    ActivateItem(LoginMode);
                    IsPanelBottomVisible = true;
                    break;
                case DisplayState.LoginIntention:
                    CurrentDisplayState = DisplayState.LoginView;
                    if(loginDecisionResultEvent.Decision != LoginDecisionResultType.SessionStarted)
                    {
                        IsPanelBottomVisible = true;
                    }
                    ActivateItem(LoginMode);
                    break;
                //Can happen that a License Error occures during runtime
                case DisplayState.LicenseError:
                case DisplayState.VersionError:
                    ResolveClientState(_lastShellClientInfo);
                    break;
                case DisplayState.ConnectionError:
                    break;
            }
        }
        public void Handle(IUILoginIntentionExpiredEvent message)
        {
            if(CurrentDisplayState == DisplayState.LoginIntention)
            {
                LoginIntention = null;
                CurrentDisplayState = DisplayState.LoginView;
                IsPanelBottomVisible = true;
                ActivateItem(LoginMode);
            }
        }
        public void Handle(IUISessionSetupChangedEvent message)
        {
            ReplaceLoginMode(message.Settings);
        }
        public void Handle(IUIClientInfoChangedEvent message)
        {
            _lastShellClientInfo = message.ClientInfo;
            ResolveClientState(_lastShellClientInfo);
        }
        public void Handle(IUILoginIntendedEvent loginIntendedEvent)
        {
            Logger.Info($"LoginIntention Event arrived with Intention ID: {loginIntendedEvent.Intention.IntentionId}");
            CurrentDisplayState = DisplayState.LoginIntention;
            IsPanelBottomVisible = false;
            ReplaceLoginIntention(loginIntendedEvent.Intention,true);
        }
        public void Handle(IUINetworkStateChanged message)
        {
            if(message.NewStatus == NetworkConnectionStatus.Disconnected)
            {
                CurrentDisplayState = DisplayState.ConnectionError;
                ReplaceLoginMessage(_viewModelFactory.Build(MessageType.LocalConnectionProblem), true);
            }
            else
            {
                //We are connected again
                CurrentDisplayState = BeforeErrorDisplayState;
                switch(BeforeErrorDisplayState)
                {
                    case DisplayState.Initialization:
                        ReplaceLoginMessage(_viewModelFactory.Build(MessageType.Connecting), true);
                        break;
                    case DisplayState.LoginView:
                        ActivateItem(LoginMode);
                        break;
                    //If we had an LoginIntention we must have had an LoginView we can go back to
                    case DisplayState.LoginIntention:
                        CurrentDisplayState = DisplayState.LoginView;
                        ActivateItem(LoginMode);
                        break;
                    case DisplayState.LicenseError:
                    case DisplayState.VersionError:
                        ResolveClientState(_lastShellClientInfo);
                        break;
                }
            }
        }
        #endregion

        #region Private Methods
        private void ResolveClientState(IShellClientInfo clientInfo)
        {
            //If version is out of date 
            if (clientInfo.VersionStatus == ShellVersionStatus.UpdateRequired)
            {
                ReplaceLoginMessage(_viewModelFactory.Build(MessageType.ShellVersionOutOfDate), true);
                CurrentDisplayState = DisplayState.VersionError;
                return;
            }
            //If License is not valid we will have a connection but no communication with server
           if (clientInfo.LicenseStatus != LicenseStatus.LicenseValid)
            {
                MessageType licenseError = MessageType.Unknown;
                switch (clientInfo.LicenseStatus)
                {
                    case LicenseStatus.InvalidUsernamePassword:
                        licenseError = MessageType.InvalidUsernameOrPassword;
                        break;
                    case LicenseStatus.LicenseRevoked:
                        licenseError = MessageType.LicenseRevoked;
                        break;
                    case LicenseStatus.LicenseSuspended:
                        licenseError = MessageType.LicenseSuspended;
                        break;
                    case LicenseStatus.LicenseNotDeployed:
                        licenseError = MessageType.LicenseNotDeployed;
                        break;
                    case LicenseStatus.LicenseNotLinked:
                        licenseError = MessageType.LicenseNotLinked;
                        break;
                    case LicenseStatus.LicenseNotFound:
                        licenseError = MessageType.LicenseNotFound;
                        break;
                    case LicenseStatus.StationRevoked:
                        licenseError = MessageType.StationRevoked;
                        break;
                    case LicenseStatus.StationSuspended:
                        licenseError = MessageType.StationSuspended;
                        break;
                    default:
                        Logger.Error($"Attempted to map an LicenseState to an Error but did not found mapping, LicenseState={clientInfo.LicenseStatus}");
                        break;
                }
                ReplaceLoginMessage(_viewModelFactory.Build(licenseError), true);
                CurrentDisplayState = DisplayState.LicenseError;
                IsPanelBottomVisible = true;
                return;
            }
            //Seems all is good and we received an Valid ClientUpdate
            if (CurrentDisplayState == DisplayState.LicenseError || CurrentDisplayState == DisplayState.VersionError)
            {
                if (LoginMode != null)
                {
                    ActivateItem(LoginMode);
                    CurrentDisplayState = DisplayState.LoginView;
                }
                //We need to wait until we can build one, back to connecting
                else
                {
                    CurrentDisplayState = DisplayState.Initialization;
                    ReplaceLoginMessage(_viewModelFactory.Build(MessageType.Connecting), true);
                }
                IsPanelBottomVisible = true;
            }
        }
        private void ReplaceLoginMode(ISessionSettings sessionSettings)
        {
            //We activate if we are in initialization state or its already active
            bool activate = ActiveItem.Equals(LoginMode) || CurrentDisplayState == DisplayState.Initialization;
            Logger.Debug($"Replacing LoginMode with activate={activate}, DisplayState={CurrentDisplayState}");
            RemoveLoginMode();
            LoginMode = _viewModelFactory.Build(sessionSettings);
            Logger.Debug($"New Login Mode={LoginMode.Mode}");
            Items.Add(LoginMode);
            if(activate)
            {
                Logger.Info($"Setting DisplayState={DisplayState.LoginView}, and activating LoginMode");
                CurrentDisplayState = DisplayState.LoginView;
                ActivateItem(LoginMode);
            }
        }
        private void ReplaceLoginMessage(LoginMessageViewModel newLoginMessage, bool activate)
        {
            Logger.Info($"Replacing Login Message with activate={activate}");
            Items.Remove(LoginMessage);
            LoginMessage.Dispose();
            LoginMessage = newLoginMessage;
            Items.Add(LoginMessage);
            if(activate)ActivateItem(LoginMessage);
        }
        private void ReplaceLoginIntention(ILoginIntention intention, bool activate)
        {
            Logger.Info($"Replacing LoginIntention with activate={activate}");
            RemoveLoginIntention();
            LoginIntention = _viewModelFactory.Build(intention);
            Items.Add(LoginIntention);
            if(activate)ActivateItem(LoginIntention);
        }
        private void RemoveLoginIntention()
        {
            if (LoginIntention != null)
            {
                Logger.Info($"Removing LoginIntention with Id={LoginIntention.LoginIntentionId}");
                if(LoginIntention is IDisposable disposable)disposable.Dispose();
                Items.Remove(LoginIntention);
            }
        }
        private void RemoveLoginMode()
        {
            if(LoginMode != null)
            {
                Logger.Info($"Removing LoginMode={LoginMode.Mode}");
                if(LoginMode is IDisposable disposable)disposable.Dispose();
                Items.Remove(LoginMode);
            }
        }
        #endregion

        #region Overrides
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(view is LoginView loginView)
            {
                View = loginView;
                BackgroundButtomImage = loginView.ButtomImage;
            }
            ActivateItem(LoginMessage);
        }

        public override void ActivateItem(ILoginPageViewModel item)
        {
            switch(item)
            {
                case LoginOpenModeViewModel _:
                    IsSettingsButtonVisible = false;
                    break;
                default:
                    IsSettingsButtonVisible = true;
                    break;
            }
            base.ActivateItem(item);
        }

        protected override void OnActivate()
        {
            ActivateItem(ActiveItem);
            base.OnActivate();
        }
        #endregion
        enum DisplayState
        {
            Initialization,
            LoginView,
            LoginIntention,
            LicenseError,
            VersionError,
            ConnectionError,
        }
    }
}
