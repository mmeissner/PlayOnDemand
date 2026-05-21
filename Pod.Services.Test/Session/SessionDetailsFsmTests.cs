#region Licence
/****************************************************************
 *  Filename: SessionDetailsFsmTests.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pod.Data;
using Pod.Data.Models.Shell;
using Pod.Data.Models.Users;
using Pod.Enums;
using Pod.Services.Station;
using Pod.Services.System;
using Pod.Services.Test.TestFixtures;
using Xunit;

namespace Pod.Services.Test.Session
{
    /// <summary>
    /// Tests for the Session FSM transitions that <c>ShellService</c> drives via
    /// <see cref="SessionDetails"/>. The spec calls this "SessionService" but the
    /// authoritative state machine lives on the <see cref="SessionDetails"/> aggregate
    /// (and is enforced by the <see cref="Pod.Data.Models.Shell.Session"/> entity it owns).
    /// We test the state-machine surface — Requested → Delivered → Started → Ended,
    /// plus Canceled, DeliveryTimeout, ResponseTimeout — against a persisted Station
    /// from the InMemory provider so the validation checks (e.g. StationId != Empty)
    /// pass.
    /// </summary>
    public class SessionDetailsFsmTests
    {
        private const string SourceIp = "127.0.0.1";
        private const string StationPassword = "Password-1234";

        private static async Task<Pod.Data.Models.Shell.Station> SeedStationAsync(PodDbContext ctx)
        {
            var user = new ApplicationUser
            {
                    Id = Guid.NewGuid(),
                    UserName = "fsm-user-" + Guid.NewGuid().ToString("N"),
                    NormalizedUserName = ("FSM-USER-" + Guid.NewGuid().ToString("N")).ToUpperInvariant(),
                    Email = "fsm@example.com",
                    NormalizedEmail = "FSM@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var stationService = new StationService(
                    MockLogger.For<StationService>(),
                    new PublisherHub<ClientCommandType>(),
                    new StationResponseHub(MockLogger.For<ResponderHub<ClientResponse>>()),
                    new SystemSettingsService(),
                    ctx);
            var vm = await stationService.CreateNewStation(user.Id, "FsmStation", StationPassword);
            Assert.True(vm.IsSuccess(), "Test seed: " + vm.ToErrorString());

            return await ctx.Stations
                    .Include(x => x.SessionDetails)
                        .ThenInclude(x => x.Session)
                    .FirstAsync(x => x.Id == vm.ReturnValue.StationId);
        }

        // ------------------------------------------------------------------
        // RequestSession
        // ------------------------------------------------------------------

        [Fact]
        public async Task RequestSession_with_no_existing_session_creates_a_Requested_session()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);

            var result = station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);

            Assert.True(result.IsSuccess());
            Assert.Equal(SessionResponse.Success, result.ReturnValue);
            Assert.NotNull(station.SessionDetails.Session);
            Assert.Equal(SessionState.Requested, station.SessionDetails.Session.State);
            Assert.Equal(RequestSource.WebApi, station.SessionDetails.Session.RequestedBy);
            Assert.Equal(SourceIp, station.SessionDetails.Session.RequestFromIpAddress);
        }

        [Fact]
        public async Task RequestSession_with_undefined_source_returns_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);

            var result = station.SessionDetails.RequestSession(RequestSource.Undefined, SourceIp);

            Assert.True(result.HasError());
        }

        // ------------------------------------------------------------------
        // RequestDelivery (Requested -> Delivered)
        // ------------------------------------------------------------------

        [Fact]
        public async Task RequestDelivery_after_RequestSession_transitions_to_Delivered()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            // SaveChanges propagates the new Session.Id back to SessionDetails.SessionId
            // via EF's FK-from-PK fixup, satisfying HasSession() on subsequent calls.
            await ctx.SaveChangesAsync();

            var connectionId = Guid.NewGuid();
            var result = station.SessionDetails.RequestDelivery(connectionId);

            Assert.True(result.IsSuccess());
            Assert.Equal(SessionResponse.Success, result.ReturnValue);
            Assert.Equal(SessionState.Delivered, station.SessionDetails.Session.State);
            Assert.Equal(connectionId, station.SessionDetails.Session.SendToConnectionId);
            Assert.NotNull(station.SessionDetails.Session.SendOnUtc);
        }

        [Fact]
        public async Task RequestDelivery_with_empty_connectionId_returns_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);

            var result = station.SessionDetails.RequestDelivery(Guid.Empty);

            Assert.True(result.HasError());
        }

        [Fact]
        public async Task RequestDelivery_with_no_active_session_returns_StateMismatch()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);

            var result = station.SessionDetails.RequestDelivery(Guid.NewGuid());

            Assert.Equal(SessionResponse.StateMismatch, result.ReturnValue);
        }

        // ------------------------------------------------------------------
        // SetResponse (Delivered -> Started, or Delivered -> Canceled)
        // ------------------------------------------------------------------

        [Fact]
        public async Task SetResponse_with_accepted_true_transitions_Delivered_to_Started()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            await ctx.SaveChangesAsync();
            var connectionId = Guid.NewGuid();
            station.SessionDetails.RequestDelivery(connectionId);
            await ctx.SaveChangesAsync();

            var result = station.SessionDetails.SetResponse(connectionId, accepted: true);

            Assert.True(result.IsSuccess());
            Assert.Equal(SessionResponse.Success, result.ReturnValue);
            Assert.Equal(SessionState.Started, station.SessionDetails.Session.State);
            Assert.NotNull(station.SessionDetails.Session.StartedUtc);
        }

        [Fact]
        public async Task SetResponse_with_accepted_false_transitions_Delivered_to_Canceled_and_closes_session()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            await ctx.SaveChangesAsync();
            var connectionId = Guid.NewGuid();
            station.SessionDetails.RequestDelivery(connectionId);
            await ctx.SaveChangesAsync();
            // Capture the Session BEFORE SetResponse clears it on cancellation.
            var session = station.SessionDetails.Session;

            var result = station.SessionDetails.SetResponse(connectionId, accepted: false);

            Assert.Equal(SessionResponse.Success, result.ReturnValue);
            // SessionDetails.ClearStates() unlinks the session on cancel (terminal state).
            Assert.Null(station.SessionDetails.SessionId);
            // The session itself was transitioned to Canceled and closed.
            Assert.Equal(SessionState.Canceled, session.State);
            Assert.True(session.IsClosed);
        }

        [Fact]
        public async Task SetResponse_with_wrong_connectionId_returns_ConnectionMismatch()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            await ctx.SaveChangesAsync();
            var pickupConnectionId = Guid.NewGuid();
            station.SessionDetails.RequestDelivery(pickupConnectionId);
            await ctx.SaveChangesAsync();

            var result = station.SessionDetails.SetResponse(
                    Guid.NewGuid(), // different connection id
                    accepted: true);

            Assert.Equal(SessionResponse.ConnectionMismatch, result.ReturnValue);
            Assert.Equal(SessionState.Delivered, station.SessionDetails.Session.State);
        }

        // ------------------------------------------------------------------
        // EndSession (Started -> Ended)
        // ------------------------------------------------------------------

        [Fact]
        public async Task EndSession_from_Started_transitions_to_Ended_and_records_StopReason()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            await ctx.SaveChangesAsync();
            var connectionId = Guid.NewGuid();
            station.SessionDetails.RequestDelivery(connectionId);
            await ctx.SaveChangesAsync();
            station.SessionDetails.SetResponse(connectionId, accepted: true);
            await ctx.SaveChangesAsync();
            var session = station.SessionDetails.Session;

            var result = station.SessionDetails.EndSession(connectionId, StopReason.UserLogout);

            Assert.True(result.IsSuccess());
            Assert.Equal(SessionResponse.Success, result.ReturnValue);
            Assert.Equal(SessionState.Ended, session.State);
            Assert.True(session.IsClosed);
            Assert.Equal(StopReason.UserLogout, session.StopReason);
            Assert.NotNull(session.StoppedUtc);
            // ClearStates removes the SessionId from SessionDetails after terminal transition.
            Assert.Null(station.SessionDetails.SessionId);
        }

        [Fact]
        public async Task EndSession_without_running_session_returns_StateMismatch()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);

            var result = station.SessionDetails.EndSession(Guid.NewGuid(), StopReason.UserLogout);
            Assert.Equal(SessionResponse.StateMismatch, result.ReturnValue);
        }

        [Fact]
        public async Task EndSession_with_unknown_StopReason_returns_error()
        {
            using var ctx = InMemoryDbContextFactory.Create();
            var station = await SeedStationAsync(ctx);
            station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
            await ctx.SaveChangesAsync();
            var connectionId = Guid.NewGuid();
            station.SessionDetails.RequestDelivery(connectionId);
            await ctx.SaveChangesAsync();
            station.SessionDetails.SetResponse(connectionId, accepted: true);
            await ctx.SaveChangesAsync();

            var result = station.SessionDetails.EndSession(connectionId, StopReason.Unknown);

            Assert.True(result.HasError());
        }

        // ------------------------------------------------------------------
        // DeliveryTimeout (Requested -> DeliveryTimeout when delivery interval exceeded)
        // ------------------------------------------------------------------

        [Fact]
        public async Task DeliveryTimeout_fires_when_session_is_reloaded_from_db_after_delivery_window()
        {
            // IsPreRunTimeOut compares RequestedOnUtc + timeout against the entity's
            // LoadedFromDatabaseUtc, which is set ONLY by the parameterless constructor
            // (used by EF when materializing from the database). Newly-constructed
            // Sessions have LoadedFromDatabaseUtc = DateTime.MinValue, so the timeout
            // check can never fire against an in-memory session. To exercise the timeout
            // path we must round-trip through the database in a separate context.
            var dbName = "deliv-timeout-" + Guid.NewGuid().ToString("N");
            Guid stationId;
            using (var ctx = InMemoryDbContextFactory.Create(dbName))
            {
                var station = await SeedStationAsync(ctx);
                stationId = station.Id;
                station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
                await ctx.SaveChangesAsync();
                // Backdate RequestedOnUtc on the persisted session to a moment beyond the
                // maximum delivery window.
                BackdateRequestedOnUtc(station.SessionDetails.Session,
                        DateTime.UtcNow.Subtract(
                                SessionDetails.MaximumTimeoutLoginRequestDelivery +
                                TimeSpan.FromSeconds(5)));
                await ctx.SaveChangesAsync();
            }

            using (var ctx = InMemoryDbContextFactory.Create(dbName))
            {
                // Re-load the station and its SessionDetails+Session so that the new
                // Session entity is materialized via the parameterless ctor and
                // LoadedFromDatabaseUtc = DateTime.UtcNow.
                var station = await ctx.Stations
                        .Include(x => x.SessionDetails)
                            .ThenInclude(x => x.Session)
                        .FirstAsync(x => x.Id == stationId);
                var firstSession = station.SessionDetails.Session;
                Assert.NotNull(firstSession);

                var second = station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);

                Assert.Equal(SessionResponse.Success, second.ReturnValue);
                Assert.Equal(SessionState.DeliveryTimeout, firstSession.State);
                Assert.True(firstSession.IsClosed);
                // The new session replaced the old one.
                Assert.NotSame(firstSession, station.SessionDetails.Session);
                Assert.Equal(SessionState.Requested, station.SessionDetails.Session.State);
            }
        }

        // ------------------------------------------------------------------
        // ResponseTimeout (Delivered -> ResponseTimeout when response interval exceeded)
        // ------------------------------------------------------------------

        [Fact]
        public async Task ResponseTimeout_fires_when_session_is_reloaded_from_db_after_response_window()
        {
            // Same root cause as the DeliveryTimeout test: IsPreRunTimeOut needs a
            // LoadedFromDatabaseUtc set by EF materialization. We seed + drive the session
            // up to Delivered in one context, then re-query in a second context to get a
            // freshly-loaded entity, then drive the next transition.
            var dbName = "resp-timeout-" + Guid.NewGuid().ToString("N");
            Guid stationId;
            var connectionId = Guid.NewGuid();
            using (var ctx = InMemoryDbContextFactory.Create(dbName))
            {
                var station = await SeedStationAsync(ctx);
                stationId = station.Id;
                station.SessionDetails.RequestSession(RequestSource.WebApi, SourceIp);
                await ctx.SaveChangesAsync();
                station.SessionDetails.RequestDelivery(connectionId);
                await ctx.SaveChangesAsync();
                // Backdate SendOnUtc so the response window has lapsed.
                BackdateSendOnUtc(station.SessionDetails.Session,
                        DateTime.UtcNow.Subtract(
                                SessionDetails.MaximumTimeoutLoginRequestResponse +
                                TimeSpan.FromSeconds(5)));
                await ctx.SaveChangesAsync();
            }

            using (var ctx = InMemoryDbContextFactory.Create(dbName))
            {
                var station = await ctx.Stations
                        .Include(x => x.SessionDetails)
                            .ThenInclude(x => x.Session)
                        .FirstAsync(x => x.Id == stationId);
                var session = station.SessionDetails.Session;
                Assert.NotNull(session);
                Assert.Equal(SessionState.Delivered, session.State);

                // RequestDelivery surfaces the timeout via IsPreRunTimeOut.
                var result = station.SessionDetails.RequestDelivery(connectionId);
                Assert.Equal(SessionResponse.Timeout, result.ReturnValue);
                Assert.Equal(SessionState.ResponseTimeout, session.State);
                Assert.True(session.IsClosed);
                Assert.Null(station.SessionDetails.SessionId);
            }
        }

        // ------------------------------------------------------------------
        // helpers — backdate private timestamps via reflection
        // ------------------------------------------------------------------

        private static void BackdateRequestedOnUtc(Pod.Data.Models.Shell.Session session, DateTime newValue)
        {
            SetPrivateProperty(session, nameof(Pod.Data.Models.Shell.Session.RequestedOnUtc), newValue);
        }

        private static void BackdateSendOnUtc(Pod.Data.Models.Shell.Session session, DateTime newValue)
        {
            SetPrivateProperty(session, nameof(Pod.Data.Models.Shell.Session.SendOnUtc), (DateTime?)newValue);
        }

        private static void SetPrivateProperty(object target, string propertyName, object value)
        {
            var prop = target.GetType().GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null)
            {
                throw new InvalidOperationException("Property not found: " + propertyName);
            }
            prop.SetValue(target, value, null);
        }
    }
}
