#region Licence
/****************************************************************
 *  Filename: ContextInitializer.cs
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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Pod.Data
{
    /// <summary>
    /// Will run Migrations and allows to add fixes between specific migrations, these could be anything that might need to manipulate data between migrations
    /// Be aware that this initializer will not be called or used by EF Commandline utilities,
    /// these means you should not use ef migrations database update command if you want to have any of this code executed 
    /// Anyway creation of migrations needs still to be done with the command line utilities.
    /// This initializer will also execute all <see cref="IDbSetupTask"/> that can be used to populate the database
    /// </summary>
    public class ContextInitializer
    {
        private readonly IDesignTimeDbContextFactory<PodDbContext> _dbContextFactory;
        private readonly IServiceProvider _serviceProvider;

        public ContextInitializer(IDesignTimeDbContextFactory<PodDbContext> dbContextFactory, IServiceProvider serviceProvider)
        {
            
            _dbContextFactory = dbContextFactory;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Async version of <see cref="Initialize"/>
        /// </summary>
        /// <returns>Task</returns>
        public async Task InitializeAsync()
        {
            using(var dbContext = _dbContextFactory.CreateDbContext(
                    new string[]
                    //see https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory"
                    {
                            "unused"
                    }))
            {
                // Skip migrations for non-relational providers (InMemory tests).
                if (dbContext.Database.IsRelational())
                {
                    // Create database if does not exists, execute all pending Migrations.
                    // Run Migrations Step by Step and allows to fix data between
                    var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                    if(pendingMigrations.Any())
                    {
                        var migratorService = dbContext.Database.GetService<IMigrator>();
                        foreach(var targetMigration in pendingMigrations) migratorService.Migrate(targetMigration);
                    }
                }
            }

            //Get all Setup Tasks to know about them
            var setupTasks = _serviceProvider.GetServices<IDbSetupTask>();
            if (setupTasks == null) return;

            //Order SetupTasks
            foreach (var setupTask in setupTasks.OrderBy(x => x.Priority))
            {
                //Execute each SetupTask in its own scope to prevent interference
                using (var taskScope = _serviceProvider.CreateScope())
                {
                    var task = taskScope.ServiceProvider.GetRequiredService(setupTask.GetType()) as IDbSetupTask;
                    task?.Execute();
                }
            }
        }

        /// <summary>
        /// Creates Db, applies pending migrations and calls each <see cref="IDbSetupTask"/> to populate the db
        /// </summary>
        /// <returns>Task</returns>
        public void Initialize()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext(
                    new string[]
                    //see https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation#from-a-design-time-factory"
                    {
                            "unused"
                    }))
            {
                // Skip migrations for non-relational providers (InMemory tests). The model
                // is materialised via EnsureCreated() in the test fixtures instead.
                if (dbContext.Database.IsRelational())
                {
                    // Create database if does not exists, execute all pending Migrations.
                    // Run Migrations Step by Step and allows to fix data between
                    var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
                    if (pendingMigrations.Any())
                    {
                        var migratorService = dbContext.Database.GetService<IMigrator>();
                        foreach (var targetMigration in pendingMigrations) migratorService.Migrate(targetMigration);
                    }
                }
            }

            //Get all Setup Tasks to know about them
            var setupTasks = _serviceProvider.GetServices<IDbSetupTask>();
            if (setupTasks == null) return;

            //Order SetupTasks
            foreach (var setupTask in setupTasks.OrderBy(x => x.Priority))
            {
                //Execute each SetupTask in its own scope to prevent interference
                using (var taskScope = _serviceProvider.CreateScope())
                {
                    var task = taskScope.ServiceProvider.GetRequiredService(setupTask.GetType()) as IDbSetupTask;
                    task?.Execute();
                }
            }
        }
    }
}