#region Licence
/****************************************************************
 *  Filename: AuthViewModels.cs
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
namespace Pod.ViewModels.Auth {

    /// <summary>
    /// Provides Access Information for calls that require Authorization
    /// </summary>
    public class AccessTokenViewModel
    {
        public AccessTokenViewModel() { }
        /// <summary>
        /// The Access Token for Bearer Authentication
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Seconds after the Access Token expires
        /// </summary>
        public int ExpiresIn { get; set; }

        public AccessTokenViewModel(string token, int expiresIn)
        {
            Token = token;
            ExpiresIn = expiresIn;
        }
    }

    /// <summary>
    /// Authorization Request Response ViewModel
    /// </summary>
    public class LoginResponseViewModel
    {
        public LoginResponseViewModel() { }
        public LoginResponseViewModel(AccessTokenViewModel accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
        /// <summary>
        /// The Access Token ViewModel
        /// </summary>
        public AccessTokenViewModel AccessToken { get; set; }

        /// <summary>
        /// Long lived Refresh Token to request new Access Tokens
        /// Refresh Tokens can be invalidated in some cases like Logout,Password Change,...
        /// </summary>
        public string RefreshToken { get; set; }
    }
}