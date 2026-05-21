#region Licence
/****************************************************************
 *  Filename: IUISession.cs
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
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages {
    public interface IUISession : IUISessionData
    {
        /// <summary>
        /// Fired when session data has changed, and provides all time latest state on subscription
        /// </summary>
        IObservable<IUISession> WhenSessionUpdated { get; }
        
        /// <summary>
        /// Request session to stop.
        /// </summary>
        void RequestStopSession(SessionStopReason reason);
    }

    /// <summary>
    /// Represents just the Data for the Session and should be clone able
    /// </summary>
    public interface IUISessionData
    {
        /// <summary>
        /// A unique Session Id provided from the Server
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// The Session Type that is related to the Session Rate
        /// The Type indicates if a Rate is present
        /// </summary>
        SessionType Type { get; }
        /// <summary>
        /// (UTC) Time the session was started.
        /// </summary>
        DateTime Started { get; }

        /// <summary>
        /// (UTC) Time the session was stopped (null if still running).
        /// </summary>
        DateTime? Stopped { get; }

        /// <summary>
        /// Reason of stopping the Session (if stopped). Null if session still running.
        /// </summary>
        SessionStopReason? StopReason { get; }

        /// <summary>
        /// Billing rate of current session.
        /// </summary>
        ISessionRate SessionRate { get; }
    }
}