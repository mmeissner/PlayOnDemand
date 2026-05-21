#region Licence
/****************************************************************
 *  Filename: RefreshAccessTokenProvider.cs
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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pod.Services.Authentication
{


    /// <summary>
    /// Creates RefreshTokens that are very long lived
    /// Subclass of DataProtectorTokenProvider to allow different settings for these providers
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class RefreshAccessTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public RefreshAccessTokenProvider(
                IDataProtectionProvider dataProtectionProvider,
                IOptions<RefreshAccessTokenProviderOptions> options,
                ILogger<DataProtectorTokenProvider<TUser>> logger) :
                base(dataProtectionProvider, options, logger)
        { }
    }

    /// <summary>
    /// Class to implement RefreshAccessToken specific Options that can be overwritten through Config
    /// and registered in IOC dedicated for Refresh Tokens
    /// </summary>
    public class RefreshAccessTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        /// <summary>
        /// Defines the purpose during <see cref="UserManager{TUser}.GenerateUserTokenAsync"/>
        /// </summary>
        public string RefreshTokenKey { get; set; } = "RefreshToken";
    }
}
