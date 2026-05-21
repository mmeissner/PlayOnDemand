#region Licence
/****************************************************************
 *  Filename: GrpcServerConfig.cs
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
namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// Legacy config DTO that mapped <c>appsettings.json:GrpcServerConfig</c>
    /// onto the old <c>Grpc.Core.Server</c> hosted in
    /// <c>Pod.Web.Center.ServicesHosted.GrpcServicesServer</c> (see
    /// <see cref="GrpcServer"/> for migration notes).
    ///
    /// Kept intact so the existing <c>appsettings.json</c> section binds
    /// without throwing during the cutover. Most fields are now informational
    /// only — Kestrel handles host/port/TLS for the AspNetCore-hosted gRPC
    /// path; the cert paths in <see cref="SslCredentialFiles"/> should be
    /// migrated to <c>Kestrel:Endpoints:Https:Certificate</c> in
    /// <c>appsettings.json</c> as part of the Pod.Web.Center integration step.
    ///
    /// Once that integration lands, this class can be deleted (along with
    /// the <c>GrpcServerConfig</c> section in appsettings.json).
    /// </summary>
    public class GrpcServerConfig
    {
        /// <summary>
        /// The IP to listen for incoming connections; <c>0.0.0.0</c> = all.
        /// AspNetCore: configure under <c>Kestrel:Endpoints</c> instead.
        /// </summary>
        public string GrpcHost { get; set; } = "0.0.0.0";

        /// <summary>
        /// The port to listen for incoming connections.
        /// AspNetCore: configure under <c>Kestrel:Endpoints</c> instead.
        /// </summary>
        public int? GrpcPort { get; set; } = 50061;

        /// <summary>
        /// Legacy gRPC.Core tuning knob. No-op under Grpc.AspNetCore — the
        /// equivalent is <c>Kestrel:Limits:Http2:MaxStreamsPerConnection</c>.
        /// </summary>
        public int? GrpcRequestCallTokensPerCompletionQueue { get; set; } = 32768;

        /// <summary>
        /// Legacy gRPC.Core tuning knob. No-op under Grpc.AspNetCore — Kestrel
        /// uses the .NET thread-pool which is already CPU-bounded.
        /// </summary>
        public int? SetThreadPoolSize { get; set; } = null;

        /// <summary>
        /// Legacy switch that attached a Grpc.Core internal logger. Under
        /// Grpc.AspNetCore the framework already logs through ILogger out of
        /// the box (category <c>Grpc.AspNetCore.Server.*</c>); set log levels
        /// in <c>nlog.config</c> / <c>Logging</c> instead.
        /// </summary>
        public bool LogGrpc { get; set; } = false;

        /// <summary>
        /// Whether the server requires the TLS handshake to include a client
        /// certificate. Defaults to false (the kiosk's per-call
        /// <c>(identity, password)</c> metadata is the actual auth, see
        /// <see cref="GrpcMetadataAuthenticationHandler"/>). Under
        /// Grpc.AspNetCore configure via
        /// <c>Kestrel:Endpoints:Https:ClientCertificateMode</c>.
        /// </summary>
        public bool ForceClientCertificate { get; set; } = false;

        /// <summary>
        /// Paths to the server's TLS material. Under Grpc.AspNetCore configure
        /// the cert + key directly under <c>Kestrel:Endpoints:Https:Certificate</c>;
        /// the root client CA path becomes
        /// <c>Kestrel:Endpoints:Https:ClientCertificateMode</c> + a custom
        /// validator if mTLS is enabled.
        /// </summary>
        public SslCredentials SslCredentialFiles { get; set; } = new SslCredentials();
    }

    /// <summary>
    /// TSL/SSL config for gRPC certificates (legacy DTO; see
    /// <see cref="GrpcServerConfig"/> for AspNetCore migration notes).
    /// </summary>
    public class SslCredentials
    {
        /// <summary>The certificate used by the server.</summary>
        public string CertificateChainFile { get; set; } = "ssl_credentials/server.crt";

        /// <summary>Private key for the server certificate.</summary>
        public string PrivateKeyFile { get; set; } = "ssl_credentials/server.key";

        /// <summary>Root CA whose chain valid client certs must match
        /// (only used when <see cref="GrpcServerConfig.ForceClientCertificate"/>
        /// is true).</summary>
        public string RootClientCertificateFile { get; set; } = "ssl_credentials/ca.crt";
    }
}
