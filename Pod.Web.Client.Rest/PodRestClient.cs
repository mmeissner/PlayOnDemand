#region Licence
/****************************************************************
 *  Filename: PodRestClient.cs
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
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pod.DtoModels;
using Pod.ViewModels.Auth;
using Pod.Web.Client.Rest.Api.v1;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serialization;

namespace Pod.Web.Client.Rest
{

    /// <summary>
    /// Client to Access Api Functions
    /// </summary>
    public class PodRestClient : RestClient
    {
        private readonly PodAuthenticator _authenticator;
        /// <summary>
        /// Creates an instances of an RestClient for Api access
        /// </summary>
        /// <param name="baseUrl">The base Url of the Api</param>
        /// <param name="username">Username for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        public PodRestClient(string baseUrl, string username, string password):base(baseUrl)
        {
            _authenticator = new PodAuthenticator(username, password);
            Authenticator = _authenticator;
            UseSerializer(() => new JsonNetSerializer());
        }

        public void SetCredentials(string username, string password)
        {
            _authenticator.SetCredentials(username,password);
        }

        /// <summary>
        /// Invalidates the Refresh token
        /// </summary>
        public IRestResponse Logout()
        {
            var result = Execute(ApiAuthenticationExtensions.Logout());
            _authenticator.OnLogout();
            return result;
        }
    }

    /// <summary>
    /// Serializer for RestSharp using Json.Net
    /// </summary>
    public class JsonNetSerializer : IRestSerializer
    {
        public string Serialize(object obj) =>
                JsonConvert.SerializeObject(obj);

        public string Serialize(Parameter parameter) =>
                JsonConvert.SerializeObject(parameter.Value);

        public T Deserialize<T>(IRestResponse response) =>
                JsonConvert.DeserializeObject<T>(response.Content, GetSettings());

        public string[] SupportedContentTypes { get; } =
            {
                    "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
            };

        public string ContentType { get; set; } = "application/json";

        public DataFormat DataFormat { get; } = DataFormat.Json;
        public JsonSerializerSettings GetSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }
    }

    /// <summary>
    /// Authenticator for RestSharp that handles Token Authentication for the Api
    /// </summary>
    public class PodAuthenticator : IAuthenticator
    {
        private readonly Timer _refreshTokenTimer;

        private bool _hasToken = false;
        private bool _tokenRefreshRequired = false;
        private string _password;
        private string _refreshToken = null;
        private string _accessToken = null;

        /// <summary>
        /// Initializes an new Authenticator class that can be used by RestSharp
        /// </summary>
        /// <param name="username">Username for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        public PodAuthenticator(string username, string password)
        {
            Username = username;
            _password = password;
            _refreshTokenTimer = new Timer(state => { _tokenRefreshRequired = true; });
        }

        internal string Username { get; private set; }

        public void SetCredentials(string username, string password)
        {
            OnLogout();
            Username = username;
            _password = password;
        }
        /// <summary>
        /// RestSharp Authenticate with implementation for Token Authentication
        /// </summary>
        /// <param name="client">The caller</param>
        /// <param name="request">The request</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // Refresh the Token
            if(_tokenRefreshRequired)
            {
                var authClient = new RestClient(client.BaseUrl);
                authClient.UseSerializer(() => new JsonNetSerializer());
                var response = authClient.Execute<AccessTokenViewModel>(
                        ApiAuthenticationExtensions.RefreshToken(
                                new RequestTokenRefreshDto
                                {
                                        RefreshToken = _refreshToken
                                }));
                if(response.IsSuccessful)
                {
                    _accessToken = response.Data.Token;
                    request.AddHeader("Authorization", $"Bearer {response.Data.Token}");
                    var timeToRefreshToken =
                            TimeSpan.FromSeconds(Convert.ToDouble(response.Data.ExpiresIn - 10));
                    _refreshTokenTimer.Change(timeToRefreshToken, Timeout.InfiniteTimeSpan);
                    _hasToken = true;
                }
            }

            //Get the Token
            if(!_hasToken)
            {
                var authClient = new RestClient(client.BaseUrl);
                authClient.UseSerializer(() => new JsonNetSerializer());
                var response = authClient.Execute<LoginResponseViewModel>(
                        ApiAuthenticationExtensions.Login(
                                new RequestLoginModelDto
                                {
                                        Username = Username,
                                        Password = _password
                                }));
                if(response.IsSuccessful)
                {
                    _accessToken = response.Data.AccessToken.Token;
                    _refreshToken = response.Data.RefreshToken;
                    var timeToRefreshToken = TimeSpan.FromSeconds(
                            Convert.ToDouble(response.Data.AccessToken.ExpiresIn - 10));
                    _refreshTokenTimer.Change(timeToRefreshToken, Timeout.InfiniteTimeSpan);
                    _hasToken = true;
                }
            }
            request.AddHeader("Authorization", $"Bearer {_accessToken}");
        }

        /// <summary>
        /// Resets Tokens, Timers and control flags
        /// </summary>
        internal void OnLogout()
        {
            _refreshTokenTimer.Change(Timeout.Infinite, -1);
            _hasToken = false;
            _tokenRefreshRequired = false;
            _refreshToken = null;
        }
    }
}