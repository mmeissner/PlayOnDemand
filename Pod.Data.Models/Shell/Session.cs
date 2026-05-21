#region Licence
/****************************************************************
 *  Filename: Session.cs
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
using System.Diagnostics.CodeAnalysis;
using Pod.Data.Infrastructure;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Represents a session for an Station
    /// </summary>
    public class Session
    {
        private HashSet<ChangeRequest> _changeRequests;
        private Session() { LoadedFromDatabaseUtc = DateTime.UtcNow; }
        internal Session(Guid stationId, RequestSource source, string sourceIpAddress, string requestReference = null)
        {
            RequestedOnUtc = DateTime.UtcNow;
            RequestFromIpAddress = sourceIpAddress;
            RequestedBy = source;
            State = SessionState.Requested;
            StationId = stationId;
            RequestReference = requestReference;
        }

        /// <summary>
        /// The Id of the session
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The DateTime the object was retrieved from the database
        /// </summary>
        public DateTime LoadedFromDatabaseUtc { get; }

        /// <summary>
        /// Represents if a session is finalized/closed
        /// Closed sessions are final
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// State of the Session that the session is currently at
        /// </summary>
        public SessionState State { get; private set; }

        /// <summary>
        /// The DateTime the session was requested/created
        /// </summary>
        public DateTime RequestedOnUtc { get; private set; }

        /// <summary>
        /// The Source Type the session was requested by
        /// </summary>
        public RequestSource RequestedBy { get; private set; }

        /// <summary>
        /// The Ip Address the session was requested from
        /// </summary>
        public string RequestFromIpAddress { get; private set; }

        /// <summary>
        /// The Reference that was provided on the creation
        /// </summary>
        public string RequestReference { get; private set; }

        /// <summary>
        /// The DateTIme the Session was send/picked up to/by the station
        /// </summary>
        public DateTime? SendOnUtc { get; private set; }

        /// <summary>
        /// The Connection Id that was used during the pickup of the session 
        /// </summary>
        public Guid? SendToConnectionId { get; private set; }

        /// <summary>
        /// The DateTime the session was accepted by the station and started
        /// </summary>
        public DateTime? StartedUtc { get; private set; }

        /// <summary>
        /// The valid duration that this session has
        /// If null then there is no limit 
        /// </summary>
        public TimeSpan? Duration { get; private set; }

        /// <summary>
        /// The DateTime the session was stopped
        /// </summary>
        public DateTime? StoppedUtc { get; private set; }

        /// <summary>
        /// The Reason the Session was stopped
        /// </summary>
        public StopReason StopReason { get; private set; }

        /// <summary>
        /// A SessionRuleId for the session
        /// Null if no Rule is set
        /// </summary>
        public Guid? SessionRuleId { get; private set; }

        /// <summary>
        /// The NavigationProperty for the SessionRule
        /// </summary>
        public SessionRule SessionRule { get; private set; }

        /// <summary>
        /// The StationId this session belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// NavigationProperty for the Station
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// Aggregation Root for the Session
        /// </summary>
        public SessionDetails SessionDetails { get; private set; }

        /// <summary>
        /// Collection of all change requests
        /// </summary>
        public IReadOnlyCollection<ChangeRequest> ChangeRequests => _changeRequests;

        /// <summary>
        /// Adds a Start Condition and creates a rule by this
        /// </summary>
        /// <param name="duration">The max duration for the session</param>
        /// <returns></returns>
        public IResult AddStartCondition(TimeSpan duration)
        {
            var result = new Result();
            result.ArgNotLowerOrEqualThen(
                    (TimeSpan)duration,
                    nameof(duration),
                    TimeSpan.Zero,
                    nameof(TimeSpan.Zero),
                    UserError.SessionRuleDurationInvalid);
            var sessionRule = AddOrGetRule();
            result.Add(sessionRule);
            if(result.IsSuccess())
            {
                sessionRule.ReturnValue.SetStartDuration(duration);
            }

            return result;
        }

        /// <summary>
        /// Request to change the State to Delivered
        /// </summary>
        /// <param name="connectionId">The connectionId provided by the client</param>
        /// <param name="timeoutLoginRequestDelivery">Timeout value for the Delivery to check against</param>
        /// <param name="timeoutLoginRequestResponse">Timeout value for the Response to check against</param>
        /// <returns></returns>
        internal SessionResponse RequestDelivery(
                Guid connectionId,
                TimeSpan timeoutLoginRequestDelivery,
                TimeSpan timeoutLoginRequestResponse)
        {
            if(IsPreRunTimeOut(timeoutLoginRequestDelivery, timeoutLoginRequestResponse))
            {
                return SessionResponse.Timeout;
            }

            if(State != SessionState.Requested && State != SessionState.Delivered)
            {
                return SessionResponse.StateMismatch;
            }

            if(State == SessionState.Delivered && SendToConnectionId != connectionId)
            {
                return SessionResponse.ConnectionMismatch;
            }

            SendOnUtc = DateTime.UtcNow;
            SendToConnectionId = connectionId;
            State = SessionState.Delivered;
            return SessionResponse.Success;
        }

        /// <summary>
        /// Handles the confirmation received from a client for a session
        /// </summary>
        /// <param name="connectionId">The connectionId from the client</param>
        /// <param name="accepted">Value if the client accepted the session</param>
        /// <param name="timeoutLoginRequestDelivery">Timeout value for the Delivery to check against</param>
        /// <param name="timeoutLoginRequestResponse">Timeout value for the Response to check against</param>
        /// <returns></returns>
        internal SessionResponse SetConfirmation(
                Guid connectionId,
                bool accepted,
                TimeSpan timeoutLoginRequestDelivery,
                TimeSpan timeoutLoginRequestResponse)
        {
            if(SendToConnectionId != null && SendToConnectionId != connectionId)
            {
                //It does not belong to this connection
                return SessionResponse.ConnectionMismatch;
            }

            switch(State)
            {
                //We set the desired State
                case SessionState.Delivered:
                    if(IsPreRunTimeOut(timeoutLoginRequestDelivery, timeoutLoginRequestResponse))
                    {
                        return SessionResponse.Timeout;
                    }

                    if(accepted)
                    {
                        Duration = SessionRule?.StartDuration;
                        StartedUtc = DateTime.UtcNow;
                        State = SessionState.Started;
                    }
                    else
                    {
                        State = SessionState.Canceled;
                        IsClosed = true;
                    }

                    return SessionResponse.Success;

                //We verify if the desired state is the current
                case SessionState.Started:
                    if(accepted) return SessionResponse.Success;
                    break;
                case SessionState.Canceled:
                    if(!accepted) return SessionResponse.Success;
                    break;
                default:
                    return SessionResponse.StateMismatch;
            }

            return SessionResponse.StateMismatch;
        }

        /// <summary>
        /// Determines whether the Session is timed out on delivery or on a login response.
        /// </summary>
        /// <param name="timeoutLoginRequestDelivery">The timeout for a login request delivery.</param>
        /// <param name="timeoutLoginRequestResponse">The timeout for a login request response.</param>
        /// <returns>
        ///   <c>true</c> if it's timed out; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsPreRunTimeOut(
                TimeSpan timeoutLoginRequestDelivery,
                TimeSpan timeoutLoginRequestResponse)
        {
            if(State == SessionState.Requested)
            {
                if(RequestedOnUtc.Add(timeoutLoginRequestDelivery) < LoadedFromDatabaseUtc)
                {
                    State = SessionState.DeliveryTimeout;
                    IsClosed = true;
                    return true;
                }
            }
            else if(State == SessionState.Delivered)
            {
                if(SendOnUtc.Value.Add(timeoutLoginRequestResponse) < LoadedFromDatabaseUtc)
                {
                    State = SessionState.ResponseTimeout;
                    IsClosed = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Requests the End of the Session
        /// </summary>
        /// <param name="connectionId">The connection Id the request originates from</param>
        /// <param name="reason">The reason provided</param>
        /// <returns>Input value for further handing</returns>
        internal SessionResponse EndSession(Guid connectionId, StopReason reason)
        {
            if(connectionId != SendToConnectionId) return SessionResponse.ConnectionMismatch;
            return EndSession(reason);
        }

        /// <summary>
        /// Requests the End of the Session
        /// </summary>
        /// <param name="reason">The reason provided</param>
        /// <returns>Input value for further handing</returns>
        internal SessionResponse EndSession(StopReason reason)
        {
            if(State != SessionState.Started) return SessionResponse.StateMismatch;
            StoppedUtc = DateTime.UtcNow;
            StopReason = reason;
            State = SessionState.Ended;
            IsClosed = true;
            return SessionResponse.Success;
        }

        /// <summary>
        /// Requests an Change to the current session rules
        /// </summary>
        /// <param name="source">The Source the request is send from</param>
        /// <param name="sourceIp">The Ip of the source</param>
        /// <param name="timeChange">The time change requested</param>
        /// <param name="changeReference">The reference provided to add to the update</param>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        internal (SessionResponse Response, ChangeRequest changeRequest)AddChangeRequest(RequestSource source, string sourceIp, TimeSpan timeChange, string changeReference = null)
        {
            //Change only allowed during session is running 
            if(State != SessionState.Started) return (SessionResponse.StateMismatch, null);

            var newChangeRequest = new ChangeRequest(this, source, sourceIp, timeChange,changeReference);
            _changeRequests.Add(newChangeRequest);
            if(Duration == null)
            {
                //Not time limited yet
                //Set initial limitation
                Duration = newChangeRequest.CreatedOnUtc.Subtract(StartedUtc.Value) + timeChange;
            }
            else
            {
                Duration = Duration + timeChange;
            }

            //Evaluate if this causes a Stop
            if(StartedUtc.Value.Add(Duration.Value) <= newChangeRequest.CreatedOnUtc)
            {
                //End the Session
                EndSession(StopReason.LimitReached);
            }

            return (SessionResponse.Success,newChangeRequest);
        }

        private IResult<SessionRule> AddOrGetRule()
        {
            var result = new Result<SessionRule>();
            if(!result.ArgFalse(
                    StartedUtc.HasValue,
                    nameof(StartedUtc),
                    UserError.SessionCanNotBeStarted)) return result;

            if(!result.RefNotNullIfExist(SessionRule, SessionRuleId, nameof(SessionRule))) return result;
            if(this.SessionRule == null)
            {
                var newRule = new SessionRule();
                SessionRule = newRule;
                newRule.Session = this;
                result.Add(SessionRule);
            }
            else
            {
                result.Add(SessionRule);
            }

            return result;
        }
    }

    /// <summary>
    /// Sets rules for a session that the client will have to ensure
    /// </summary>
    public class SessionRule
    {
        private HashSet<SessionRuleLocalApp> _allowedApps;
        internal SessionRule() { }
        /// <summary>
        /// The Id of this Instance
        /// </summary>
        public Guid Id { get; internal set; }
        /// <summary>
        /// The Maximum Duration the Session should receive on Start 
        /// </summary>
        public TimeSpan? StartDuration { get; internal set; }
        /// <summary>
        /// The Application that should be started on Start of the Session
        /// </summary>
        public Guid? StartApplication { get; internal set; }

        /// <summary>
        /// The navigationProperty for the Session this instance belongs to
        /// </summary>
        public Session Session { get; internal set; }

        /// <summary>
        /// If set that the Applications available on the Client should be limited to this
        /// If non is set the all applications should be allowed
        /// </summary>
        public IReadOnlyCollection<SessionRuleLocalApp> AllowedApps => _allowedApps;

        /// <summary>
        /// Sets the Start Duration
        /// </summary>
        /// <param name="duration"></param>
        internal void SetStartDuration(TimeSpan duration) { StartDuration = duration; }
    }

    /// <summary>
    /// ManyToMany Link between <see cref="SessionRule"/> and <see cref="LocalApp"/>
    /// </summary>
    public class SessionRuleLocalApp
    {
        public Guid SessionRuleId { get; private set; }
        public SessionRule SessionRule { get; private set; }
        public Guid LocalAppId { get; private set; }
        public LocalApp LocalApp { get; private set; }
    }

    /// <summary>
    /// A Change Request for an Session
    /// </summary>
    public class ChangeRequest
    {
        private ChangeRequest() { }
        internal ChangeRequest(Session session, RequestSource source, string sourceIp, TimeSpan timeChange, string reference = null)
        {
            SessionId = session.Id;
            Session = session;
            RequestFrom = source;
            SourceIpAddress = sourceIp;
            TimeChange = timeChange;
            CreatedOnUtc = DateTime.UtcNow;
            Reference = reference;
        }

        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The DateTime this instance was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }
        /// <summary>
        /// The Source this request was received from
        /// </summary>
        public RequestSource RequestFrom { get; private set; }
        /// <summary>
        /// The Ip Address of the Source
        /// </summary>
        public string SourceIpAddress { get; private set; }

        /// <summary>
        /// The Reference provided from the sender for this change request 
        /// </summary>
        public string Reference { get; private set; }

        /// <summary>
        /// The change in time requested
        /// </summary>
        public TimeSpan TimeChange { get; private set; }

        /// <summary>
        /// The SessionId this changes request belongs to
        /// </summary>
        public Guid SessionId { get; private set; }

        /// <summary>
        /// The Navigation Property for the Session
        /// </summary>
        public Session Session { get; private set; }
    }
}