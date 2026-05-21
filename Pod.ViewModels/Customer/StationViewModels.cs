#region Licence
/****************************************************************
 *  Filename: StationViewModels.cs
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
using Pod.Enums;

namespace Pod.ViewModels.Customer
{
    /// <summary>
    /// Running Session on an Station
    /// </summary>
    public class SessionViewModel
    {
        /// <summary>
        /// The Id of the Session
        /// </summary>
        public Guid SessionId { get; set; }
        /// <summary>
        /// Reference provided by Session creator
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// The State of the Session
        /// </summary>
        public SessionState State { get; set; }
        /// <summary>
        /// The DateTime a Session was started or null if not yet started
        /// </summary>
        public DateTime? StartedOnUtc { get; set; }
        /// <summary>
        /// The Duration with that a session was started or null if the session was not limited by time on start
        /// </summary>
        public TimeSpan? StartDuration { get; set; }
        /// <summary>
        /// The current Duration or null if there is no limit
        /// </summary>
        public TimeSpan? MaxDurationLimit { get; set; }
    }

    /// <summary>
    /// An logged session for an Station
    /// </summary>
    public class SessionLogViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }
        /// <summary>
        /// The Session Id
        /// </summary>
        public Guid SessionId { get; set; }
        /// <summary>
        /// The entity that requested this session
        /// </summary>
        public RequestSource RequestedBy { get; set; }
        /// <summary>
        /// The last known State of the Session
        /// </summary>
        public SessionState LatestState { get; set; }
        /// <summary>
        /// The DateTime the session was stared or null if it was never started
        /// </summary>
        public DateTime? StartedUtc { get; set; }
        /// <summary>
        /// A reference that can be provided during a request for a session through an API
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// The maximum duration this session was allowed to run
        /// </summary>
        public TimeSpan? MaxDurationLimit { get; set; }
        /// <summary>
        /// The DateTime the session was declared as Ended
        /// </summary>
        public DateTime? EndedUtc { get; set; }
        /// <summary>
        /// The cause of the stop of the session
        /// </summary>
        public StopReason? StoppedBy { get; set; }
        /// <summary>
        /// The Change requests received for this session
        /// </summary>
        public IEnumerable<ChangeRequestViewModel> ChangeRequests { get; set; }
    }

    /// <summary>
    /// An request to change an Session
    /// </summary>
    public class ChangeRequestViewModel
    {
        /// <summary>
        /// The Change Requests Id
        /// </summary>
        public Guid Id { get;set; }
        /// <summary>
        /// The DateTime the request was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// The provided reference for this change request
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// The requested Time Change for the Duration of the Session 
        /// </summary>
        public TimeSpan TimeChange { get; set; }
    }

    /// <summary>
    /// Represents the current state of the Station
    /// </summary>
    public class StationCurrentStateViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// The current Name of the Station
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The current Mode the station is set to
        /// </summary>
        public StationControlMode ControlMode { get; set; }

        /// <summary>
        /// The current Network state of the station
        /// </summary>
        public NetworkState? NetworkState { get; set; }

        /// <summary>
        /// The current Session running on the server or null if no session is running
        /// </summary>
        public SessionViewModel Session { get; set; }
    }

    /// <summary>
    /// Response for a new created Session
    /// </summary>
    public class CreatedSessionViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }
        /// <summary>
        /// The Id of the new Session
        /// </summary>
        public Guid SessionId { get; set; }

    }

    /// <summary>
    /// Response for a new created Session
    /// </summary>
    public class UpdatedSessionViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }
        /// <summary>
        /// The Id of the new Session
        /// </summary>
        public Guid SessionId { get; set; }
        /// <summary>
        /// The Id of the new Session
        /// </summary>
        public Guid ChangeRequestId { get; set; }

    }

    public class StoppedSessionViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }
        /// <summary>
        /// The Id of the new Session
        /// </summary>
        public Guid SessionId { get; set; }
    }

    /// <summary>
    /// The Station Settings
    /// </summary>
    public class StationSettingsViewModel
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// The Name of the Station
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The currently set QR-Code set for the station
        /// This QR Code is only displayed in <see cref="StationControlMode.RemoteWithQrCode"/>
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// The current operation mode of the station
        /// </summary>
        public StationControlMode ControlMode { get; set; }
    }

    /// <summary>
    /// Request for an new Session
    /// </summary>
    public class StationSessionRequest
    {
        /// <summary>
        /// The Station Id
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// The Reference provided for the session
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// The Duration with that the session was requested or null if there is no time limit
        /// </summary>
        public TimeSpan? Duration { get; set; }

    }

    /// <summary>
    /// Request for an Update of the currently running session
    /// </summary>
    public class StationSessionUpdateRequest
    {        
        /// <summary>
        /// The Reference for the Update Request
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// The Timespan for the change of the Duration
        /// This time will be added or subtract from the current duration
        /// If no Duration exists then this will set an initial duration from the current point in time
        /// </summary>
        public TimeSpan Duration { get; set; }

    }

    /// <summary>
    /// Log entry for an station server connection
    /// </summary>
    public class StationConnectionLogViewModel
    {
        /// <summary>
        /// The Id of the entry
        /// </summary>
        public Guid Id { get;  set; }

        /// <summary>
        /// The Server Id the station connected to
        /// </summary>
        public Guid ServerId { get; set; }

        /// <summary>
        /// The Connection Id assigned by the connect server
        /// </summary>
        public Guid ConnectionId { get; set; }

        /// <summary>
        /// The Device Identity of the connecting client
        /// </summary>
        public string DeviceIdentityId { get; set; }

        /// <summary>
        /// The DateTime the Client Requested a Server
        /// </summary>
        public DateTime RequestedServerOnUtc { get; set; }

        /// <summary>
        /// The DateTime the Client connected to a Server
        /// </summary>
        public DateTime? ConnectedToServerOnUtc { get; set; }

        /// <summary>
        /// The DateTime the client was considered Disconnected
        /// </summary>
        public DateTime DisconnectedOnUtc { get; set; }

        /// <summary>
        /// The cause of the Disconnect
        /// </summary>
        public ConnectionClosedBy ClosedBy { get; set; }
    }

    /// <summary>
    /// An ApiKey for an Station
    /// </summary>
    public class StationApiKeyViewModel
    {
        /// <summary>
        /// Creation Date Time for this ApiKey
        /// </summary>
        public DateTime CreateOnUtc { get; set; }
        /// <summary>
        /// A name that can be given for each API Key
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The public key component
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// The secret for HMAC signing and verification
        /// </summary>
        public string Secret { get; set; }
    }
}
