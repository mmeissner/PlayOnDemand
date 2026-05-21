#region Licence
/****************************************************************
 *  Filename: PodDbContext.cs
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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data.Config;
using Pod.Data.Exceptions;
using Pod.Data.Models;
using Pod.Data.Models.Billing;
using Pod.Data.Models.Mail;
using Pod.Data.Models.Servers;
using Pod.Data.Models.Shell;
using Pod.Data.Models.Users;

namespace Pod.Data
{
    /// <summary>
    /// Applications Database Context of EF Core
    /// </summary>
    public class PodDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly bool _logConcurrencyExceptionDetails;
        private static readonly LoggerFactory _loggerFactory =
                new LoggerFactory(
                        new[]
                        {
                                //Outputs Information about the SQL Query if in Debug Mode
                                //Does not log information in Release Mode
                                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
                        });
        #region Constructor 
        internal PodDbContext(DbContextOptions<PodDbContext> options, bool logConcurrencyExceptionDetails = false)
                : base(options)
        {
            _logConcurrencyExceptionDetails = logConcurrencyExceptionDetails;
        }
        #endregion


        #region DbSet
        public DbSet<ShellServer> Servers { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<StationApiKey> StationApiKeys { get; set; }
        public DbSet<ApplicationRoot> ApplicationRoots { get;set; }
        public DbSet<DeviceIdentity> DeviceIdentities { get; set; }
        public DbSet<StationSettings> StationSettings { get; set; }
        public DbSet<SubscriptionState> SubscriptionStates { get; set; }
        public DbSet<ConnectionState> ConnectionStates { get; set; }
        public DbSet<SessionDetails> SessionDetails { get; set; }
        public DbSet<SubscriptionOrder> SubscriptionOrders { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<SubscriptionChange> SubscriptionChanges { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<LocalApp> LocalApps { get; set; }
        public DbSet<UniqueApp> UniqueApps { get; set; }
        public DbSet<ClosedConnection> ClosedConnections { get; set; }
        public DbSet<EMailAccountData> EMailAccounts { get; set; }
        public DbSet<EmailContentTemplate> EmailContentTemplates { get; set; }
        public DbSet<EMailAccountDataEMailContentTemplate> EmailAccTemplateLinks { get; set; }
        public DbSet<EmailSendOrder> EmailSendOrders { get; set; }
        #endregion

        #region Model Config

        /// <summary>
        /// Configures Entity Models
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MUST call base first: Identity (since v10) registers additional schema
            // (notably IdentityUserPasskey<TKey> / IdentityPasskeyData). Without this
            // call, those entities are auto-discovered through navigations but never
            // get a primary key, breaking model build.
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityUserConfig(modelBuilder));
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityUserClaimConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityUserLoginConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityUserTokenConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityRolesConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityRoleClaimsConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.IdentityUserRoleConfig());

            modelBuilder.ApplyConfiguration(new ModelConfig.ShellServerConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.StationConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.ApplicationRootConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.StationSettingsConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SubscriptionStateConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.ConnectionStateConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SessionDetailsConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.DeviceIdentityConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.StationApiKeyConfig());

            modelBuilder.ApplyConfiguration(new ModelConfig.ClosedConnectionConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SessionConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.ChangeRequestConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SessionRuleConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SessionRuleLocalAppConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.LocalAppConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SubscriptionOrderConfig(modelBuilder));
            modelBuilder.ApplyConfiguration(new ModelConfig.SubscriptionPaymentConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.SubscriptionChangeConfig());

            modelBuilder.ApplyConfiguration(new ModelConfig.EMailAccountDataConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.EmailContentTemplateConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.ContentTemplateVariableConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.EMailAccountDataEMailContentTemplateConfig());
            modelBuilder.ApplyConfiguration(new ModelConfig.EmailSendOrderConfig());
        }
        #endregion

        /// <summary>
        /// Configures the DbContext
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }

        #region Overrides to handle concurrency exceptions
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if(!_logConcurrencyExceptionDetails)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch(DbUpdateConcurrencyException e)
            {
                throw DetailedConcurrencyException.Wrap(e);
            }
        }

        public override async Task<int> SaveChangesAsync(
                bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if(!_logConcurrencyExceptionDetails)
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch(DbUpdateConcurrencyException e)
            {
                throw DetailedConcurrencyException.Wrap(e);
            }
        }

        public override int SaveChanges()
        {
            if(!_logConcurrencyExceptionDetails)
            {
                return base.SaveChanges();
            }

            try
            {
                return base.SaveChanges();
            }
            catch(DbUpdateConcurrencyException e)
            {
                throw DetailedConcurrencyException.Wrap(e);
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if(!_logConcurrencyExceptionDetails)
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }

            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch(DbUpdateConcurrencyException e)
            {
                throw DetailedConcurrencyException.Wrap(e);
            }
        }
        #endregion
    }
}