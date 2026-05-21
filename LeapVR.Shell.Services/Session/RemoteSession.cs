#region Licence
/****************************************************************
 *  Filename: RemoteSession.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Services.Data;
using LeapVR.Shell.Services.RpcServices;
using NLog;
using Pod.Data.Infrastructure;
using SessionState = Pod.Grpc.Messages.Shared.SessionState;

namespace LeapVR.Shell.Services.Session
{
    partial class RemoteSession : ILoginIntention
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Subject<ILoginIntention> _whenLoginDecisionRequired =
                new Subject<ILoginIntention>();
        private readonly Subject<LoginDecisionResultType> _whenLoginDecisionResponseArrived =
                new Subject<LoginDecisionResultType>();
        private readonly Subject<ILoginIntention> _whenLoginIntentionExpired =
                new Subject<ILoginIntention>();
        private readonly Subject<IUISession> _whenSessionStarted =
                new Subject<IUISession>();

        private readonly BehaviorSubject<IUISession> _whenSessionUpdated;


        private readonly ISessionService _sessionService;
        private readonly object _updateLock = new object();
        private readonly Timer _sessionStopTimer;
        private readonly Timer _expirationTimer;
        private volatile bool _isInitialized = false;
        private volatile bool _isExpired = false;
        private volatile bool _isSendingDecision = false;
        private volatile bool _isDecisionSend = false;
        private BaseSessionRate _sessionRate;
        private SessionDetails _sessionDetails;
        internal RemoteSession(SessionDetails sessionDetails,ISessionService sessionService)
        {
            RequestedOnUtc = sessionDetails.RequestedOnUtc;
            _sessionStopTimer = new Timer(StopSessionByTimer);
            _expirationTimer = new Timer(SendExpiredByTimer);
            _sessionRate = BaseSessionRate.Create(sessionDetails);
            _sessionDetails = sessionDetails;
            _sessionService = sessionService;
            //Last step as it will use the previously set Data
            _whenSessionUpdated = UISession.BehaviorSubjectFromRemoteSession(RequestStopSession, this);
        }
        internal DateTime RequestedOnUtc { get; }
        internal SessionState SessionStage => _sessionDetails.Stage;
        internal bool Expired => _isExpired;

        #region ILoginIntention Interface
        public Guid IntentionId => _sessionDetails.SessionId;
        public IObservable<IUISession> WhenSessionStarted => _whenSessionStarted;
        public DateTime IntentionConfirmationExpiresOnUtc =>
                _sessionDetails.DeadlineUtcForConfirmation ?? DateTime.MaxValue;
        public async Task SendLoginDecisionAsync(LoginDecisionType decision)
        {
            try
            {
                Logger.Debug("SendLoginDecisionAsync started");
                lock(_updateLock)
                {
                    //Don't allow to send a Login Decision if its not in the right state or already considered expired
                    if(_isExpired ||
                       !_isInitialized ||
                       _sessionDetails.Stage != SessionState.AwaitingConfirmation ||
                       _isSendingDecision)
                    {
                        Logger.Warn($"Can not send  LoginDecision in current State: {this.LogJson()}");
                        return;
                    }

                    Logger.Debug("LoginDecision is in Sending State");
                    _isSendingDecision = true;
                }

                try
                {
                    var result = await _sessionService.SendLoginIntentionResponseAsync(decision == LoginDecisionType.Confirm);
                    Logger.Debug($"LoginDecision received for decision = {decision}, with = {result.LogJson()}");
                    if(result.IsSuccess())
                    {
                        _isDecisionSend = true;
                        if(result.ReturnValue?.Session == null ||
                           result.ReturnValue.Session.Stage == SessionState.NoSession)
                        {
                            _whenLoginDecisionResponseArrived.OnNext(LoginDecisionResultType.Canceled);
                            _sessionService.RemoveSession(this);
                        }
                        else
                        {
                            _whenLoginDecisionResponseArrived.OnNext(LoginDecisionResultType.SessionStarted);
                            UpdateSession(result.ReturnValue.Session);
                        }
                    }
                    else
                    {
                        Logger.Error($"Error Response recieved: {result.LogJson()}");
                    }
                }
                finally
                {
                    _isSendingDecision = false;
                }
            }
            catch(Exception e)
            {
                Logger.Fatal(e, "Unhandled Exception encountered!");
                throw;
            }
            finally
            {
                Logger.Debug("SendLoginDecisionAsync finished");
            }
        }
        #endregion

        #region ILoginIntention / IUISession Shared
        public ISessionRate SessionRate => _sessionRate;
        #endregion

        #region IUISession Interface
        public Guid SessionId => _sessionDetails.SessionId;
        public IObservable<IUISession> WhenSessionUpdated => _whenSessionUpdated.AsObservable();
        internal IObservable<ILoginIntention> WhenLoginDecisionRequired => _whenLoginDecisionRequired;
        internal IObservable<LoginDecisionResultType> WhenLoginDecisionResponseArrived =>
                _whenLoginDecisionResponseArrived;
        internal IObservable<ILoginIntention> WhenLoginIntentionExpired => _whenLoginIntentionExpired;

        public SessionType Type => _sessionRate.Type;
        public DateTime Started => _sessionDetails.StartTimeUtc ?? DateTime.MinValue;
        public DateTime? Stopped { get; private set; }
        public SessionStopReason? StopReason { get; private set; }
        public void RequestStopSession(SessionStopReason reason) { RequestStopSession(reason, true, false); }
        #endregion

        public override string ToString()
        {
            var retval = base.ToString();
            switch(Type)
            {
                case SessionType.Unlimited:
                    return retval + Environment.NewLine + "Session has no limits!";
                case SessionType.Limited:
                    return retval +
                           Environment.NewLine +
                           $"Stop Time Limit={_sessionRate.CalculatedStopTimeUtc}";
                default:
                    return retval + "Unknown Session Type!";
            }
        }



        internal bool InitSession()
        {
            //Execute Actions outside of the lock
            Action initAction = null;

            //Nothing if already initialized
            if(_isInitialized) return false;
            bool isStillAlive = true;

            //Lock for Update
            lock(_updateLock)
            {
                if(_isInitialized)
                {
                    return false;
                }

                Logger.Info($"Initializing Session: {this.LogJson()}");
                //Evaluate the current stage and set the init action
                switch(_sessionDetails.Stage)
                {
                    case SessionState.AwaitingConfirmation:
                        initAction = () => _whenLoginDecisionRequired.OnNext(this);
                        isStillAlive = SetConfirmationExpirationTimer(
                                _expirationTimer,
                                IntentionConfirmationExpiresOnUtc);
                        if(!isStillAlive)
                        {
                            Logger.Debug(
                                    $"Initialization detected that the Session is already expired with Stage={_sessionDetails.Stage}");
                            _isExpired = true;
                        }

                        break;
                    case SessionState.Running:
                        //We create the Session inside the LockStatement to prevent any possible manipulations
                        var updatedSession = new UISession(RequestStopSession, WhenSessionUpdated, this);
                        initAction = () => _whenSessionStarted.OnNext(updatedSession);
                        break;
                    default:
                        throw new NotSupportedException(
                                $"Initialization of an Session with Stage = {_sessionDetails.Stage} is not supported");
                }

                if(isStillAlive)
                {
                    //Session Details and SessionData was already set in Constructor, we only need to set the StopTimer here
                    isStillAlive = SetStopTimer(_sessionStopTimer, _sessionRate);
                }
                _isInitialized = true;
            }

            Logger.Debug($"Initialization was evaluated with IsStillAlive = {isStillAlive}");
            if(isStillAlive)
            {
                //Invoke the Notifications outside of the lock statement
                //Otherwise a Deadlock will happen!!!!
                initAction();
            }

            return isStillAlive;
        }

        internal void UpdateSession(SessionDetails sessionDetails)
        {
            try
            {
                Logger.Info("UpdateSession started");
                Logger.Debug($"UpdateSession called with on: {this.LogJson()} with Update:{sessionDetails.LogJson()}");
                //Check if the Update is valid or the current Session is already considered as Stopped
                if(sessionDetails.SessionId != _sessionDetails.SessionId ||
                   StopReason != null) return;

                //Action that to perform for notification
                Action notificationAction= () => {};

                //Lock for Update
                bool sendNotifications = true;
                lock(_updateLock)
                {
                    //Don't allow to update with another Session or if the session is already stopped
                    if(sessionDetails.SessionId != _sessionDetails.SessionId ||
                       StopReason != null) return;

                    //A Session needs to be all time initialized before an update
                    if(!_isInitialized)
                    {
                        throw new NotSupportedException("Uninitialized Sessions can not be Updated !");
                    }

                    //Remember previous and new Details
                    var oldSessionDetails = _sessionDetails;
                    var newSessionDetails = sessionDetails;

                    //Update the Session Details, returns false if the Session was stopped during the update
                    //In this case notifications might been send out by RequestStopSession
                    sendNotifications = UpdateData(sessionDetails, _sessionStopTimer);

                    if(sendNotifications)
                    {

                        //Handle Notifications based on current and previous stage
                        switch (newSessionDetails.Stage)
                        {
                            case SessionState.AwaitingConfirmation:
                                //We don't do anything here as the previous state must have been the same
                                //And we do not have any mechanism to update LoginIntentions for the moment
                                return;
                            case SessionState.Running:
                                //Create a new Session state as long inside of the Lock after the Data was updated
                                var updatedSession = new UISession(RequestStopSession, WhenSessionUpdated, this);
                                if (oldSessionDetails.Stage == SessionState.Running)
                                {
                                    notificationAction = () => _whenSessionUpdated.OnNext(updatedSession);
                                }
                                else
                                {
                                    notificationAction = () => _whenSessionStarted.OnNext(updatedSession);
                                }

                                break;
                            default:
                                throw new NotSupportedException(
                                        $"UpdateSession is not supported when the new State is {newSessionDetails.Stage}!");
                        }
                    }
                }
                //Invoke the Notifications outside of the lock statement
                //Otherwise a Deadlock will happen!!!!
                if(sendNotifications) notificationAction();
            }
            finally
            {
                Logger.Info("UpdateSession finished");
            }
        }

        /// <summary>
        /// Use in case that an reconnect happened and an new Session intention is present but the old is still
        /// here and not yet expired. 
        /// </summary>
        internal void ExpirePremature()
        {
            Logger.Info($"ExpirePremature called");
            SendExpiredByTimer(null);
        }

        /// <summary>
        /// Requests the stop of the session if not already stopped
        /// </summary>
        /// <param name="reason">The stop reason.</param>
        /// <param name="sendStopToServer">if set to <c>true</c> a request will be send to the server, if <c>false</c> no request is send</param>
        /// <param name="isTimerStop">set to <c>true</c> if this request is called by the stopTimer to enable an additional check.</param>
        /// <exception cref="NotSupportedException">Uninitialized Sessions can not be Stopped !</exception>
        internal void RequestStopSession(SessionStopReason reason, bool sendStopToServer, bool isTimerStop = false)
        {
            try
            {
                Logger.Info("RequestStopSession started");
                //Check if already stopped or not even running
                if(StopReason != null || _sessionDetails.Stage != SessionState.Running) return;

                Action notification;
                //We need to check if it was handled as Intention and then do the right thing
                lock(_updateLock)
                {
                    //Check if already stopped or not running
                    if(StopReason != null || _sessionDetails.Stage != SessionState.Running) return;

                    Logger.Info($"RequestStopSession called on: {this.LogJson()}");
                    //A Session needs to be all time initialized before an update
                    if(!_isInitialized)
                    {
                        throw new NotSupportedException("Uninitialized Sessions can not be Stopped !");
                    }

                    //Last chance to cancel the Stop if it was caused by a timer but updated while the timer thread was waiting to get the update lock
                    if(isTimerStop)
                    {
                        //If the Session rates changed, it must mean that the StopTime has changed by an Update before we could get the lock
                        //Therefor the StopTimer was already readjusted and we can just leave
                        if(!_sessionRate.CalculatedStopTimeUtc.HasValue ||
                           _sessionRate.CalculatedStopTimeUtc.Value > DateTime.UtcNow)
                        {
                            Logger.Info($"Stop canceled on: {this.LogJson()}");
                            return;
                        }
                    }


                    //We Setting our Stop Values, this is the point of no return
                    StopReason = reason;
                    Stopped = DateTime.UtcNow;
                    _sessionStopTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _sessionStopTimer.Dispose();

                    //Is false if the stop was due to an Update requested from Server side
                    if(sendStopToServer)
                    {
                        Logger.Info($"Sending Stop to Server for: {this.LogJson()}");
                        var response = _sessionService.SendLogoutRequest(reason);
                        if(response.HasError())
                        {
                            Logger.Error($"SendLogoutRequest had an Error {response.ToErrorString()}");
                        }

                        _sessionService.RemoveSession(this);
                    }

                    Logger.Info("Setting Notifications");
                    //Set Notifications & create a new Session state as long inside of the Lock after the Data was updated
                    var updatedSession = new UISession(RequestStopSession, WhenSessionUpdated, this);
                    notification = () =>
                                   {
                                       _whenSessionUpdated.OnNext(updatedSession);
                                       _whenSessionUpdated.OnCompleted();
                                   };
                }

                Logger.Info("Processing Notifications");
                //TODO Check as this can still be in an UpdateLock Statement if called through UpdateData=>SetStopTimer=>RequestStopSession
                notification();
            }
            finally
            {
                Logger.Info("RequestStopSession finished");
            }
        }

        private bool SetConfirmationExpirationTimer(Timer timer, DateTime expiresOn)
        {
            var isTimerSet = true;
            var timeToExpiration = expiresOn - DateTime.UtcNow;
            if(timeToExpiration > TimeSpan.Zero)
            {
                Logger.Debug($"Expiration Timer set with Expiration Time is due in = {timeToExpiration} ");
                timer.Change(timeToExpiration, Timeout.InfiniteTimeSpan);
            }
            else
            {
                Logger.Debug(
                        $"Expiration Timer was not set as Expiration Time was due already in = {timeToExpiration}");
                isTimerSet = false;
            }

            return isTimerSet;
        }

        /// <summary>
        /// Call only inside a updateLock statement, Adjusts the StopTimer based on the provided rate
        /// </summary>
        /// <param name="timer">The timer to adjust</param>
        /// <param name="rate">The rate to evaluate</param>
        /// <returns></returns>
        private bool SetStopTimer(Timer timer, BaseSessionRate rate)
        {
            var isTimerSet = true;

            //A Stop Timer is only active on an Running Session that has a limited duration
            if(rate.CalculatedStopTimeUtc.HasValue)
            {
                //We calculate the time when to call the Stop and subtract 500 milliseconds as latency
                var timespanForStop = rate.CalculatedStopTimeUtc.Value - DateTime.UtcNow;

                //Check that the time is somewhere in the future
                if(timespanForStop.TotalSeconds > 0.5)
                {

                    //TODO The Timer does not support values higher then dueTime.TotalMilliseconds greater than 0xfffffffeL
                    //We could support it by setting an property with remaining time and then check it and reset the timer if there is remaining time
                    if(timespanForStop.TotalMilliseconds > 0xfffffffeL)
                    {
                        timespanForStop = TimeSpan.FromMilliseconds(0xfffffffeL);
                    }
                    timer.Change(timespanForStop, Timeout.InfiniteTimeSpan);
                }
                //Handle the case its already overdue
                else
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                    RequestStopSession(SessionStopReason.SessionLimitReached, true, true);
                    isTimerSet = false;
                }
            }
            //We Ensure the Stop Timer is not triggering
            else
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            return isTimerSet;
        }

        private void StopSessionByTimer(object timer)
        {
            //Translates to LimitReached
            RequestStopSession(SessionStopReason.SessionLimitReached, true, true);
        }

        private void SendExpiredByTimer(object timer)
        {
            if(_isExpired || _isDecisionSend || _isSendingDecision) return;
            lock(_updateLock)
            {
                if(_isExpired || _isDecisionSend || _isSendingDecision) return;
                if(_sessionDetails.Stage != SessionState.Running)
                {
                    Logger.Info("Setting _isExpired to true");
                    _isExpired = true;
                }
                else return;
            }

            Logger.Info("Notifing about expired Session");
            //Signal to the System that this session expired
            _whenLoginIntentionExpired.OnNext(this);
            //Signal to the remote Service that this session expired
            _sessionService.RemoveSession(this);
        }

        /// <summary>
        /// Updates the RemoteSessions Data
        /// </summary>
        /// <param name="sessionDetails"></param>
        /// <param name="sessionStopTimer"></param>
        /// <returns>True if notifications needs to be send</returns>
        bool UpdateData(SessionDetails sessionDetails, Timer sessionStopTimer)
        {
            //Update the Details
            _sessionDetails = sessionDetails;

            //Update the Rate
            _sessionRate = BaseSessionRate.Create(sessionDetails);

            //Update the StopTimer
            return SetStopTimer(sessionStopTimer, _sessionRate);
        }
    }
}