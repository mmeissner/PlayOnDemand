# Pod.Data

> The `PodDbContext` (an `IdentityDbContext` over PostgreSQL via Npgsql), its design-time factory, the migration history, and the EF Core fluent-API model configuration.

## Purpose

`Pod.Data` is the persistence layer for the server tier. It wires together the entity classes (defined in `Pod.Data.Models`) into an EF Core 2.1 `DbContext` backed by PostgreSQL, configures every relationship/index/conversion through nested fluent-API `IEntityTypeConfiguration` classes (`ModelConfig.*`), provides a `PodDbContextFactory` that EF tooling (`dotnet ef migrations …`) can pick up, and runs migrations + seed tasks on startup via `ContextInitializer`.

There is **no repository abstraction**. Services in `Pod.Services` and gRPC handlers receive a `PodDbContext` from DI directly and query/mutate via LINQ + `DbSet<T>` and explicit `SaveChangesAsync`. The intention was to keep the data access surface small and let domain methods on the entities (which return `IResult<T>`) provide the business-logic contract instead of a repository per aggregate.

The project also hosts the custom `PasswordHasher` (PBKDF2-HMAC-SHA256, format-marked, copied from ASP.NET Identity v2 source so that station passwords can be hashed without dragging the full Identity user manager into the station-auth code path).

> **Maintenance note (architecture audit):** the `Migrations/` folder stops in **July 2019**. No migrations have been added since `20190706132315_Added_SendEmailOrders`. Any entity change introduced after that date is not represented in a migration and would either fail at startup or require a fresh database. Treat this as load-bearing context when planning schema changes.

## Tech

- **Target framework:** `net10.0`.
- **Configurations:** `Debug;Release` (no `Release_ShellClient` — this project is server-only).
- **Key NuGet packages:**
  - `Microsoft.EntityFrameworkCore` 10.0.4 — ORM core.
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.0 — supplies `IdentityDbContext<TUser, TRole, TKey>` base used for `PodDbContext`. Identity 10 added passkey schema; `PodDbContext.OnModelCreating` now calls `base.OnModelCreating(modelBuilder)` so those tables migrate alongside the rest.
  - `Microsoft.EntityFrameworkCore.Tools` 10.0.4 — `dotnet ef` CLI support for migrations.
  - `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.1 — Postgres provider.
  - `Microsoft.Extensions.Configuration.Json` — needed by `PodDbContextFactory` to load `appsettings.DesignTime.json`.
  - `Microsoft.Extensions.Logging.Debug` — used by the static `LoggerFactory` inside `PodDbContext` so EF debug output goes to the IDE Output window.
- **Project references (in this repo):**
  - `Pod.Data.Models` — the entity types being configured.
  - `Pod.Enums` — error codes, currency, session-state, etc.
  - `Pod.Grpc.Utilities` — referenced for shared utilities (the `Pod.Data` library does not call gRPC, but exception/error helpers are pulled in).

## Responsibility

**It IS responsible for:**
- Defining `PodDbContext` — the single `DbContext` for the whole server.
- Mapping every `Pod.Data.Models` entity via the `ModelConfig.*` nested config classes (one per entity), invoked from `OnModelCreating`.
- Carrying the **migration history** in `Migrations/` (4 migrations + the snapshot).
- Hosting `PodDbContextFactory` (an `IDesignTimeDbContextFactory<PodDbContext>`) so `dotnet ef migrations add/list/script` works from this directory using `appsettings.DesignTime.json`.
- Hosting `ContextInitializer`, which on startup applies pending migrations one-by-one and runs every `IDbSetupTask` registered in DI (e.g. role seeding, admin user seeding, default `ShellServer`).
- Wrapping `DbUpdateConcurrencyException` in `DetailedConcurrencyException` (with serialized current-values) when the context was constructed with `logConcurrencyExceptionDetails: true`.
- Hosting `PasswordHasher` (PBKDF2-HMAC-SHA256, 10 000 iters, 128-bit salt, 256-bit subkey, format byte `0x01`).

**It is NOT responsible for:**
- Defining entities (see `Pod.Data.Models`).
- Defining the validation/error pattern (see `Pod.Data.Infrastructure`).
- Repositories — there are none. Services use `PodDbContext` directly.
- DI registration of the context — that lives in `Pod.Web.Center.Startup`.
- Running migrations from the EF CLI — `ContextInitializer` runs them at startup; `dotnet ef database update` is intentionally **not** the production path (see `ContextInitializer` summary comment).

## Public API surface

| Type | Role |
|------|------|
| `PodDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` | THE context. Internal constructor — only `PodDbContextFactory` may instantiate it. |
| `PodDbContextFactory : IDesignTimeDbContextFactory<PodDbContext>` | Two-mode factory: parameterless ctor for EF CLI tooling (reads `appsettings.DesignTime.json`); parametrised ctor for runtime DI (uses an injected `IConfiguration` + `DbContextFactoryConfig`). |
| `DbContextFactoryConfig` | Plain options POCO read from the `DbContextFactoryConfig` JSON section: `ConnectionStringName`, `LogEntityFramework`, `LogConcurrencyExceptionDetails`, `LogSensitiveData`. |
| `ContextInitializer` | Applies pending migrations with `IMigrator.Migrate(targetMigration)` (one at a time so future inter-migration data fixes can be inserted) then runs all `IDbSetupTask` services ordered by `Priority`. |
| `IDbSetupTask` | Seed-task contract: `int Priority { get; }` + `void Execute()`. Lower priority runs first (per the comment, intentionally backwards-named). Implementations live in other projects (e.g. `Pod.Web.Center` registers role/superuser/`ShellServer` seeders). |
| `PasswordHasher : IPasswordHasher` | PBKDF2-HMAC-SHA256 hasher used for station passwords. |
| `Config.ModelConfig` (static) | Holds all `IEntityTypeConfiguration<TEntity>` nested classes — see below. |
| `Config.RolesConfig` (static) | Constants `UserRole`, `AccountantRole`, `ServerManagerRole`, `CustomerSupportRole`, `AdministratorRole` plus the `Roles[]` array. |
| `Config.ConfigSuperuser` | Plain POCO: `Username`, `Email`, `Password`, `StationPassword` — bound from JSON to drive a superuser-seed task. |
| `Config.ConfigShellServer` | Plain POCO: `DisplayName`, `HostAddress`, `Port`, `InterfaceVersion` — bound from JSON to drive a default-`ShellServer` seed task. |
| `Converter.UtcDateTimeConverter` | `ConvertDateTime(DateTime?)` / `(DateTime)` — re-attaches `DateTimeKind.Utc` to values read back from PostgreSQL (Npgsql returns Unspecified). Used everywhere via `entity.Property(x => x.SomeUtcDate).HasConversion(y => y, y => UtcDateTimeConverter.ConvertDateTime(y))`. |
| `Exceptions.DetailedConcurrencyException` | Wraps `DbUpdateConcurrencyException` adding JSON-serialized `CurrentValues` of every conflicting entry to `Message`. |

### `DbSet<T>` exposed on `PodDbContext`

`Servers`, `Stations`, `StationApiKeys`, `ApplicationRoots`, `DeviceIdentities`, `StationSettings`, `SubscriptionStates`, `ConnectionStates`, `SessionDetails`, `SubscriptionOrders`, `SubscriptionPayments`, `SubscriptionChanges`, `Sessions`, `LocalApps`, `UniqueApps`, `ClosedConnections`, `EMailAccounts`, `EmailContentTemplates`, `EmailAccTemplateLinks`, `EmailSendOrders`. Identity tables (`Users`, `Roles`, `UserClaims`, `UserLogins`, `UserTokens`, `UserRoles`, `RoleClaims`) are inherited from `IdentityDbContext` and renamed via `ToTable(...)` in `IdentityXxxConfig`.

## Internal structure

```
Pod.Data/
├── PodDbContext.cs            — The DbContext: DbSets, OnModelCreating wiring, SaveChanges overrides
├── ContextFactories.cs        — PodDbContextFactory + DbContextFactoryConfig
├── ContextInitializer.cs      — Applies pending migrations + IDbSetupTask seeders
├── IDbSetupTask.cs            — Seeder contract
├── PasswordHasher.cs          — PBKDF2-HMAC-SHA256 implementation
├── appsettings.DesignTime.json — Connection string + seed config used by EF tooling only
│
├── Config/
│   ├── ModelConfig.cs         — All IEntityTypeConfiguration<T> nested classes (~700 lines)
│   ├── RolesConfig.cs         — Role-name constants
│   ├── ConfigSuperuser.cs     — JSON-bound POCO
│   └── ConfigShellServer.cs   — JSON-bound POCO
│
├── Converter/
│   └── UtcDateTimeConverter.cs — DateTime → DateTime(Utc) value converters
│
├── Exceptions/
│   └── DetailedConcurrencyException.cs
│
├── Migrations/                — EF migrations + model snapshot (see below)
│   ├── 20190609124646_CreateInitial.cs            (+ .Designer.cs)
│   ├── 20190629112627_StationApiKeySecret.cs      (+ .Designer.cs)
│   ├── 20190701121756_EmailTemplatesHtmlBody.cs   (+ .Designer.cs)
│   ├── 20190706132315_Added_SendEmailOrders.cs    (+ .Designer.cs)
│   └── PodDbContextModelSnapshot.cs
│
└── Properties/                 — empty (would hold AssemblyInfo etc.)
```

### Migration history

| Date (UTC) | Name | Summary |
|-----------:|------|---------|
| 2019-06-09 12:46:46 | **CreateInitial** | Bulk-creates the entire schema: Identity tables (`Users`, `Roles`, `UserClaims`, `UserLogins`, `UserRoles`, `UserTokens`, `RoleClaims`), `DeviceIdentities`, `EMailAccounts`, `EmailContentTemplates`, `Servers`, `UniqueApps`, `Stations`, `StationSettings`, `SubscriptionStates`, `SessionDetails`, `Sessions`, `SessionRule`, `SessionRuleLocalApp` (M:N), `ChangeRequest`, `LocalApp`, `ApplicationRoots`, `ConnectionStates`, `ClosedConnections`, `SubscriptionOrders`, `SubscriptionPayments`, `SubscriptionChanges`, `EMailAccountDataEMailContentTemplate` (M:N), `EmailVariable`. Creates two PostgreSQL sequences (`SubscriptionOrder_OrderNumber_seq` starting at 1000, `User_CustomerNumber_seq` starting at 100) so EF default-value SQL `nextval('"…_seq"')` works. |
| 2019-06-29 11:26:27 | **StationApiKeySecret** | Adds `StationApiKeys(CreatedOnUtc, PublicKey PK, SecretKey, DisplayName, StationId FK→Stations(Cascade))` + index on `StationId`. This is the per-station HMAC key/secret table consumed by the `Pod.Web.Authentication.ApiKeySecret` scheme. |
| 2019-07-01 12:17:56 | **EmailTemplatesHtmlBody** | Adds nullable `ContentHtml` column to `EmailContentTemplates` (templates were text-only before). |
| 2019-07-06 13:23:15 | **Added_SendEmailOrders** | Adds three tables: `EmailSendOrders`, `EMailReceiver` (FK→Order Cascade), `EMailVariableValue` (FK→Order Cascade). Indexes on `EmailSendOrders.SendState` (the queue worker's hot path) and on the FKs. |

`PodDbContextModelSnapshot.cs` is the EF-generated current-model snapshot used to diff against next-migration generation. **It is one snapshot frozen at the July 2019 model and is not regenerated** because no later migrations exist.

### `OnModelCreating` order

`PodDbContext.OnModelCreating` applies configurations in a specific order. Identity configurations come first (User → UserClaim → UserLogin → UserToken → Roles → RoleClaim → UserRole), then `ShellServer`, `Station`, `ApplicationRoot`, `StationSettings`, `SubscriptionState`, `ConnectionState`, `SessionDetails`, `DeviceIdentity`, `StationApiKey`, `ClosedConnection`, `Session`, `ChangeRequest`, `SessionRule`, `SessionRuleLocalApp`, `LocalApp`, `SubscriptionOrder` (passes the `ModelBuilder` so it can declare a sequence for `OrderNumber`), `SubscriptionPayment`, `SubscriptionChange`, `EMailAccountData`, `EmailContentTemplate`, `ContentTemplateVariable`, `EMailAccountDataEMailContentTemplate`, `EmailSendOrder`. Order matters because some configs declare sequences (`HasSequence<long>(…)` on the `ModelBuilder`) that other entities reference.

## Notable patterns / gotchas

- **`ModelConfig` nested-class fluent-API convention.** Every entity has a public nested class inside the static `ModelConfig` class named `<Entity>Config` implementing `IEntityTypeConfiguration<TEntity>`. They are **not** registered by `modelBuilder.ApplyConfigurationsFromAssembly(...)` — they are listed explicitly in `OnModelCreating`. When you add an entity you must also (a) add a `DbSet<T>` to `PodDbContext`, (b) add a `<Entity>Config` nested class to `ModelConfig`, (c) add a line to `OnModelCreating`, (d) generate a migration.

- **Two configs take a `ModelBuilder` in their ctor** (`IdentityUserConfig`, `SubscriptionOrderConfig`) because they declare a `HasSequence<long>(…)` on the *model* (sequences are model-level, not entity-level). When applying them you have to pass the model builder: `new ModelConfig.IdentityUserConfig(modelBuilder)`.

- **`Field` access mode for collection navigations.** Entities expose collections as `IReadOnlyCollection<T>` backed by a private `HashSet<T>`. The configs call `entity.Metadata.FindNavigation(name).SetPropertyAccessMode(PropertyAccessMode.Field)` so EF reads/writes the backing field directly, preserving the read-only public surface.

- **Optimistic concurrency via Postgres `xmin`.** Most entities (`Station`, `SessionDetails`, `Session`, `ConnectionState`, `SubscriptionState`, `StationSettings`, `ClosedConnection`, etc.) call `entity.ForNpgsqlUseXminAsConcurrencyToken()`. `SaveChanges` will raise `DbUpdateConcurrencyException` on stale writes; the overrides in `PodDbContext` re-throw as `DetailedConcurrencyException` if `_logConcurrencyExceptionDetails` was set.

- **All `*Utc` columns route through `UtcDateTimeConverter`.** Npgsql returns timestamps as `DateTimeKind.Unspecified`; the converter re-attaches `Utc`. Forgetting to register the converter on a new datetime column will silently produce non-UTC `DateTime` values downstream.

- **No repository layer.** Services and gRPC handlers take a `PodDbContext` (or a factory) from DI and write LINQ directly. Domain methods on the entities (`Station.CreateOrder(...)`, `SessionDetails.RequestSession(...)`, etc.) carry the business rules and return `IResult<T>` from `Pod.Data.Infrastructure`. If you find yourself wanting a `IStationRepository`, the pattern is to add a method to `Pod.Services` instead.

- **`PodDbContext` constructor is `internal`.** Production code must go through `PodDbContextFactory.Create()`; tests can use the same. The `IDesignTimeDbContextFactory.CreateDbContext(string[])` overload exists so EF tooling (`dotnet ef migrations …`) can spin up a context using `appsettings.DesignTime.json` from this directory.

- **`appsettings.DesignTime.json` is checked into git** with a local-dev connection string (`localhost:5432`, dev user). It is consumed only by the parameterless `PodDbContextFactory()` ctor (the EF CLI path). Production runtime uses the real config from `Pod.Web.Center`.

- **`ContextInitializer` runs migrations one at a time** via `IMigrator.Migrate(targetMigration)` rather than `Database.Migrate()` so future per-migration data-fix hooks can be inserted between steps. Note the comment: "this initializer will not be called or used by EF Commandline utilities" — the production path is `Pod.Web.Center` startup invoking `Initialize()`; running `dotnet ef database update` will not execute any `IDbSetupTask`.

- **`PasswordHasher` is a copy of ASP.NET Identity's V3 hasher** kept here so station-password verification doesn't require pulling the full `UserManager<TUser>` stack into station auth. Format-marker byte `0x01` = V3; `0x00` is recognised but throws `NotImplementedException`.

- **Stale snapshot warning.** Because the model has continued to evolve at the `Pod.Data.Models` level (entity files were last touched 2023) but no migration has been authored since 2019-07-06, `PodDbContextModelSnapshot.cs` does **not** reflect today's `OnModelCreating`. Generating a new migration in this state will produce a very large diff against the 2019 snapshot. See `docs/architecture/data-model.md` for details.

## Consumers

- `Pod.Web.Center` — composition root. Registers `PodDbContextFactory` + `PodDbContext` with DI, calls `ContextInitializer.Initialize()` in `Configure`, and wires up `RolesConfig` / `ConfigSuperuser` / `ConfigShellServer` from JSON.
- `Pod.Services` — every service receives `PodDbContext` (or a factory) via DI.
- `Pod.Grpc.ConnectHost.Server`, `Pod.Grpc.ShellHost.Server` — gRPC service handlers query `PodDbContext` directly.
- `Pod.MailEngine` — reads `EmailSendOrders` and updates send state.
- `Pod.Web.Authentication.ApiKeySecret` — looks up `StationApiKeys` for HMAC verification.
- Tests in `Pod.Data.Test` and `Pod.Services.Test`.

## Related docs

- `docs/architecture/data-model.md` — entity-relationship overview and the 2019-migration-freeze caveat.
- `docs/server/data/Pod.Data.Models/README.md` — the entity catalog being configured here.
- `docs/server/data/Pod.Data.Infrastructure/README.md` — the `Result<T>` pattern returned by entity methods.
- `docs/architecture/auth.md` — how `PasswordHasher` and `StationApiKeys` feed into station authentication.
- `docs/architecture/build-and-deploy.md` — `global.json` SDK pin (2.1.818) is what keeps `dotnet ef` working against this project.
