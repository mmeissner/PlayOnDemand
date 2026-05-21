#region Licence
/****************************************************************
 *  Filename: ModelConfig.cs
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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pod.Data.Converter;
using Pod.Data.Models.Billing;
using Pod.Data.Models.Mail;
using Pod.Data.Models.Servers;
using Pod.Data.Models.Shell;
using Pod.Data.Models.Users;

namespace Pod.Data.Config
{
    /// <summary>
    /// Backwards-compatible shim for Npgsql's pre-3.0 ForNpgsqlUseXminAsConcurrencyToken / UseXminAsConcurrencyToken
    /// (Npgsql.EFCore 10 dropped the entity-level helper; this re-implements it via a shadow property).
    /// </summary>
    internal static class XminConcurrencyExtensions
    {
        public static EntityTypeBuilder<TEntity> UseXminAsConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : class
        {
            builder.Property<uint>("xmin")
                   .HasColumnName("xmin")
                   .HasColumnType("xid")
                   .ValueGeneratedOnAddOrUpdate()
                   .IsConcurrencyToken();
            return builder;
        }
    }

    /// <summary>
    /// Configuration for all Entity Models
    /// </summary>
    public static class ModelConfig
    {
        #region Identity
        public class IdentityUserConfig : IEntityTypeConfiguration<ApplicationUser>
        {
            private readonly ModelBuilder _modelBuilder;

            public IdentityUserConfig(ModelBuilder modelBuilder) { _modelBuilder = modelBuilder; }
            public void Configure(EntityTypeBuilder<ApplicationUser> entity)
            {
                _modelBuilder.HasSequence<long>("User_CustomerNumber_seq").StartsAt(100).IncrementsBy(1);

                entity.Metadata.FindNavigation(nameof(ApplicationUser.Stations)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.Property(x => x.CustomerNumber).HasDefaultValueSql("nextval('\"User_CustomerNumber_seq\"')");

                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.CustomerNumber).IsUnique();
                // Indexes for "normalized" username and email, to allow efficient lookups
                entity.HasIndex(x => x.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
                entity.HasIndex(x => x.NormalizedEmail).HasDatabaseName("EmailIndex");

                // A concurrency token for use with the optimistic concurrency checking
                entity.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                entity.Property(x => x.UserName).HasMaxLength(256);
                entity.Property(x => x.NormalizedUserName).HasMaxLength(256);
                entity.Property(x => x.Email).HasMaxLength(256);
                entity.Property(x => x.NormalizedEmail).HasMaxLength(256);
                entity.Property(x => x.EmailConfirmed).HasMaxLength(256);

                // The relationships between User and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each User can have many UserClaims
                entity.HasMany<IdentityUserClaim<Guid>>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();

                // Each User can have many UserLogins
                entity.HasMany<IdentityUserLogin<Guid>>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();

                // Each User can have many UserTokens
                entity.HasMany<IdentityUserToken<Guid>>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();

                // Each User can have many entries in the UserRole join table
                entity.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();

                entity.HasMany<Station>(x => x.Stations).
                       WithOne(x => x.ApplicationUser).
                       HasForeignKey(x => x.ApplicationUserId).
                       IsRequired();

                // Maps to the table
                entity.ToTable("Users");
            }
        }

        public class IdentityUserClaimConfig : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> entity)
            {
                // Primary key
                entity.HasKey(x => x.Id);

                // Maps to the table
                entity.ToTable("UserClaims");
            }
        }

        public class IdentityUserLoginConfig : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> entity)
            {
                // Composite primary key consisting of the LoginProvider and the key to use
                // with that provider
                entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });

                // Limit the size of the composite key columns due to common DB restrictions
                entity.Property(l => l.LoginProvider).HasMaxLength(128);
                entity.Property(l => l.ProviderKey).HasMaxLength(128);

                entity.ToTable("UserLogins");
            }
        }

        public class IdentityUserTokenConfig : IEntityTypeConfiguration<IdentityUserToken<Guid>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> entity)
            {
                // Composite primary key consisting of the UserId, LoginProvider and Name
                entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

                // Limit the size of the composite key columns due to common DB restrictions
                entity.Property(t => t.LoginProvider).HasMaxLength(1024);
                entity.Property(t => t.Name).HasMaxLength(1024);

                // Maps to the table
                entity.ToTable("UserTokens");
            }
        }

        public class IdentityRolesConfig : IEntityTypeConfiguration<ApplicationRole>
        {
            public void Configure(EntityTypeBuilder<ApplicationRole> entity)
            {
                // Primary key
                entity.HasKey(r => r.Id);

                // Index for "normalized" role name to allow efficient lookups
                entity.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();

                // A concurrency token for use with the optimistic concurrency checking
                entity.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                entity.Property(u => u.Name).HasMaxLength(256);
                entity.Property(u => u.NormalizedName).HasMaxLength(256);

                // The relationships between Role and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each Role can have many entries in the UserRole join table
                entity.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();

                // Each Role can have many associated RoleClaims
                entity.HasMany<IdentityRoleClaim<Guid>>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();

                // Maps to the table
                entity.ToTable("Roles");
            }
        }

        public class IdentityRoleClaimsConfig : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
        {
            public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> entity)
            {
                // Primary key
                entity.HasKey(rc => rc.Id);

                // Maps to the AspNetRoleClaims table
                entity.ToTable("RoleClaims");
            }
        }

        public class IdentityUserRoleConfig : IEntityTypeConfiguration<IdentityUserRole<Guid>>
        {
            public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> entity)
            {
                // Primary key
                entity.HasKey(r => new { r.UserId, r.RoleId });

                // Maps to the AspNetUserRoles table
                entity.ToTable("UserRoles");
            }
        }
        #endregion

        public class ShellServerConfig : IEntityTypeConfiguration<ShellServer>
        {
            public void Configure(EntityTypeBuilder<ShellServer> entity)
            {
                entity.Metadata.FindNavigation(nameof(ShellServer.ConnectionHistory)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(ShellServer.ConnectedClients)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { HostAddress = x.PublicHostAddress, Port = x.PublicPort }).IsUnique();
            }
        }

        public class StationConfig : IEntityTypeConfiguration<Station>
        {
            public void Configure(EntityTypeBuilder<Station> entity)
            {
                entity.Metadata.FindNavigation(nameof(Station.ConnectionHistory)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(Station.Sessions)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(Station.ApplicationRoots)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(Station.ApiKeys)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.Property(Station.NameOfPasswordHash());
                entity.HasMany<ApplicationRoot>(x => x.ApplicationRoots).
                       WithOne(x => x.Station).
                       HasForeignKey(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<StationSettings>(x => x.StationSettings).
                       WithOne(x => x.Station).
                       HasForeignKey<StationSettings>(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<SubscriptionState>(x => x.SubscriptionState).
                       WithOne(x => x.Station).
                       HasForeignKey<SubscriptionState>(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<ConnectionState>(x => x.ConnectionState).
                       WithOne(x => x.Station).
                       HasForeignKey<ConnectionState>(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SessionDetails).
                       WithOne(x => x.Station).
                       HasForeignKey<SessionDetails>(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<Session>(x => x.Sessions).
                       WithOne(x => x.Station).
                       HasForeignKey(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<ClosedConnection>(x => x.ConnectionHistory).
                       WithOne(x => x.Station).
                       HasForeignKey(x => x.StationId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<StationApiKey>(x => x.ApiKeys).
                       WithOne(x => x.Station).
                       HasForeignKey(x => x.StationId);

                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class ApplicationRootConfig : IEntityTypeConfiguration<ApplicationRoot>
        {
            public void Configure(EntityTypeBuilder<ApplicationRoot> entity)
            {
                entity.Metadata.FindNavigation(nameof(ApplicationRoot.LocalApps)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasIndex(x => new { x.StationId, x.DeviceIdentityId }).IsUnique();
                entity.HasOne<DeviceIdentity>(x => x.DeviceIdentity).
                       WithMany(x => x.ApplicationRoots).
                       HasForeignKey(x => x.DeviceIdentityId).
                       OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<LocalApp>(x => x.LocalApps).
                       WithOne(x => x.ApplicationRoot).
                       HasForeignKey(x => x.ApplicationRootId).
                       OnDelete(DeleteBehavior.Cascade);
            }
        }

        public class SubscriptionStateConfig : IEntityTypeConfiguration<SubscriptionState>
        {
            public void Configure(EntityTypeBuilder<SubscriptionState> entity)
            {
                entity.Metadata.FindNavigation(nameof(SubscriptionState.Orders)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(SubscriptionState.SubscriptionChanges)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);

                entity.Property(x => x.StartOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.ExpiresOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class ConnectionStateConfig : IEntityTypeConfiguration<ConnectionState>
        {
            public void Configure(EntityTypeBuilder<ConnectionState> entity)
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.DeviceIdentity).
                       WithOne(x => x.ConnectionState).
                       HasForeignKey<ConnectionState>(x => x.DeviceIdentityId);

                entity.HasOne(x => x.ShellServer).
                       WithMany(x => x.ConnectedClients).
                       HasForeignKey(x => x.ShellServerId);

                entity.Ignore(x => x.LoadedFromDatabaseUtc);
                entity.Property(x => x.ConnectedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.LastHeartbeatOnUtc).
                       HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class DeviceIdentityConfig : IEntityTypeConfiguration<DeviceIdentity>
        {
            public void Configure(EntityTypeBuilder<DeviceIdentity> entity)
            {
                entity.Metadata.FindNavigation(nameof(DeviceIdentity.ApplicationRoots)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(DeviceIdentity.ClosedConnections)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);

                entity.HasMany(x => x.ApplicationRoots).
                       WithOne(x => x.DeviceIdentity).
                       HasForeignKey(x => x.DeviceIdentityId);

                entity.HasMany(x => x.ClosedConnections).
                       WithOne(x => x.DeviceIdentity).
                       HasForeignKey(x => x.DeviceIdentityId).
                       IsRequired();
            }
        }

        public class ClosedConnectionConfig : IEntityTypeConfiguration<ClosedConnection>
        {
            public void Configure(EntityTypeBuilder<ClosedConnection> entity)
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.ConnectionId);
                entity.HasOne(x => x.Server).
                       WithMany(x => x.ConnectionHistory).
                       HasForeignKey(x => x.ServerId);

                entity.Property(x => x.RequestedServerOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.DisconnectedOnUtc).
                       HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class StationApiKeyConfig : IEntityTypeConfiguration<StationApiKey>
        {
            public void Configure(EntityTypeBuilder<StationApiKey> entity)
            {
                entity.HasKey(x => x.PublicKey);
                entity.HasOne(x => x.Station).
                       WithMany(x => x.ApiKeys).
                       HasForeignKey(x => x.StationId).
                       IsRequired(true);
                entity.Property(x => x.CreatedOnUtc).
                       HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
            }
        }

        public class StationSettingsConfig : IEntityTypeConfiguration<StationSettings>
        {
            public void Configure(EntityTypeBuilder<StationSettings> entity)
            {
                entity.HasKey(x => x.Id);
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class SessionDetailsConfig : IEntityTypeConfiguration<SessionDetails>
        {
            public void Configure(EntityTypeBuilder<SessionDetails> entity)
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Session).
                       WithOne(x => x.SessionDetails).
                       HasForeignKey<SessionDetails>(x => x.SessionId);

                entity.UseXminAsConcurrencyToken();
            }
        }

        public class SessionConfig : IEntityTypeConfiguration<Session>
        {
            public void Configure(EntityTypeBuilder<Session> entity)
            {
                entity.Metadata.FindNavigation(nameof(Session.ChangeRequests)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.HasMany(x => x.ChangeRequests).
                       WithOne(x => x.Session).
                       HasForeignKey(x => x.SessionId).
                       OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.SessionRule).
                       WithOne(x => x.Session).
                       HasForeignKey<Session>(x => x.SessionRuleId);

                entity.Property(x => x.RequestedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.SendOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.StartedUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.StoppedUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));

                entity.UseXminAsConcurrencyToken();
            }
        }

        public class ChangeRequestConfig : IEntityTypeConfiguration<ChangeRequest>
        {
            public void Configure(EntityTypeBuilder<ChangeRequest> entity)
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.CreatedOnUtc).
                       HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
            }
        }

        public class SessionRuleConfig : IEntityTypeConfiguration<SessionRule>
        {
            public void Configure(EntityTypeBuilder<SessionRule> entity)
            {
                entity.Metadata.FindNavigation(nameof(SessionRule.AllowedApps)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasKey(x => x.Id);
            }
        }

        public class SessionRuleLocalAppConfig : IEntityTypeConfiguration<SessionRuleLocalApp>
        {
            public void Configure(EntityTypeBuilder<SessionRuleLocalApp> entity)
            {
                entity.HasKey(x => new { x.SessionRuleId, x.LocalAppId });
                entity.HasOne(x => x.SessionRule).
                       WithMany(x => x.AllowedApps);
                entity.HasOne(x => x.LocalApp).
                       WithMany(x => x.SessionRules);
            }
        }

        public class LocalAppConfig : IEntityTypeConfiguration<LocalApp>
        {
            public void Configure(EntityTypeBuilder<LocalApp> entity)
            {
                entity.Metadata.FindNavigation(nameof(LocalApp.SessionRules)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.ApplicationRootId, x.UniqueAppId }).IsUnique();
                entity.HasIndex(x => x.InstanceVersion).IsUnique(false);
                entity.HasOne(x => x.ApplicationRoot).
                       WithMany(x => x.LocalApps).
                       HasForeignKey(x=> x.ApplicationRootId).
                       IsRequired();
                entity.HasOne(x => x.UniqueApp).
                       WithMany(x => x.LocalApps);
            }
        }

        public class UniqueAppConfig : IEntityTypeConfiguration<UniqueApp>
        {
            public void Configure(EntityTypeBuilder<UniqueApp> entity)
            {
                entity.Metadata.FindNavigation(nameof(UniqueApp.LocalApps)).SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
            }
        }

        public class SubscriptionOrderConfig : IEntityTypeConfiguration<SubscriptionOrder>
        {
            private readonly ModelBuilder _modelBuilder;

            public SubscriptionOrderConfig(ModelBuilder modelBuilder) { _modelBuilder = modelBuilder; }
            public void Configure(EntityTypeBuilder<SubscriptionOrder> entity)
            {
                _modelBuilder.HasSequence<long>("SubscriptionOrder_OrderNumber_seq").StartsAt(1000).IncrementsBy(1);
                entity.Property(x => x.OrderNumber).HasDefaultValueSql("nextval('\"SubscriptionOrder_OrderNumber_seq\"')");

                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.OrderNumber).IsUnique();
                entity.HasIndex(x => x.ApplicationUserId);
                entity.HasOne(x => x.SubscriptionPayment).
                       WithOne(x => x.SubscriptionOrder).
                       HasForeignKey<SubscriptionOrder>(x => x.SubscriptionPaymentId);
                entity.HasOne(x => x.SubscriptionState).
                       WithMany(x => x.Orders).
                       HasForeignKey(x => x.SubscriptionStateId);

                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.ExpiresOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class SubscriptionPaymentConfig : IEntityTypeConfiguration<SubscriptionPayment>
        {
            public void Configure(EntityTypeBuilder<SubscriptionPayment> entity)
            {
                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.PaymentReceivedDate).
                       HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.HasKey(x => x.Id);
            }
        }

        public class SubscriptionChangeConfig : IEntityTypeConfiguration<SubscriptionChange>
        {
            public void Configure(EntityTypeBuilder<SubscriptionChange> entity)
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.SubscriptionPayment).
                       WithOne(x => x.SubscriptionChange).
                       HasForeignKey<SubscriptionChange>(x => x.SubscriptionPaymentId);
                entity.HasOne(x => x.SubscriptionState).
                       WithMany(x => x.SubscriptionChanges).
                       HasForeignKey(x => x.SubscriptionStateId);
                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.ExtendsFromUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.ExtendsToUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.UseXminAsConcurrencyToken();
            }
        }

        public class EmailContentTemplateConfig : IEntityTypeConfiguration<EmailContentTemplate>
        {
            public void Configure(EntityTypeBuilder<EmailContentTemplate> entity)
            {
                entity.Metadata.FindNavigation(nameof(EmailContentTemplate.EmailAccounts)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.HasMany<EmailVariable>(x => x.Variables).
                       WithOne().
                       HasForeignKey(x => x.EmailContentTemplateId).
                       IsRequired(true).
                       OnDelete(DeleteBehavior.Cascade); ;
                //This FindNavigation Entries need to stay after the Many definition otherwise it will not find the Property
                entity.Metadata.FindNavigation(nameof(EmailContentTemplate.Variables)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
            }
        }

        public class ContentTemplateVariableConfig : IEntityTypeConfiguration<TemplateVariable>
        {
            public void Configure(EntityTypeBuilder<TemplateVariable> entity)
            {
                entity.HasKey(x => x.Id);
            }
        }

        public class EMailAccountDataConfig : IEntityTypeConfiguration<EMailAccountData>
        {
            public void Configure(EntityTypeBuilder<EMailAccountData> entity)
            {
                entity.Metadata.FindNavigation(nameof(EMailAccountData.EmailContentTemplates)).
                       SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasKey(x => x.Id);
            }
        }

        public class EMailAccountDataEMailContentTemplateConfig : IEntityTypeConfiguration<EMailAccountDataEMailContentTemplate>
        {
            public void Configure(EntityTypeBuilder<EMailAccountDataEMailContentTemplate> entity)
            {
                entity.HasKey(x => new { x.EMailAccountDataId, x.EMailContentTemplateId });
                entity.HasOne(x => x.EMailAccountData).
                       WithMany(x => x.EmailContentTemplates);
                entity.HasOne(x => x.EmailContentTemplate).
                       WithMany(x => x.EmailAccounts);
            }
        }

        public class EmailSendOrderConfig : IEntityTypeConfiguration<EmailSendOrder>
        {
            public void Configure(EntityTypeBuilder<EmailSendOrder> entity)
            {
                entity.Metadata.FindNavigation(nameof(EmailSendOrder.Variables)).SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.Metadata.FindNavigation(nameof(EmailSendOrder.Receivers)).SetPropertyAccessMode(PropertyAccessMode.Field);

                entity.HasKey(x => x.Id);
                entity.HasMany(x => x.Receivers).
                       WithOne(x => x.SendOrder).
                       HasForeignKey(x => x.EmailSendOrderId).
                       OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.Variables).
                       WithOne(x => x.SendOrder).
                       HasForeignKey(x => x.EmailSendOrderId).
                       OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => x.SendState);
                entity.Property(x => x.CreatedOnUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
                entity.Property(x => x.LastSendAttemptUtc).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y));
            }
        }
    }
}