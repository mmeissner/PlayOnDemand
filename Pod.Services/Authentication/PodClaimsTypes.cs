#region Licence
/****************************************************************
 *  Filename: PodClaimsTypes.cs
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
namespace Pod.Services.Authentication {
    /// <summary>
    /// Claim type containing the User Id encoded in the JWT
    /// </summary>
    public static class PodClaimsTypes
    {
        /// <summary>
        /// The Claim Type Key for the User Id
        /// This one is set with all JWT Tokens
        /// </summary>
        public const string UserId = "UserId";

        /// <summary>
        /// The Claim Type Key for the Station Id
        /// This one is set with ApiKeys for Stations
        /// </summary>
        public const string ApiKeyStationId = "ApiKeyStationId";

        /// <summary>
        /// The Claim Type Key for the User Id based on the Station Id
        /// This one is set with ApiKeys for Stations
        /// </summary>
        public const string ApiKeyUserId = "ApiKeyUserId";
    }
}