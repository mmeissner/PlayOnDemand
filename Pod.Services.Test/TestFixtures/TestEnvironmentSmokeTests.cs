#region Licence
/****************************************************************
 *  Filename: TestEnvironmentSmokeTests.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Pod.Data.Models.Users;
using Xunit;

namespace Pod.Services.Test.TestFixtures
{
    /// <summary>
    /// Sanity-check tests for the test infrastructure itself. If these fail the rest of the
    /// suite is meaningless.
    /// </summary>
    public class TestEnvironmentSmokeTests
    {
        [Fact]
        public void InMemoryDbContextFactory_creates_a_usable_context()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            Assert.NotNull(ctx);
            Assert.False(ctx.Stations.Any());
        }

        [Fact]
        public void Two_factory_calls_with_different_names_return_isolated_databases()
        {
            using var ctxA = InMemoryDbContextFactory.Create("env-smoke-a");
            using var ctxB = InMemoryDbContextFactory.Create("env-smoke-b");
            // EF Core's in-memory provider is intentionally non-relational.
            Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", ctxA.Database.ProviderName);
            Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", ctxB.Database.ProviderName);
            // Default name behaviour: each unnamed Create() gets a fresh name.
            using var ctxC = InMemoryDbContextFactory.Create();
            using var ctxD = InMemoryDbContextFactory.Create();
            Assert.NotNull(ctxC);
            Assert.NotNull(ctxD);
        }

        [Fact]
        public async Task IdentityTestHarness_resolves_a_working_UserManager_and_SignInManager()
        {
            using var harness = IdentityTestHarness.Build();
            Assert.NotNull(harness.UserManager);
            Assert.NotNull(harness.SignInManager);
            Assert.NotNull(harness.RoleManager);

            var user = await harness.CreateConfirmedUserAsync(
                    "smoke-user",
                    "smoke@example.com",
                    "Password-1234");
            Assert.NotNull(user);
            Assert.NotEqual(global::System.Guid.Empty, user.Id);

            var found = await harness.UserManager.FindByNameAsync("smoke-user");
            Assert.NotNull(found);
            Assert.Equal(user.Id, found.Id);
            Assert.True(await harness.UserManager.IsEmailConfirmedAsync(found));
        }
    }
}
