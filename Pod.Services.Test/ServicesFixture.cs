#region Licence
/****************************************************************
 *  Filename: ServicesFixture.cs
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
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pod.Data;
using Pod.Data.Models;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Servers;
using Pod.Services.Accountant;
using Pod.Services.Administrator;
using Pod.Services.Applications;
using Pod.Services.ConnectHost;
using Pod.Services.Customer;
using Pod.Services.ServerManager;
using Pod.Services.ShellHost;
using Pod.Services.Station;
using Pod.Services.Support;
using Pod.Services.User;
using Pod.Test.Utilities;

namespace Pod.Services.Test
{
    public class ServicesFixture : InfrastructureFixture
    {
        protected override void RegisterOwnServices(ServiceCollection services)
        {
            services.AddScoped<ConnectService>().
                     AddScoped<AccountantService>().
                     AddScoped<AdminService>().
                     AddScoped<StationService>().
                     AddScoped<CustomerSubscriptionService>().
                     AddScoped<CustomerSupportService>().
                     AddScoped<ServerManagerService>().
                     AddScoped<UserService>().
                     AddScoped<ShellService>().
                     AddScoped<ShellApplicationService>().
                     AddScoped<IUniqueAppFactory, UniqueAppFactory>().
                     AddSingleton(typeof(PublisherHub<>)).
                     AddSingleton<StationResponseHub>().
                     AddSingleton<ShellServer>(
                             provider =>
                             {
                                 using(var scope = provider.CreateScope())
                                 {
                                     var dbContext = scope.ServiceProvider.GetRequiredService<PodDbContext>();
                                     var server = dbContext.Servers.Find(1L);
                                     if(server == null)
                                         throw new NotSupportedException(
                                                 "There must be at least one ShellServer available to run this service");
                                     return server;
                                 }
                             });
        }
        public PodDbContext GetDbContext()
        {
            return GetServiceProvider().GetRequiredService<PodDbContext>();
        }
    }
}
