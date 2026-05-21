#region Licence
/****************************************************************
 *  Filename: IdentityTestHarness.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Models.Users;

namespace Pod.Services.Test.TestFixtures
{
    /// <summary>
    /// Spins up a minimal DI container with EF-Core-backed ASP.NET Identity (using the
    /// in-memory provider) so tests can exercise <see cref="UserManager{TUser}"/> and
    /// <see cref="SignInManager{TUser}"/> without standing up the whole web host.
    /// Each <see cref="Build"/> call returns an isolated container with a fresh database.
    /// </summary>
    public sealed class IdentityTestHarness : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _databaseName;

        private IdentityTestHarness(ServiceProvider serviceProvider, string databaseName)
        {
            _serviceProvider = serviceProvider;
            _databaseName = databaseName;
        }

        public IServiceProvider Services => _serviceProvider;

        /// <summary>
        /// The database name backing this harness. Pass it to
        /// <see cref="InMemoryDbContextFactory.Create"/> if you need a second context that
        /// sees the same data.
        /// </summary>
        public string DatabaseName => _databaseName;

        public PodDbContext CreateDbContext()
        {
            // Reuse the same database name so the harness's DbContext sees the same data
            // as UserManager / SignInManager.
            return InMemoryDbContextFactory.Create(_databaseName);
        }

        public UserManager<ApplicationUser> UserManager => _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        public SignInManager<ApplicationUser> SignInManager => _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        public RoleManager<ApplicationRole> RoleManager => _serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        /// <summary>
        /// Creates a confirmed user with the supplied credentials and returns the persisted entity.
        /// </summary>
        public async Task<ApplicationUser> CreateConfirmedUserAsync(string userName, string email, string password)
        {
            var user = new ApplicationUser { UserName = userName, Email = email };
            var createResult = await UserManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                        "Failed to create test user: " + string.Join("; ", createResult.Errors));
            }
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmResult = await UserManager.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
            {
                throw new InvalidOperationException(
                        "Failed to confirm test user email: " + string.Join("; ", confirmResult.Errors));
            }
            return user;
        }

        /// <summary>
        /// Build a fresh harness against an in-memory database with a unique name.
        /// </summary>
        public static IdentityTestHarness Build(Action<IdentityOptions> configureIdentity = null)
        {
            var databaseName = "PodTestIdentity-" + Guid.NewGuid().ToString("N");

            var services = new ServiceCollection();
            services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Warning));

            // PodDbContext has an internal ctor; we cannot use the standard
            // services.AddDbContext<>() because it tries to call a public ctor. Instead
            // register the context as scoped via the InMemoryDbContextFactory which uses
            // reflection. Tests that share a database name will see the same data.
            services.AddScoped<PodDbContext>(_ => InMemoryDbContextFactory.Create(databaseName));

            services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
                    {
                        opts.Password.RequireDigit = true;
                        opts.Password.RequireLowercase = true;
                        opts.Password.RequireUppercase = true;
                        opts.Password.RequireNonAlphanumeric = true;
                        opts.Password.RequiredLength = 10;
                        opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        opts.Lockout.MaxFailedAccessAttempts = 4;
                        opts.Lockout.AllowedForNewUsers = true;
                        opts.SignIn.RequireConfirmedEmail = true;
                        configureIdentity?.Invoke(opts);
                    })
                    .AddEntityFrameworkStores<PodDbContext>()
                    .AddDefaultTokenProviders();

            // SignInManager needs an HttpContextAccessor; CheckPasswordSignInAsync (the only
            // path our tests exercise) doesn't really use it, but DI still resolves it.
            services.AddHttpContextAccessor();
            services.AddAuthentication();

            var provider = services.BuildServiceProvider();
            return new IdentityTestHarness(provider, databaseName);
        }

        public void Dispose() => _serviceProvider.Dispose();
    }
}
