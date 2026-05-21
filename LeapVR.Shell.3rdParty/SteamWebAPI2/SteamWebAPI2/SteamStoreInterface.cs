#region Licence
/****************************************************************
 *  Filename: SteamStoreInterface.cs
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWebAPI2
{
    /// <summary>
    /// Represents an interface into the Steam Store Web API
    /// </summary>
    public abstract class SteamStoreInterface
    {
        // HTTPS only. The plain-HTTP endpoint now responds with a 302 redirect
        // (or silently times out on port 80 under any sustained burst) - the
        // kiosk used to see "task was canceled" on most appdetails calls
        // because the HTTP client timed out before the redirect was followed.
        private const string steamStoreApiBaseUrl = "https://store.steampowered.com/api/";
        private readonly SteamStoreRequest steamStoreRequest;
#pragma warning disable 169
        private readonly string endpointName;
#pragma warning restore 169

        /// <summary>
        /// Constructs and maps the default objects for Steam Store Web API use
        /// </summary>
        public SteamStoreInterface()
        {
            this.steamStoreRequest = new SteamStoreRequest(steamStoreApiBaseUrl);

            AutoMapperConfiguration.Initialize();
        }

        /// <summary>
        /// Constructs and maps based on a custom Steam Store Web API URL
        /// </summary>
        /// <param name="steamStoreApiBaseUrl">Steam Store Web API URL</param>
        public SteamStoreInterface(string steamStoreApiBaseUrl)
        {
            this.steamStoreRequest = new SteamStoreRequest(steamStoreApiBaseUrl);

            AutoMapperConfiguration.Initialize();
        }

        /// <summary>
        /// Calls a endpoint on the constructed Web API with parameters
        /// </summary>
        /// <typeparam name="T">Type of object which will be deserialized from the response</typeparam>
        /// <param name="endpointName">Endpoint to call on the interface</param>
        /// <param name="parameters">Parameters to pass to the endpoint</param>
        /// <returns>Deserialized response object</returns>
        internal async Task<T> CallMethodAsync<T>(string endpointName, IList<SteamWebRequestParameter> parameters = null,TimeSpan? timeout = null)
        {
            Debug.Assert(!String.IsNullOrEmpty(endpointName));

            return await steamStoreRequest.SendStoreRequestAsync<T>(endpointName, parameters,timeout);
        }

        public void Cancel()
        {
            steamStoreRequest?.Cancel();
        }
    }
}
