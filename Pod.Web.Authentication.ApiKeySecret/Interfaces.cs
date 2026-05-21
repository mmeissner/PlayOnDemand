#region Licence
/****************************************************************
 *  Filename: Interfaces.cs
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
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pod.Web.Authentication.ApiKeySecret
{
    /// <summary>
    /// Validates an Authentication Request with an API Key
    /// </summary>
    public interface IApiKeySecretValidator 
    {
        /// <summary>
        /// Returns a Validation/Authentication Response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IApiKeySecretResponse> Validate(IApiKeySecretRequest request);
    }

    /// <summary>
    /// Authentication Validation Response
    /// </summary>
    public interface IApiKeySecretResponse
    {       
        /// <summary>
        /// Indicates if the Authentication Succeeded
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Indicates if all was ok except that the signature did not match
        /// </summary>
        bool IsInvalidSignature { get; }

        /// <summary>
        /// The authenticated Identity
        /// </summary>
        ClaimsPrincipal ClaimsPrincipal { get; }
    }

    /// <summary>
    /// Holds information about the Http Request with ApiKey Authentication
    /// </summary>
    public interface IApiKeySecretRequest
    {
        string[] AuthorizationHeaderArray { get; }
        HttpRequest HttpRequest { get; }
        
    }
}
