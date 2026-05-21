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

namespace Pod.Data.Test.Fixtures
{
    /// <summary>
    /// Reflection-based factory for instantiating <see cref="PodDbContext"/> against EF Core
    /// InMemory. PodDbContext has an internal constructor, so we bypass DI activation here.
    /// </summary>
    public static class InMemoryDbContextFactory
    {
        public static PodDbContext Create(string testName = null)
        {
            var dbName = $"PodDataTest-{testName ?? Guid.NewGuid().ToString("N")}";
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
                    "Could not locate the internal PodDbContext(DbContextOptions, bool) constructor via reflection.");
            }

            var context = (PodDbContext)ctor.Invoke(new object[] { options, false });
            context.Database.EnsureCreated();
            return context;
        }
    }
}
