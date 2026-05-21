#region Licence
/****************************************************************
 *  Filename: ApiKeySecretHandler.cs
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
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Pod.Web.Authentication.ApiKeySecret
{
    /// <summary>
    /// Handles authentication for ApiKeys with Secret and HMAC SHA256 
    /// </summary>
    public class ApiKeySecretHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "amx";
        private readonly IApiKeySecretValidator _validator;

        public ApiKeySecretHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, IApiKeySecretValidator validator) : base(options, logger, encoder)
        {
            _validator = validator;
        }

        /// <summary>
        /// Interface Implementation for AuthenticationHandler
        /// </summary>
        /// <returns>Authentication Result</returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Use the public IHeaderDictionary API instead of the (now-inaccessible) Kestrel-internal HttpRequestHeaders.
            // The Authorization header can carry multiple values; pick the first one starting with our scheme.
            var authorization = Context.Request.Headers[HeaderNames.Authorization]
                .FirstOrDefault(x => x != null && x.StartsWith(AuthenticationScheme));
            
            //There is no suitable authentication data in the Header for this Handler
            if(authorization == null) return AuthenticateResult.NoResult();

            //Get the data from the Header
            var authorizationHeaderArray = GetAuthorizationHeaderValues(authorization);
            if (authorizationHeaderArray == null) return AuthenticateResult.Fail("Malformed amx authorization header.");

            //Validate the Request
            var validationResult = await _validator.Validate(new ApiKeySecretRequest(authorizationHeaderArray, Context.Request));

            //Check if Validation was a success
            if (validationResult.IsSuccess)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(validationResult.ClaimsPrincipal, AuthenticationScheme));
            }
            //Check if it failed because it was malformed
            if(validationResult.IsInvalidSignature)
            {
                return AuthenticateResult.Fail("Invalid amx signature.");
            }
            return AuthenticateResult.Fail("Invalid amx authorization header.");
        }

        /// <summary>
        /// In this authentication scheme multiple values are  provided in the header as a single string
        /// We split here these string to receive the single values 
        /// </summary>
        /// <param name="httpAuthHeader">The authorization value from the header without the scheme prefix</param>
        /// <returns>Array with single values or null</returns>
        private static string[] GetAuthorizationHeaderValues(string httpAuthHeader)
        {
            //The Authentication scheme is separated from the data with a space
            //The single values are separated by :
            var credArray = httpAuthHeader.Split(' ')[1].Split(':');
            return credArray.Length == 4 && !credArray.Any(string.IsNullOrWhiteSpace) ? credArray : null;
        }
    }
}