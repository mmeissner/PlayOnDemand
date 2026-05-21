#region Licence
/****************************************************************
 *  Filename: InMemoryDbContextFactory.cs
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
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pod.Data;

namespace Pod.Services.Test.TestFixtures
{
    /// <summary>
    /// Test helper that constructs a fresh <see cref="PodDbContext"/> backed by EF Core's
    /// in-memory provider, with a unique database name per call so individual test methods
    /// stay isolated.
    ///
    /// <para><b>Why reflection?</b> The <c>PodDbContext</c> constructor is <c>internal</c> to
    /// the <c>Pod.Data</c> assembly. The migration scope for this branch deliberately leaves
    /// <c>Pod.Data</c> untouched, so we use reflection here rather than adding an
    /// <c>InternalsVisibleTo</c> attribute to that assembly. This is test-only infrastructure.</para>
    ///
    /// <para><b>Why a model customizer?</b> ASP.NET Identity in .NET 10 added an
    /// <c>IdentityPasskeyData</c> entity that is automatically registered when a context
    /// derives from <c>IdentityDbContext&lt;...&gt;</c>. <c>PodDbContext.OnModelCreating</c>
    /// does not call <c>base.OnModelCreating(modelBuilder)</c> and applies its own
    /// configurations to the legacy Identity entities only, so the new passkey entity is
    /// left without a primary key. Tests can't fix the production OnModelCreating without
    /// touching <c>Pod.Data</c>; we instead inject an <c>IModelCustomizer</c> that runs
    /// after the user's <c>OnModelCreating</c> and explicitly Ignore()s the unconfigured
    /// types.</para>
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="PodDbContext"/> backed by a fresh in-memory database.
        /// </summary>
        /// <param name="databaseName">
        /// Optional database name. When omitted a fresh GUID-based name is generated so each
        /// invocation receives an isolated store.
        /// </param>
        public static PodDbContext Create(string databaseName = null)
        {
            databaseName ??= "PodTest-" + Guid.NewGuid().ToString("N");

            var options = new DbContextOptionsBuilder<PodDbContext>()
                    .UseInMemoryDatabase(databaseName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .EnableSensitiveDataLogging()
                    .ReplaceService<IModelCustomizer, IgnoreUnconfiguredIdentityModelCustomizer>()
                    .Options;

            var ctor = typeof(PodDbContext).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(DbContextOptions<PodDbContext>), typeof(bool) },
                    modifiers: null);

            if (ctor == null)
            {
                throw new InvalidOperationException(
                        "Could not locate the internal PodDbContext(DbContextOptions, bool) constructor via reflection. " +
                        "Has the constructor signature changed?");
            }

            return (PodDbContext)ctor.Invoke(new object[] { options, false });
        }
    }

    /// <summary>
    /// Runs the production <see cref="ModelCustomizer"/> first, then ignores the .NET 10
    /// Identity entities that <c>PodDbContext.OnModelCreating</c> doesn't configure.
    /// </summary>
    internal sealed class IgnoreUnconfiguredIdentityModelCustomizer : ModelCustomizer
    {
        public IgnoreUnconfiguredIdentityModelCustomizer(ModelCustomizerDependencies dependencies)
                : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            // Identity 10 added new entity types (IdentityPasskeyData and the generic
            // IdentityUserPasskey<TKey>) that PodDbContext.OnModelCreating doesn't
            // configure (it doesn't call base.OnModelCreating). Sweep the model after
            // the user's OnModelCreating runs and Ignore any Identity-namespaced types
            // whose name contains "Passkey".
            var passkeyTypes = new global::System.Collections.Generic.List<Type>();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var ns = entityType.ClrType.Namespace ?? string.Empty;
                if (ns.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal) &&
                    entityType.ClrType.Name.Contains("Passkey", StringComparison.Ordinal))
                {
                    passkeyTypes.Add(entityType.ClrType);
                }
            }
            foreach (var t in passkeyTypes)
            {
                modelBuilder.Ignore(t);
            }
        }
    }
}
