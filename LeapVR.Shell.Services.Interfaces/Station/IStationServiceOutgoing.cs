#region Licence
/****************************************************************
 *  Filename: IStationServiceOutgoing.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-29
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
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Services.Interfaces.Station
{
    /// <summary>
    /// Outgoing service for Station-Server communication releated to network communication.
    /// </summary>
    public interface IStationServiceOutgoing : IDisposable
    {
        /// <summary>
        /// Flag indicating if connection to Host server has been established.
        /// </summary>
        bool IsGrpcHostConnected { get; }

        /// <summary>
        /// Establishes connection to Connect server (server that is deciding on which Host server to connect).
        /// </summary>
        /// <param name="grpcHostAddress"></param>
        /// <param name="grpcHostPort"></param>
        void ConnectGrpcHostServer(string grpcHostAddress, int grpcHostPort);

        /// <summary>
        /// Tries to call Connect server to obtain <see cref="grpcHostAddress"/> and <see cref="grpcHostPort"/> of Host server.
        /// </summary>
        /// <param name="isGrpcConnectReachable">Indicates of Connect server is reachable (recieves requests and sends responses).</param>
        /// <param name="responseStatusCode">Indicates GRPC Status code that call resulted in.</param>
        /// <param name="grpcHostAddress">If call suceeded holds address of Host Server assigned to station.</param>
        /// <param name="grpcHostPort">If call suceeded holds port of Host server assigned to station.</param>
        /// <returns>Boolean indicating if call completly suceeded.</returns>
        bool TryCallGrpcConnectServer(out bool isGrpcConnectReachable, out int responseStatusCode, out string grpcHostAddress, out int grpcHostPort);

        /// <summary>
        /// Tries to get new details about current station. Server will return status code `NoChanges` if no data changed since last call.
        /// </summary>
        /// <param name="stationDisplayName"></param>
        /// <param name="locationDisplayName"></param>
        /// <param name="platformDisplayName"></param>
        /// <param name="deviceSerialnumber"></param>
        /// <returns>Bool indicating if call completly suceeded.</returns>
        bool TryGetStationDetails(out string stationDisplayName, out string locationDisplayName, out string platformDisplayName, out string deviceSerialnumber);

        /// <summary>
        /// Tries to notify server that station has came online.
        /// Doesn't throw exceptions.
        /// </summary>
        /// <param name="recommendedPingInterval">Server recommendation on interval of network ping.</param>
        /// <param name="timeoutPingInterval">Time server waits for valid ping until it assumes Station went offline unexpectedly.</param>
        /// <returns>Boolean indicating if call was successful.</returns>
        bool TryNotifyStarted(out bool isGrpcConnected, out bool isGrpcHostReachable, out int responseStatusCode, out TimeSpan recommendedPingInterval, out TimeSpan timeoutPingInterval);

        /// <summary>
        /// Tries to ping the server to check network status.
        /// Doesn't throw exceptions.
        /// </summary>
        /// <param name="recommendedPingInterval">Currently used ping interval</param>
        /// <param name="newRecommendedPingInterval">New server recommendation on interval of network ping. Null means no changes.</param>
        /// <param name="newTimeoutPingInterval">New time server waits for valid ping until it assumes Station went offline unexpectedly. Null means no changes.</param>
        /// <returns>Boolean indicating if call was successful.</returns>
        bool TryPing(out bool isGrpcConnected, out bool isGrpcHostReachable, out int responseStatusCode, TimeSpan recommendedPingInterval, out TimeSpan? newRecommendedPingInterval, out TimeSpan? newTimeoutPingInterval);

        /// <summary>
        /// Tries to notify server that station is going offline.
        /// Doesn't throw exceptions.
        /// </summary>
        /// <returns>Boolean indicating if call was successful.</returns>
        bool TryNotifyStoped(out bool sGrpcConnected, out bool isGrpcHostReachable, out int responseStatusCode);

        ISessionSettings GetStationSessionSetup();

        Task<(bool IsGrpcConnected, bool IsGrpcHostReachable, bool IsErrorResponse, bool IsLongCallTimeouted, IStationUpdateMessage ServerPushMessage)> TryLongGetServerPushMessage(CancellationToken ct);
    }
}
