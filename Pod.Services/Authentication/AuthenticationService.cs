#region Licence
/****************************************************************
 *  Filename: AuthenticationService.cs
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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.ViewModels.Auth;

namespace Pod.Services.Authentication
{
    /// <summary>
    /// Service proving functionality to Authenticate, Receive and Renew JWT Tokens
    /// </summary>
    public class AuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly RefreshAccessTokenProviderOptions _refreshTokenOptions;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly JwtSecurityTokenHandler _securityTokenHandler;
        public AuthenticationService(
                ILogger<AuthenticationService> logger,
                SignInManager<ApplicationUser> singInManager,
                RoleManager<ApplicationRole> roleManager,
                RefreshAccessTokenProviderOptions refreshTokenOptions,
                JwtIssuerOptions jwtOptions)
        {
            _logger = logger;
            _signInManager = singInManager;
            _roleManager = roleManager;
            _refreshTokenOptions = refreshTokenOptions;
            _jwtOptions = jwtOptions;
            _securityTokenHandler = new JwtSecurityTokenHandler();
        }

        /// <summary>
        /// Creates a Access Token
        /// </summary>
        /// <param name="username">The Username for the Account the token is for</param>
        /// <param name="password">The Password for the Username</param>
        /// <returns>Result of the Login</returns>
        public async Task<IResult<LoginResponseViewModel>> GetTokenByLogin(string username, string password)
        {
            var result = new Result<LoginResponseViewModel>();
            result.ArgNotNullOrWhitespace(username, nameof(username), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(password, nameof(password), UserError.UserIdentityPasswordMismatch);
            if(result.HasError()) return result;

            var user = await _signInManager.UserManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if(result.AddSignResult(signInResult).HasError()) return result;

            return result.Add(
                    new LoginResponseViewModel(
                            await CreateAccessToken(user),
                            await _signInManager.UserManager.GenerateUserTokenAsync(
                                    user,
                                    _refreshTokenOptions.Name,
                                    _refreshTokenOptions.RefreshTokenKey)));
        }

        /// <summary>
        /// Invalidates the Refresh Token
        /// </summary>
        /// <param name="username">The Username of the Account to invalidate the Refresh Token</param>
        /// <returns></returns>
        public async Task<Result> LogoutUser(string username)
        {
            var result = new Result();
            result.ArgNotNullOrWhitespace(username,nameof(username), UserError.UserIdentityInvalidUserName);
            if(result.HasError()) return result;
            var user = await _signInManager.UserManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName))return result;

            //This will remove the Refresh Token as when Password changed User will need to go Login again
            var identityResult = await _signInManager.UserManager.RemoveAuthenticationTokenAsync(user, _refreshTokenOptions.Name, _refreshTokenOptions.RefreshTokenKey);
            return result.Add(identityResult);
        }

        /// <summary>
        /// Requests a Access Token refresh with an expired accessToken and a long lived refresh Token
        /// </summary>
        /// <param name="oldAccessToken">An valid access token that might be expired</param>
        /// <param name="refreshToken">The current active refresh Token</param>
        /// <returns>The Access Token Result</returns>
        public async Task<IResult<AccessTokenViewModel>> RefreshToken(string oldAccessToken, string refreshToken)
        {
            var result = new Result<AccessTokenViewModel>();
            result.ArgNotNullOrWhitespace(oldAccessToken, nameof(oldAccessToken), UserError.UserIdentityInvalidUserName);
            result.ArgNotNullOrWhitespace(refreshToken, nameof(refreshToken), UserError.UserIdentityInvalidRefreshToken);
            if(result.HasError()) return result;

            if(!TryGetPrincipalFromExpiredToken(oldAccessToken, out var claimsPrincipal))
            {
                return result.Add("Invalid access token", UserError.UserIdentityInvalidToken);
            }
            var username = claimsPrincipal.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _signInManager.UserManager.FindByNameAsync(username);
            if(!result.ValueNotNull(user, nameof(user), UserError.UserIdentityInvalidUserName)) return result;
            if(!result.ValueTrue(
                    await _signInManager.UserManager.VerifyUserTokenAsync(
                            user,
                            _refreshTokenOptions.Name,
                            _refreshTokenOptions.RefreshTokenKey,
                            refreshToken),
                    nameof(refreshToken),
                    UserError.UserIdentityInvalidToken))
            {
                return result;
            }
            return result.Add(await CreateAccessToken(user));
        }

        private bool TryGetPrincipalFromExpiredToken(string token, out ClaimsPrincipal claimsPrincipal)
        {
            var tokenValidationParameters = new TokenValidationParameters
                                            {
                                                    ValidateIssuer = true,
                                                    ValidIssuer =_jwtOptions.Issuer,
                                                    ValidateAudience = true,
                                                    ValidAudience =_jwtOptions.Audience,
                                                    ValidateIssuerSigningKey = true,
                                                    IssuerSigningKey = _jwtOptions.SigningCredentials.Key,
                                                    RequireExpirationTime = false,
                                                    ValidateLifetime = false,
                                                    ClockSkew = TimeSpan.Zero
                                            };
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if(jwtSecurityToken == null ||
               !jwtSecurityToken.Header.Alg.Equals(
                       SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase))
            {
                claimsPrincipal = null;
                return false;
            }

            claimsPrincipal = principal;
            return true;
        }

        /// <summary>
        /// Generates an Access Token
        /// </summary>
        /// <param name="user">The User the token is for</param>
        /// <returns>An Access Token</returns>
        private async Task<AccessTokenViewModel> CreateAccessToken(ApplicationUser user)
        {
            // Get valid claims and pass them into JWT
            var claims = await GetValidClaims(user);

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    notBefore: _jwtOptions.NotBefore,
                    expires: _jwtOptions.Expiration,
                    signingCredentials: _jwtOptions.SigningCredentials);

            return new AccessTokenViewModel(
                    _securityTokenHandler.WriteToken(jwt),
                    (int)_jwtOptions.ValidFor.TotalSeconds);
        }

        /// <summary>
        /// Gets the Claims for an User
        /// </summary>
        /// <param name="user">The User to get the claims for</param>
        /// <returns>Collection of Claims of this user</returns>
        private async Task<List<Claim>> GetValidClaims(ApplicationUser user)
        {
            IdentityOptions options = new IdentityOptions();
            var claims = new List<Claim>
                         {
                                 new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                                 new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                                 new Claim(JwtRegisteredClaimNames.Iat,ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(),ClaimValueTypes.Integer64),
                                 new Claim(PodClaimsTypes.UserId, user.Id.ToString()),
                         };
            var userClaims = await _signInManager.UserManager.GetClaimsAsync(user);
            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);
            claims.AddRange(userClaims);
            foreach(var userRole in userRoles)
            {
                //claims.Add(new Claim(ClaimTypes.Role, userRole));
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if(role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach(Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            return claims;
        }
        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round(
                    (date.ToUniversalTime() -
                     new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}