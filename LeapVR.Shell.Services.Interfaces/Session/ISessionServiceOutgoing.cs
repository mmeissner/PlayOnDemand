#region Licence
/****************************************************************
 *  Filename: ISessionServiceOutgoing.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-14
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Services.Interfaces.Exceptions;

namespace LeapVR.Shell.Services.Interfaces.Session
{
    /// <summary>
    /// Outgoing service for Station-Server communication releated to billing session keeping.
    /// </summary>
    public interface ISessionServiceOutgoing
    {
        /// <summary>
        /// Notifies the server that there is Anonymous Session intent on this Station.
        /// </summary>
        /// <returns>Server reply indicating success or failure of intent.</returns>
        IntendAnonymousSessionResult IntendAnonymousSession();

        /// <summary>
        /// Informs server about <see cref="ILoginDecisionResult"/> in reponse to <see cref="ILoginIntention"/>.
        /// </summary>
        /// <param name="decision">Decision made</param>
        /// <returns><see cref="ILoginDecisionResult"/></returns>
        /// <exception cref="GrpcConnectionException">Thrown when connection with GRPC server fails</exception>
        /// <exception cref="GrpcUnexpectedCodeException">Thrown when server returns non expected error code.</exception>
        ILoginDecisionResult MakeLoginDecision(ILoginDecision decision);

        /// <summary>
        /// When network connectivity is lost and restored, it it used to re-synchronise with the server by discarding any pending Login Intention.
        /// </summary>
        void DiscardLoginIntention();

        /// <summary>
        /// Performs Long-call the server listening for new LoginIntention.
        /// </summary>
        /// <returns>LoginIntention if server pushed one, or null and flag isTImeouted = true if long-call timeouted.</returns>
        Task<(ILoginIntention Intention, bool isTimeouted)> LongGetLogInIntentionAsync();

        /// <summary>
        /// Notifies the server that client recieved LoginIntention.
        /// </summary>
        /// <param name="intention"></param>
        void AckLoginIntention(ILoginIntention intention);

        /// <summary>
        /// Gets currently running <see cref="ISessionState"/> for station from the server.
        /// </summary>
        /// <returns><see cref="ISessionState"/></returns>
        /// <exception cref="GrpcConnectionException">Thrown when connection with GRPC server fails</exception>
        /// <exception cref="GrpcUnexpectedCodeException">Thrown when server returns non expected error code.</exception>
        ISessionState GetSession();

        /// <summary>
        /// Heartbeats server about currently running session and gets current <see cref="ISessionState"/>.
        /// </summary>
        /// <returns><see cref="ISessionState"/></returns>
        /// <exception cref="GrpcConnectionException">Thrown when connection with GRPC server fails</exception>
        /// <exception cref="GrpcUnexpectedCodeException">Thrown when server returns non expected error code.</exception>
        ISessionState PingSession();

        /// <summary>
        /// Stops currently running session and gets current <see cref="ISessionState"/>.
        /// </summary>
        /// <param name="reason">Reason why Station is stopping the current Session.</param>
        /// <returns><see cref="ISessionState"/></returns>
        /// <exception cref="GrpcConnectionException">Thrown when connection with GRPC server fails</exception>
        /// <exception cref="GrpcUnexpectedCodeException">Thrown when server returns non expected error code.</exception>
        ISessionState StopSession(SessionStopReason reason);
    }
}
