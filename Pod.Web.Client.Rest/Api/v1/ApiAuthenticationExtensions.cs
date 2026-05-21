#region Licence
/****************************************************************
 *  Filename: ApiAuthenticationExtensions.cs
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
using Pod.DtoModels;
using RestSharp;

namespace Pod.Web.Client.Rest.Api.v1 {
    /// <summary>
    /// Authentication Api Requests implementation
    /// </summary>
    internal static class ApiAuthenticationExtensions
    {
        /// <summary>
        /// Receive an Access and Refresh Token
        /// </summary>
        /// <param name="request">Login Request Dto</param>>
        public static IRestRequest Login(RequestLoginModelDto request)
        {
            return new RestRequest("/api/v1/auth/login", Method.POST, DataFormat.Json).AddJsonBody(request);
        }

        /// <summary>
        /// Invalidate the Refresh token
        /// </summary>
        public static IRestRequest Logout()
        {
            return new RestRequest("/api/v1/auth/logout", Method.POST, DataFormat.Json);
        }

        /// <summary>
        /// Receive a new Access token
        /// </summary>
        /// <param name="request">Refresh Token Dto</param>
        public static IRestRequest RefreshToken(RequestTokenRefreshDto request)
        {
            return new RestRequest("/api/v1/auth/refreshToken", Method.POST, DataFormat.Json).AddJsonBody(request);
        }
    }
}