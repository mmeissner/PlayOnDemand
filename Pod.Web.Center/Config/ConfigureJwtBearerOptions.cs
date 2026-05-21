#region Licence
/****************************************************************
 *  Filename: ConfigureJwtBearerOptions.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pod.Services.Authentication;

namespace Pod.Web.Center.Config {
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private JwtIssuerOptions _jwtIssuerOptions;
        public ConfigureJwtBearerOptions(JwtIssuerOptions jwtIssuerOptions)
        {
            _jwtIssuerOptions = jwtIssuerOptions;
        }
        public void Configure(string name, JwtBearerOptions options)
        {

            var jwtBearerTokenValidationParameters = new TokenValidationParameters
                                                     {
                                                             ValidateIssuer = true,
                                                             ValidIssuer =_jwtIssuerOptions.Issuer,
                                                             ValidateAudience = true,
                                                             ValidAudience =_jwtIssuerOptions.Audience,
                                                             ValidateIssuerSigningKey = true,
                                                             IssuerSigningKey = _jwtIssuerOptions.SigningCredentials.Key,
                                                             RequireExpirationTime = true,
                                                             ValidateLifetime = true,
                                                             ClockSkew = TimeSpan.Zero
                                                     };

            options.ClaimsIssuer = _jwtIssuerOptions.Issuer;
            options.TokenValidationParameters = jwtBearerTokenValidationParameters;
            options.SaveToken = true;
            options.Events = new JwtBearerEvents
                             {
                                     OnAuthenticationFailed = context =>
                                                              {
                                                                  if(context.Exception.GetType() ==typeof(SecurityTokenExpiredException))
                                                                  {
                                                                      context.Response.Headers.Add("Token-Expired","true");
                                                                  }
                                                                  return Task.CompletedTask;
                                                              }
                             };
        }
        void IConfigureOptions<JwtBearerOptions>.Configure(JwtBearerOptions options) { Configure(null,options); }
    }
}