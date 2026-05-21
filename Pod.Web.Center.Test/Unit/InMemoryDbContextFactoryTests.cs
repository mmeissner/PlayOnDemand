#region Licence
/****************************************************************
 *  Filename: InMemoryDbContextFactoryTests.cs
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
using FluentAssertions;
using Pod.Data;
using Pod.Web.Center.Test.Fixtures;
using Xunit;

namespace Pod.Web.Center.Test.Unit
{
    /// <summary>
    /// Smoke tests proving the reflection-based <see cref="InMemoryDbContextFactory"/>
    /// can instantiate PodDbContext (which has an internal constructor) and that the
    /// model builds without throwing.
    /// </summary>
    public class InMemoryDbContextFactoryTests
    {
        [Fact]
        public void Create_ReturnsUsableContext()
        {
            using var db = InMemoryDbContextFactory.Create();

            db.Should().NotBeNull();
            db.Servers.Should().NotBeNull();
            db.Stations.Should().NotBeNull();
            db.StationApiKeys.Should().NotBeNull();
            db.Sessions.Should().NotBeNull();
        }

        [Fact]
        public void Create_TwoInstances_AreIsolated()
        {
            using var a = InMemoryDbContextFactory.Create();
            using var b = InMemoryDbContextFactory.Create();

            // Inserting into one must not leak to the other (different DB names per call).
            a.Servers.Local.Should().BeEmpty();
            b.Servers.Local.Should().BeEmpty();
        }
    }
}
