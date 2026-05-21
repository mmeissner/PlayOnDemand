# Pod.Services.Test

> Integration tests for `Pod.Services` — exercises the full service stack (REST-ish entry points, gRPC `ShellService` flow) against a real PostgreSQL backend.

## Purpose

Where `Pod.Data.Test` validates *entities* round-tripping through the database, this project validates *services* — the layer that the REST controllers and gRPC services delegate to. The fixture (`ServicesFixture`) wires up almost the entire `Pod.Services` graph (`ConnectService`, `ShellService`, `StationService`, `CustomerSubscriptionService`, `CustomerSupportService`, `AccountantService`, `AdminService`, `ServerManagerService`, `UserService`, `ShellApplicationService`, `PublisherHub<>`, `StationResponseHub`, `ShellServer` singleton) on top of the same `InfrastructureFixture` used by `Pod.Data.Test`.

The headline test, `ShellClientIntegrationTest.SimpleConnectAndCreateSessionTest`, drives the **full station ↔ server happy path** end to end — server creation, station creation, `ConnectService.RequestServer`, `ShellService.ConnectToServer`, login request, login intention pickup, login confirmation, session-state polling, logout, disconnect — calling each service in its own DI scope to mirror real per-request lifetime, all asserting against the actual session state machine in the database.

## Tech
- **Target framework:** `net10.0` (migrated from `netcoreapp2.1` in phase 1)
- **Key NuGet packages:**
  - `Microsoft.NET.Test.Sdk` 17.12.0
  - `xunit` 2.9.3 / `xunit.runner.visualstudio` 3.0.0
  - `Microsoft.EntityFrameworkCore.InMemory` 10.0.4 — fast in-process DbContext
    backing for the new characterization tests; no Postgres dependency
  - `Moq` 4.20.72 — optional, available if a future test needs to mock
    collaborators
- **Project references (in this repo):**
  - `Pod.Services` (now an **explicit** reference, no longer transitive through
    `Pod.Test.Utilities`)
  - `Pod.Data`, `Pod.Data.Infrastructure`, `Pod.Data.Models`, `Pod.Enums`,
    `Pod.ViewModels`
  - `Pod.Test.Utilities` (still referenced for `InfrastructureFixture` and the
    `[TestPriority]` orderer used by the older skipped Postgres-backed tests)

## Responsibility

What it IS:
- **Characterization unit tests** (the bulk of the suite as of the .NET 10
  migration) for `AuthenticationService`, `StationService`,
  `StationApiKeyService`, the session-FSM transitions on `SessionDetails`, and
  `Extensions.VerifyCredentials`. Backed by EF Core's InMemory provider via the
  `TestFixtures/InMemoryDbContextFactory` helper — no Postgres dependency.
- The earlier Postgres-backed `IntTestUserStationService` integration tests and
  the dense gRPC-equivalent `ShellClientIntegrationTest` are still present but
  currently skipped (see "Skipped tests" below).

What it is NOT:
- Not currently a coverage check for the gRPC flow — `ShellClientIntegrationTest`
  is skipped pending the parallel `Grpc.AspNetCore` migration.
- Not coverage of `AccountantService`, `AdminService`, `ServerManagerService`,
  `CustomerSupportService`, `UserService`, `ShellApplicationService`, or
  `PublisherHub<>`. They are still untested. Adding fixtures for them is
  straightforward — copy the pattern in `Station/StationServiceTests.cs`.

## Public API surface

xUnit test classes (post-migration):

| Class | Tests | Coverage |
|---|---|---|
| `TestFixtures/InMemoryDbContextFactory` | (helper) | Returns a fresh `PodDbContext` per call, backed by EF Core's InMemory provider with a unique database name. Constructed via reflection because `PodDbContext`'s constructor is `internal` and the migration deliberately doesn't touch `Pod.Data` to add `InternalsVisibleTo`. Injects an `IModelCustomizer` that ignores the new `IdentityPasskeyData` / `IdentityUserPasskey<TKey>` entities — workaround for the fact that `PodDbContext.OnModelCreating` doesn't call `base.OnModelCreating`. |
| `TestFixtures/IdentityTestHarness` | (helper) | Spins up a minimal DI container with EF-Core-backed Identity using the InMemory provider; exposes `UserManager`, `SignInManager`, `RoleManager`, plus a `CreateConfirmedUserAsync` helper. |
| `TestFixtures/MockLogger` | (helper) | `For<T>()` returns `NullLogger<T>.Instance`. |
| `TestFixtures/TestEnvironmentSmokeTests` | 3 `[Fact]`s | Sanity-check the fixtures themselves. |
| `Authentication/AuthenticationServiceTests` | 13 `[Fact]`s | `GetTokenByLogin` happy + 5 error paths + lockout + 3 known-issue characterization tests; `LogoutUser` happy + 2 error paths + the stateless-refresh-token-survives-logout fact; `RefreshToken` happy + garbage input + empty input. |
| `Station/StationServiceTests` | 7 `[Fact]`s | `CreateNewStation` happy + 4 input validation errors + per-user station-limit enforcement; `GetStationsDisplayDetails` ownership isolation. |
| `Station/StationApiKeyServiceTests` | 6 `[Fact]`s | `CreateStationApiKey` happy + distinct-secrets + 2 validation errors; `GetStationApiKey` malformed-guid + unknown-guid. |
| `Session/SessionDetailsFsmTests` | 13 `[Fact]`s | Full happy path Requested → Delivered → Started → Ended; SetResponse(false) cancellation; ConnectionMismatch; StateMismatch on no-active-session; Unknown StopReason; DeliveryTimeout and ResponseTimeout via DB round-trip; input validation. |
| `Extensions/VerifyCredentialsTests` | 6 `[Fact]`s | Happy path + wrong password + unknown station + empty inputs + the generic overload. |
| `ServicesFixture : InfrastructureFixture` | (fixture) | Used only by the skipped legacy tests below. Registers the full `Pod.Services` graph + a singleton `ShellServer` resolved from the DB. |
| `IntTestUserStationService` | 2 `[Fact(Skip="…")]`s | `SessionViews`, `StationDisplayDetails`. Skipped — needs Postgres + the `InfrastructureFixture` wiring rebuilt for net10. |
| `ShellClientIntegrationTest` | 1 `[Fact(Skip="…")]` | `SimpleConnectAndCreateSessionTest`. Skipped — depends on the gRPC stack which is being migrated to `Grpc.AspNetCore` in a parallel branch. |

Total: **51 facts, 48 passing, 3 skipped**.

## Internal structure

```
Pod.Services.Test/
├── TestFixtures/
│   ├── InMemoryDbContextFactory.cs       ← PodDbContext via InMemory provider
│   ├── IdentityTestHarness.cs            ← minimal Identity DI container
│   ├── MockLogger.cs                     ← NullLogger<T> helper
│   └── TestEnvironmentSmokeTests.cs      ← fixture sanity checks
├── Authentication/
│   └── AuthenticationServiceTests.cs
├── Station/
│   ├── StationServiceTests.cs
│   └── StationApiKeyServiceTests.cs
├── Session/
│   └── SessionDetailsFsmTests.cs
├── Extensions/
│   └── VerifyCredentialsTests.cs
├── ServicesFixture.cs                    ← (legacy) DI fixture for skipped tests
├── IntTestUserStationService.cs          ← (skipped) Postgres-backed integration
└── ShellClientIntegrationTest.cs         ← (skipped) gRPC integration
```

## Notable patterns / gotchas

- **`[TestCaseOrderer]` typo fixed.** `IntTestUserStationService` previously
  declared `[TestCaseOrderer("Pod.Data.Test.PriorityOrderer", "Pod.Data.Test")]`
  — both the type name and assembly name were wrong, so the attribute was silently
  ignored and `[TestPriority]` decorations had no effect. Fixed to the actual
  location `Pod.Test.Utilities.PriorityOrderer` in `Pod.Test.Utilities`. Old behaviour
  is documented in this README for historical context only.
- **`IntTestUserStationService` lives in the wrong namespace.** Its file declares
  `namespace Pod.Data.Test` even though it's compiled into `Pod.Services.Test`.
  Harmless but misleading; left as-is because the tests are skipped anyway and
  changing the namespace risks breaking the `[TestCaseOrderer]` resolution.
- **`InMemoryDbContextFactory` ignores Identity-Passkey types.** ASP.NET Identity
  10 added `IdentityPasskeyData` and `IdentityUserPasskey<TKey>`. They're
  auto-discovered through `IdentityUser` navigations, but `PodDbContext.OnModelCreating`
  doesn't call `base.OnModelCreating` and so never configures them, causing model
  validation to throw on first use. The factory installs an `IModelCustomizer` that
  sweeps the model after the user's `OnModelCreating` and `Ignore`s any
  `Microsoft.AspNetCore.Identity.*` type whose CLR name contains "Passkey". When
  `Pod.Data` is updated to call `base.OnModelCreating` (or otherwise handle the new
  Identity entities) this workaround can be removed.
- **`PodDbContext` is constructed via reflection.** Its ctor is `internal` to the
  `Pod.Data` assembly and this migration deliberately doesn't touch `Pod.Data`.
  Reflection is the lesser evil; if `Pod.Data` later exposes a public ctor or adds
  `[assembly: InternalsVisibleTo("Pod.Services.Test")]`, the reflection in
  `InMemoryDbContextFactory.Create` should be replaced with a direct `new` call.
- **Per-call DI scopes.** `ShellClientIntegrationTest` opens a fresh
  `_container.CreateScope()` for almost every call. This intentionally mirrors how
  ASP.NET Core scopes per request, and would expose any service that incorrectly
  assumed singleton state across calls. (Still relevant once the test is
  re-enabled.)
- **Three tests document KNOWN ISSUES in production code** with explanatory
  comments naming the fix location:
  - `GetTokenByLogin_with_wrong_password_characterizes_today_behavior_known_issue`
    — `Extensions.AddSignResult` doesn't add an error on
    `SignInResult.Failed`, so wrong passwords still issue tokens.
  - `RefreshToken_with_garbage_access_token_throws` — uncaught
    `SecurityTokenMalformedException` from the JWT handler.
  - `LogoutUser_with_valid_username_succeeds` notes the stateless-refresh-token
    invariant (matches `docs/architecture/auth.md`).
  These tests will need updating when those bugs are fixed.

## Skipped tests

| Test | Reason |
|---|---|
| `ShellClientIntegrationTest.SimpleConnectAndCreateSessionTest` | gRPC stack is mid-migration to `Grpc.AspNetCore`. The orchestrator re-enables this after `Pod.Grpc.*` lands. |
| `IntTestUserStationService.SessionViews` | Postgres-backed; depends on `InfrastructureFixture` wiring that still targets the live Npgsql provider. Re-enable once `Pod.Test.Utilities/InfrastructureFixture` is updated. |
| `IntTestUserStationService.StationDisplayDetails` | Same as above. |

## Consumers

Nobody. Leaf test project (`<IsPackable>false`).

## Related docs

- `docs/server/Pod.Services/README.md` — the system under test
- `docs/server/grpc/Pod.Grpc.ShellHost.Server/README.md` — the gRPC service that delegates to `ShellService`
- `docs/server/tests/Pod.Test.Utilities/README.md` — the shared infrastructure
- `docs/architecture/session-lifecycle.md` — the protocol `SimpleConnectAndCreateSessionTest` walks through
