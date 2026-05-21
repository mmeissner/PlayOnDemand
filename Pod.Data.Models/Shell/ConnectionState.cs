#region Licence
/****************************************************************
 *  Filename: ConnectionState.cs
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
using Pod.Data.Infrastructure;
using Pod.Data.Models.Servers;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Provides Connection data for a Station and is the aggregation root for Connections
    /// </summary>
    public class ConnectionState
    {
        private ConnectionState()
        {
            LoadedFromDatabaseUtc = DateTime.UtcNow;
        }
        internal ConnectionState(Station station)
        {
            Station = station;
            NetworkState = NetworkState.Disconnected;
        }
        /// <summary>
        /// The id of this instance
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// DateTime this instance was received from the Db
        /// </summary>
        public DateTime LoadedFromDatabaseUtc { get; }

        /// <summary>
        /// The current valid Network State of the Station
        /// </summary>
        public NetworkState NetworkState { get; private set; }

        /// <summary>
        /// The current valid DateTime a Server was requested
        /// </summary>
        public DateTime? ServerRequestOnUtc { get; private set; }

        /// <summary>
        /// The current valid unique ConnectionId identifying the last connection
        /// </summary>
        public Guid? ConnectionId { get; private set; }

        /// <summary>
        /// The current valid DateTime the Station Connected to a ShellServer 
        /// </summary>
        public DateTime? ConnectedOnUtc { get; private set; }

        /// <summary>
        /// The current valid last time a Heartbeat was received from this station
        /// </summary>
        public DateTime? LastHeartbeatOnUtc { get; private set; }

        /// <summary>
        /// The Device Identity of the Station owning the current connection
        /// </summary>
        public string DeviceIdentityId { get; private set; }

        /// <summary>
        /// Navigation Property to the Device Identity
        /// </summary>
        public DeviceIdentity DeviceIdentity { get; private set; }

        /// <summary>
        /// The current valid ShellServerId this Station should connect or is connected to
        /// </summary>
        public Guid? ShellServerId { get; private set; }

        /// <summary>
        /// The navigation property for the Shell Server
        /// </summary>
        public ShellServer ShellServer { get; private set; }

        /// <summary>
        /// The Station Id this instance belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// The Navigation Property for the Station this instance belongs to
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// Request to set the Connection state to Connecting 
        /// </summary>
        /// <param name="newAssignedServerId">The Server Id for the request</param>
        /// <param name="identityId">the device identity</param>
        /// <param name="reconnectConnectionId">The connectionId in case of reconnect after connection loss</param>
        /// <returns></returns>
        public IResult<ConnectionRequestResponse> RequestConnecting(Guid newAssignedServerId, string identityId, Guid? reconnectConnectionId = null)
        {
            var result = new Result<ConnectionRequestResponse>();
            //Validation Input Parameter
            result.ArgNotEqual(newAssignedServerId,nameof(newAssignedServerId),0,UserError.ConnectionInvalidParameter);
            result.ArgNotNullOrWhitespace(identityId,nameof(identityId),UserError.ConnectionInvalidParameter);
            result.RefNotNullIfExist(ShellServer,ShellServerId,nameof(ShellServer));

            if(result.HasError()) return result;
            //Evaluate Current State
            switch(NetworkState)
            {
                case NetworkState.Disconnected:
                    SetToConnecting(newAssignedServerId,identityId,Guid.NewGuid());
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success));
                case NetworkState.Connecting:
                    //Is it another Client that tries to Connect ?
                    if(DeviceIdentityId != identityId) 
                    {
                        if(IsConnectionAlive())return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.OtherDeviceIdentityConnected));
                    }
                    SetToConnecting(newAssignedServerId,identityId,Guid.NewGuid());
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success));
                case NetworkState.Connected:
                    //Is same client trying to connect, but still has Connected state
                    //This could happen if the ClientSoftware was ungracefully closed
                    //and could not inform the server about its disconnect
                    //or if server lost connection and tries to resume
                    ConnectionClosedBy reason = ConnectionClosedBy.Undefined;
                    Guid connectionId = Guid.NewGuid();
                    bool closeLastSessionIfExist = true;
                    if(IsConnectionAlive())
                    {
                        if(DeviceIdentityId != identityId)
                        {
                            return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.OtherDeviceIdentityConnected));
                        }
                        //We need to assign the same ConnectionId on a reconnect so that client can resume session operations
                        if(reconnectConnectionId.HasValue &&
                           ConnectionId.HasValue &&
                           reconnectConnectionId.Value == ConnectionId.Value)
                        {
                            reason = ConnectionClosedBy.Reconnect;
                            connectionId = ConnectionId.Value;
                            closeLastSessionIfExist = false;
                        }
                        //We need can provide a new ConnectionId and close remaining sessions as client must be closed ungracefully
                        else
                        {
                            reason = ConnectionClosedBy.UngracefulDisconnect;
                        }
                    }
                    else reason = ConnectionClosedBy.Timeout;
                    var createClosedConnection = ClosedConnection.Create(this,reason);
                    SetToConnecting(newAssignedServerId,identityId,connectionId);
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success,createClosedConnection,closeLastSessionIfExist));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Requests to set the Connection State to Connected
        /// </summary>
        /// <param name="connectedServerId">The Server Id requesting the switch</param>
        /// <param name="connectionId">The connection Id the Client provided</param>
        /// <returns></returns>
        public IResult<ConnectionRequestResponse> RequestConnected(Guid connectedServerId, Guid connectionId)
        {
            var result = new Result<ConnectionRequestResponse>();
            //Validation Input Parameter
            result.ArgNotEqual(connectedServerId,nameof(connectedServerId),0,UserError.ConnectionInvalidParameter);
            result.ArgNotEqual(connectionId,nameof(connectionId),Guid.Empty,UserError.ConnectionInvalidId);

            if(result.HasError()) return result;

            //Its the wrong server
            if(connectedServerId != ShellServerId)
            {
                return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ServerMismatch));
            }

            //Evaluate Response
            switch(NetworkState)
            {
                case NetworkState.Disconnected:
                    //Need to be in Connected or Connecting State
                    //The state could have been set automatically due to Timeout or actively
                    //In neither case the Station needs to request a new server
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.NetworkStateMismatch));
                case NetworkState.Connecting:
                    //Is not the connection currently handled
                    if(connectionId != ConnectionId)
                    {
                        return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionIdMismatch));
                    }
                    SetConnected();
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success));
                case NetworkState.Connected:
                    if(connectionId != ConnectionId)
                    {
                        return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionIdMismatch));
                    }
                    //The station is considered as connected but wants to set the state again
                    //Maybe its an reconnection attempt after a lost connection before the a timeout was reached
                    var createClosedConnection = ClosedConnection.Create(this,ConnectionClosedBy.Reconnect);
                    SetConnected();
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success,createClosedConnection,true));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Request to set the Connection State to Disconnected
        /// </summary>
        /// <param name="connectedServerId">The Server requesting the switch</param>
        /// <param name="connectionId">The connectionId provided by the client</param>
        /// <returns></returns>
        public IResult<ConnectionRequestResponse> RequestDisconnected(Guid connectedServerId, Guid connectionId)
        {
            var result = new Result<ConnectionRequestResponse>();
            //Validation Input Parameter
            result.ArgNotEqual(connectedServerId,nameof(connectedServerId),0,UserError.ConnectionInvalidParameter);
            result.ArgNotEqual(connectionId,nameof(connectionId),Guid.Empty,UserError.ConnectionInvalidId);

            //Its the wrong server
            if(connectedServerId != ShellServerId)
            {
                return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ServerMismatch));
            }

            switch(NetworkState)
            {
                case NetworkState.Disconnected:
                    //Already considered Disconnected
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.NetworkStateMismatch));
                case NetworkState.Connecting:
                    if(connectionId == ConnectionId)
                    {
                        //Client seems to try to set Disconnected state before he received initial state
                        //As the Connection Id confirms that its the client currently connecting we just reset the state and let him
                        SetToDisconnected();
                        return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success));
                    }
                    //If its not your connection you cant request to close it
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionIdMismatch));
                case NetworkState.Connected:
                    //If its not your connection you cant request to close it
                    if(connectionId != ConnectionId)return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionIdMismatch));
                    var createClosedConnection = ClosedConnection.Create(this,ConnectionClosedBy.Client);
                    SetToDisconnected();
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success,createClosedConnection,true));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Request to set the Last Heartbeat received value
        /// </summary>
        /// <param name="connectedServerId">The ServerId requesting the change</param>
        /// <param name="connectionId">The connectionId provided by the Client</param>
        /// <returns></returns>
        public IResult<ConnectionRequestResponse> RequestHeartbeat(Guid connectedServerId, Guid connectionId)
        {
            var result = new Result<ConnectionRequestResponse>();
            //Validation Input Parameter
            result.ArgNotEqual(connectedServerId,nameof(connectedServerId),0,UserError.ConnectionInvalidParameter);
            result.ArgNotEqual(connectionId,nameof(connectionId),Guid.Empty,UserError.ConnectionInvalidId);
            result.RefNotNullIfExist(ShellServer,ShellServerId,nameof(ShellServer));
            //Its the wrong server
            if(connectedServerId != ShellServerId)
            {
                return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ServerMismatch));
            }
            //Wrong ConnectionId
            if(!ConnectionId.HasValue || ConnectionId.Value != connectionId)
            {
                if(connectedServerId != ShellServerId)
                {
                    return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionIdMismatch));
                }
            }
            //Wrong Network State
            if(NetworkState != NetworkState.Connected)
            {
                return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.NetworkStateMismatch));
            }
            //Connection Deadline exceeded
            if(!IsConnectionAlive())
            {
                return result.Add(SetTimeout(false));
            }
            LastHeartbeatOnUtc = LoadedFromDatabaseUtc;
            return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.Success));
        }

        /// <summary>
        /// Request to Timeout this Connection and reset states
        /// </summary>
        /// <returns></returns>
        public IResult<ConnectionRequestResponse> RequestTimeout()
        {
            var result = new Result<ConnectionRequestResponse>();
            //Validation Input Parameter
            result.RefNotNullIfExist(ShellServer,ShellServerId,nameof(ShellServer));
            if(result.HasError()) return result;

            if(IsConnectionAlive())return result.Add(new ConnectionRequestResponse(ConnectionRequestResult.ConnectionStillAlive));
            return result.Add(SetTimeout(true));
        }

        private void SetToConnecting(Guid shellServerId, string deviceIdentityId, Guid connectionId)
        {
            NetworkState = NetworkState.Connecting;
            ServerRequestOnUtc = DateTime.UtcNow;
            ConnectionId = connectionId;
            ConnectedOnUtc = null;
            LastHeartbeatOnUtc = null;
            DeviceIdentityId = deviceIdentityId;
            ShellServerId = shellServerId;
        }
        private void SetConnected()
        {
            var now = DateTime.UtcNow;
            NetworkState = NetworkState.Connected;
            ConnectedOnUtc = now;
            LastHeartbeatOnUtc = now;
        }
        private void SetToDisconnected()
        {
            NetworkState = NetworkState.Disconnected;
            ServerRequestOnUtc = null;
            ConnectionId = null;
            ConnectedOnUtc = null;
            LastHeartbeatOnUtc = null;
            DeviceIdentityId = null;
            DeviceIdentity = null;
            ShellServer = null;
            ShellServerId = null;
        }
        private bool IsConnectionAlive() { return IsConnectionAlive(LoadedFromDatabaseUtc); }
        private bool IsConnectionAlive(DateTime checkAgainst)
        {
            switch(NetworkState)
            {
                case NetworkState.Disconnected:
                    return false;
                case NetworkState.Connecting:
                    // ReSharper disable once PossibleInvalidOperationException
                    if(ServerRequestOnUtc.Value.Add(ShellServer.ConnectTimeout) < checkAgainst)
                    {
                        return false;
                    }
                    return true;
                case NetworkState.Connected:
                    // ReSharper disable once PossibleInvalidOperationException
                    if(LastHeartbeatOnUtc.Value.Add(ShellServer.HeartbeatTimeout) < checkAgainst)
                    {
                        return false;
                    }
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private ConnectionRequestResponse SetTimeout(bool responseSuccessResult)
        {
            bool closeLastSessionIfExist = NetworkState == NetworkState.Connected;
            var createClosedConnection = ClosedConnection.Create(this,ConnectionClosedBy.Timeout);
            SetToDisconnected();
            var requestResult = responseSuccessResult ? ConnectionRequestResult.Success :
                    ConnectionRequestResult.ConnectionTimedOut;
            return new ConnectionRequestResponse(requestResult,createClosedConnection,closeLastSessionIfExist);
        } 
        public struct ConnectionRequestResponse
        {
            public ConnectionRequestResponse(ConnectionRequestResult result, bool closeLastSessionIfExist = false)
            {
                Result = result;
                HasClosedConnection = false;
                ClosedConnection = null;
                CloseLastSessionIfExist = closeLastSessionIfExist;
            }

            public ConnectionRequestResponse(ConnectionRequestResult result, ClosedConnection closedConnection,bool closeLastSessionIfExist = false)
            {
                Result = result;
                HasClosedConnection = true;
                CloseLastSessionIfExist = closeLastSessionIfExist;
                ClosedConnection = closedConnection;
            }
            public readonly ConnectionRequestResult Result;
            public readonly bool CloseLastSessionIfExist;
            public readonly bool HasClosedConnection;
            public readonly ClosedConnection ClosedConnection;
        }
    }

    /// <summary>
    /// Identifies a local client installation
    /// </summary>
    public class DeviceIdentity
    {
        private HashSet<ClosedConnection> _closedConnections;
        private HashSet<ApplicationRoot> _applicationRoots;

        private DeviceIdentity() { }
        private DeviceIdentity(string uniqueIdentity)
        {
            Id = uniqueIdentity;
            CreatedUtc = DateTime.UtcNow;
        }
        /// <summary>
        /// The Id of this Instance
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The DateTime this instance was created
        /// </summary>
        public DateTime CreatedUtc { get; private set; }

        /// <summary>
        /// The Connection State of this Instance
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// The ApplicationRoot for this instance
        /// </summary>
        public IReadOnlyCollection<ApplicationRoot> ApplicationRoots { get; private set; }

        /// <summary>
        /// All closed connections for this instance
        /// </summary>
        public IReadOnlyCollection<ClosedConnection> ClosedConnections => _closedConnections;

        /// <summary>
        /// Creates a new Instance
        /// </summary>
        /// <param name="uniqueIdentity"></param>
        /// <returns></returns>
        public static DeviceIdentity Create(string uniqueIdentity) { return new DeviceIdentity(uniqueIdentity); }
    }

    /// <summary>
    /// A Closed Connection 
    /// </summary>
    public class ClosedConnection
    {
        private ClosedConnection() { }
        private ClosedConnection(ConnectionState connectionState, ConnectionClosedBy closedBy)
        {
            // ReSharper disable PossibleInvalidOperationException
            ConnectionId = connectionState.ConnectionId.Value;
            //This value should be all time set otherwise there should be no Connection to close
            RequestedServerOnUtc = connectionState.ServerRequestOnUtc.Value;
            ConnectedToServerOnUtc = connectionState.ConnectedOnUtc;
            DeviceIdentityId = connectionState.DeviceIdentityId;
            DisconnectedOnUtc = DateTime.UtcNow;
            ServerId = connectionState.ShellServerId.Value;
            StationId = connectionState.StationId;
            ClosedBy = closedBy;
        }
        /// <summary>
        /// The Instance Id
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The unique ConnectionId that connection had
        /// </summary>
        public Guid ConnectionId { get; private set; }

        /// <summary>
        /// The DateTime the Server was requested
        /// </summary>
        public DateTime RequestedServerOnUtc { get; private set; }

        /// <summary>
        /// The DateTIme the Connection was established to the server
        /// </summary>
        public DateTime? ConnectedToServerOnUtc { get; private set; }

        /// <summary>
        /// The DateTime the connection was declared Closed/Disconnected
        /// </summary>
        public DateTime DisconnectedOnUtc { get; private set; }

        /// <summary>
        /// The Cause of the Disconnect
        /// </summary>
        public ConnectionClosedBy ClosedBy { get; private set; }

        /// <summary>
        /// The StationId the connection belongs to
        /// </summary>
        public Guid StationId { get; private set; }

        /// <summary>
        /// The NavigationProperty for the Station
        /// </summary>
        public Station Station { get;private set; }

        /// <summary>
        /// The Server this Connection was made to
        /// </summary>
        public Guid ServerId { get; private set; }

        /// <summary>
        /// The NavigationProperty for the Server
        /// </summary>
        public ShellServer Server { get; private set; } 

        /// <summary>
        /// The DeviceIdentityId this Connection belongs to
        /// </summary>
        public string DeviceIdentityId { get; private set; }

        /// <summary>
        /// The Navigation Property for the DeviceIdentity
        /// </summary>
        public DeviceIdentity DeviceIdentity { get; private set; }

        /// <summary>
        /// Creates a new ClosedConnection
        /// </summary>
        /// <param name="connectionState">The Connection State to create from</param>
        /// <param name="reason">The reason for the disconnect</param>
        /// <returns></returns>
        internal static ClosedConnection Create(ConnectionState connectionState, ConnectionClosedBy reason)
        {
            return new ClosedConnection(connectionState,reason);
        }
    }
}