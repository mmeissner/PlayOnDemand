#region Licence
/****************************************************************
 *  Filename: RemoteServicesSet.cs
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
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Controllers.Station;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Services.Data;
using LeapVR.Shell.Services.Factory;
using LeapVR.Shell.Services.RpcServices;
using LeapVR.Shell.Services.Session;
using NLog;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.Grpc.Messages.Shared;
using SessionState = Pod.Grpc.Messages.Shared.SessionState;

namespace LeapVR.Shell.Services
{
    partial class RemoteServicesSet : IRemoteServiceSet, ISessionService, IDisposable
    {
        public const uint InterfaceVersion = 0;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly RemoteServiceFactory _serviceFactory;
        private readonly HashSet<IDisposable> _disposables;
        private readonly  SemaphoreSlim _connectionStateSemaphore = new SemaphoreSlim(1,1);
        private readonly object _sessionLock = new object();
        private readonly SemaphoreSlim _disposeServicesProtectionSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _notificationCts;
        private CancellationTokenSource _rpcCallCts;
        
        private readonly Subject<IShellClientInfo> _whenShellClientDisplayInfoChanged = new Subject<IShellClientInfo>();
        private readonly Subject<ISessionSettings> _whenSessionSettingsChanged = new Subject<ISessionSettings>();
        private readonly Subject<ConnectionState> _whenConnectionStateChanged = new Subject<ConnectionState>();
        private readonly Subject<IServiceErrorInfo> _whenServiceErrorOccured = new Subject<IServiceErrorInfo>();

        private readonly Subject<ILoginIntention> _whenLoginDecisionRequired = new Subject<ILoginIntention>();
        private readonly Subject<LoginDecisionResultType> _whenLoginDecisionResponseArrived = new Subject<LoginDecisionResultType>();
        private readonly Subject<ILoginIntention> _whenLoginIntentionExpired = new Subject<ILoginIntention>();
        private readonly Subject<IUISession> _whenSessionStarted = new Subject<IUISession>();


        private ShellServer _shellServer;
        private ShellService _shellService;
        private ApplicationService _applicationService;
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        private bool _isFirstSessionUpdateOnConnect = false;
        private bool _isDisconnectOperationInProgress = false;
        private Task<IResult> _notificationTask;
        private Timer _heartBeatTimer;

        private IShellClientInfo _clientInfo;
        private SubscribedSession _serviceRemoteSession;

        private bool _isDisposed = false;

        private ConnectionState State => _connectionState;

        public RemoteServicesSet(RemoteServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _disposables = new HashSet<IDisposable> {_notificationCts, _rpcCallCts};
            _clientInfo = new ShellClientInfo
                          {
                                  LicenseStatus = LicenseStatus.Unknown,
                                  VersionStatus = ShellVersionStatus.Unknown,
                                  SerialNumber = "Unknown",
                                  StationDisplayName = "Unknown"
                          };
        }

        #region IRemoteServiceSet Interface
        public IObservable<IShellClientInfo> WhenShellClientDisplayInfoChanged => _whenShellClientDisplayInfoChanged;
        public IObservable<ISessionSettings> WhenSessionSettingsChanged => _whenSessionSettingsChanged;
        public IObservable<ConnectionState> WhenConnectionStateChanged => _whenConnectionStateChanged;
        public IObservable<IServiceErrorInfo> WhenServiceErrorOccured => _whenServiceErrorOccured;

        public IObservable<ILoginIntention> WhenLoginDecisionRequired => _whenLoginDecisionRequired;
        public IObservable<LoginDecisionResultType> WhenLoginDecisionResponseArrived => _whenLoginDecisionResponseArrived;
        public IObservable<ILoginIntention> WhenLoginIntentionExpired => _whenLoginIntentionExpired;
        public IObservable<IUISession> WhenSessionStarted => _whenSessionStarted;

        private SubscribedSession ServiceRemoteSession
        {
            get => _serviceRemoteSession;
            set
            {
                if(_serviceRemoteSession != value)
                {
                    _serviceRemoteSession?.Dispose();
                } 
                _serviceRemoteSession = value;
            }
        }

        /// <summary>
        /// Connects to this Service to make it useable
        /// </summary>
        /// <param name="stationId">The Station Id to connect with</param>
        /// <param name="password">The Password for the Station</param>
        /// <param name="serviceSetMainCts">Allows cancellation of this call</param>
        /// <returns></returns>
        public async Task<IResult<bool>> ConnectAsync(string stationId, string password,CancellationTokenSource serviceSetMainCts)
        {
            //Return Value, only true if Connected
            bool isSuccess = false;
            var result = new Result<bool>();
            Logger.Info("Trying to ConnectAsync");
            try
            {

                //Set the State to Connecting if we are Disconnected

                if (!await SetConnectionStateAsync(ConnectionState.Connecting, (x) =>
                                                                   {
                                                                       if(x != ConnectionState.Disconnected)return false;
                                                                       return true;
                                                                   }))
                {
                    return result.Add("Services are not Connected", UserError.ShellClient_NotConnected);
                }
                
                //Required Variables for outside use 
                var connectServiceDisposables = new HashSet<IDisposable>();
                var shellServiceDisposables = new HashSet<IDisposable>();
                _notificationCts = CreateCtsLinkedToMain(serviceSetMainCts, out _);
                _rpcCallCts = CreateCtsLinkedToMain(serviceSetMainCts, out _);
                ShellServer shellServer;

                //Try Connect Server
                try
                {
                    //Get Connect Service
                    var connectService = _serviceFactory.GetConnectService(stationId, password);

                    //Collect Temporary Disposables
                    connectServiceDisposables.Add(connectService);

                    connectServiceDisposables.Add(
                            connectService.WhenErrorOccures.Subscribe(x => OnServiceError(ServiceType.Connect, x)));
                    connectServiceDisposables.Add(
                            connectService.WhenConnectionChanges.Subscribe(WhenConnectionChanges));

                    //Connect Grpc Client to Connect Service
                    var connectedToConnectServer = await connectService.ConnectServiceAsync();
                    if(connectedToConnectServer.HasError())
                    {
                        return result.Add(connectedToConnectServer);
                    }

                    //Get ShellHost from Service
                    var shellHostResult = await connectService.GetShellHostAsync(_rpcCallCts.Token,_shellServer?.ConnectionId);
                    if(shellHostResult.HasError())
                    {
                        return result.Add(shellHostResult);
                    }

                    shellServer = shellHostResult.ReturnValue;
                }
                //Dispose all Connect Server related disposables
                finally
                {
                    foreach(IDisposable disposable in connectServiceDisposables)
                    {
                        disposable.Dispose();
                    }
                }

                //Try ShellServer 
                try
                {
                    //Connect to ShellHost Service
                    _serviceFactory.GetStationService(
                            shellServer,
                            stationId,
                            password,
                            out _shellService,
                            out _applicationService);

                    //Subscribe to Notifications from our Services
                    shellServiceDisposables.Add(_shellService.WhenErrorOccures.Subscribe(x => OnServiceError(ServiceType.ShellHost, x)));
                    shellServiceDisposables.Add(_applicationService.WhenErrorOccures.Subscribe(x => OnServiceError(ServiceType.ShellHost, x)));
                    shellServiceDisposables.Add(_shellService.WhenConnectionChanges.Subscribe(WhenConnectionChanges));
                    shellServiceDisposables.Add(_applicationService.WhenConnectionChanges.Subscribe(WhenConnectionChanges));

                    //Subscribe to Server Messages from our Services
                    shellServiceDisposables.Add(
                            _shellService.WhenServerMessage.Subscribe(
                                    OnServerMessage,
                                    OnServerMessageErrorIntercept,
                                    OnServerMessagesCompleted));



                    //Connect Grpc Client to ShellService
                    var connectedToShellServer = await _shellService.ConnectServiceAsync();
                    if(connectedToShellServer.HasError())
                    {
                        return result.Add(connectedToShellServer);
                    }

                    //Send Connect RPC to ShellService
                    var connectShellHostResult = await _shellService.ConnectToShellServerAsync(_rpcCallCts.Token);
                    if(connectShellHostResult.HasError())
                    {
                        return result.Add(connectShellHostResult);
                    }

                    //Create HeartbeatTimer and start off The Notifications Task
                    _heartBeatTimer = new Timer(SendHeartbeat, _heartBeatTimer, Timeout.Infinite, Timeout.Infinite);
                    shellServiceDisposables.Add(_heartBeatTimer);
                    _shellServer = shellServer;

                    //Set Flag to Handle first Session Update on Connect
                    _isFirstSessionUpdateOnConnect = true;
                    _notificationTask = Task.Run(()=>_shellService.GetNotificationsAsync(_notificationCts.Token));
                    isSuccess = true;
                    return result.Add(true);
                }
                catch(Exception e)
                {
                    if(e is TaskCanceledException)
                    {
                        Logger.Info("Connect Task was canceled");
                        isSuccess = false;
                        return result.Add("Cancellation occured", UserError.ShellClient_WasCanceled);
                    }
                    else return result.Add(e.Message, UserError.InternalError);
                }
                finally
                {
                    if(!isSuccess)
                    {
                        //Reset all References
                        ResetAllReferences();
                        //Dispose all if we did not succeed
                        foreach(IDisposable disposable in shellServiceDisposables)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
            finally
            {
                //Set the State to Connected or Disconnected
                await SetConnectionStateAsync(isSuccess ? ConnectionState.Connected : ConnectionState.Disconnected);
                Logger.Info("ConnectAsync finished");
            }
        }
        
        /// <summary>
        /// Disconnects from the Service
        /// Needs to be called if Service was previously Connected
        /// </summary>
        /// <returns>True if the Disconnect was Graceful otherwise false</returns>
        public async Task<IResult<bool>> DisconnectAsync()
        {
            try
            {
                Logger.Info("DisconnectAsync started");
                var retval = new Result<bool>();
                //Set the State to Disconnecting
                if(!await SetConnectionStateAsync(
                        ConnectionState.Disconnecting,
                        (x) =>
                        {
                            if(x != ConnectionState.Connected) return false;
                            _isDisconnectOperationInProgress = true;
                            return true;
                        }))
                {
                    return retval.Add("Services are not Connected", UserError.ShellClient_NotConnected);
                }

                try
                {
                    //Get the reference as it get removed if the task ends 
                    var notificationTask = _notificationTask;
                    _shellService.DisconnectFromShellHost(_rpcCallCts.Token);
                    notificationTask?.Wait(TimeSpan.FromSeconds(5));
                }
                catch(Exception e)
                {
                    if(e is TaskCanceledException)
                    {
                        Logger.Warn("Disconnect Task was canceled");
                    }
                    else
                    {
                        Logger.Error(e, "Disconnect had exception");
                    }
                }

                //Just Cancel them in Case of timeout
                _notificationCts?.Cancel();
                _rpcCallCts?.Cancel();

                //All CleanUp Operations and setting of the Connection State happens in OnServerMessagesCompletedIntercept
                return retval.Add(true);
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception on Disconnect");
                throw;
            }
            finally
            {
                Logger.Info("DisconnectAsync finished");
            }
        }
        
        /// <summary>
        /// Requests the Server to Start a Session
        /// </summary>
        /// <returns>The result</returns>
        public async Task<IResult<IntendAnonymousSessionResult>> SendLoginRequestAsync()
        {
            var retval = new Result<IntendAnonymousSessionResult>();
            try
            {
                if(State != ConnectionState.Connected)
                {
                    return retval.Add("Cant send, Service is not Connected ", UserError.ShellClient_NotConnected);
                }

                //If this request gets accepted, the server should give us an notification message to pickup this new intention
                //therefor we do not do here anything more then sending the request and returning the response
                var response = await _shellService.SendLoginRequestAsync(_rpcCallCts.Token);
                if(response.IsSuccess())
                {
                    return new Result<IntendAnonymousSessionResult>().Add(IntendAnonymousSessionResult.Success);
                }
                //Something went wrong or we were not allowed to create an session
                retval.Add(response);
                return retval;
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("SendLoginRequestAsync was canceled");
                    return retval.Add("Cancellation occured", UserError.ShellClient_WasCanceled);
                }

                return retval.Add(e.Message, UserError.InternalError);
            }
        }
        #endregion

        #region Methods used by RemoteSession
        public async Task<IResult<LoginDecisionResponse>> SendLoginIntentionResponseAsync(bool isAccepted)
        {
            try
            {
                await _disposeServicesProtectionSemaphore.WaitAsync();
                if (_shellService != null)
                {
                    return await _shellService.SendLoginIntentionResponseAsync(isAccepted, _rpcCallCts.Token);
                }
                return new Result<LoginDecisionResponse>().Add("Shell Service Reference is not set, Client is probably not connected", UserError.ShellClient_NotConnected);
            }
            finally
            {
                _disposeServicesProtectionSemaphore.Release();
            }
        }
        public IResult<LogoutResponse> SendLogoutRequest(SessionStopReason reason)
        {
            try
            {
                _disposeServicesProtectionSemaphore.Wait();
                if (_shellService != null)
                {
                    return _shellService.SendLogoutRequest(reason, _rpcCallCts.Token);
                }
                return new Result<LogoutResponse>().Add("Shell Service Reference is not set, Client is probably not connected", UserError.ShellClient_NotConnected);
            }
            finally
            {
                _disposeServicesProtectionSemaphore.Release();
            }
        }
        public void RemoveSession(RemoteSession session)
        {
            Logger.Debug("Trying to remove Session");
            lock (_sessionLock)
            {
                if(ServiceRemoteSession.Session == session)
                {
                    Logger.Info("Setting Session to null");
                    ServiceRemoteSession = null;
                }
            }
            Logger.Debug("Session removed");
        }
        #endregion

        #region Server Message Handlers
        private void OnServerMessage(ServerMessage message)
        {
            Logger.Info($"Intercepted Message from Server = {message}!");
            switch(message)
            {
                //Handle Server Settings internally only
                case ServerMessage.UpdateServerSettings:
                    UpdateServerSettings();
                    return;
                case ServerMessage.UpdateClientSettings:
                    UpdateClientSettings();
                    return;
                case ServerMessage.Unknown:
                    Logger.Error("Intercepted Message is of type Unknown!");
                    return;
                case ServerMessage.GetLoginRequest:
                    GetLoginRequest();
                    return;
                case ServerMessage.UpdateSession:
                    //Flag is set on first Connect/Reconnect to true
                    //This allows to handle a session that might be already running or in another state were
                    //we would normally receive server messages for
                    UpdateSession(_isFirstSessionUpdateOnConnect);
                    _isFirstSessionUpdateOnConnect = false;
                    break;
                case ServerMessage.SendHeartbeat:
                    SendHeartbeat(_heartBeatTimer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message), message, null);
            }
        }
        private void OnServerMessageErrorIntercept(Exception exception)
        {
            Logger.Error(exception, "Intercepted Error from RPC Service!");
        }
        
        /// <summary>
        /// Gets called when the Server Notification Stream Ends
        /// This signals functions as Disconnect signalling.
        /// Functional operation can not be guaranteed anymore without receiving server messages
        /// The Server will End the Notifications gracefully after a Disconnect was send to the Client
        /// </summary>
        private void OnServerMessagesCompleted()
        {
            try
            {
                Logger.Info("OnServerMessagesCompleted started!");
                //That is a disconnect even we did not send a disconnect request
                //We can check if the Disconnect Operation in Process Flag is set to decide if the Disconnect
                //Is accidentally or intentionally
                SetConnectionState(
                        ConnectionState.Disconnected,
                        () =>
                        {
                            if(!_isDisconnectOperationInProgress)
                            {
                                //In case of failure we Cancel here
                                _notificationCts?.Cancel();
                                _rpcCallCts?.Cancel();
                            }
                            else _isDisconnectOperationInProgress = false;

                            ResetAllReferences();
                        });
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception on OnServerMessagesCompleted");
            }
            finally
            {
                Logger.Info("OnServerMessagesCompleted finished!");
            }
        }

        #region Set Connection State variants
        private void SetConnectionState(ConnectionState newState, Action action)
        {
            try
            {
                Logger.Info("Set ConnectionState started");
                try
                {
                    _connectionStateSemaphore.Wait();
                    Logger.Debug("Set ConnectionState Semaphore aquired");
                    _connectionState = newState;
                    action();
                }
                finally
                {
                    _connectionStateSemaphore.Release();
                    Logger.Debug("Set ConnectionState Semaphore released");
                }
            }
            finally
            {
                _whenConnectionStateChanged.OnNext(newState);
                Logger.Info("Set ConnectionState finished");
            }

        }
        private ConnectionState GetConnectionState()
        {
            try
            {
                Logger.Info("Get ConnectionState started");
                _connectionStateSemaphore.Wait();
                return _connectionState;
            }
            finally
            {
                _connectionStateSemaphore.Release();
                Logger.Debug("Get ConnectionState Semaphore released");
            }
        }
        private async Task SetConnectionStateAsync(ConnectionState newState, Action action = null)
        {
            try
            {
                Logger.Info("Set ConnectionStateAsync started");
                try
                {
                    await _connectionStateSemaphore.WaitAsync();
                    Logger.Debug("Set ConnectionStateAsync Semaphore aquired");
                    _connectionState = newState;
                    action?.Invoke();
                }
                finally
                {
                    _connectionStateSemaphore.Release();
                    Logger.Debug("Set ConnectionStateAsync Semaphore released");
                }
            }
            finally
            {
                _whenConnectionStateChanged.OnNext(newState);
                Logger.Info("Set ConnectionStateAsync finished");
            }
        }
        
        private async Task<bool> SetConnectionStateAsync(ConnectionState newState, Func<ConnectionState, bool> condition)
        {
            bool hasNewState = false;
            try
            {
                Logger.Info("Set ConnectionStateAsync started");
                try
                {
                    await _connectionStateSemaphore.WaitAsync();
                    Logger.Debug("Set ConnectionStateAsync Semaphore aquired");
                    if (condition(_connectionState))
                    {
                        _connectionState = newState;
                        hasNewState = true;
                    }
                    return hasNewState;
                }
                finally
                {
                    _connectionStateSemaphore.Release();
                    Logger.Debug("Set ConnectionStateAsync Semaphore released");
                }
            }
            finally
            {
                if(hasNewState)
                {
                    _whenConnectionStateChanged.OnNext(newState);
                }
                Logger.Info("Set ConnectionStateAsync finished");
            }
        }
        private void OnServiceError(ServiceType type, IServiceErrorInfo serviceErrorInfo)
        {
            Logger.Debug($"Remote Controller Received ServiceError with Type={type}");
            ShellClientInfo clientDisplayInfo;

            //If we had already an connection, we keep the previous data and update it
            //with the error state
            if (_clientInfo != null)
            {
                clientDisplayInfo = ShellClientInfo.CloneFrom(_clientInfo);
            }
            else
            {
                clientDisplayInfo = new ShellClientInfo
                                    {
                                            LicenseStatus = LicenseStatus.Unknown,
                                            VersionStatus = ShellVersionStatus.Unknown,
                                            SerialNumber = "Unknown",
                                            StationDisplayName = "Unknown"
                                    };
            }
            //Analyze the Error to set values
            switch (serviceErrorInfo)
            {
                case ILicenseError licenseError:
                    clientDisplayInfo.LicenseStatus = licenseError.State;
                    clientDisplayInfo.StationDisplayName = licenseError.ErrorString;
                    //_insufficientLicenseState = true;
                    break;
                case IVersionError versionError:
                    clientDisplayInfo.VersionStatus = versionError.State;
                    //_clientRequiresUpdate = true;
                    break;

            }
            _whenShellClientDisplayInfoChanged.OnNext(clientDisplayInfo);
            _whenServiceErrorOccured.OnNext(serviceErrorInfo);
        }

        private void WhenConnectionChanges(IRpcConnection obj)
        {

        }
        #endregion


        #endregion

        #region Server Message Handlers
        private void SendHeartbeat(object timer)
        {
            //Protect from being Disposed or All Canceled
            try
            {
                Logger.Info("Sending Heartbeat");
                _shellService.SendHeartbeat(_rpcCallCts.Token);
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("SendHeartbeat Task was canceled");
                }
                else Logger.Warn(e, "Sending Heartbeat Failed");
            }
        }
        private void UpdateServerSettings()
        {
            //Protect from being Disposed or All Canceled
            try
            {
                //Update Skew
                Logger.Info("Trying to Get Server Settings");
                var result = _shellService.GetServerSettings(_rpcCallCts.Token);
                if(result.IsSuccess())
                {
                    //Set Time skew between Server and Client
                    var timeSkew = result.ReturnValue.ServerTimeUtcNow - result.ReturnValue.LocalTimeUtcNow;
                    Logger.Info($"Updating Timeskew to: {timeSkew}");
                    _applicationService.TimeSkew = timeSkew;
                    _shellService.TimeSkew = timeSkew;

                    //Update Heartbeat, send one imminently and then in specified period
                    Logger.Info($"Setting HeartbeatInterval to {result.ReturnValue.HeartbeatInterval}!");
                    _heartBeatTimer.Change(0, Convert.ToUInt32(result.ReturnValue.HeartbeatInterval.TotalMilliseconds));
                }
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("UpdateServerSettings Task was canceled");
                }
                else Logger.Warn(e, "Updating Server Settings Failed");
            }
        }
        private void UpdateClientSettings()
        {
            //Protect from being Disposed or All Canceled
            try
            {
                Logger.Info("Updating Client Settings");
                var result = _shellService.GetClientSettings(_rpcCallCts.Token);
                if(result.IsSuccess())
                {
                    _clientInfo = new ShellClientInfo()
                                  {
                                          LicenseStatus = LicenseStatus.LicenseValid,
                                          SerialNumber = "N/A",
                                          StationDisplayName = result.ReturnValue.DisplayName,
                                          VersionStatus = ShellVersionStatus.UpToDate,
                                  };
                    ISessionSettings sessionSettings;
                    switch(result.ReturnValue.Mode)
                    {
                        case ControlMode.Local:
                            sessionSettings = new LocalMode();
                            break;
                        case ControlMode.Remote:
                            sessionSettings = new RemoteMode();
                            break;
                        case ControlMode.RemoteWithQrCode:
                            sessionSettings = new RemoteMode {QrUrl = result.ReturnValue.QrCode};
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    _whenShellClientDisplayInfoChanged.OnNext(_clientInfo);
                    _whenSessionSettingsChanged.OnNext(sessionSettings);
                }
                else
                {
                    Logger.Error($"Could not Get Client Settings: {result.ToErrorString()}");
                }
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("UpdateClientSettings Task was canceled");
                }
                else Logger.Warn(e, "UpdateClientSettings Failed");
            }
        }
        private void UpdateSession(bool isReconnect)
        {
            Logger.Debug($"Trying to Update Session State from Server with isReconnect Flag = {isReconnect}");
            IResult<Data.SessionState> result = null;
            try
            {
                //The Call can change the Session and is executed inside of a lock
                //This method is only allowed to be called by the notification Thread and only once a time
                //As we otherwise might get a deadlock
                lock(_sessionLock)
                {
                    result = _shellService.GetSessionState(_rpcCallCts.Token);
                    var currentSession = ServiceRemoteSession?.Session;
                    Logger.Debug($"Received Result from Server = {result.LogJson()}");
                    if(result.IsSuccess())
                    {
                        //Session Stopped on Server
                        if(result.ReturnValue?.State == null)
                        {
                            // The server told us the current session is gone but
                            // GetSessionState doesn't carry the WHY. A paying
                            // customer must never see their session silently
                            // disappear, so we fall through to SessionLimitReached
                            // (which shows the user *a* block screen even if the
                            // wording isn't exactly right) rather than to a
                            // station-side reason like StationLogout that would
                            // transition quietly back to LoginView.
                            //
                            // The right long-term fix is to plumb the server's
                            // Session.StopReason through the gRPC contract (or via
                            // a follow-up REST query) and map RemoteLogout / etc.
                            // to a localized message that says "your session was
                            // ended by the operator" - tracked in
                            // docs/usage/kiosk-known-issues.md.
                            currentSession?.RequestStopSession(SessionStopReason.SessionLimitReached,false,false);
                            ServiceRemoteSession = null;
                            return;
                        }

                        //Handle following cases
                        //1. An Existing Session was Updated with New Data
                        //2. An Existing Session was Updated and Stopped
                        //3. An Reconnect or first time Connect occured
                        //  3.1 An Old Session is still present in same or older state then on the server
                        //  3.2 An Old Session is still present but already closed on the server
                        //  All other situations should generally be ruled out through mechanisms on the server side

                        var sessionDetails = result.ReturnValue.State;

                        //We don't have a Session locally
                        if(currentSession  == null)
                        {
                            switch(sessionDetails.Stage)
                            {
                                case SessionState.LoginRequested:
                                    //This situation should normally not occur but we still try to handle it
                                    if(isReconnect)
                                    {
                                        GetLoginRequest();
                                    }
                                    break;
                                case SessionState.Running:
                                case SessionState.AwaitingConfirmation:
                                    //This situation should normally not occur but we still try to handle it
                                    ServiceRemoteSession = CreateSession(sessionDetails);
                                    break;
                                default:
                                    //Nothing to do for other states
                                    return;
                            }
                        }
                        //We have a session locally and
                        else
                        {
                            //Not Remotely
                            if(sessionDetails.Stage == SessionState.NoSession)
                            {
                                //Stop our Session
                                //Its essential to not send to the Server as we might run into a deadlock if in that same time
                                //a manual or timer based stop request is pending
                                switch(currentSession.SessionStage)
                                {
                                    case SessionState.AwaitingConfirmation:

                                        if(!currentSession.Expired) currentSession.ExpirePremature();
                                        break;
                                    default:
                                        //Just call it, but without calling us back( would lead to possible deadlock if session stop timer fires also in same time)
                                        currentSession.RequestStopSession(SessionStopReason.Unknown,false);
                                        //Not send to us back so we must null here
                                        ServiceRemoteSession = null;
                                        break;
                                }
                            }
                            //Remotely
                            else
                            {
                                //We update it if its the same one
                                if(sessionDetails.SessionId == currentSession.IntentionId)
                                {
                                    currentSession.UpdateSession(sessionDetails);
                                }
                                //The one on the server is newer, that should normally not have happened
                                else if(sessionDetails.RequestedOnUtc > currentSession.RequestedOnUtc)
                                {
                                    //Lets try to handle it anyway
                                    switch(currentSession.SessionStage)
                                    {
                                        case SessionState.AwaitingConfirmation:
                                            if(!currentSession.Expired) currentSession.ExpirePremature();
                                            break;
                                        default:
                                            //Just call it, but without calling us back( would lead to possible deadlock if session stop timer fires also in same time)
                                            currentSession.RequestStopSession(SessionStopReason.Unknown,false);
                                            //Not send to us back so we must null here
                                            ServiceRemoteSession = null;
                                            break;
                                    }
                                    //Now lets handle the new one
                                    switch(sessionDetails.Stage)
                                    {
                                        case SessionState.LoginRequested:
                                            //This situation should normally not occur but we still try to handle it
                                            if(isReconnect)
                                            {
                                                GetLoginRequest();
                                            }
                                            break;
                                        case SessionState.Running:
                                        case SessionState.AwaitingConfirmation:
                                            //This situation should normally not occur but we still try to handle it
                                            ServiceRemoteSession = CreateSession(sessionDetails);
                                            break;
                                        default:
                                            //Nothing to do for other states
                                            return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //No State to work with provided
                    }
                }
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("UpdateSession was canceled");
                }
                else
                {
                    Logger.Error(e, "Error in UpdateSession occured");
                }
            }
            finally
            {
                if(result != null && result.HasError())
                {
                    Logger.Warn($"UpdateSession failed because of: {result.ToErrorString()}");
                }
                Logger.Debug($"Trying to Update Session State from Server finished");
            }
        }

        private void GetLoginRequest()
        {
            Logger.Debug("Trying to Get Login Request from Server");
            //Protect from being Disposed or All Canceled
            try
            {
                //The Call can change the Session and is executed inside of a lock
                lock(_sessionLock)
                {
                    var result = _shellService.GetLoginIntention(_rpcCallCts.Token);
                    Logger.Debug($"Received response for Login Request from Server = {result.LogJson()}");
                    if(result.IsSuccess())
                    {
                        if(result.ReturnValue.SessionDetails == null)
                        {
                            Logger.Warn("Server send no session details but return value was not success");
                            return;
                        }
                        //A new Login Request was received
                        if(ServiceRemoteSession == null)
                        {
                            //Create the new Session and set it
                            ServiceRemoteSession = CreateSession(result.ReturnValue.SessionDetails);
                        }
                        //We do have a session, so this must be a call occured during a disconnect on reconnect
                        else
                        {
                            Logger.Debug($"We do have a session set, trying to handle state");
                            //Its the same as received
                            if(result.ReturnValue.SessionDetails.SessionId == ServiceRemoteSession.Session.IntentionId)
                            {
                                //Send as Update
                                ServiceRemoteSession.Session.UpdateSession(result.ReturnValue.SessionDetails);
                            }
                            else if(result.ReturnValue.SessionDetails.RequestedOnUtc > ServiceRemoteSession.Session.RequestedOnUtc)
                            {
                                ServiceRemoteSession = CreateSession(result.ReturnValue.SessionDetails);
                                Logger.Error($"Can not link current active session with received one from server: {ServiceRemoteSession.LogJson()} ServerData:{result.ReturnValue.SessionDetails.LogJson()}");
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if(e is TaskCanceledException)
                {
                    Logger.Info("GetLoginRequest Task was canceled");
                }
                else Logger.Warn(e, "GetLoginRequest Failed");
            }
        }
        #endregion


        /// <summary>
        /// Creates an new Session based on the SessionDetails
        /// </summary>
        /// <param name="sessionDetails">Null if the <see cref="RemoteSession"/> would not be anymore valid/alive otherwise the new <see cref="RemoteSession"/></param>
        /// <returns></returns>
        private SubscribedSession CreateSession(SessionDetails sessionDetails)
        {
            var newSession = new SubscribedSession(new RemoteSession(sessionDetails, this),this);
            return newSession.Session.InitSession() ? newSession : null;
        }

        private void ResetAllReferences()
        {
            try
            {
                _disposeServicesProtectionSemaphore.Wait(TimeSpan.FromSeconds(5));
                //Reset all References
                Logger.Info("Resetting all References");
                _notificationTask = null;
                _heartBeatTimer = null;
                try
                {
                    _applicationService?.Dispose();
                    _shellService?.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error during Reseting all References");
                }
                _applicationService = null;
                _shellService = null;
                _notificationCts = null;
                _rpcCallCts = null;
                _shellServer = null;
                Logger.Info("Resetting all References finished");
            }
            finally
            {
                _disposeServicesProtectionSemaphore.Release();
            }
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;
            foreach(IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
        private static CancellationTokenSource CreateCtsLinkedToMain(
                CancellationTokenSource mainTokenSource,
                out CancellationTokenSource tokenSource)
        {
            tokenSource = new CancellationTokenSource();
            return CancellationTokenSource.CreateLinkedTokenSource(
                    mainTokenSource.Token,
                    tokenSource.Token);
        }
    }
}