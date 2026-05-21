#region Licence
/****************************************************************
 *  Filename: GrpcChannelCredentialsHandler.cs
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
using System.IO;
using Grpc.Core;
using Grpc.Core.Internal;
using Grpc.Core.Utils;
using NLog;
using Pod.Grpc.Base.Const;

namespace Pod.Grpc.Base.Client
{
    /// <summary>
    /// Creates Grpc Channel Credentials
    /// Can create Credentials with SSL only or SSL with Client Certificate
    /// Includes CallCredentials if set
    /// Requires at least a Root Certificate 
    /// </summary>
    public class GrpcChannelCredentialsHandler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string _identity;
        private string _password;
        private bool _hasChannelCredentials;
        GrpcChannelCredentialsHandler(string serverRootCert, string clientCertChain, string clientPrivateKey)
        {
            ServerRootCert = serverRootCert;
            ClientCertChain = clientCertChain;
            ClientPrivateKey = clientPrivateKey;
        }
        private string ServerRootCert { get; }
        private string ClientCertChain { get; }
        private string ClientPrivateKey { get; }
        public void SetChannelCredentials(string identity, string password)
        {
            _hasChannelCredentials = true;
            _identity = identity;
            _password = password;
        }

        /// <summary>
        /// Builds the Channel Credentials from SSLCredentials and CallCredentials
        /// If no ClientCert or ClientPrivate Key is provided, then the Client will not send a cert to the Server
        /// </summary>
        /// <returns></returns>
        public ChannelCredentials GetCredentials()
        {
            KeyCertificatePair keypair = null;
            SslCredentials sslCredentials = null;
            if (!string.IsNullOrEmpty(ClientCertChain) && !string.IsNullOrEmpty(ClientPrivateKey))
            {
                keypair = new KeyCertificatePair(ClientCertChain, ClientPrivateKey);
            }
            //Creates SSL Credentials for Server SSL
            if(keypair != null)
            {
                sslCredentials = new SslCredentials(ServerRootCert, keypair);
            }
            //Creates SSL Credentials for Client SSL
            else
            {
                sslCredentials = new SslCredentials(ServerRootCert);
            }
            if (!_hasChannelCredentials) return sslCredentials;
            var callCredentials = CallCredentials.FromInterceptor(
                //Sets the Client Credentials per Call as Metadata in the Header in each request
                    new AsyncAuthInterceptor(
                            (context, metadata) =>
                            {
                                metadata.Add(AuthConstants.ShellClientIdentityKey,_identity);
                                metadata.Add(AuthConstants.ShellClientPasswordKey,_password);
                                return TaskUtils.CompletedTask;
                            }));
            return ChannelCredentials.Create(sslCredentials, callCredentials);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverRootCert">The SSL Root Certificate to verify the Server</param>
        /// <param name="clientCertChain">Set for client side ssl</param>
        /// <param name="clientPrivateKey">Set for client side ssl</param>
        /// <returns></returns>
        public static GrpcChannelCredentialsHandler Create(string serverRootCert, string clientCertChain = null, string clientPrivateKey = null)
        {
            try
            {
                if (String.IsNullOrEmpty(serverRootCert))
                {
                    _logger.Error("No Root CA Certificate provided!");
                    return null;
                }
                if (String.IsNullOrEmpty(clientCertChain) || String.IsNullOrEmpty(clientPrivateKey))
                {
                    _logger.Debug("No Client Certificate provided, this will require a Client SSL connection");
                }
                return new GrpcChannelCredentialsHandler(serverRootCert, clientCertChain, clientPrivateKey);
            }
            catch(Exception exception)
            {
                _logger.Error(exception, "Error during read of certificate files!");
                return null;
            }
        }
    }
}