# Pod.Test.Utilities

> Shared test infrastructure: a DI/EF Core fixture base class, an xUnit priority orderer, and a default `appsettings.json`.

## Purpose

Centralises the boilerplate that every server-side test project would otherwise duplicate: building a `ServiceCollection` with EF Core + ASP.NET Identity + a `PodDbContext`, dropping & re-creating the database once per process, and creating throw-away test users with confirmed emails.

`Pod.Data.Test` and `Pod.Services.Test` both extend `InfrastructureFixture` to avoid re-implementing this every time.

The `PriorityOrderer` + `TestPriorityAttribute` pair is meant to let test authors pin an order for sequence-dependent integration tests (e.g. "create user before logging in"). In practice both consuming projects reference the orderer with the wrong type name, so it never actually runs — see the gotcha section.

## Tech
- **Target framework:** `net10.0`
- **Key NuGet packages:**
  - `xunit` 2.4.0 — needed for `ITestCaseOrderer` / `ITestCase` interfaces in `HelperOrder.cs`
  - `Microsoft.Extensions.Configuration.Binder` 2.1.1 — `configuration.Bind(...)` calls
  - `Microsoft.Extensions.DependencyInjection.Abstractions` 2.1.1 — `ServiceCollection`
- **Manual `Reference` (HintPath):** `xunit.core.dll` is referenced directly via a hardcoded NuGet cache path:
  ```
  ..\..\..\..\Users\LeapVR\.nuget\packages\xunit.extensibility.core\2.4.0\lib\netstandard2.0\xunit.core.dll
  ```
  This will only resolve on the original developer's machine. See gotchas.
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure`
  - `Pod.Data`
  - `Pod.Services` (only used because the fixture happens to need access — consumers transitively pick this up)

## Responsibility

What it IS:
- Base fixture for "I need a clean PostgreSQL DB + DI container with Identity registered" tests.
- Helpers for spinning up ApplicationUsers in tests.
- Cross-project xUnit ordering primitives.

What it is NOT:
- Not a mocking library, not a builder pattern, not a `WebApplicationFactory<T>` style host. There is no in-memory DB option.
- Not a place to put domain test data factories — those tend to live in the consuming projects (`EfCoreAspFixture.CreateStations`, `CreateShellServer`, etc.).

## Public API surface

| Type | Notes |
|---|---|
| `InfrastructureFixture` *(abstract)* | `GetServiceProvider()` (lazy, locked); `EnsureClearDb()` (process-wide latch); `CreateTestUser()` (auto-incrementing username/email, password = `"Password-1234"`, email confirmation pre-applied); `protected abstract RegisterOwnServices(ServiceCollection)` for subclasses to add their service graph; constants: `Password = "Password-1234"`. |
| `TestPriorityAttribute(int priority)` | xUnit method-level attribute consumed by `PriorityOrderer`. |
| `PriorityOrderer : ITestCaseOrderer` | Sorts test cases first by `[TestPriority]` (default 0), then alphabetically by method name. |

`InfrastructureFixture` registers (in `RegisterDefaultServices`):
- `IConfiguration` from `appsettings.json` + user secrets (`8d0f9b82-d878-4917-b968-c977fc85f9b9`) + environment variables
- `DbContextFactoryConfig`, `ConfigSuperuser`, `ConfigShellServer` bound from configuration
- `PodDbContextFactory`, `IDesignTimeDbContextFactory<PodDbContext>`, `ContextInitializer`
- Transient `PodDbContext` resolved through the factory
- ASP.NET Identity with the same password / lockout / email-confirmation policy as production
- `ILoggerFactory`, `ILogger<>` and `IServiceProvider`

## Internal structure

```
Pod.Test.Utilities/
├── InfrastructureFixture.cs   ← abstract DI/EF fixture base
├── HelperOrder.cs             ← TestPriorityAttribute + PriorityOrderer
└── appsettings.json           ← default dev config (PostgreSQL @ localhost:5432, dev user/password)
```

## Notable patterns / gotchas

- **Hardcoded `HintPath` for `xunit.core.dll`** points at `C:\Users\LeapVR\.nuget\...`. This breaks restore on any other machine and is the same problem documented for the simulators (the architecture audit calls out broken `..\..\..\..\Program Files\dotnet\sdk\NuGetFallbackFolder\...` HintPaths in other projects — this one is in `~\.nuget\packages\` instead). Future agents: **don't waste time wondering why this doesn't restore cleanly on a fresh checkout** — replace the manual `<Reference>` with a normal `<PackageReference Include="xunit.extensibility.core" />`, or remove it (xUnit 2.4.0 already brings core in transitively via the `xunit` meta-package consumers use).
- **`PriorityOrderer` is currently dead code.** Its callers (`Pod.Data.Test.IntTestModelsAndPersistance`, `Pod.Data.Test.IntTestSubscriptionService`, `Pod.Services.Test.IntTestUserStationService`) all use the attribute string `[TestCaseOrderer("Pod.Data.Test.PriorityOrderer", "Pod.Data.Test")]` — but the type's actual full name is `Pod.Test.Utilities.PriorityOrderer` and it lives in the `Pod.Test.Utilities` assembly. xUnit silently falls back to default ordering. Fix the strings and the attribute starts working.
- **Process-wide `dbCleared` static latch.** `EnsureClearDb()` only drops the DB the first time it is called *per test host*. Subsequent calls are no-ops. This is intentional (so 50 tests don't drop & re-seed 50 times) but it means tests must be tolerant of pre-existing state from earlier tests in the same run.
- **Email confirmation in `CreateTestUser`.** The fixture both creates the user *and* confirms the email — production sign-up requires the user to click an emailed link, but tests need a fully usable account.
- **The `appsettings.json` in this project's output is shipped as content.** Both `Pod.Data.Test` and `Pod.Services.Test` link `Pod.Web.Center/appsettings.json` instead — meaning this project's own `appsettings.json` is mostly relevant if a new test project consumes the fixture without supplying its own.

## Consumers

- `Pod.Data.Test`
- `Pod.Services.Test`

## Related docs

- `docs/server/data/Pod.Data/README.md` — the `PodDbContext` and `ContextInitializer` registered in the fixture
- `docs/server/data/Pod.Data.Models/README.md` — `ApplicationUser`, `ApplicationRole`, `DbContextFactoryConfig`, `ConfigSuperuser`
- `docs/server/Pod.Services/README.md` — the services consumers register on top of this fixture
- `docs/server/tests/Pod.Data.Test/README.md` and `docs/server/tests/Pod.Services.Test/README.md` — the consumers
