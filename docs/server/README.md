# Server Tier — `Pod.*`

> ASP.NET Core 10 backend that exposes a REST API to operators, gRPC services to stations on the same Kestrel pipeline, and runs the email + connection-health background workers.

## Where the server lives in the system

```
Operator browser / Flutter UI ──HTTPS/JWT──> Pod.Web.Center (Kestrel)  ──┐
                                                                          │
LeapPlay.Shell.exe (station) ──gRPC/h2 + (identity, password) metadata──>┘
                                                                          │
                                            ┌─────────────────────────────┴──────────┐
                                            │ DI container (built-in)                │
                                            │  Controllers ─> Pod.Services ─> EF 10  │
                                            │  Grpc.AspNetCore endpoints ─> Pod.Services ─> EF 10 │
                                            │  Hosted: PublisherHub coordinator,     │
                                            │          SendEmailServiceHosted,       │
                                            │          ConnectionHealthServiceHosted │
                                            └────────────────────────────────────────┘
                                                                          │
                                                          PostgreSQL 16 (EF Core 10 / Npgsql 10)
```

`Pod.Web.Center` is the only deployable in this tier. Everything else (`Pod.Services`, `Pod.Data*`, `Pod.Grpc.*`, etc.) is a class library compiled into it. REST and gRPC share the same Kestrel listener (`:443` with LE-issued cert, or `:80` h2c when LE is disabled) — there is no separate gRPC port anymore.

## The server projects at a glance

| Project | Target | Role |
|---|---|---|
| `Pod.Web.Center` | net10.0 | Kestrel host. `Startup.cs` wires DI; `Program.cs` uses the generic-host pattern and runs the EF migration step before hosted services start; controllers in `Areas/Api/v1/`. The only project with `Microsoft.NET.Sdk.Web`. |
| `Pod.Services` | net10.0 | All business logic. One folder per domain (Authentication, Station, Email, ShellHost…). Talks straight to `PodDbContext` — no repository layer. |
| `Pod.MailEngine` | netstandard2.0 | Templated email engine. SMTP (MailKit) + Gmail OAuth2. Variable substitution. |
| `Pod.LetsEncrypt` | net10.0 | Custom ACME v2 client (Certes 3.x) integrated as ASP.NET middleware + hosted service. Auto-issues/renews TLS certs for the Kestrel listener. |
| `Pod.Enums` | netstandard2.0 | Pure enums shared across server, gRPC, REST DTOs. No code, no dependencies. |
| `Pod.DtoModels` | netstandard2.0 | REST **request** DTOs (`Request*Dto`). Validation attributes (`[Required]`, `[MinLength]`). |
| `Pod.ViewModels` | netstandard2.0 | REST **response** view models (`*ViewModel`). |
| `Pod.ViewModels.Expressions` | netstandard2.0 | `Expression<Func<TEntity, TViewModel>>` projections used by `Pod.Services` LINQ-to-EF queries. Keeps services from materialising entire entities. The `ToStationCurrentStateVm` projection inlines `SessionViewModel` construction so EF Core 10's tighter LINQ translator can lower the whole tree to SQL. |
| `Pod.Web.Authentication.ApiKeySecret` | net10.0 | The `amx` HMAC-SHA256 REST authentication scheme (handler + interfaces only — the validator implementation lives in `Pod.Web.Center`). REST-only; runs on endpoints that opt in via `[Authorize(AuthenticationSchemes = "amx")]`. **Does not gate gRPC** (scheme #3 / `grpc-station` does). |
| `Pod.Web.Client.Rest` | netstandard2.0 | RestSharp-based public SDK for the REST API. Used by station code in some flows. |
| `Pod.Web.Client.Rest.Internal` | netstandard2.0 | Extension methods adding internal (`api/v1/internal/*`) endpoints onto the public `PodRestClient`. |
| (`AspNetCoreRateLimit` is now consumed as the NuGet package 5.0.0; the vendored copy under `Pod.Web.Center.3rdParty/AspNetCoreRateLimit` was removed.) | | |

## Request flow — REST (operator, browser, SDK)

1. Browser/SDK sends `https://api/.../api/v1/auth/login` with JSON body.
2. `IpRateLimitMiddleware` (vendored) runs first — IP throttling (configured in `appsettings.json` `IpRateLimiting`).
3. `JwtBearer` middleware validates the `Authorization: Bearer <jwt>` header (issued by `AuthenticationService`).
4. Controller in `Pod.Web.Center/Areas/Api/v1/` deserialises a `Request*Dto` from `Pod.DtoModels`, validates `ModelState`.
5. Controller calls a service in `Pod.Services` (e.g. `StationService.GetStationCurrentState`). Service returns `IResult<TViewModel>`.
6. `ResultPresenter.GetResult(...)` in `Pod.Web.Center/Presenter/` translates: success → `200 OK` with view model, error → `400 Bad Request` with the full `IResult` body (so `UserError` codes leak to clients — this is intentional).

## Request flow — gRPC (station kiosk)

1. Station opens a long-lived TLS gRPC channel to the same `:443` (or `:80` h2c when LE is disabled) the REST API listens on. Per-call auth is the `(identity, password)` metadata pair the kiosk attaches via `CallCredentials.FromInterceptor`; the server's `GrpcMetadataAuthenticationHandler` (scheme `grpc-station`) reads those headers and PBKDF2-verifies the password against `Station.PasswordHash` via the pluggable `IGrpcStationCredentialVerifier`. No mTLS, no client certs — the previously-sketched cert-licensing layer was excised in v1.0.0.
2. gRPC services live on the same Kestrel pipeline as REST; registered in `Startup.ConfigureServices` via `services.AddGrpc()` and mapped in `Startup.Configure` via `endpoints.MapGrpcService<ShellHostServiceGrpc>()`, `endpoints.MapGrpcService<ConnectHostServiceGrpc>()`, `endpoints.MapGrpcService<ShellApplicationServiceGrpc>()`. One process, one port, one TLS cert.
3. Calls dispatch to one of those gRPC services — they live in `Pod.Grpc.*.Server` projects.
4. Each gRPC service delegates to a `Pod.Services` class (e.g. `ShellService`, `ConnectService`).
5. For machine clients hitting REST (the station's own REST surface), the `amx` HMAC scheme is wired — see `Pod.Web.Authentication.ApiKeySecret`. Stations use it for `[Authorize(AuthenticationSchemes = "amx")]` endpoints under `StationController`. The `(StationApiKey.PublicKey, Secret)` pair is the kiosk's REST credential; the same kiosk holds the `(StationId, Station.Password)` pair for its gRPC traffic.

## Dependency direction

```
Pod.Enums              (leaf — no deps)
    │
    ├── Pod.DtoModels
    ├── Pod.ViewModels  ──> Pod.Data.Models (for VM enums shared with entities)
    │       │
    │       └── Pod.ViewModels.Expressions
    │
    ├── Pod.MailEngine ──> Pod.Data.Infrastructure, Pod.Data.Models
    │
    └── Pod.Services ──> all of the above + Pod.Data + Pod.Grpc.Messages

Pod.Web.Authentication.ApiKeySecret  (handler-only; no Pod.* deps)

Pod.LetsEncrypt   (no Pod.* deps; pure ASP.NET Core middleware/hosted-service lib)

Pod.Web.Center ──> Pod.Services + Pod.Grpc.*.Server + Pod.LetsEncrypt
                 + Pod.Web.Authentication.ApiKeySecret + AspNetCoreRateLimit

Pod.Web.Client.Rest          (consumed by external SDK users + stress test)
Pod.Web.Client.Rest.Internal (extends Pod.Web.Client.Rest)
```

The rule: lower-numbered projects in the list never reference higher ones. `Pod.Enums` is the bottom of the world.

## Server-tier conventions

- **`IResult<T>` everywhere.** Defined in `Pod.Data.Infrastructure`. Services return `IResult<T>`; controllers convert via `ResultPresenter`. Business errors are `UserError` enum codes — **no exceptions for control flow**.
- **No repository layer.** Services inject `PodDbContext` directly (scoped). LINQ queries use the projection helpers in `Pod.ViewModels.Expressions` to avoid loading full entity graphs.
- **DI uses ASP.NET Core's built-in container.** No third-party container. Registrations are all in `Pod.Web.Center/Startup.cs::ConfigureServices`. Services are mostly scoped; gRPC services + `*Hub`s are singletons.
- **Three auth schemes total, plus one orphaned cert path** (see `docs/architecture/auth.md`): JWT (`Bearer`) for human users on REST; `amx` (HMAC-SHA256 over `publicKey:HmacSignature:nonce:timestamp`) for the kiosk's REST calls — today this gates every method on `StationController`; and `(identity, password)` plain gRPC metadata for the kiosk's gRPC traffic — server PBKDF2-verifies against `Station.PasswordHash`. The gRPC channel uses **server-cert TLS only** (`ForceClientCertificate=false`); the kiosk does not present a client cert. There was also an originally intended **cert-based station-licensing** path (per-station x509 certs with `LicenseId` in the CN) — the client-side plumbing (`LeapCertLicense` etc.) is still in the repo but the server side was never finished; it was superseded by the `(StationApiKey.PublicKey, Secret)` model. Don't conflate the `amx` scheme with the gRPC station auth — they share words like "Station" and "ApiKey" but use different secrets, different transports, different code paths.
- **Two Swagger documents**: `v1` (public) and `v1_internal` (support/admin). Split by `ApiExplorerGroupPerVersionConvention` and the `[ApiExplorerSettings(GroupName = ...)]` attribute on controllers.
- **`SetCompatibilityVersion(Version_2_1)`** is locked — do not bump without re-validating the auth + rate-limit pipeline.
- **`global.json` pins SDK 10.0.204** with `rollForward: latestFeature`.
- **DTO vs ViewModel split** is a one-way contract: `Request*Dto` for input, `*ViewModel` for output. Never reuse a DTO as a response.

## Background services hosted in `Pod.Web.Center`

| `IHostedService` | Purpose | Interval |
|---|---|---|
| `GrpcHostedServer` | Coordinator only: broadcasts `PublisherHub<ClientCommandType>.Disconnect` on host stop so connected stations notice. The standalone `Grpc.Core.Server` it used to boot is gone — gRPC services map onto Kestrel via `endpoints.MapGrpcService<>()`. | host stop |
| `SendEmailServiceHosted` | Drains `EmailOrder` rows queued by `EMailService.QueueMail(...)`. | 90 s |
| `ConnectionHealthServiceHosted` | Marks orphaned stations as `Disconnected` after heartbeat timeout. | 5 min |
| `CertificateRequestService` (only when `LetsEncryptOptions.IsEnabled`) | ACME challenge + cert renewal. | 12 h |

## Where to look for X

| Want to… | Edit |
|---|---|
| Add a REST endpoint | `Pod.Web.Center/Areas/Api/v1/<X>Controller.cs` + DTO in `Pod.DtoModels` + service method in `Pod.Services/<X>/` |
| Add a response shape | `Pod.ViewModels/<area>/` + projection in `Pod.ViewModels.Expressions/` |
| Add an error code | `Pod.Enums/UserErrors.cs` |
| Tweak rate limits | `appsettings.json` → `IpRateLimiting`, `IpRateLimitPolicies` |
| Change JWT lifetime | `appsettings.json` → `JwtIssuerOptionsConfig` |
| Wire a new email template | DB-seeded via `DbSetupEmail` (`Pod.Web.Center/DbSetup.cs`); engine in `Pod.MailEngine` |
| Enable HTTPS via Let's Encrypt | `appsettings.json` → `LetsEncryptOptions.IsEnabled = true` + set `Hosts[]` |

## Related docs

- `docs/architecture/overview.md` — full system topology
- `docs/architecture/auth.md` — the three coexisting schemes (JWT REST, `amx` HMAC REST, gRPC `(identity, password)` plain metadata)
- `docs/architecture/grpc.md` — gRPC contracts and the `IResult<T>` pattern over the wire
- `docs/server/data/` — `Pod.Data*` (DbContext, entities, `Result<T>` helpers)
- `docs/server/grpc/` — `Pod.Grpc.*` (proto, server hosting, client helpers)
