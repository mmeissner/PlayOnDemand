#region Licence
/****************************************************************
 *  Filename: ContextFactories.cs
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using static System.String;

namespace Pod.Data
{
    /// <summary>
    /// Creates a DB Context based on provided configuration
    /// Supports Migrations and with fallback to a design time specific appsettings file.
    /// This is useful if an ef command is issued from the project directory of this library
    /// In this case a appsettings.DesignTime.json can be included that does not conflict with
    /// other appsetting files of projects
    /// </summary>
    public class PodDbContextFactory : IDesignTimeDbContextFactory<PodDbContext>
    {
        public const string DesignTimeAppSettingsFile = "appsettings.DesignTime.json";
        private readonly DbContextOptionsBuilder<PodDbContext> _dbContextOptions;
        private readonly DbContextOptionsBuilder<PodDbContext> _dbContextMigrationOptions;
        private readonly bool _canCreateDbContext = false;
        private readonly bool _logConcurrencyException = false;

        /// <summary>
        /// This constructor is dedicated for command line utilities as
        /// eg. dotnet ef migrations add/list and so on
        /// </summary>
        public PodDbContextFactory()
        {
            _dbContextMigrationOptions = BuildOptions(
                    DesignTimeAppSettingsFile,
                    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds),
                    null);
        }


        /// <summary>
        /// Creates the Factory
        /// </summary>
        /// <param name="loggerFactory">If any logging is enabled then the logger factory needs to be provided</param>
        /// <param name="configuration">The Configuration with the Connection Strings</param>
        /// <param name="factoryConfig">The Config Settings for the DBContext</param>
        public PodDbContextFactory(
                IConfiguration configuration, DbContextFactoryConfig factoryConfig, ILoggerFactory loggerFactory)
        {
            _dbContextOptions = BuildOptions(configuration, factoryConfig, null, loggerFactory);
            _dbContextMigrationOptions = BuildOptions(
                    configuration,
                    factoryConfig,
                    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds),
                    loggerFactory);
            _logConcurrencyException = factoryConfig.LogConcurrencyExceptionDetails;
            _canCreateDbContext = true;
        }

        /// <summary>
        /// Creates a DbContext for general use
        /// </summary>
        /// <returns></returns>
        public PodDbContext Create()
        {
            if(_canCreateDbContext) return new PodDbContext(_dbContextOptions.Options, _logConcurrencyException);
            throw new InvalidOperationException(
                    "A DbContext can not be created as there is no sufficient configuration provided");
        }

        /// <summary>
        /// Creates a DbContext for Migrations only
        /// If no configuration objects are provided during construction of the factory,
        /// the factory will try to find an design time specific appsettings file
        /// </summary>
        /// <param name="args">not supported</param>
        /// <returns></returns>
        public PodDbContext CreateDbContext(string[] args)
        {
            return new PodDbContext(_dbContextMigrationOptions.Options, _logConcurrencyException);
        }

        /// <summary>
        /// Builds DbContext options with settings from an Json OptionsFile
        /// </summary>
        /// <param name="settingsFile">The name of the appsettings json file to use</param>
        /// <param name="npgsqlOptionsAction"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        DbContextOptionsBuilder<PodDbContext> BuildOptions(
                string settingsFile,
                Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null,
                ILoggerFactory loggerFactory = null)
        {
            if(IsNullOrWhiteSpace(settingsFile))
            {
                throw new InvalidOperationException($"{nameof(settingsFile)} can not be null or whitespace!");
            }

            var appSettingsConfig =
                    new ConfigurationBuilder().AddJsonFile(settingsFile, optional: false, reloadOnChange: false).Build();
            var contextConfig = appSettingsConfig.GetSection(nameof(DbContextFactoryConfig)).Get<DbContextFactoryConfig>();
            return BuildOptions(appSettingsConfig, contextConfig, npgsqlOptionsAction, loggerFactory);
        }

        /// <summary>
        /// Builds an DbContext Option with the settings from the provided objects
        /// </summary>
        /// <param name="appSettingsConfig"></param>
        /// <param name="contextConfig"></param>
        /// <param name="npgsqlOptionsAction"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        DbContextOptionsBuilder<PodDbContext> BuildOptions(
                IConfiguration appSettingsConfig,
                DbContextFactoryConfig contextConfig,
                Action<NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = null,
                ILoggerFactory loggerFactory = null)
        {
            //Check required Config
            if(appSettingsConfig == null || contextConfig == null)
            {
                throw new InvalidOperationException(
                        $"{nameof(IConfiguration)} and {nameof(DbContextFactoryConfig)} can not be null!");
            }

            //Prepare the Connection string
            var connectionString = appSettingsConfig.GetConnectionString(contextConfig.ConnectionStringName);

            //Create the Options Builder and build the options
            var optionsBuilder = new DbContextOptionsBuilder<PodDbContext>();
            if(contextConfig.LogEntityFramework)
            {
                if(loggerFactory == null)
                    throw new InvalidOperationException(
                            "Logging is enabled but there is no ILoggerFactory provided. Check your IOC Config or Constructor parameters");
                optionsBuilder.UseLoggerFactory(loggerFactory);
            }

            optionsBuilder.EnableSensitiveDataLogging(contextConfig.LogSensitiveData).
                           UseNpgsql(connectionString, npgsqlOptionsAction);

            return optionsBuilder;
        }
    }
    
    /// <summary>
    /// Configruation for Entity Framework
    /// </summary>
    public class DbContextFactoryConfig
    {
        /// <summary>
        /// The Name to of the Connection String to use for Db Connections
        /// </summary>
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Allows to enable logging for EF
        /// </summary>
        public bool LogEntityFramework { get; set; }

        /// <summary>
        /// Allows logging of Concurrency exceptions
        /// </summary>
        public bool LogConcurrencyExceptionDetails { get; set; }

        /// <summary>
        /// Allows to write query variables into logs that might expose sensitive data
        /// </summary>
        public bool LogSensitiveData { get; set; }
    }
}