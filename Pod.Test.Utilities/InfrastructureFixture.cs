#region Licence
/****************************************************************
 *  Filename: InfrastructureFixture.cs
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Pod.Data;
using Pod.Data.Config;
using Pod.Data.Models.Users;

namespace Pod.Test.Utilities
{
    public abstract class InfrastructureFixture  
    {
        private static bool dbCleared = false;
        private static readonly object _dbclearLock = new object();

        private static int _userCounter = 0;
        public const string Password = "Password-1234";

        private bool _serviceProviderBuild = false;
        private readonly object _serviceProviderLock = new object();
        private ServiceProvider _serviceProvider;

        protected abstract void RegisterOwnServices(ServiceCollection services);

        public ServiceProvider GetServiceProvider()
        {
            if(_serviceProviderBuild) return _serviceProvider;
            lock(_serviceProviderLock)
            {
                if(_serviceProviderBuild) return _serviceProvider;
                var services = new ServiceCollection();
                RegisterDefaultServices(services);
                RegisterOwnServices(services);
                _serviceProvider = services.BuildServiceProvider();
                _serviceProviderBuild = true;
                return _serviceProvider;
            }
        }

        public void EnsureClearDb()
        {
            if(dbCleared) return;
            lock(_dbclearLock)
            {
                if(dbCleared) return;
                ((PodDbContext)GetServiceProvider().GetRequiredService(typeof(PodDbContext))).Database.EnsureDeleted();
                var dbInitializer =
                        GetServiceProvider().GetRequiredService(typeof(ContextInitializer)) as ContextInitializer;
                if(dbInitializer == null)
                    throw new NullReferenceException($"Could not get {typeof(ContextInitializer)}");
                dbInitializer.Initialize();
                dbCleared = true;
            }
        }

        private void RegisterDefaultServices(ServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder().SetBasePath(Environment.CurrentDirectory).
                                                                      AddJsonFile("appsettings.json", optional: true).
                                                                      //Add your UserSecretsId Here
                                                                      AddUserSecrets("8d0f9b82-d878-4917-b968-c977fc85f9b9").
                                                                      AddEnvironmentVariables().
                                                                      Build();
            services.AddSingleton(configuration);
            configuration.Bind(nameof(DbContextFactoryConfig));
            configuration.Bind(nameof(ConfigSuperuser));

            services.AddSingleton(configuration.GetSection(nameof(DbContextFactoryConfig)).Get<DbContextFactoryConfig>());
            services.AddSingleton(configuration.GetSection(nameof(ConfigSuperuser)).Get<ConfigSuperuser>());
            services.AddSingleton(configuration.GetSection(nameof(ConfigShellServer)).Get<ConfigShellServer>());

            //Register with resolver as class has also a empty constructor for commandline utils
            services.AddSingleton<PodDbContextFactory>(resolver => new PodDbContextFactory(
                                                               resolver.GetRequiredService<IConfiguration>(),
                                                               resolver.GetRequiredService<DbContextFactoryConfig>(),
                                                               resolver.GetRequiredService<ILoggerFactory>()));
            services.AddSingleton<IDesignTimeDbContextFactory<PodDbContext>, PodDbContextFactory>();
            services.AddSingleton<ContextInitializer>();
            services.AddTransient<PodDbContext>(resolver => resolver.GetService<PodDbContextFactory>().Create());
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddIdentity<ApplicationUser, ApplicationRole>(
                             options =>
                             {
                                 options.Password.RequireDigit = true;
                                 options.Password.RequireLowercase = true;
                                 options.Password.RequireNonAlphanumeric = true;
                                 options.Password.RequireUppercase = true;
                                 options.Password.RequiredLength = 10;
                                 options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                                 options.Lockout.MaxFailedAccessAttempts = 4;
                                 options.Lockout.AllowedForNewUsers = false;
                                 options.SignIn.RequireConfirmedEmail = true;
                             }).
                     AddDefaultTokenProviders().
                     AddEntityFrameworkStores<PodDbContext>();
            services.AddSingleton<IServiceProvider>(provider => provider);
            services.AddLogging();
        }

        public async Task<ApplicationUser> CreateTestUser()
        {
            var num = Interlocked.Increment(ref _userCounter);
            var email = $"TestUser_{num}@Testmail.com";
            var user = new ApplicationUser {UserName = $"TestUser_{num}", Email = email};
            using(var scope = GetServiceProvider().CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var result = await userManager.CreateAsync(user, Password);
                if(!result.Succeeded)throw new InvalidOperationException();
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                result = await userManager.ConfirmEmailAsync(user, code);
                if(!result.Succeeded)throw new InvalidOperationException();
                return user;
            }
        }
    }
}
