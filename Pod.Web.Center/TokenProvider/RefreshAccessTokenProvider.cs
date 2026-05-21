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

namespace Pod.Web.Center.TokenProvider
{
    /// <summary>
    /// Subclass of DataProtectorTokenProvider to allow different settings for these providers
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class PasswordResetTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public PasswordResetTokenProvider(
                IDataProtectionProvider dataProtectionProvider,
                IOptions<PasswordResetTokenProviderOptions> options,
                ILogger<DataProtectorTokenProvider<TUser>> logger) :
                base(dataProtectionProvider, options, logger) { }
    }

    public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions { }


    /// <summary>
    /// Subclass of DataProtectorTokenProvider to allow different settings for these providers
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public EmailConfirmationTokenProvider(
                IDataProtectionProvider dataProtectionProvider,
                IOptions<EmailConfirmationTokenProviderOptions> options,
                ILogger<DataProtectorTokenProvider<TUser>> logger)
                : base(dataProtectionProvider, options, logger) { }
    }

    public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions { }
}