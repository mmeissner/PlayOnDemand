#region Licence
/****************************************************************
 *  Filename: ResponderHub.cs
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
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pod.Data.Infrastructure;

namespace Pod.Services
{

    /// <summary>
    /// Allows to wait for a certain response for a message/request send to an receiver/client
    /// </summary>
    public class StationResponseHub: ResponderHub<ClientResponse>
    {
        public StationResponseHub(ILogger<ResponderHub<ClientResponse>> logger) : base(logger) { }

        /// <summary>
        /// Allows to await an Request/Command from a Station
        /// </summary>
        /// <param name="stationId">The Station to await a request from</param>
        /// <param name="eventType">The command/request to await</param>
        /// <param name="receivedAfterDateTimeUtc">
        /// The DateTime UTC after the command has to be received.
        /// This is required as we first notify the client to do a certain action an then we going to listen.
        /// So we do need to buffer some received commands from an station. To certify that the command that was received
        /// is the one we were awaiting</param>
        /// <param name="timeout">The maximum amount of time to wait</param>
        /// <returns>The Result as Client response</returns>
        public async Task<ClientResponse> WaitForResponse(Guid stationId,ClientRequestType eventType,DateTime receivedAfterDateTimeUtc,TimeSpan timeout)
        {
            return await base.WaitForResponse(
                    response => 
                            response.StationId == stationId &&
                            response.EventType == eventType && 
                            response.CreatedUtc >= receivedAfterDateTimeUtc,
                    () => new ClientResponse(stationId, eventType),
                    timeout);
        }
    }

    /// <summary>
    /// Allows to Wait for an Response of <typeparam name="T">The Message type published</typeparam>
    /// Has an message replay buffer of 2 seconds that means if an response is detected it might also stale
    /// How
    /// </summary>
    /// <typeparam name="T">The type published and awaited</typeparam>
    /// TODO: Evaluate the possibility to provide a Func to WaitForResponse and to execute commands in the sequence of Observe(without await), call func, await Observe. This might remove the need for an replay buffer
    public abstract class ResponderHub<T>
    {
        private readonly ILogger<ResponderHub<T>> _logger;
        /// <summary>
        /// A service needs to first send an request to an client and can then start to await an response
        /// This means that a response could arrive before the service started to await it
        /// For that reason we need to use a ReplaySubject with a time buffer
        /// </summary>
        private readonly ReplaySubject<T> _messageReceivedSubject = new ReplaySubject<T>(TimeSpan.FromSeconds(2));

        protected ResponderHub(ILogger<ResponderHub<T>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Publish(T message)
        {
            _messageReceivedSubject.OnNext(message);
        }

        /// <summary>
        /// Waits for a message that fulfills the specified predicate.
        /// </summary>
        /// <param name="wherePredicate">The condition T must fulfill.</param>
        /// <param name="timeoutVal">The function returning an instance of T in case of an timeout.</param>
        /// <param name="maxTime">The maximum time to wait before an timeout occurs.</param>
        /// <returns></returns>
        protected async Task<T> WaitForResponse(Func<T,bool> wherePredicate,Func<T> timeoutVal, TimeSpan maxTime) 
        {
            try
            {

                return await _messageReceivedSubject.ObserveOn(Scheduler.Default).Where(wherePredicate).Timeout(maxTime).TakeUntil(wherePredicate).FirstAsync();
                
            }
            catch(TimeoutException)
            {
                return timeoutVal.Invoke();
            }
        }
    }


    /// <summary>
    /// Represents a awaited Response related to an Request that was made to the Client
    /// </summary>
    public class ClientResponse
    {
        public ClientResponse(Guid stationId, ClientRequestType eventType)
        {
            CreatedUtc = DateTime.UtcNow;
            StationId = stationId;
            EventType = eventType;
            IsTimeoutResponse = true;
        }
        public ClientResponse(Guid stationId,ClientRequestType eventType, IResult response)
        {
            CreatedUtc = DateTime.UtcNow;
            StationId = stationId;
            ResponseForClient = response;
            EventType = eventType;
            IsTimeoutResponse = false;
        }
        /// <summary>
        /// DateTime the Response was created
        /// </summary>
        public DateTime CreatedUtc { get; }

        /// <summary>
        /// Is true when the Request was not replied in the allowed time
        /// </summary>
        public bool IsTimeoutResponse { get; }

        /// <summary>
        /// The Station Id that is Response was received from
        /// </summary>
        public Guid StationId { get; }

        /// <summary>
        /// The received Command/Request from the client
        /// </summary>
        public ClientRequestType EventType { get; }

        /// <summary>
        /// The Result that is send to the Client as Response to his request
        /// </summary>
        public IResult ResponseForClient  { get;  }
    }

    /// <summary>
    /// Collection of Requests that are received from an Client
    /// </summary>
    public enum ClientRequestType
    {
        /// <summary>
        /// Invalid request
        /// </summary>
        Unset,
        /// <summary>
        /// Client Requested to Update Heartbeat
        /// </summary>
        SetHeartbeat,
        /// <summary>
        /// Client Requested the Client Settings from Server
        /// </summary>
        GetClientSettings,
        /// <summary>
        /// Client Requested a Login
        /// </summary>
        RequestClientLogin,
        /// <summary>
        /// Client Requested to provide a pending Login Intention
        /// </summary>
        GetLoginIntention,
        /// <summary>
        /// Client Provided a Login Response for an Intention
        /// </summary>
        SetLoginResponse,
        /// <summary>
        /// Client Requested the current Session information from the Server
        /// </summary>
        GetSessionState,
        /// <summary>
        /// Client Requested a Logout from the current Session
        /// </summary>
        RequestLogout,
        /// <summary>
        /// Client Requested a Connect
        /// </summary>
        RequestConnect,
        /// <summary>
        /// Client Informed about a Disconnect
        /// </summary>
        RequestDisconnect,
        /// <summary>
        /// Client Requested an Application Synchronization
        /// </summary>
        AppsSyncRequest,
        /// <summary>
        /// Client provides application data for an synchronization
        /// </summary>
        AppsSync,
        /// <summary>
        /// Client informs about an Uninstalled application
        /// </summary>
        AppUninstalled,
        /// <summary>
        /// Client informs about an installed application
        /// </summary>
        AppInstalled,
        /// <summary>
        /// Client informs about an updated application
        /// </summary>
        AppUpdated,
    }
}
