#region Licence
/****************************************************************
 *  Filename: JwtIssuerOptions.cs
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Pod.Services.Authentication
{
    /// <summary>
    /// Options required for JWT Token generation
    /// </summary>
    public class JwtIssuerOptions
    {
        public JwtIssuerOptions(JwtIssuerOptionsConfig config, AuthConfig authConfig)
        {
            SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authConfig.SecretKey)),
                    SecurityAlgorithms.HmacSha256);
            Issuer = config.Issuer;
            Audience = config.Audience;
            ValidFor = config.ValidFor;
        }

        /// <summary>
        /// 4.1.1.  "iss" (Issuer) Claim - The "iss" (issuer) claim identifies the principal that issued the JWT.
        /// </summary>
        public string Issuer { get;  }

        /// <summary>
        /// 4.1.3.  "aud" (Audience) Claim - The "aud" (audience) claim identifies the recipients that the JWT is intended for.
        /// </summary>
        public string Audience { get;}

        /// <summary>
        /// 4.1.4.  "exp" (Expiration Time) Claim - The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.
        /// </summary>
        public DateTime Expiration => IssuedAt.Add(ValidFor);

        /// <summary>
        /// 4.1.5.  "nbf" (Not Before) Claim - The "nbf" (not before) claim identifies the time before which the JWT MUST NOT be accepted for processing.
        /// </summary>
        public DateTime NotBefore => DateTime.UtcNow;

        /// <summary>
        /// 4.1.6.  "iat" (Issued At) Claim - The "iat" (issued at) claim identifies the time at which the JWT was issued.
        /// </summary>
        public DateTime IssuedAt => DateTime.UtcNow;

        /// <summary>
        /// Set the timespan the token will be valid for (default is 120 min)
        /// </summary>
        public TimeSpan ValidFor { get;}
        /// <summary>
        /// "jti" (JWT ID) Claim (default ID is a GUID)
        /// </summary>
        public Func<Task<string>> JtiGenerator =>
                () => Task.FromResult(Guid.NewGuid().ToString());

        /// <summary>
        /// The signing key to use when generating tokens.
        /// </summary>
        public SigningCredentials SigningCredentials  { get; }
    }

    /// <summary>
    /// Options for Token Generation
    /// </summary>
    public class JwtIssuerOptionsConfig
    {
        /// <summary>
        /// 4.1.1.  "iss" (Issuer) Claim - The "iss" (issuer) claim identifies the principal that issued the JWT.
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 4.1.3.  "aud" (Audience) Claim - The "aud" (audience) claim identifies the recipients that the JWT is intended for.
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// The Time that a Token is valid
        /// </summary>
        public TimeSpan ValidFor { get; set; }
    }
}
