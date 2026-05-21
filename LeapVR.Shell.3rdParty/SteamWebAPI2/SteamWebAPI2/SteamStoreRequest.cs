#region Licence
/****************************************************************
 *  Filename: SteamStoreRequest.cs
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWebAPI2
{
    /// <summary>
    /// Represents a request to send to a Steam Store Web API
    /// </summary>
    internal class SteamStoreRequest
    {
        private string steamStoreApiBaseUrl;
        private HttpClient _httpClient;

        /// <summary>
        /// Constructs a Steam Store Web API request
        /// </summary>
        /// <param name="steamStoreApiBaseUrl">Steam Store Web API URL</param>
        public SteamStoreRequest(string steamStoreApiBaseUrl)
        {
            if (String.IsNullOrEmpty(steamStoreApiBaseUrl))
            {
                throw new ArgumentNullException("steamStoreApiBaseUrl");
            }

            this.steamStoreApiBaseUrl = steamStoreApiBaseUrl;
        }

        /// <summary>
        /// Sends a request to a Steam Store Web API endpoint
        /// </summary>
        /// <typeparam name="T">Type of object which will be deserialized from the response</typeparam>
        /// <param name="endpointName">Endpoint to call on the interface</param>
        /// <returns></returns>
        public async Task<T> SendStoreRequestAsync<T>(string endpointName,TimeSpan? timeout = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(endpointName));

            return await SendStoreRequestAsync<T>(endpointName, null,timeout);
        }

        /// <summary>
        /// Sends a request to a Steam Store Web API endpoint with parameters
        /// </summary>
        /// <typeparam name="T">Type of object which will be deserialized from the response</typeparam>
        /// <param name="endpointName">Endpoint to call on the interface</param>
        /// <param name="parameters">Parameters to pass to the endpoint</param>
        /// <returns>Deserialized response object</returns>
        public async Task<T> SendStoreRequestAsync<T>(string endpointName, IList<SteamWebRequestParameter> parameters, TimeSpan? timeout = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(endpointName));

            if (parameters == null)
            {
                parameters = new List<SteamWebRequestParameter>();
            }

            string command = BuildRequestCommand(endpointName, parameters);
            try
            {
                string response = await GetHttpStringResponseAsync(command, timeout).ConfigureAwait(false);
                var deserializedResult = JsonConvert.DeserializeObject<T>(response);
                return deserializedResult;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        /// <summary>
        /// Returns a string from an HTTP request and removes tabs and newlines
        /// </summary>
        /// <param name="command">Command (method endpoint) to send to an interface</param>
        /// <returns>HTTP response as a string without tabs and newlines</returns>
        private async Task<string> GetHttpStringResponseAsync(string command, TimeSpan? timeout = null)
        {
            using(_httpClient = new HttpClient())
            {
                if(timeout.HasValue)
                {
                    _httpClient.Timeout = timeout.Value;
                }
                string response = await _httpClient.GetStringAsync(command);
                response = response.Replace("\n", "");
                response = response.Replace("\t", "");
                return response;
            }
        }

        /// <summary>
        /// Builds a command to send with a request so that parameters and formats are correct
        /// </summary>
        /// <param name="endpointName">Endpoint to call on the interface</param>
        /// <param name="parameters">Parameters to send to the endpoint</param>
        /// <returns>Deserialized response object</returns>
        public string BuildRequestCommand(string endpointName, IList<SteamWebRequestParameter> parameters)
        {
            Debug.Assert(!String.IsNullOrEmpty(endpointName));

            if (steamStoreApiBaseUrl.EndsWith("/"))
            {
                steamStoreApiBaseUrl = steamStoreApiBaseUrl.Remove(steamStoreApiBaseUrl.Length - 1, 1);
            }

            string commandUrl = String.Format("{0}/{1}/", steamStoreApiBaseUrl, endpointName);

            // if we have parameters, join them together with & delimiter and append them to the command URL
            if (parameters != null && parameters.Count > 0)
            {
                string parameterString = String.Join("&", parameters);
                commandUrl += String.Format("?{0}", parameterString);
            }

            return commandUrl;
        }

        public void Cancel()
        {
            _httpClient?.CancelPendingRequests();
        }
    }
}
