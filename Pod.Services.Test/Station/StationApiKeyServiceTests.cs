#region Licence
/****************************************************************
 *  Filename: StationApiKeyServiceTests.cs
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
    /// Characterization tests for <see cref="StationApiKeyService"/>.
    /// </summary>
    public class StationApiKeyServiceTests
    {
        private const string StationPassword = "Password-1234";

        private static async Task<ApplicationUser> SeedUserAsync(PodDbContext ctx, string name = "owner")
        {
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

        private static async Task<(ApplicationUser user, Pod.Data.Models.Shell.Station station)>
                SeedUserAndStationAsync(PodDbContext ctx)
        {
            var user = await SeedUserAsync(ctx);
            var stationService = new StationService(
                    MockLogger.For<StationService>(),
                    new PublisherHub<ClientCommandType>(),
                    new StationResponseHub(MockLogger.For<ResponderHub<ClientResponse>>()),
                    new SystemSettingsService(),
                    ctx);
            var stationVm = await stationService.CreateNewStation(user.Id, "S1", StationPassword);
            if (stationVm.HasError())
            {
                throw new InvalidOperationException(
                        "Test seed: CreateNewStation failed: " + stationVm.ToErrorString());
            }
            var station = await ctx.Stations.FirstAsync(x => x.Id == stationVm.ReturnValue.StationId);
            return (user, station);
        }

        // ------------------------------------------------------------------
        // CreateStationApiKey
        // ------------------------------------------------------------------

        [Fact]
        public async Task CreateStationApiKey_for_owned_station_returns_viewmodel_with_PublicKey_and_Secret()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var (user, station) = await SeedUserAndStationAsync(ctx);

            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);
            var result = await service.CreateStationApiKey(user.Id, station.Id, "kiosk-1");

            Assert.True(result.IsSuccess(), "Expected success but got: " + result.ToErrorString());
            Assert.NotNull(result.ReturnValue);
            Assert.False(string.IsNullOrWhiteSpace(result.ReturnValue.PublicKey),
                    "CreateStationApiKey must populate the public key on the response");
            Assert.True(Guid.TryParseExact(result.ReturnValue.PublicKey, "N", out var parsed) &&
                        parsed != Guid.Empty,
                    "PublicKey must be a non-empty Guid serialized as N-format string, got: " + result.ReturnValue.PublicKey);
            Assert.False(string.IsNullOrWhiteSpace(result.ReturnValue.Secret),
                    "CreateStationApiKey must populate the secret on the response (only chance to read it)");
        }

        [Fact]
        public async Task GetStationApiKeys_NEVER_returns_Secret_field_only_mint_does()
        {
            // Regression coverage for the secret-exposure bug surfaced by the
            // docker-compose end-to-end smoke during v1.0.0 prep: the list endpoint
            // (GET /api/v1/Stations/{id}/apikeys) used the same projection as the
            // mint endpoint, leaking every kiosk's persisted credential to anyone
            // with read access to the operator UI. Fix: GetStationApiKeys uses
            // FuncFromStationApiKeyNoSecret which sets Secret to null. The mint
            // endpoint (CreateStationApiKey) still returns the secret because
            // that's the one and only chance the operator has to read it.
            using var ctx = InMemoryDbContextFactory.Create();
            var (user, station) = await SeedUserAndStationAsync(ctx);

            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);
            var mintResult = await service.CreateStationApiKey(user.Id, station.Id, "kiosk-secret-leak-test");
            Assert.True(mintResult.IsSuccess());
            Assert.False(string.IsNullOrWhiteSpace(mintResult.ReturnValue.Secret),
                    "mint must return the secret - only chance to read it");

            var listResult = await service.GetStationApiKeys(user.Id, station.Id);
            Assert.True(listResult.IsSuccess(), listResult.ToErrorString());
            foreach (var key in listResult.ReturnValue)
            {
                Assert.False(string.IsNullOrWhiteSpace(key.PublicKey),
                        "list must still return PublicKey + Name + CreateOnUtc");
                Assert.False(string.IsNullOrWhiteSpace(key.Name));
                Assert.Null(key.Secret);
            }
        }

        [Fact]
        public async Task CreateStationApiKey_generates_distinct_secrets_on_each_call()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var (user, station) = await SeedUserAndStationAsync(ctx);

            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);
            var a = await service.CreateStationApiKey(user.Id, station.Id, "kiosk-A");
            var b = await service.CreateStationApiKey(user.Id, station.Id, "kiosk-B");

            Assert.True(a.IsSuccess());
            Assert.True(b.IsSuccess());
            Assert.NotEqual(a.ReturnValue.PublicKey, b.ReturnValue.PublicKey);
            Assert.NotEqual(a.ReturnValue.Secret, b.ReturnValue.Secret);
        }

        [Fact]
        public async Task CreateStationApiKey_with_empty_userId_returns_InvalidUserId_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);

            var result = await service.CreateStationApiKey(Guid.Empty, Guid.NewGuid(), "kiosk-1");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.UserIdentityInvalidId));
        }

        [Fact]
        public async Task CreateStationApiKey_for_station_owned_by_someone_else_returns_StationNotFound_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var (_, station) = await SeedUserAndStationAsync(ctx);
            var intruder = await SeedUserAsync(ctx, "intruder");

            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);
            var result = await service.CreateStationApiKey(intruder.Id, station.Id, "kiosk-1");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.StationNotFound));
        }

        [Fact]
        public async Task GetStationApiKey_with_invalid_guid_returns_PublicKeyInvalid_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);

            var result = await service.GetStationApiKey("definitely-not-a-guid");

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.StationPublicKeyInvalid));
        }

        [Fact]
        public async Task GetStationApiKey_with_unknown_guid_returns_PublicKeyNotFound_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var service = new StationApiKeyService(MockLogger.For<StationApiKeyService>(), ctx);

            var result = await service.GetStationApiKey(Guid.NewGuid().ToString("N"));

            Assert.True(result.HasError());
            Assert.True(result.ContainsKey(UserError.StationPublicKeyNotFound));
        }
    }
}
