#region Licence
/****************************************************************
 *  Filename: SessionDetails.cs
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
using Pod.Data.Infrastructure;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Holds Session related information and act as aggregation root for sessions
    /// If a station has a currently running session, it will be tracked here.
    /// </summary>
    public class SessionDetails
    {
        public static readonly TimeSpan DefaultTimeoutLoginRequestDelivery = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan MaximumTimeoutLoginRequestDelivery = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan MinimumTimeoutLoginRequestDelivery = TimeSpan.FromSeconds(2);

        public static readonly TimeSpan DefaultUserTimeForLoginRequestResponse = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan DefaultTimeoutLoginRequestResponse = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan MaximumTimeoutLoginRequestResponse = TimeSpan.FromSeconds(360);
        public static readonly TimeSpan MinimumTimeoutLoginRequestResponse = TimeSpan.FromSeconds(2);

        private SessionDetails() { }
        internal SessionDetails(Station station)
        {
            Station = station;
            TimeoutLoginRequestDelivery = DefaultTimeoutLoginRequestDelivery;
            TimeoutLoginRequestResponse = DefaultTimeoutLoginRequestResponse;
            UserTimeForLoginRequestResponse = DefaultUserTimeForLoginRequestResponse;
        }

        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Time interval in that a station needs to pickup a LoginIntention
        /// otherwise the session will be considered timed out
        /// </summary>
        public TimeSpan TimeoutLoginRequestDelivery { get; private set; }

        /// <summary>
        /// The Time interval in that a session needs to be confirmed or canceled
        /// otherwise the session will be considered timed out
        /// </summary>
        public TimeSpan TimeoutLoginRequestResponse { get; private set; }

        /// <summary>
        /// The Time Interval a User will get to Confirm or Cancel a Session,
        /// This need to be lower then the  <see cref="TimeoutLoginRequestResponse"/>
        /// </summary>
        public TimeSpan UserTimeForLoginRequestResponse { get; private set; }

        /// <summary>
        /// The Station Id this instance belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// Navigation Property for the Station
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// Navigation Property for the Session
        /// </summary>
        public Session Session { get; private set; }

        /// <summary>
        /// The Session Id 
        /// </summary>
        public Guid? SessionId { get; private set; }


        /// <summary>
        /// Requests a new Session
        /// </summary>
        /// <param name="source">The Source Type that requests this new session</param>
        /// <param name="sourceIpAddress">The Ip the request comes from</param>
        /// <param name="requestRef">A reference that should be added to the new session</param>
        /// <returns></returns>
        public IResult<SessionResponse> RequestSession(RequestSource source, string sourceIpAddress,string requestRef = null)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));
            result.ArgNotEnum(typeof(RequestSource), source, RequestSource.Undefined, nameof(source));
            result.ArgNotNullOrWhitespace(sourceIpAddress, nameof(source));
            result.ValueNotEqual(StationId, nameof(StationId), Guid.Empty);

            if(result.HasError()) return result;

            if(HasSession())
            {
                if(Session.IsPreRunTimeOut(TimeoutLoginRequestDelivery, TimeoutLoginRequestResponse))
                {
                    ClearStates();
                }
                else return result.Add(SessionResponse.StateMismatch);
            }
            Session = new Session(StationId, source, sourceIpAddress,requestRef);
            return result.Add(SessionResponse.Success);
        }

        /// <summary>
        /// Pickup/Delivery request that is normally send by a Station to confirm
        /// that this session has been noticed and is going to be processed on the station
        /// </summary>
        /// <param name="connectionId">The connection Id from the Station that called this method</param>
        /// <returns>Result</returns>
        public IResult<SessionResponse> RequestDelivery(Guid connectionId)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs
            result.ArgNotEqual(connectionId, nameof(connectionId), Guid.Empty);
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));

            if(result.HasError()) return result;

            if(!HasSession()) return result.Add(SessionResponse.StateMismatch);
            result.Add(Session.RequestDelivery(connectionId, TimeoutLoginRequestDelivery, TimeoutLoginRequestResponse));
            if(result.ReturnValue == SessionResponse.Timeout)
            {
                ClearStates();
            }
            return result;
        }

        /// <summary>
        /// Sets or Cancels the session and is send by the station.
        /// When a session is accepted it is considered as started,
        /// if not then it's considered canceled
        /// </summary>
        /// <param name="connectionId">The connection Id from the Station that called this method</param>
        /// <param name="accepted">True if accepted, false if canceled</param>
        /// <returns>Result</returns>
        public IResult<SessionResponse> SetResponse(Guid connectionId, bool accepted)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs
            result.ArgNotEqual(connectionId, nameof(connectionId), Guid.Empty);
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));

            if(!HasSession()) return result.Add(SessionResponse.StateMismatch);
            var confirmationResult = Session.SetConfirmation(connectionId, accepted, TimeoutLoginRequestDelivery, TimeoutLoginRequestResponse);
            if(confirmationResult == SessionResponse.Success && !accepted ||
               confirmationResult == SessionResponse.Timeout)
            {
                ClearStates();
            }
            return result.Add(confirmationResult);
        }

        /// <summary>
        /// Request to an update of this sessions data
        /// </summary>
        /// <param name="source">The source type of the update</param>
        /// <param name="sourceIpAddress">The Ip from where the request was send</param>
        /// <param name="timeChange">The update of time requested</param>
        /// <param name="reference">A reference that will be added to the new created update</param>
        /// <returns>Result</returns>
        public IResult<(SessionResponse Response,ChangeRequest ChangeRequest)> RequestSessionChange(
                RequestSource source, string sourceIpAddress, TimeSpan timeChange,string reference = null)
        {
            var result = new Result<(SessionResponse Response, ChangeRequest ChangeRequest)>();
            //Validate State and Inputs
            result.ArgNotEnum(typeof(RequestSource), source, RequestSource.Undefined, nameof(source));
            result.ArgNotNullOrWhitespace(sourceIpAddress, nameof(source));
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));
            result.ArgNotEqual(timeChange, nameof(timeChange), TimeSpan.Zero, UserError.SessionInvalidTimeChange);

            //Must have a Session and it must be started
            if(!HasSession() || Session.State != SessionState.Started) return result.Add((SessionResponse.StateMismatch, null));

            //Must have Change Requests included
            if(!result.RefNotNull(Session.ChangeRequests, nameof(Session.ChangeRequests))) return result;

            result.Add(Session.AddChangeRequest(source, sourceIpAddress, timeChange,reference));
            if(Session.State != SessionState.Started)ClearStates();
            return result;
        }

        /// <summary>
        /// To End the current Session by an connected Station
        /// </summary>
        /// <param name="connectionId">The Clients connection Id that wants to end the session</param>
        /// <param name="reason">The provided reason for the end</param>
        /// <returns>result</returns>
        public IResult<SessionResponse> EndSession(Guid connectionId, StopReason reason)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs
            result.ArgNotEnum(typeof(StopReason), reason, StopReason.Unknown, nameof(reason));
            result.ArgNotEqual(connectionId, nameof(connectionId), Guid.Empty);
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));

            if(!HasSession()) return result.Add(SessionResponse.StateMismatch);
            result.Add(Session.EndSession(connectionId, reason));
            ClearStates();
            return result;
        }

        /// <summary>
        /// Ends the current session from an endpoint that is not the itself station
        /// </summary>
        /// <param name="reason">The provided reason for the end</param>
        /// <returns>result</returns>
        public IResult<SessionResponse> EndSession(StopReason reason)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs
            result.ArgNotEnum(typeof(StopReason), reason, StopReason.Unknown, nameof(reason));
            result.RefNotNullIfExist(Session, SessionId, nameof(Session));

            if (!HasSession()) return result.Add(SessionResponse.StateMismatch);
            result.Add(Session.EndSession(reason));
            ClearStates();
            return result;
        }

        /// <summary>
        /// Process a Connection Request Response to handle this connection request response and set States
        /// for this session. This is due to that the connection request does indicate an problem or connection interruption on the
        /// client side. This must be handled and might require to close a session
        /// </summary>
        /// <param name="addClosedConnection"></param>
        /// <param name="connectionResponse"></param>
        /// <returns>true if saving to the Database is required, false if not</returns>
        public bool HandleConnectResponse(Action<ClosedConnection> addClosedConnection, ConnectionState.ConnectionRequestResponse connectionResponse)
        {
            bool requiresSave = false;
            if(connectionResponse.CloseLastSessionIfExist)
            {
                if(SessionId != null)
                {
                    requiresSave = true;
                    if(Session.State == SessionState.Started)
                    {
                        Session.EndSession(StopReason.ConnectionLoss);
                    }
                    ClearStates();
                }
            }

            if(connectionResponse.HasClosedConnection)
            {
                requiresSave = true;
                addClosedConnection(connectionResponse.ClosedConnection);
            }
            return requiresSave;
        }

        /// <summary>
        /// Set Custom values for several time intervals
        /// </summary>
        /// <param name="deliveryTimeoutAfter">The Time interval in that a station needs to pickup a LoginIntention</param>
        /// <param name="userTimeForResponse">The Time Interval a User will get to Confirm or Cancel a Session</param>
        /// <param name="responseTimeoutAfter">The Time interval in that a session needs to be confirmed or canceled</param>
        /// <returns>result</returns>
        public IResult SetIntentionTimeouts(
                TimeSpan deliveryTimeoutAfter,
                TimeSpan userTimeForResponse,
                TimeSpan responseTimeoutAfter)
        {
            var result = new Result<SessionResponse>();
            //Validate State and Inputs

            result.ArgNotHigherThen(
                    deliveryTimeoutAfter,
                    nameof(deliveryTimeoutAfter),
                    MaximumTimeoutLoginRequestDelivery,
                    nameof(MaximumTimeoutLoginRequestDelivery));
            result.ArgNotLowerThen(
                    deliveryTimeoutAfter,
                    nameof(deliveryTimeoutAfter),
                    MinimumTimeoutLoginRequestDelivery,
                    nameof(MinimumTimeoutLoginRequestDelivery));
            result.ArgNotHigherThen(
                    responseTimeoutAfter,
                    nameof(responseTimeoutAfter),
                    MaximumTimeoutLoginRequestResponse,
                    nameof(MaximumTimeoutLoginRequestResponse));
            result.ArgNotLowerThen(
                    responseTimeoutAfter,
                    nameof(responseTimeoutAfter),
                    MinimumTimeoutLoginRequestResponse,
                    nameof(MinimumTimeoutLoginRequestResponse));
            result.ArgNotLowerThen(
                    responseTimeoutAfter,
                    nameof(responseTimeoutAfter),
                    userTimeForResponse,
                    nameof(userTimeForResponse));
            if(result.HasError()) return result;

            TimeoutLoginRequestDelivery = deliveryTimeoutAfter;
            TimeoutLoginRequestResponse = responseTimeoutAfter;
            UserTimeForLoginRequestResponse = userTimeForResponse;
            return result;
        }

        /// <summary>
        /// Checks if there is a Session currently running
        /// </summary>
        /// <returns></returns>
        private bool HasSession() { return SessionId != null; }

        /// <summary>
        /// Unlink the Session
        /// </summary>
        private void ClearStates()
        {
            SessionId = null;
            Session = null;
        }
    }
}