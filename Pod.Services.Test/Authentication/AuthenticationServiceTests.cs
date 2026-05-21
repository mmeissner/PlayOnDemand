#region Licence
/****************************************************************
 *  Filename: AuthenticationServiceTests.cs
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
using Microsoft.AspNetCore.Identity;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Authentication;
using Pod.Services.Test.TestFixtures;
using Xunit;

namespace Pod.Services.Test.Authentication
{
    /// <summary>
    /// Characterization tests for <see cref="AuthenticationService"/>.GetTokenByLogin and
    /// related JWT issue/refresh paths. Uses an InMemory-backed Identity stack so tests run
    /// without Postgres. Each test creates its own harness for isolation.
    /// </summary>
    public class AuthenticationServiceTests
    {
        private const string ValidPassword = "Password-1234";

        private static AuthenticationService BuildService(IdentityTestHarness harness)
        {
            var jwtConfig = new JwtIssuerOptionsConfig
            {
                    Issuer = "tests",
                    Audience = "tests-audience",
                    ValidFor = TimeSpan.FromMinutes(15),
            };
            var authConfig = new AuthConfig
            {
                    SecretKey = "tests-symmetric-secret-key-must-be-long-enough-for-hmacsha256",
            };
            var jwtOptions = new JwtIssuerOptions(jwtConfig, authConfig);
            var refreshOpts = new RefreshAccessTokenProviderOptions { Name = "Default" };
            return new AuthenticationService(
                    MockLogger.For<AuthenticationService>(),
                    harness.SignInManager,
                    harness.RoleManager,
                    refreshOpts,
                    jwtOptions);
        }

        // ------------------------------------------------------------------
        // GetTokenByLogin (called LoginAsync in the spec; the production
        // method name is GetTokenByLogin in AuthenticationService.cs)
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetTokenByLogin_with_correct_credentials_returns_access_and_refresh_tokens()
        {
            using var harness = IdentityTestHarness.Build();
            await harness.CreateConfirmedUserAsync("alice", "alice@example.com", ValidPassword);

            var service = BuildService(harness);
            var result = await service.GetTokenByLogin("alice", ValidPassword);

            Assert.True(result.IsSuccess(), "Expected success but got: " + result.ToErrorString());
            Assert.NotNull(result.ReturnValue);
            Assert.False(string.IsNullOrWhiteSpace(result.ReturnValue.AccessToken.Token));
            Assert.False(string.IsNullOrWhiteSpace(result.ReturnValue.RefreshToken));
            Assert.True(result.ReturnValue.AccessToken.ExpiresIn > 0);
        }

        [Fact]
        public async Task GetTokenByLogin_with_wrong_password_returns_PasswordMismatch_and_no_token()
        {
            // Regression coverage for a real security bug surfaced by the docker-compose
            // end-to-end smoke test during v1.0.0 prep: when SignInManager.CheckPasswordSignInAsync
            // returned SignInResult.Failed (plain wrong password, no lockout / not-allowed flag),
            // Extensions.AddSignResult silently fell through and AuthenticationService issued a
            // fresh access token for unauthenticated callers. Fixed in Extensions.AddSignResult
            // by adding an explicit UserIdentityPasswordMismatch branch for the !Succeeded /
            // !IsLockedOut / !IsNotAllowed combination.
            using var harness = IdentityTestHarness.Build();
            await harness.CreateConfirmedUserAsync("bob", "bob@example.com", ValidPassword);

            var service = BuildService(harness);
            var result = await service.GetTokenByLogin("bob", "Wrong-Password-99");

            Assert.True(result.HasError(), "wrong password must reject the login");
            Assert.True(result.ContainsKey(UserError.UserIdentityPasswordMismatch),
                    "expected UserIdentityPasswordMismatch in: " + result.ToErrorString());
            Assert.Null(result.ReturnValue);
        }

        [Fact]
        public async Task GetTokenByLogin_with_unknown_username_returns_InvalidUserName_error()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.GetTokenByLogin("ghost", ValidPassword);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidUserName));
            Assert.Null(result.ReturnValue);
        }

        [Fact]
        public async Task GetTokenByLogin_with_empty_username_returns_InvalidUserName_error()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.GetTokenByLogin("", ValidPassword);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidUserName));
        }

        [Fact]
        public async Task GetTokenByLogin_with_empty_password_returns_PasswordMismatch_error()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.GetTokenByLogin("anyone", "");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityPasswordMismatch));
        }

        [Fact]
        public async Task GetTokenByLogin_with_unconfirmed_email_returns_NotAllowedToLogin_error()
        {
            using var harness = IdentityTestHarness.Build();
            // Create an unconfirmed user — bypass the harness helper which auto-confirms.
            var user = new ApplicationUser { UserName = "carol", Email = "carol@example.com" };
            var create = await harness.UserManager.CreateAsync(user, ValidPassword);
            Assert.True(create.Succeeded);

            var service = BuildService(harness);
            // CheckPasswordSignInAsync respects RequireConfirmedEmail when the SignInManager
            // is configured for it. The harness sets opts.SignIn.RequireConfirmedEmail = true.
            var result = await service.GetTokenByLogin("carol", ValidPassword);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityNotAllowedToLogin),
                    "Expected UserIdentityNotAllowedToLogin but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task GetTokenByLogin_after_lockout_is_set_directly_returns_LockedOut_error()
        {
            // AuthenticationService.GetTokenByLogin calls CheckPasswordSignInAsync with
            // lockOutOnFailure: false, so failed login attempts do NOT advance the lockout
            // counter via this code path. (This is intentional or a bug — out of scope.)
            // To exercise the IsLockedOut branch of Extensions.AddSignResult we lock the
            // user directly via UserManager.
            using var harness = IdentityTestHarness.Build();
            var user = await harness.CreateConfirmedUserAsync("dave", "dave@example.com", ValidPassword);

            // Lock the user until far in the future.
            var lockResult = await harness.UserManager.SetLockoutEndDateAsync(
                    user, DateTimeOffset.UtcNow.AddHours(1));
            Assert.True(lockResult.Succeeded);

            var service = BuildService(harness);
            var result = await service.GetTokenByLogin("dave", ValidPassword);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityIsLockedOut),
                    "Expected UserIdentityIsLockedOut but got: " + result.ToErrorString());
        }

        // ------------------------------------------------------------------
        // LogoutUser
        // ------------------------------------------------------------------

        [Fact]
        public async Task LogoutUser_with_valid_username_succeeds()
        {
            using var harness = IdentityTestHarness.Build();
            await harness.CreateConfirmedUserAsync("eve", "eve@example.com", ValidPassword);

            var service = BuildService(harness);
            var login = await service.GetTokenByLogin("eve", ValidPassword);
            Assert.True(login.IsSuccess());

            var logout = await service.LogoutUser("eve");
            Assert.True(logout.IsSuccess(), "Expected success but got: " + logout.ToErrorString());

            // Characterization note: The refresh token is issued by the
            // DataProtectorTokenProvider-derived RefreshAccessTokenProvider, which is
            // STATELESS. RemoveAuthenticationTokenAsync (called by LogoutUser) removes
            // rows from AspNetUserTokens, but stateless tokens never lived there. The
            // refresh token therefore remains valid after logout — matching what
            // docs/architecture/auth.md says explicitly ("the access JWT remains valid
            // until natural expiry" and refresh is invalidated only "via the user's
            // security stamp").
            var refresh = await service.RefreshToken(
                    login.ReturnValue.AccessToken.Token,
                    login.ReturnValue.RefreshToken);
            Assert.True(refresh.IsSuccess(),
                    "Documented behaviour: logout does not invalidate stateless refresh tokens.");
        }

        [Fact]
        public async Task LogoutUser_with_unknown_username_returns_InvalidUserName_error()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.LogoutUser("ghost");
            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidUserName));
        }

        [Fact]
        public async Task LogoutUser_with_empty_username_returns_InvalidUserName_error()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.LogoutUser("");
            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidUserName));
        }

        // ------------------------------------------------------------------
        // RefreshToken
        // ------------------------------------------------------------------

        [Fact]
        public async Task RefreshToken_with_valid_pair_returns_a_fresh_access_token()
        {
            using var harness = IdentityTestHarness.Build();
            await harness.CreateConfirmedUserAsync("frank", "frank@example.com", ValidPassword);

            var service = BuildService(harness);
            var login = await service.GetTokenByLogin("frank", ValidPassword);
            Assert.True(login.IsSuccess());

            var refresh = await service.RefreshToken(
                    login.ReturnValue.AccessToken.Token,
                    login.ReturnValue.RefreshToken);

            Assert.True(refresh.IsSuccess(), "Expected success but got: " + refresh.ToErrorString());
            Assert.NotNull(refresh.ReturnValue);
            Assert.False(string.IsNullOrWhiteSpace(refresh.ReturnValue.Token));
        }

        [Fact]
        public async Task RefreshToken_with_garbage_access_token_throws()
        {
            // KNOWN ISSUE: AuthenticationService.TryGetPrincipalFromExpiredToken does NOT
            // wrap JwtSecurityTokenHandler.ValidateToken in a try/catch. The handler throws
            // SecurityTokenMalformedException on garbage input rather than returning false.
            // The service therefore propagates the exception instead of returning the
            // documented UserError.UserIdentityInvalidToken result. This test documents
            // the current behaviour. Wrap the call in try/catch and return the error to
            // fix.
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                    await service.RefreshToken("not-a-jwt", "irrelevant-refresh"));
        }

        [Fact]
        public async Task RefreshToken_with_empty_inputs_returns_appropriate_errors()
        {
            using var harness = IdentityTestHarness.Build();
            var service = BuildService(harness);

            var result = await service.RefreshToken("", "");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidUserName) ||
                        result.ContainsKey(UserError.UserIdentityInvalidRefreshToken));
        }
    }
}
