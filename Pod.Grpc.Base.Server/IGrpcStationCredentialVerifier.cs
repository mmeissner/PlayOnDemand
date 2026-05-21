#region Licence
/****************************************************************
 *  Filename: IGrpcStationCredentialVerifier.cs
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
using System.Threading.Tasks;
using Pod.Data.Infrastructure;

namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// Verifies a station's <see cref="ClientCredentials"/> for the gRPC
    /// authentication scheme. Abstracted out so
    /// <see cref="GrpcMetadataAuthenticationHandler"/> can be unit-tested
    /// without spinning up an EF Core <c>PodDbContext</c>.
    ///
    /// The default production registration in Pod.Web.Center binds this to
    /// <see cref="DefaultGrpcStationCredentialVerifier"/>, which delegates to
    /// <c>Pod.Services.Extensions.VerifyCredentials</c> (the existing PBKDF2
    /// path against <c>Station.PasswordHash</c>).
    /// </summary>
    public interface IGrpcStationCredentialVerifier
    {
        /// <summary>
        /// Returns success when the (StationId, Password) pair authenticates;
        /// any error result is treated as authentication failure by the
        /// handler.
        /// </summary>
        Task<Result> VerifyAsync(ClientCredentials credentials);
    }
}
