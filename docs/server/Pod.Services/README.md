# Pod.Services

> All server-side business logic. One folder per domain, one `*Service` class per folder, talking straight to `PodDbContext` with no repository layer.

## Purpose

`Pod.Services` is where the verbs of the system live. Controllers and gRPC services are thin wrappers — they handle transport, deserialise input, hand off to a service method, and translate `IResult<T>` into a transport response. Anything that decides "what should happen" — checking permissions, mutating entities, projecting query results, queuing emails, signing JWTs, validating session state transitions — happens here.

The project sits between `Pod.Web.Center`/`Pod.Grpc.*.Server` (above) and `Pod.Data`/`Pod.MailEngine`/`Pod.ViewModels.Expressions` (below). Services depend directly on `PodDbContext`; there is no repository layer or unit-of-work abstraction. Reads use the `Expression<Func<TEntity, TViewModel>>` projections from `Pod.ViewModels.Expressions` to produce view models without materialising whole entity graphs.

Two cross-cutting concerns also live here: the `PublisherHub<T>` and `StationResponseHub` pair (an in-process pub/sub used by gRPC services to push commands to stations and await their responses), and a long file of `Extensions.cs` mapping helpers between `Pod.Enums`, `Pod.Data.Models` enums, gRPC enums, and Identity results.

## Tech

- **Target framework:** `net10.0` (migrated from `netcoreapp2.1` in phase 1 of the
  .NET 10 migration)
- **Key NuGet packages:**
  - `FrameworkReference Microsoft.AspNetCore.App` — replaces the legacy
    `Microsoft.AspNetCore.App` 2.1.1 package reference; surfaces `SignInManager`,
    `RoleManager`, `IdentityResult`, `PasswordHasher`
  - `Microsoft.AspNetCore.Cryptography.KeyDerivation` 10.0.4 — used inside auth flows
  - `System.IdentityModel.Tokens.Jwt` 8.14.0 — JWT issuance + validation. In
    `netcoreapp2.1` this lived inside the AspNetCore shared framework; from .NET 6
    onward it must be referenced explicitly.
  - `System.Reactive` 6.0.1 — `ReplaySubject` powers `ResponderHub<T>` (await-a-response-with-timeout pattern)
  - `WebMarkupMin.Core` 2.20.0 — minifies HTML email bodies before sending
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure` — `IResult<T>`, `Result`, validator extensions (`ArgNotNull`, `ValueTrue`, …)
  - `Pod.Data.Models` — entity classes
  - `Pod.Data` — `PodDbContext`, `PodDbContextFactory`
  - `Pod.Enums` — shared enums (`UserError`, `SessionState`, `NetworkState`, …)
  - `Pod.Grpc.Messages` — protobuf-generated DTOs + converters (used by gRPC-facing services)
  - `Pod.MailEngine` — `EMailTemplateSenderFactory`, `EMailTemplate`, `IVariableParser`
  - `Pod.ViewModels` — output DTO types
  - `Pod.ViewModels.Expressions` — projection helpers used in EF queries

## Responsibility

**Is responsible for:**
- All business rules and validation beyond DTO-level `[Required]`/`[MinLength]`
- Direct EF queries (`_podContext.Stations.Where(...).Select(ToStationCurrentStateVm.FromStation())`)
- Issuing JWT access + refresh tokens (`AuthenticationService`)
- Verifying station credentials for gRPC (`Extensions.VerifyCredentials`)
- Enforcing the session state machine (`ShellService`)
- Queuing emails to the `EmailOrder` table (`EMailService`) — actual SMTP send happens in the hosted background worker
- Cross-station push-notify + await-response pattern (`PublisherHub<T>` + `StationResponseHub`)
- Mapping between the system's three enum dialects (`Pod.Enums`, `Pod.Grpc.Messages.Shared`, `Pod.Data.Models.*`)

**Is NOT responsible for:**
- HTTP transport / JSON / model binding — `Pod.Web.Center` controllers do that
- gRPC transport / protobuf — `Pod.Grpc.*.Server` projects do that
- Persistence schema or migrations — `Pod.Data` owns those
- SMTP / Gmail OAuth wire-level work — `Pod.MailEngine` owns that
- Sending emails on a schedule — `SendEmailServiceHosted` (in `Pod.Web.Center/ServicesHosted/`) does that

## Public API surface

Roughly one `*Service` class per folder. All consumed via DI; constructor injection is the only style.

| Folder | Type | What it does |
|---|---|---|
| `Accountant/` | `AccountantService` | Subscription/billing accounting; payment reference generation |
| `Administrator/` | `AdminService` | Role assignment, user lookup/delete (Identity-backed) |
| `Applications/` | `ShellApplicationService`, `IUniqueAppFactory` (impl `UniqueAppFactory`), `AppIdEncoder`, `AppUpdate` | Manages installed-app inventory pushed up by stations |
| `Authentication/` | `AuthenticationService`, `JwtIssuerOptions`, `AuthConfig`, `PodClaimsTypes`, `PodUserClaimsPrincipalFactory`, `RefreshAccessTokenProvider` | JWT issue + refresh; claim-type constants (`UserId`, `ApiKeyUserId`, `ApiKeyStationId`) |
| `ConnectHost/` | `ConnectService` | Station connect/disconnect business logic for the gRPC connect host |
| `Customer/` | `CustomerSubscriptionService` | Subscription lifecycle for end customers |
| `Email/` | `EMailService`, `EMailSenderService`, `EmailVariableHelper`, `SendMailCommand` | Queues outbound email orders, drains the queue with `MailEngine` |
| `Health/` | `ConnectionHealthService` | Detects orphaned connections and marks stations disconnected |
| `ServerManager/` | `ServerManagerService` | ShellServer registration + capacity reporting |
| `ShellHost/` | `ShellService` | Session state machine: request → deliver → start → end |
| `Station/` | `StationService`, `StationApiKeyService` | Station CRUD; `StationApiKey` (HMAC public/secret pair) issuance and lookup for the `amx` REST scheme. Independent from the kiosk's gRPC auth, which uses `Station.PasswordHash` (PBKDF2) verified by `Extensions.VerifyCredentials`. |
| `Support/` | `CustomerSupportService`, `StationSupportService` | Internal support endpoints (look up other users' data) |
| `System/` | `SystemSettingsService` | In-memory system settings (registered as singleton; not yet persisted) |
| `User/` | `UserService` | End-user account management beyond Identity defaults |
| (root) | `PublisherHub<T>` | Per-station message queue + `IServerStreamWriter<T>` writer pump |
| (root) | `StationResponseHub` (`ResponderHub<ClientResponse>`) | Await a response from a station within a timeout, using a 2-s `ReplaySubject` buffer for races |
| (root) | `Extensions` | Big static class of mapping/validation helpers (gRPC enums ↔ DB enums, `IdentityResult` → `IResult`, etc.) |

## Internal structure

```
Pod.Services/
├── Accountant/        AccountantService.cs
├── Administrator/     AdminService.cs
├── Applications/      AppIdEncoder.cs, AppUpdate.cs, ShellApplicationService.cs, UniqueAppFactory.cs
├── Authentication/    AuthConfig.cs, AuthenticationService.cs, JwtIssuerOptions.cs,
│                      PodClaimsTypes.cs, PodUserClaimsPrincipalFactory.cs,
│                      RefreshAccessTokenProvider.cs
├── ConnectHost/       ConnectService.cs
├── Customer/          CustomerSubscriptionService.cs
├── Email/             EMailSenderService.cs, EMailService.cs,
│                      EmailVariableHelper.cs, SendMailCommand.cs
├── Health/            ConnectionHealthService.cs
├── ServerManager/     ServerManagerService.cs
├── ShellHost/         ShellService.cs               (the session state machine)
├── Station/           StationService.cs, StationApiKeyService.cs
├── Support/           CustomerSupportService.cs, StationSupportService.cs
├── System/            SystemSettingsService.cs
├── User/              UserService.cs
├── Extensions.cs      ~470 lines of mapping & validator helpers (see below)
├── PublisherHub.cs    generic per-receiver in-process queue + ClientCommandType enum
└── ResponderHub.cs    abstract await-a-response base + StationResponseHub +
                       ClientResponse + ClientRequestType enum
```

## Notable patterns / gotchas

- **`IResult<T>` is the only return type for service methods that can fail.** Never throw `ArgumentException` from a service — `result.ArgNotNull(...)` / `result.ArgNotNullOrWhitespace(...)` / `result.ArgOutOfRange(...)` (defined in `Pod.Data.Infrastructure`) record an error against a `UserError` code, and the caller does `if (result.HasError()) return result;`. Exceptions are reserved for actually-unexpected failures.
- **Services are scoped (`AddScoped`).** `PodDbContext` is also scoped, so services share the context within a single HTTP / gRPC request.
- **`PublisherHub<ClientCommandType>` and `StationResponseHub` are singletons.** They are the only stateful long-lived objects in this project. The `ReplaySubject` in `ResponderHub<T>` has a 2-second buffer to handle the race where a service sends a request to a station, then starts awaiting the response only after the station has already replied — the buffer lets the `await` see slightly-stale events. **Do not change this without re-reading the doc-comment on `_messageReceivedSubject`.**
- **`StationResponseHub.WaitForResponse` filters by `receivedAfterDateTimeUtc`** — required because the buffer can deliver responses from previous interactions. Always pass `DateTime.UtcNow` *before* you publish the request.
- **No repository layer.** Services build LINQ queries against `_podContext` directly. To keep it sane, projections live in `Pod.ViewModels.Expressions/` and are consumed via `.Select(ToStationCurrentStateVm.FromStation())`. Only use `.AsNoTracking()` for reads that don't need updates — see `StationService.GetStationsCurrentState` for the canonical example.
- **`Extensions.cs` is a junk drawer of mappers** — gRPC enum ↔ DB enum, `SignInResult`/`IdentityResult` → `Result`, `SessionDetails` → `Pod.Grpc.Messages.ShellHost.SessionDetails`, etc. It's the second-largest file in the project and worth a one-time read before adding more.
- **`ShellApplicationService` plays the role of "applications service" for both gRPC and the operator UI.** It is registered both as a scoped service and (`ShellApplicationServiceGrpc`, in `Pod.Grpc.ShellHost.Server`) as a singleton wrapper.
- **`AuthenticationService` issues both access tokens (short-lived JWT) and refresh tokens (Identity-stored via `UserManager.GenerateUserTokenAsync` with the `RefreshAccessTokenProviderOptions.Name` provider).** Logging out invalidates only the refresh token via `RemoveAuthenticationTokenAsync` — the access JWT remains valid until natural expiry. Document this if anyone asks "why isn't logout instant".
- **`EmailVariableHelper` knows the standard variable substitutions** (`Username`, `WebHostRoot`, `EMailVerificationTokenLink`, …). When adding a new template variable, add the enum value to `Pod.Enums.TemplateVariableKey` first, then teach `EmailVariableHelper` how to resolve it.
- **`SystemSettingsService` is in-memory and singleton.** The XML doc on it explicitly says "currently not persistent". If/when it's persisted, this comment needs updating.

## net10 migration notes (phase 1)

- **`Pod.Services.csproj`**: `netcoreapp2.1` → `net10.0`. Replaced the legacy
  `Microsoft.AspNetCore.App` 2.1.1 PackageReference with a `FrameworkReference`.
  Bumped `Microsoft.AspNetCore.Cryptography.KeyDerivation` to 10.0.4, `System.Reactive`
  to 6.0.1, `WebMarkupMin.Core` to 2.20.0. Added explicit
  `System.IdentityModel.Tokens.Jwt` 8.14.0 (no longer transitively pulled in by
  AspNetCore).
- **`RefreshAccessTokenProvider<TUser>`**: the .NET 10 `DataProtectorTokenProvider`
  base class requires an `ILogger<DataProtectorTokenProvider<TUser>>` constructor
  parameter; added it to the subclass.
- **`EMailService.EMailAccountAddTemplateAsync`**: `DbSet<T>.FindAsync` returns
  `ValueTask<T>` in EF Core 3+, so the previous `Task.WhenAll(emailTemplateTask,
  emailAccountTask)` no longer compiles. Awaited the two queries sequentially
  (EF Core has never supported concurrent operations on the same `DbContext`).
- **Known live bugs surfaced by the new characterization tests** in
  `Pod.Services.Test/Authentication/`:
  - `Extensions.AddSignResult` does not add any error for `SignInResult.Failed`
    (the plain wrong-password case), so `GetTokenByLogin` issues a valid token on
    bad password. Fix is one line in `Extensions.cs`.
  - `AuthenticationService.TryGetPrincipalFromExpiredToken` does not wrap
    `JwtSecurityTokenHandler.ValidateToken` in try/catch — garbage input
    propagates as `SecurityTokenMalformedException` instead of returning the
    documented `UserError.UserIdentityInvalidToken`.
  Both are out of scope for the framework migration; tests document the current
  behaviour with explanatory comments referencing the fix location.

## Consumers

Direct project references (the `<ProjectReference>` lookup):

- `Pod.Web.Center` — controllers + hosted services + DI registrations
- `Pod.Grpc.ConnectHost.Server`, `Pod.Grpc.ShellHost.Server` — gRPC service implementations call into `ShellService`, `ConnectService`, `ShellApplicationService`
- `Pod.Grpc.Base.Server` — uses the helper extensions
- `Pod.Test.Utilities`, `Pod.Data.Test` — test infrastructure

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/architecture/session-lifecycle.md`](../../architecture/session-lifecycle.md) — the state machine `ShellService` enforces
- [`docs/architecture/auth.md`](../../architecture/auth.md) — JWT issuance + claim layout (issued by `AuthenticationService`)
- [`docs/server/data/Pod.Data.Infrastructure/`](../data/Pod.Data.Infrastructure/) — the `IResult<T>` validator surface
