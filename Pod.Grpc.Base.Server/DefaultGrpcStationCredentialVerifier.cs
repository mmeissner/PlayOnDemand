#region Licence
/****************************************************************
 *  Filename: DefaultGrpcStationCredentialVerifier.cs
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
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Services;

namespace Pod.Grpc.Base.Server
{
    /// <summary>
    /// Default <see cref="IGrpcStationCredentialVerifier"/> that runs the
    /// existing <c>credentials.VerifyCredentials(PodDbContext)</c> path —
    /// loads the <c>Station</c> by id and PBKDF2-compares the password
    /// against <c>Station.PasswordHash</c>.
    ///
    /// Lives in the production server assembly so the test project can
    /// substitute a fake without dragging in EF Core.
    /// </summary>
    public sealed class DefaultGrpcStationCredentialVerifier : IGrpcStationCredentialVerifier
    {
        private readonly PodDbContext _dbContext;

        public DefaultGrpcStationCredentialVerifier(PodDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public Task<Result> VerifyAsync(ClientCredentials credentials)
        {
            return credentials.VerifyCredentials(_dbContext);
        }
    }
}
