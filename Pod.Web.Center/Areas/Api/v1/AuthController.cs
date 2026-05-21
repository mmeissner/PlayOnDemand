#region Licence
/****************************************************************
 *  Filename: AuthController.cs
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
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pod.DtoModels;
using Pod.Services.Authentication;
using Pod.ViewModels.Auth;
using Pod.Web.Center.Presenter;
using Swashbuckle.AspNetCore.Annotations;

namespace Pod.Web.Center.Areas.Api.v1
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [SwaggerTag("Access Token functions")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        public AuthController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Receive an Access and Refresh Token
        /// </summary>
        /// <param name="loginRequest">Login Request Dto</param>
        // POST api/v1/auth/login
        [ProducesResponseType(typeof(LoginResponseViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] RequestLoginModelDto loginRequest)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return ResultPresenter.GetResult(
                    await
                            _authenticationService.GetTokenByLogin(
                                    loginRequest.Username,
                                    loginRequest.Password));
        }

        /// <summary>
        /// Invalidate the Refresh token
        /// </summary>
        // POST api/v1/auth/logout
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            string username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(username == null) return BadRequest();
            return ResultPresenter.GetResult(await _authenticationService.LogoutUser(username));
        }

        /// <summary>
        /// Receive a new Access token
        /// </summary>
        /// <param name="requestTokenRefresh">Refresh Token Dto</param>
        /// <param name="bearerToken">Any previous Access Token.</param>
        [ProducesResponseType(typeof(AccessTokenViewModel), 200)]
        [ProducesResponseType(typeof(IDictionary<string, string[]>), 400)]
        [HttpPost("refreshToken")]
        public async Task<ActionResult> RefreshToken([FromBody] RequestTokenRefreshDto requestTokenRefresh, [FromHeaderAttribute(Name = "Authorization")] string bearerToken)
        {
            if(!ModelState.IsValid)return BadRequest(ModelState);

            var oldAccessToken = bearerToken;
            if(string.IsNullOrWhiteSpace(oldAccessToken)) return BadRequest();
            oldAccessToken = oldAccessToken.Replace("Bearer ", "").Trim();
            return ResultPresenter.GetResult(await _authenticationService.RefreshToken(oldAccessToken, requestTokenRefresh.RefreshToken));
        }
    }
}