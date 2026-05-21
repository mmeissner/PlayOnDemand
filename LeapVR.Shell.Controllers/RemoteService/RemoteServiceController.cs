#region Licence
/****************************************************************
 *  Filename: RemoteServiceController.cs
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
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Utilities.Windows;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.Controllers.RemoteService
{
    public class RemoteServiceController : IRemoteServiceController, IRunLevelMsgReceiver
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int MinimumPasswordLength = 5;
        //private string _stationId = "838c3a21-aacb-4012-8011-f20f8767f09f";
        //private string _password = "Password_1234";
        private string _stationId;
        private string _password;
        private readonly IConfigFileRepository<LoginConfig> _configFileRepository;
        private readonly IRemoteServiceSet _remoteServiceSet;
        private readonly ILocalMachine _localMachine;
        private readonly IUIMessageBroker _messageBroker;
        private readonly Subject<IUISession> _whenSessionStarted = new Subject<IUISession>();
        private readonly Subject<IUISession> _whenSessionStopped = new Subject<IUISession>();
        private readonly SemaphoreSlim _connectStateSemaphore = new SemaphoreSlim(1,1);

        private CancellationTokenSource _remoteServicesCancellationTokenSource;
        private readonly object _sessionLock = new object();
        private ConnectState _connectState;
        private NetworkConnectionStatus _networkConnectState = NetworkConnectionStatus.Disconnected;
        private LoginConfig _loginConfig;
        private IUISession _session;
        private IDisposable _sessionStoppedSubscription;
        private IDisposable _sessionUpdatedSubscription;

        #region Internal for other Controllers
        internal IObservable<IUISession> WhenSessionStarted => _whenSessionStarted;
        internal IObservable<IUISession> WhenSessionStopped => _whenSessionStopped;
        internal bool IsSessionRunning()
        {
            lock (_sessionLock)
            {
                return _session != null && _session.StopReason == null;
            }
        }
        #endregion

        public RemoteServiceController(IConfigFileRepository<LoginConfig> configFileRepository,IRemoteServiceSet remoteServiceSet,ILocalMachine localMachine, IUIMessageBroker messageBroker)
        {
            QuickLeap.AssertNotNull(configFileRepository,remoteServiceSet,messageBroker,localMachine);
            _configFileRepository = configFileRepository;
            _localMachine = localMachine;
            _messageBroker = messageBroker;
            _remoteServiceSet = remoteServiceSet;
            _remoteServiceSet.WhenConnectionStateChanged.ObserveOnDispatcher(DispatcherPriority.Normal).Subscribe(OnConnectionStateChanged);
            _remoteServiceSet.WhenServiceErrorOccured.Subscribe(OnServiceError);
            _remoteServiceSet.WhenSessionSettingsChanged.Subscribe(OnSessionSettingsChanged);
            _remoteServiceSet.WhenShellClientDisplayInfoChanged.Subscribe(OnShellClientDisplayInfoUpdated);
            _remoteServiceSet.WhenLoginIntentionExpired.Subscribe(OnLoginIntentionExpired);
            _remoteServiceSet.WhenSessionStarted.Subscribe(OnSessionStarted);
            _remoteServiceSet.WhenLoginDecisionRequired.Subscribe(
                    loginIntention => OnLoginDecisionRequiredAsync(loginIntention).Forget());
            _remoteServiceSet.WhenLoginDecisionResponseArrived.Subscribe(OnSendLoginDecisionResponse);
        }
        
        #region IRemoteService Controller Properties
        public IShellClientInfo LatestShellClientInfo { get; private set; }
        public string StationId => _stationId;
        public bool HasStationIdSet { get; private set; }
        public bool HasPasswordSet { get; private set; }
        public bool AutoLogin { get; private set; }
        public ConnectState ConnectState => _connectState;
        #endregion

        #region IRemoteService Controller Methods
        public async Task<Pod.Data.Infrastructure.IResult<bool>> ConnectAsync()
        {
            _remoteServicesCancellationTokenSource = new CancellationTokenSource();
            return await _remoteServiceSet.ConnectAsync(_stationId,_password,_remoteServicesCancellationTokenSource);
        }
        public async Task<Pod.Data.Infrastructure.IResult<bool>> DisconnectAsync()
        {
            try
            {
                Logger.Info("Trying to Disconnect Async");
                return await _remoteServiceSet.DisconnectAsync();
            }
            finally
            {
                Logger.Info("Disconnect Async Finished");
            }
        }


        /// <summary>
        /// Sends the anonymous session login intention to server, server provides a answear
        /// Server will then Return an Event of LoginIntention (LongCallEvent Loop)
        /// In case the Rate is <see cref="INoBillingSessionRate"/> the Controller will
        /// return an <see cref="LoginDecisionType.Confirm"/> in method <see cref="OnLoginDecisionRequiredAsync"/>,
        /// If this confirmation goes through a new Session Event will be
        /// published
        /// </summary>
        /// <returns></returns>
        public async Task<IntendAnonymousSessionResult> SendAnonymousSessionLoginIntentionAsync()
        {
            //There are cases were we would receive a Success but there will still no Session start
            //This is due to the fact that we only place an Intention but this intention is not yet properly processed
            //This cases might happen when any of this Errors happen in RemoteServiceController
            //LongCallGetLoginIntentionsAsync fails
            //AcknowledgeLoginIntentionAsync call fails
            //SendLoginDecisionAsync call fails 

            try
            {
                Logger.Info("Requesting new LoginIntention");
                var response = await _remoteServiceSet.SendLoginRequestAsync();
                if (response.HasError()) return IntendAnonymousSessionResult.Unknown;
                return response.ReturnValue;
            }
            finally
            {
                Logger.Info("LoginIntention Request Finished");
            }
        }

        /// <summary>
        /// Should be called after a new StationId was entered
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns>true if the station id is valid, false if its not valid</returns>
        public bool SetStationId(string stationId)
        {
            //Must be an valid Guid
            if(String.IsNullOrWhiteSpace(stationId) || !Guid.TryParse(stationId, out _))
            {
                HasStationIdSet = false;
                _stationId = "";
                _loginConfig.StationId = "";
            }
            else
            {
                HasStationIdSet = true;
                _stationId = stationId;
                _loginConfig.StationId = StringCipher.Encrypt(stationId, _localMachine.VBoxFingerprint);
            }
            _configFileRepository.Store(_loginConfig);
            return HasStationIdSet;
        }

        /// <summary>
        /// Should be called every time after a new password was entered
        /// </summary>
        /// <param name="password"></param>
        /// <returns>true if the password is valid, false if not</returns>
        public bool SetPassword(string password)
        {
            //Get the current Config
            bool validPassword;

            //Should meet the criteria to be a valid password
            if (!String.IsNullOrWhiteSpace(password) && password.Length >= MinimumPasswordLength)
            {
                validPassword = true;
                _password = password;
                //Set the Password in the Config if we need to Store it or set as empty if we should not store it
                _loginConfig.Password = _loginConfig.AutoLogin ? StringCipher.Encrypt(password, _localMachine.VBoxFingerprint) : "";
            }
            else
            {
                validPassword = false;
                _password = "";
                _loginConfig.Password = "";
            }
            _configFileRepository.Store(_loginConfig);
            HasPasswordSet = validPassword;
            return validPassword;
        }

        public void SetAutoLogin(bool autoLoginEnabled)
        {
            if (autoLoginEnabled)
            {
                //Check if its a change
                if(!_loginConfig.AutoLogin)
                {
                    //And Set the Password if we currently have a valid one
                    if(HasPasswordSet)
                    {
                        _loginConfig.Password = StringCipher.Encrypt(_password, _localMachine.VBoxFingerprint);
                    }
                }
            }
            else
            {
                //Check if its a change
                if (_loginConfig.AutoLogin)
                {
                    _loginConfig.Password = "";
                }
            }
            //Set the wanted value and persist
            _loginConfig.AutoLogin = autoLoginEnabled;
            _configFileRepository.Store(_loginConfig);
        }

        public void OpenConnectDialog()
        {
            _messageBroker.PublishAsync(new UIConnectDialogEvent(this, false)).Forget();
        }
        #endregion
        
        public void OnStationMessage(StationMessage messages)
        {
            switch(messages)
            {
                case StationMessage.GuiStarted:
                    StartUp();
                    break;
                case StationMessage.InitStop:
                    //Stop the Session if there is any
                    _session?.RequestStopSession(SessionStopReason.StationShutdown);
                    break;
                case StationMessage.Stop:
                    var t = DisconnectAsync().Result;
                    break;
                case StationMessage.InitStart:

                    break;
            }
        }

        private void StartUp()
        {
            try
            {
                Logger.Info("Startup Routine started");
                _loginConfig = _configFileRepository.Get();
                if (String.IsNullOrWhiteSpace(_loginConfig.Password))
                {
                    HasPasswordSet = false;
                }
                else
                {
                    _password = StringCipher.Decrypt(_loginConfig.Password, _localMachine.VBoxFingerprint);
                    HasPasswordSet = !String.IsNullOrWhiteSpace(_password);
                }

                if (String.IsNullOrWhiteSpace(_loginConfig.StationId))
                {
                    HasStationIdSet = false;
                }
                else
                {
                    _stationId = StringCipher.Decrypt(_loginConfig.StationId, _localMachine.VBoxFingerprint);
                    HasStationIdSet = !String.IsNullOrWhiteSpace(_stationId);
                }
                AutoLogin = _loginConfig.AutoLogin;
                //Show Dialog to Login
                _messageBroker.Publish(new UIConnectDialogEvent(this, true));
            }
            finally
            {
                Logger.Info("Startup Routine finished");
            }
        }
       
        #region RemoteServiceMessageHub Callbacks
        private void OnShellClientDisplayInfoUpdated(IShellClientInfo shellClientInfo)
        {
            try
            {
                Logger.Info("OnShellClientDisplayInfo Updated started");
                LatestShellClientInfo = shellClientInfo;
                _messageBroker.Publish(new UIClientInfoChangedEvent(shellClientInfo));
            }
            finally
            {
                Logger.Info("OnShellClientDisplayInfo Updated finished");
            }
        }
        private void OnSessionSettingsChanged(ISessionSettings sessionSettings)
        {
            try
            {
                Logger.Info("OnSession Settings Changed started");
                _messageBroker.Publish(new UISessionSetupChangedEvent(sessionSettings));
            }
            finally
            {
                Logger.Info("OnSession Settings Changed finished");
            }

        }
        private async Task OnLoginDecisionRequiredAsync(ILoginIntention intention)
        {
            try
            {
                Logger.Info("OnLoginDecision Required Async started");
                //We directly Send a confirmation in case of an INoBillingSessionRate 
                if (intention.SessionRate is INoBillingSessionRate)
                {
                    await intention.SendLoginDecisionAsync(LoginDecisionType.Confirm);
                    return;
                }
                //In all other cases we raise a LoginIntendedEvent that can be handled in the GUI
                _messageBroker.Publish(new UILoginIntendedEvent(intention));
            }
            finally
            {
                Logger.Info("OnLoginDecision Required Async finished");
            }
        }
        private void OnLoginIntentionExpired(ILoginIntention loginIntention)
        {
            try
            {
                Logger.Info("OnLoginIntention Expired started");
                _messageBroker.Publish(new UILoginIntentionExpiredEvent(loginIntention));
            }
            finally
            {
                Logger.Info("OnLoginIntention Expired finished");
            }
        }
        private void OnSendLoginDecisionResponse(LoginDecisionResultType result)
        {
            try
            {
                Logger.Info("OnSendLoginDecision Response started");
                _messageBroker.Publish(new UILoginDecisionResultEvent(result));
            }
            finally
            {
                Logger.Info("OnSendLoginDecision Response finished");
            }
        }
        
        private void OnSessionStarted(IUISession session)
        {
            try
            {
                Logger.Info("On OnSessionStarted started");
                lock (_sessionLock)
                {
                    _session = session;
                    _sessionStoppedSubscription = session.WhenSessionUpdated.Where(x => x.StopReason != null).Subscribe(OnSessionStopped);
                    _sessionUpdatedSubscription = session.WhenSessionUpdated.Subscribe(OnSessionUpdated);
                }
                _whenSessionStarted.OnNext(session);
                _messageBroker.Publish(new UISessionStartedEvent(session));
            }
            finally
            {
                Logger.Info("On OnSessionStarted finished");
            }
        }
        private void OnSessionUpdated(IUISession session)
        {
            try
            {
                Logger.Info("On Session Updated started");
                _messageBroker.Publish(new UISessionUpdatedEvent(session));
            }
            finally
            {
                Logger.Info("On Session Updated finished");
            }
        }
        private void OnSessionStopped(IUISession session)
        {
            Logger.Info("On Session stopped started");
            try
            {
                lock (_sessionLock)
                {
                    _sessionStoppedSubscription?.Dispose();
                    _sessionUpdatedSubscription?.Dispose();
                    if (_session != null)
                    {
                        if (session.SessionId != _session.SessionId)
                        {
                            Logger.Error($"Received an Session stop for an Session (Id={session.SessionId}) that is not the current Session (Id={_session.SessionId}");
                        }
                        else _session = null;
                    }
                }
                _whenSessionStopped.OnNext(session);
                _messageBroker.Publish(new UISessionStopedEvent(session));

                //Show the Connect Dialog when the Session stopped and we are not connected
                if (ConnectState != ConnectState.Connected)
                {
                    _messageBroker.Publish(new UIConnectDialogEvent(this, false));
                }
            }
            finally
            {
                Logger.Info("On Session stopped finished");
            }
        }

        private void OnConnectionStateChanged(ConnectionState newState)
        {

                ConnectState connectStateResult;
                switch (newState)
                {
                    case ConnectionState.Disconnected:
                        connectStateResult = ConnectState.Disconnected;
                        break;
                    case ConnectionState.Connecting:
                        connectStateResult = ConnectState.Connecting;
                        break;
                    case ConnectionState.Connected:
                        connectStateResult = ConnectState.Connected;
                        break;
                    case ConnectionState.Disconnecting:
                        connectStateResult = ConnectState.Disconnecting;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
                }
                SetConnectionState(connectStateResult);
        }
        private void OnServiceError(IServiceErrorInfo serviceError){}
        private void SetConnectionState(ConnectState newState)
        {
            Logger.Info("Set Connection State started");
            try
            {
                _connectStateSemaphore.Wait();

                //Old states
                var oldConnectState = _connectState;
                var oldNetworkState = _networkConnectState;
                
                //Connect state is more fine-grade and can be used here to detect changes 
                if (oldConnectState == newState)return;

                //Check if we need notifications for Network State
                var newNetworkState = ConvertConnectState(newState);
                if(oldNetworkState != newNetworkState)
                {
                    _networkConnectState = newNetworkState;
                    _messageBroker.Publish(new UINetworkStateChanged(oldNetworkState,newNetworkState));
                }

                //Process Notifications for ConnectionState
                _connectState = newState;
                _messageBroker.Publish(new UIConnectionStateChangedEvent(oldConnectState,newState));

                //Show Connect Dialog only when Session is not running and we have an Disconnect State
                if(newState == ConnectState.Disconnected && !IsSessionRunning())
                {
                    _messageBroker.PublishAsync(new UIConnectDialogEvent(this, false)).Forget();
                }

            }
            finally
            {
                _connectStateSemaphore.Release();
                Logger.Info("Set Connection State finished");
            }
        }

        private NetworkConnectionStatus ConvertConnectState(ConnectState state)
        {
            switch(state)
            {
                case ConnectState.Disconnected:
                    return NetworkConnectionStatus.Disconnected;
                case ConnectState.Connecting:
                    return NetworkConnectionStatus.Disconnected;
                case ConnectState.Connected:
                    return NetworkConnectionStatus.Connected;
                case ConnectState.Disconnecting:
                    return NetworkConnectionStatus.Connected;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        #endregion

    }
}