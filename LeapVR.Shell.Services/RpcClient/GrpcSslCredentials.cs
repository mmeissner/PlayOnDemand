#region Licence
/****************************************************************
 *  Filename: GrpcSslCredentials.cs
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
using LeapVR.Shell.Domain.Models.Customization;
using NLog;

namespace LeapVR.Shell.Services.RpcClient
{
    /// <summary>
    /// Carries the optional server-root-CA PEM blob used to verify the gRPC server.
    /// Production deployments hit a Let's Encrypt-signed endpoint and leave
    /// <see cref="ServerRootCert"/> null (system trust store does the job).
    /// </summary>
    public class GrpcSslCredentials
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private GrpcSslCredentials(string serverRootCert)
        {
            ServerRootCert = serverRootCert;
        }

        /// <summary>
        /// The Root CA Certificate. Null when the kiosk talks to a server with a
        /// publicly trusted (e.g. Let's Encrypt) certificate.
        /// </summary>
        public string ServerRootCert { get; }

        public static GrpcSslCredentials Create(string basePath, IServerConfig serverConfig)
        {
            try
            {
                return new GrpcSslCredentials(serverConfig.GetServerRootCert(basePath));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Error during read of certificate files!");
                return null;
            }
        }
    }
}
