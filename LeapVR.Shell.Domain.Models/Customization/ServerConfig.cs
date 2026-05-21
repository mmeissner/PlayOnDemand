#region Licence
/****************************************************************
 *  Filename: ServerConfig.cs
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
using NLog;

namespace LeapVR.Shell.Domain.Models.Customization
{
    /// <summary>
    /// Connect-server endpoint + optional server-root-CA path for TLS verification.
    /// Production deployments use a Let's Encrypt cert signed by a publicly trusted
    /// root, so the Windows certificate store already trusts the chain and
    /// <see cref="GetServerRootCert"/> only needs to return a value for private/dev CAs.
    /// </summary>
    public interface IServerConfig
    {
        string ConnectServerHost { get; }
        uint ConnectServerPort { get; }
        string GetServerRootCert(string basePath);
    }

    public class ServerConfig : ConfigObject, IServerConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Hostname of the Pod.Web.Center connect endpoint. The kiosk's first call
        /// hits <c>ConnectHostService</c> on this host to obtain its shell-server
        /// assignment.
        /// </summary>
        public string ConnectServerHost { get; set; } = "localhost";

        /// <summary>
        /// Port the connect endpoint listens on. The Docker Compose deployment
        /// terminates TLS on 443 and serves both REST and gRPC on the same port.
        /// </summary>
        public uint ConnectServerPort { get; set; } = 443;

        /// <summary>
        /// Optional path (relative to <c>basePath</c>) to a PEM-encoded root
        /// CA. Leave empty when the server uses a publicly trusted (e.g. Let's
        /// Encrypt) certificate.
        /// </summary>
        public string ServiceRootCert { get; set; } = string.Empty;

        public string GetServerRootCert(string basePath)
        {
            if (string.IsNullOrWhiteSpace(ServiceRootCert)) return null;

            Logger.Debug("Try to read root certificate files");
            if (!ReadFileContent(basePath, ServiceRootCert, out var serverRootCert))
            {
                Logger.Error("Root certificate was null or empty!");
                return null;
            }

            return serverRootCert;
        }

        private static bool ReadFileContent(string baseDirectory, string filePath, out string fileContent)
        {
            fileContent = null;
            if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var resultPath = Path.Combine(baseDirectory, filePath);
            if (!File.Exists(resultPath))
            {
                Logger.Debug($"Try to read certificate file from: {resultPath}");
                return false;
            }

            try
            {
                fileContent = File.ReadAllText(resultPath);
                return !string.IsNullOrWhiteSpace(fileContent);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }
    }
}
