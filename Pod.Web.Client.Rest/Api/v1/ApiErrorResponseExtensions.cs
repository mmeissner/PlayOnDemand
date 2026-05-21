#region Licence
/****************************************************************
 *  Filename: ApiErrorResponseExtensions.cs
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
using System.Collections.Generic;
using Newtonsoft.Json;
using Pod.Enums;
using RestSharp;

namespace Pod.Web.Client.Rest.Api.v1 {
    /// <summary>
    /// Class to convert an Error Response from the Api
    /// </summary>
    public static class ApiErrorResponseExtensions
    {
        /// <summary>
        /// Provides detailed information about an unsuccessful request
        /// </summary>
        /// <param name="response">The api response</param>
        /// <returns>returns [null] if there is no error, or a dictionary with error information if deserializable</returns>
        public static Dictionary<UserError, List<string>> GetErrors(this IRestResponse response)
        {
            if(response.IsSuccessful)return new Dictionary<UserError, List<string>>();
            return JsonConvert.DeserializeObject<Dictionary<UserError, List<string>>>(response.Content);
        }
    }
}