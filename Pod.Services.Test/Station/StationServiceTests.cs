#region Licence
/****************************************************************
 *  Filename: StationServiceTests.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pod.Data;
using Pod.Data.Models.Shell;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Station;
using Pod.Services.System;
using Pod.Services.Test.TestFixtures;
using Xunit;

namespace Pod.Services.Test.Station
{
    /// <summary>
    /// Characterization tests for <see cref="StationService"/>.CreateNewStation and
    /// related happy paths. Backed by EF Core's InMemory provider.
    /// </summary>
    public class StationServiceTests
    {
        private const string ValidPassword = "Password-1234";

        private static StationService BuildService(PodDbContext ctx, SystemSettingsService settings = null)
        {
            return new StationService(
                    MockLogger.For<StationService>(),
                    new PublisherHub<ClientCommandType>(),
                    new StationResponseHub(MockLogger.For<ResponderHub<ClientResponse>>()),
                    settings ?? new SystemSettingsService(),
                    ctx);
        }

        private static async Task<ApplicationUser> SeedUserAsync(PodDbContext ctx, string name = "tester")
        {
            // The InMemory provider does NOT enforce the CustomerNumber DEFAULT VALUE SQL
            // (which uses a Postgres sequence). Set the FK columns directly via reflection
            // is overkill for tests that only need a user row to exist; seed via the
            // public ApplicationUser ctor and let InMemory accept the defaults.
            var user = new ApplicationUser
            {
                    Id = Guid.NewGuid(),
                    UserName = name,
                    NormalizedUserName = name.ToUpperInvariant(),
                    Email = name + "@example.com",
                    NormalizedEmail = (name + "@example.com").ToUpperInvariant(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            return user;
        }

        // ------------------------------------------------------------------
        // CreateNewStation
        // ------------------------------------------------------------------

        [Fact]
        public async Task CreateNewStation_with_valid_inputs_returns_success_and_persists_station()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var user = await SeedUserAsync(ctx);
            var service = BuildService(ctx);

            var result = await service.CreateNewStation(user.Id, "MyStation", ValidPassword);

            Assert.True(result.IsSuccess(), "Expected success but got: " + result.ToErrorString());
            Assert.NotNull(result.ReturnValue);
            Assert.NotEqual(Guid.Empty, result.ReturnValue.StationId);
            Assert.Equal("MyStation", result.ReturnValue.DisplayName);
            Assert.Equal(StationControlMode.Local, result.ReturnValue.ControlMode);

            // Verify persisted in DB
            var dbStation = await ctx.Stations.FirstOrDefaultAsync(x => x.Id == result.ReturnValue.StationId);
            Assert.NotNull(dbStation);
            Assert.Equal(user.Id, dbStation.ApplicationUserId);
        }

        [Fact]
        public async Task CreateNewStation_with_empty_userId_returns_InvalidUserId_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var service = BuildService(ctx);

            var result = await service.CreateNewStation(Guid.Empty, "AStation", ValidPassword);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidId));
        }

        [Fact]
        public async Task CreateNewStation_with_blank_displayName_returns_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var user = await SeedUserAsync(ctx);
            var service = BuildService(ctx);

            var result = await service.CreateNewStation(user.Id, "   ", ValidPassword);

            Assert.True(result.HasError());
        }

        [Fact]
        public async Task CreateNewStation_with_password_too_short_returns_PasswordTooShort_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var user = await SeedUserAsync(ctx);
            var service = BuildService(ctx);

            var result = await service.CreateNewStation(user.Id, "MyStation", "short");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.StationPasswordTooShort),
                    "Expected StationPasswordTooShort but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task CreateNewStation_with_password_missing_upper_case_returns_NoUpperChars_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var user = await SeedUserAsync(ctx);
            var service = BuildService(ctx);

            var result = await service.CreateNewStation(user.Id, "MyStation", "all-lower-99");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.StationPasswordHasNoUpperChars),
                    "Expected StationPasswordHasNoUpperChars but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task CreateNewStation_enforces_per_user_station_limit()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var user = await SeedUserAsync(ctx);
            var settings = new SystemSettingsService();
            settings.SetSystemSettings(userRegistrationEnabled: true, maxStationsPerUser: 2);
            var service = BuildService(ctx, settings);

            var first = await service.CreateNewStation(user.Id, "Station1", ValidPassword);
            var second = await service.CreateNewStation(user.Id, "Station2", ValidPassword);
            var third = await service.CreateNewStation(user.Id, "Station3", ValidPassword);

            Assert.True(first.IsSuccess(), "first: " + first.ToErrorString());
            Assert.True(second.IsSuccess(), "second: " + second.ToErrorString());

            Assert.True(third.HasError());
            Assert.True(third.ContainsKey(UserError.StationMaxAmountReached),
                    "Expected StationMaxAmountReached on third creation but got: " + third.ToErrorString());
        }

        // ------------------------------------------------------------------
        // GetStationsDisplayDetails
        // ------------------------------------------------------------------

        [Fact]
        public async Task GetStationsDisplayDetails_returns_only_stations_owned_by_user()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var alice = await SeedUserAsync(ctx, "alice");
            var bob = await SeedUserAsync(ctx, "bob");
            var service = BuildService(ctx);

            _ = await service.CreateNewStation(alice.Id, "Alice-1", ValidPassword);
            _ = await service.CreateNewStation(alice.Id, "Alice-2", ValidPassword);
            _ = await service.CreateNewStation(bob.Id, "Bob-1", ValidPassword);

            var aliceList = await service.GetStationsDisplayDetails(alice.Id);
            Assert.True(aliceList.IsSuccess());
            Assert.Equal(2, aliceList.ReturnValue.Count());

            var bobList = await service.GetStationsDisplayDetails(bob.Id);
            Assert.True(bobList.IsSuccess());
            Assert.Single(bobList.ReturnValue);
        }
    }
}
