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
using Pod.Data;

namespace Pod.Web.Center.Test.Fixtures
{
    /// <summary>
    /// Factory for creating isolated <see cref="PodDbContext"/> instances backed by EF Core's
    /// InMemory provider. Each call to <see cref="Create"/> returns a context against a fresh
    /// database (unique name), so tests can run in parallel without interference.
    ///
    /// PodDbContext has an internal constructor, so we instantiate via reflection to avoid
    /// modifying Pod.Data with InternalsVisibleTo. This mirrors the helper in Pod.Services.Test.
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        public static PodDbContext Create(string testName = null)
        {
            var dbName = $"PodWebTest-{testName ?? Guid.NewGuid().ToString("N")}";
            var options = new DbContextOptionsBuilder<PodDbContext>()
                .UseInMemoryDatabase(dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
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

            var context = (PodDbContext)ctor.Invoke(new object[] { options, false });
            context.Database.EnsureCreated();
            return context;
        }
    }
}
