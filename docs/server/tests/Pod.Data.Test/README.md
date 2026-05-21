# Pod.Data.Test

> Integration tests for `Pod.Data` (PostgreSQL + EF Core) and a few `Pod.Services` flows that hit the database.

## Purpose

Exercises the persistence layer end-to-end against a real PostgreSQL instance. These are **not** isolated unit tests — every test class derives from `EfCoreAspFixture : InfrastructureFixture` (in `Pod.Test.Utilities`), which builds a full DI container, drops & re-creates the database, then runs Identity-aware test users through actual EF Core queries.

The suite mostly verifies the **domain entities own their state transitions** (Station, ConnectionState, SessionDetails, SubscriptionState/Order/Payment/Change, ShellServer, DeviceIdentity) and that those transitions round-trip through PostgreSQL with their navigation graphs intact. It also includes one true unit test for the custom `PasswordHasher`.

This is the primary place where the connection state-machine, the heartbeat-timeout path, and the subscription billing flow are validated end-to-end before they reach production.

## Tech
- **Target framework:** `net10.0`
- **Key NuGet packages:**
  - `Microsoft.NET.Test.Sdk` 17.x — VSTest host
  - `xunit` 2.x / `xunit.runner.visualstudio` 2.x — test framework + VS runner
  - `Microsoft.EntityFrameworkCore.InMemory` 10.0.4 — backing for `InMemoryDbContextFactory` (no real Postgres needed)
  - `Microsoft.Extensions.Configuration.Binder` — needed to bind the linked `appsettings.json`
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure`
  - `Pod.Data.Models`
  - `Pod.Services` (used by `IntTestSubscriptionService`)
  - `Pod.Test.Utilities` (provides `InfrastructureFixture`, `PriorityOrderer`, `TestPriorityAttribute`)
- **Runtime config:** the `Pod.Web.Center/appsettings.json` is `Link`-ed in as `appsettings.json` so the fixture can read the connection string and `ConfigSuperuser` block.

## Responsibility

What it IS:
- Integration tests for `Pod.Data` entities and their behaviour-bearing methods.
- Integration tests for `CustomerSubscriptionService` (order creation, payment, listing of paid/unpaid orders).
- A single unit test for `Pod.Services.PasswordHasher`.

What it is NOT:
- Not a fast in-memory test suite — it requires a live PostgreSQL on the configured host and **wipes the schema** on first use (see `EnsureClearDb()`).
- Not coverage of REST controllers, gRPC services, or the higher-level `ShellService` (those are in `Pod.Services.Test`).
- Not a replacement for missing unit tests around the `SessionDetails` state machine in isolation — coverage is integration-style and incidental.

## Public API surface

xUnit test classes (no public surface intended for consumers):

| Class | Tests | Coverage |
|---|---|---|
| `EfCoreAspFixture : InfrastructureFixture` | (fixture) | DI registration (`CustomerSubscriptionService`, `StationService`); helpers `CreateStations`, `CreateShellServer`, `CreateDeviceIdentity`. |
| `IntTestModelsAndPersistance` | 8 `[Fact]`s | `CreateStation`, `CreateSubscription` (full order → pay → extend cycle), `CreateShellServer` (uniqueness), `CreateDeviceIdentity` (uniqueness), `ConnectionStateTest` (Disconnected → Connecting → Connected → Disconnected), `ConnectionIdentitiesBehaviourTest` (multi-server reconnect), `ConnectionTimeoutTest` (heartbeat timeout via `Thread.Sleep`), `SimulateConnection` (heartbeat loop + timeout-induced close), `SimulateSession` (Requested → Delivered → Started → Ended). |
| `IntTestSubscriptionService` | 2 `[Fact]`s | `GetUnpaidOrders` (paid/unpaid/expired filtering), `RequestOrder` (max-active-orders enforcement). |
| `UnitTest_PasswordHasher` | 1 `[Fact]` | Hash + verify success/fail. |
| `Helper` (static) | — | `DateTime` extension predicates `DifSmallerThen` / `DifBiggerThen`. |

Total: **~11 facts**. Honest assessment: the connection state-machine and the subscription order/payment cycle are well covered; the session state machine has one happy-path test (`SimulateSession`) but no failure-mode coverage; everything else in `Pod.Services` is untested here.

## Internal structure

```
Pod.Data.Test/
├── EFCoreAspFixture.cs            ← DI fixture (extends Pod.Test.Utilities.InfrastructureFixture)
├── IntTestModelsAndPersistance.cs ← 8 entity / state-machine tests
├── IntTestSubscriptionService.cs  ← 2 service tests
└── UnitTest_PasswordHasher.cs     ← 1 pure unit test
```

## Notable patterns / gotchas

- **Tests share one database.** The fixture has a process-wide `dbCleared` latch (`InfrastructureFixture.EnsureClearDb`). Only the *first* test run drops & re-seeds; later tests in the same run rely on each test creating its own users/stations with fresh names. This means tests are **not** independently runnable in arbitrary order without that one-time wipe — be careful when running a single test.
- **Test ordering.** Two classes are decorated `[TestCaseOrderer("Pod.Data.Test.PriorityOrderer", "Pod.Data.Test")]` referencing a class **defined in `Pod.Test.Utilities`** under the namespace `Pod.Test.Utilities` — the attribute string is wrong (it points to `Pod.Data.Test.PriorityOrderer` which does not exist in this assembly). xUnit will silently fall back to default ordering. Combined with no `[TestPriority]` decorations on the test methods, ordering is effectively unspecified today.
- **`Thread.Sleep` based timing.** `ConnectionTimeoutTest` and `SimulateConnection` sleep on `ShellServer.MinimumHeartbeatTimeout` / `MinimumHeartbeatInterval`. These tests are inherently slow and flaky under CPU pressure.
- **PostgreSQL precision quirks.** Comments call out that `DateTime` equality is checked with a tolerance because Npgsql truncates sub-millisecond precision (`Assert.True((order.ExpiresOnUtc - orderExpirationTime).Duration() < TimeSpan.FromMilliseconds(1))`).
- **`appsettings.json` is linked**, not copied — it tracks `Pod.Web.Center/appsettings.json` so tests automatically use the real dev connection string.
- **Hardcoded `UserSecretsId`** in `InfrastructureFixture` (`8d0f9b82-d878-4917-b968-c977fc85f9b9`) — set per-developer secrets there to override the connection string without touching the linked file.

## Consumers

Nobody. This is a leaf test project (`<IsPackable>false`).

## Related docs

- `docs/server/data/Pod.Data/README.md` — the system under test
- `docs/server/data/Pod.Data.Models/README.md` — the entities being exercised
- `docs/server/tests/Pod.Test.Utilities/README.md` — the shared `InfrastructureFixture`
- `docs/architecture/data-model.md` — entity relationships
- `docs/architecture/session-lifecycle.md` — what `SimulateSession` is asserting
