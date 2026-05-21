#region Licence
/****************************************************************
 *  Filename: RpcHostDetails.cs
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

namespace Pod.Grpc.Base.Client
{
    public class RpcHostDetails : IRpcHostDetails
    {
        protected RpcHostDetails(string serverHost, uint port)
        {
            ServerHost = serverHost;
            ServerPort = port;
        }
        public string ServerHost { get; }
        public uint ServerPort { get;}

        public static RpcHostDetails Create(string serverHost, uint port)
        {
            var retval = new RpcHostDetails(serverHost, port);
            retval.Verify();
            return retval;
        }
        protected void Verify()
        {
            if (ServerPort <= 0 || ServerPort > 65535)
                throw new ArgumentException($"The provided: '{ServerPort}' is not a valid port for socket communication!");
        }
    }

    public class RpcShellHostDetails : RpcHostDetails, IRpcShellHostDetails
    {
        protected RpcShellHostDetails(string serverHost, uint port, Guid connectionId) : base(serverHost, port)
        {
            ConnectionId = connectionId;
        }
        public Guid ConnectionId { get;}

        public static RpcShellHostDetails Create(string serverHost, uint port, Guid connectionId)
        {
            var retval = new RpcShellHostDetails(serverHost, port,connectionId);
            retval.Verify();
            return retval;
        }
        private new void Verify()
        {
            if(ConnectionId == Guid.Empty)
            {
                throw new ArgumentException($"The provided: '{ConnectionId}' is empty and not valid!");
            }
            base.Verify();
        }
    }

    public interface IRpcHostDetails
    {
        string ServerHost { get; }
        uint ServerPort { get; }
    }

    public interface IRpcShellHostDetails : IRpcHostDetails
    {
        Guid ConnectionId { get; }
    }
}
