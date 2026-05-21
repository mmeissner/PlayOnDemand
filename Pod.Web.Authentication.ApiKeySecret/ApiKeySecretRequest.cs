#region Licence
/****************************************************************
 *  Filename: ApiKeySecretRequest.cs
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
using Microsoft.AspNetCore.Http;

namespace Pod.Web.Authentication.ApiKeySecret {
    class ApiKeySecretRequest : IApiKeySecretRequest
    {
        public ApiKeySecretRequest(string[] authorizationHeaderArray, HttpRequest request)
        {
            AuthorizationHeaderArray = authorizationHeaderArray;
            HttpRequest = request;
        }
        public string[] AuthorizationHeaderArray { get; }
        public HttpRequest HttpRequest { get; }
    }
}