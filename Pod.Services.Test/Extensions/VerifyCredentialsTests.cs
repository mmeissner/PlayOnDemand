#region Licence
/****************************************************************
 *  Filename: VerifyCredentialsTests.cs
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
using Microsoft.EntityFrameworkCore;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Station;
using Pod.Services.System;
using Pod.Services.Test.TestFixtures;
using Xunit;

namespace Pod.Services.Test.Extensions
{
    /// <summary>
    /// Characterization tests for <see cref="Pod.Services.Extensions.VerifyCredentials"/>,
    /// the credential check at the entry of every gRPC ShellService method.
    /// </summary>
    public class VerifyCredentialsTests
    {
        private const string StationPassword = "Password-1234";

        private static async Task<(Pod.Data.Models.Shell.Station station, PodDbContext ctx)>
                SeedStationAsync(string sharedDbName)
        {
            var ctx = InMemoryDbContextFactory.Create(sharedDbName);
            var user = new ApplicationUser
            {
                    Id = Guid.NewGuid(),
                    UserName = "v-user-" + Guid.NewGuid().ToString("N"),
                    NormalizedUserName = ("V-USER-" + Guid.NewGuid().ToString("N")).ToUpperInvariant(),
                    Email = "v@example.com",
                    NormalizedEmail = "V@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var stationService = new StationService(
                    MockLogger.For<StationService>(),
                    new PublisherHub<ClientCommandType>(),
                    new StationResponseHub(MockLogger.For<ResponderHub<ClientResponse>>()),
                    new SystemSettingsService(),
                    ctx);
            var vm = await stationService.CreateNewStation(user.Id, "VerifyStation", StationPassword);
            Assert.True(vm.IsSuccess(), "Seed: " + vm.ToErrorString());

            var station = await ctx.Stations.FirstAsync(x => x.Id == vm.ReturnValue.StationId);
            return (station, ctx);
        }

        [Fact]
        public async Task VerifyCredentials_with_correct_StationId_and_password_succeeds()
        {
            var dbName = "verify-success-" + Guid.NewGuid().ToString("N");
            var (station, _) = await SeedStationAsync(dbName);
            using var ctx = InMemoryDbContextFactory.Create(dbName);

            var credentials = new ClientCredentials
            {
                    StationId = station.Id,
                    Password = StationPassword,
            };

            var result = await credentials.VerifyCredentials(ctx);

            Assert.True(result.IsSuccess(), "Expected success but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task VerifyCredentials_with_wrong_password_returns_InvalidPassword_error()
        {
            var dbName = "verify-wrongpw-" + Guid.NewGuid().ToString("N");
            var (station, _) = await SeedStationAsync(dbName);
            using var ctx = InMemoryDbContextFactory.Create(dbName);

            var credentials = new ClientCredentials
            {
                    StationId = station.Id,
                    Password = "Wrong-Password-99",
            };

            var result = await credentials.VerifyCredentials(ctx);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidPassword),
                    "Expected ShellClientInvalidPassword but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task VerifyCredentials_with_unknown_station_returns_InvalidStationId_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var credentials = new ClientCredentials
            {
                    StationId = Guid.NewGuid(),
                    Password = StationPassword,
            };

            var result = await credentials.VerifyCredentials(ctx);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidStationId),
                    "Expected ShellClientInvalidStationId but got: " + result.ToErrorString());
        }

        [Fact]
        public async Task VerifyCredentials_with_empty_StationId_returns_InvalidStationId_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var credentials = new ClientCredentials
            {
                    StationId = Guid.Empty,
                    Password = StationPassword,
            };

            var result = await credentials.VerifyCredentials(ctx);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidStationId));
        }

        [Fact]
        public async Task VerifyCredentials_with_empty_password_returns_InvalidPassword_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var credentials = new ClientCredentials
            {
                    StationId = Guid.NewGuid(),
                    Password = "",
            };

            var result = await credentials.VerifyCredentials(ctx);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidPassword));
        }

        [Fact]
        public async Task VerifyCredentials_generic_overload_propagates_inner_errors()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var credentials = new ClientCredentials
            {
                    StationId = Guid.Empty,
                    Password = "",
            };

            var result = await credentials.VerifyCredentials<object>(ctx);

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidStationId));
            Assert.True(result.ContainsKey(UserError.ShellClientInvalidPassword));
            // Result<T>.ReturnValue is the default when the result carries errors.
            Assert.Null(result.ReturnValue);
        }
    }
}
