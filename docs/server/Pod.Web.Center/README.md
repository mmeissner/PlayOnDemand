# Pod.Web.Center

> The ASP.NET Core 10 host process — Kestrel + REST controllers + Razor account pages + gRPC services (on the same Kestrel pipeline) + background workers, all wired in one `Startup`.

## Purpose

This is the only deployable artefact in the `Pod.*` server tier. Every other `Pod.*` project is a class library that lands in this project's `bin/` at build time. Run `dotnet Pod.Web.Center.dll` and you get the public REST API, Swagger UI, the operator account portal (Razor Pages), the station-facing gRPC services, and three background services (email queue, connection-health sweeper, optional Let's Encrypt renewal).

`Startup.cs` is the manifest of how the system is composed: it reads `appsettings.json`, configures Identity + JWT + the custom `amx` HMAC + the `grpc-station` metadata scheme, registers every service from `Pod.Services`, sets up Swagger with two documents (`v1` public, `v1_internal` for support/admin), and pipes everything through ASP.NET Core's built-in DI container.

`Program.cs` uses the generic-host pattern (`Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(...).UseStartup<Startup>()`), runs the EF migration step before `host.RunAsync()` to avoid the hosted-service race, and configures Kestrel for the two deployment shapes: with `LetsEncryptOptions.IsEnabled=true` it binds `:80` (ACME challenge endpoint) + `:443` (REST + gRPC over HTTP/2 with the dynamically-issued LE cert); with LE disabled it binds `:80` only (HTTP/1.1 + h2c). Both shapes serve REST and gRPC on the same pipeline.

## Tech

- **Target framework:** `net10.0` (SDK pinned to `10.0.204` with `rollForward: latestFeature` by repo-root `global.json`)
- **SDK:** `Microsoft.NET.Sdk.Web`
- **Key NuGet packages:**
  - `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.0 — JWT scheme (separate package since .NET 6)
  - `Microsoft.AspNetCore.Mvc.NewtonsoftJson` 10.0.0 — Newtonsoft serialiser for the controllers (camelCase + `StringEnumConverter` preserved from 2.1)
  - `Microsoft.EntityFrameworkCore.Design` 10.0.4 — design-time tools so `dotnet ef migrations` resolves the startup project
  - `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` 10.0.0 — `/health` endpoint backing
  - `Grpc.AspNetCore` 2.66.0 — gRPC on the Kestrel pipeline (replaces the legacy `Grpc.Core.Server` standalone host)
  - `Newtonsoft.Json` 13.0.4
  - `NLog.Web.AspNetCore` 5.x — logging adapter (config in `nlog.config`)
  - `Swashbuckle.AspNetCore` 10.1.7 + `Swashbuckle.AspNetCore.Annotations` 10.1.7 + `Swashbuckle.AspNetCore.Filters` 10.0.1 — Swagger with the new `Microsoft.OpenApi` 2.x flat namespace
  - `AspNetCoreRateLimit` 5.0.0 (NuGet) — replaces the previously-vendored copy under `Pod.Web.Center.3rdParty/`
- **Project references (in this repo):**
  - `Pod.Data.Models`, `Pod.Data` — entities + `PodDbContext`
  - `Pod.DtoModels` — request DTOs
  - `Pod.Grpc.ConnectHost.Server`, `Pod.Grpc.ShellHost.Server` — gRPC service implementations (mapped via `endpoints.MapGrpcService<>()`)
  - `Pod.LetsEncrypt` — ACME middleware + hosted service
  - `Pod.Web.Authentication.ApiKeySecret` — `amx` auth handler (validator implementation lives here in this project, see `Authentication/`)
  - (transitively): `Pod.Services`, `Pod.MailEngine`, `Pod.ViewModels`, `Pod.ViewModels.Expressions`, `Pod.Enums`, `Pod.Data.Infrastructure`, `Pod.Grpc.Messages`, etc.

## Responsibility

**Is responsible for:**
- HTTP listener (Kestrel) and the entire ASP.NET Core middleware pipeline ordering
- DI registration of every server-side service, options class, and hosted service (in `Startup.ConfigureServices`)
- REST controller endpoints under `Areas/Api/v1/`
- Razor Pages for the operator account portal (`Pages/Account/`)
- Swagger UI hosting (custom `index.html` embedded as resource, custom CSS)
- Database seeding via `IDbSetupTask` chain at startup (`DbSetupUsers`, `DbSetupShellServer`, `DbSetupEmail`)
- The `ApiKeySecretValidator` implementation (the `amx` HMAC validation logic, including replay-attack cache)
- JWT bearer configuration (`Config/ConfigureJwtBearerOptions.cs`)
- Standalone gRPC server hosting (separate from Kestrel; see `ServicesHosted/GrpcHostedServer.cs` + `GrpcServicesServer.cs`)

**Is NOT responsible for:**
- Business logic — that lives in `Pod.Services`
- Persistence — `Pod.Data` owns the `PodDbContext` and migrations
- Email sending — `Pod.MailEngine` owns SMTP/Gmail logic
- gRPC contract types or protobuf — those are in `Pod.Grpc.Base` and `Pod.Grpc.Messages`
- ACME protocol details — `Pod.LetsEncrypt` owns those

## Public API surface

This is a deployable, not a library — it has no "public API" in the .NET sense. Its outward surface is **HTTP endpoints**:

- `api/v1/auth/{login|logout|refreshToken}` — `AuthController`, JWT issuance
- `api/v1/accounts/...` — `AccountsController`, registration / email confirmation / password reset
- `api/v1/stations` (per-user collection) and `api/v1/station` (single, station-authenticated via `amx`) — `StationsController`, `StationController`
- `api/v1/email/...` — `EmailController`, email account + template CRUD
- `api/v1/server/...` — `ServerController`, ShellServer config
- `api/v1/internal/admin/...` — `AdminController`, role + user management (Identity-protected, group `v1_internal`)
- `api/v1/internal/support/...` — `SupportController`, customer support tooling
- `/` — Swagger UI (root path; serves both `v1` and `v1_internal` documents)
- `/Pages/Account/...` — Razor account portal
- `/health` — `Microsoft.Extensions.Diagnostics.HealthChecks` endpoint with DB connectivity check (`AddDbContextCheck<PodDbContext>()`)
- gRPC services share Kestrel on the same `:443` (or `:80` h2c) listener — `ShellHostServiceGrpc`, `ConnectHostServiceGrpc`, `ShellApplicationServiceGrpc` — registered via `endpoints.MapGrpcService<>()` in `Startup.Configure`. No separate port.

## Internal structure

```
Pod.Web.Center/
├── Program.cs                       NLog wire-up + Kestrel builder + LetsEncrypt cert selector
├── Startup.cs                       partial — ConfigureServices + Configure (the composition root)
├── DbSetup.cs                       IDbSetupTask implementations (Users, ShellServer, Email)
├── Helper.cs                        ClaimsPrincipal extensions (e.g. GetStationApiKeyData)
├── ModelConverter.cs                ad-hoc DTO ↔ entity converters used by controllers
├── nlog.config                      file + console targets, log levels per area
├── appsettings.json                 default config (DB conn, JWT secret, Grpc, RateLimit, LetsEncrypt)
├── appsettings.Development.json     dev overrides
├── libman.json                      client-side static-asset (LibMan) manifest for wwwroot/lib
│
├── Areas/Api/v1/                    REST controllers — one per domain
│   ├── AuthController.cs
│   ├── AccountsController.cs
│   ├── AdminController.cs           (api/v1/internal/admin/*)
│   ├── EmailController.cs
│   ├── ServerController.cs          (api/v1/internal/server/*)
│   ├── StationController.cs         (single station, [Authorize(amx)])
│   ├── StationsController.cs        (operator's station collection, [Authorize(JWT)])
│   └── SupportController.cs         (api/v1/internal/support/*)
│
├── Authentication/
│   └── ApiKeySecretValidator.cs     IApiKeySecretValidator impl: HMAC verify + replay cache
│                                    + 10s clock-skew limit + 20 MB body-hash cap
│
├── Config/
│   ├── ConfigureJwtBearerOptions.cs IConfigureOptions<JwtBearerOptions> — symmetric key + issuer
│   └── webAppConfig.cs              POCO bound to "webAppConfig" section
│
├── Presenter/
│   └── ResultPresenter.cs           the universal IResult<T> -> ActionResult bridge
│                                    (Success -> 200; HasError -> 400 + IResult body)
│
├── ServicesHosted/                  IHostedService implementations
│   ├── GrpcHostedServer.cs          PublisherHub<ClientCommandType>.Disconnect broadcast on host stop
│   │                                (the previous "starts a separate Grpc.Core.Server" role is gone -
│   │                                gRPC is mapped onto Kestrel via endpoints.MapGrpcService<>())
│   ├── SendEmailServiceHosted.cs    every 90 s -> EMailService.SendEmailOrders
│   └── ConnectionHealthServiceHosted.cs  every 5 min -> ConnectionHealthService
│
├── Swagger/
│   ├── ApiExplorerGroupPerVersionConvention.cs  splits controllers into v1 / v1_internal
│   ├── AuthorizationOperationFilter.cs          adds [Authorize] visualisation
│   ├── EnumDocumentFilter.cs                    enum → string in schema
│   ├── OnlyApiResponseAndRequestFilterOrdered.cs  sorts schema models
│   ├── SchemaIdStrategy.cs                      strips "Dto"/"ViewModel" suffix
│   ├── Examples/RequestExamples.cs              Swashbuckle.Filters example providers
│   └── index.html                               custom Swagger UI shell (embedded resource)
│
├── TokenProvider/
│   └── RefreshAccessTokenProvider.cs   3 DataProtectorTokenProvider subclasses
│                                       (Refresh, EmailConfirmation, PasswordReset)
│                                       so each can have its own lifespan
│
├── EmailTemplates/                  Seed HTML/TXT for system emails (used by DbSetupEmail)
│   ├── RegisterMail.{html,txt}
│   ├── ResendEmailVerificationMail.{html,txt}
│   └── ForgotPasswordMail.{html,txt}
│
├── Pages/                           Razor Pages (operator portal)
│   ├── Account/                     login, register, confirm-email pages
│   ├── Sandbox/                     dev-only test pages
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── Properties/
│   └── launchSettings.json
│
├── wwwroot/                         static assets
│   ├── images/
│   ├── lib/                         (LibMan-restored client libs)
│   └── swagger-ui/                  custom CSS, favicons, logo
│
└── Pod.Web.Center.xml               XML doc output (referenced by Swagger IncludeXmlComments)
```

## Notable patterns / gotchas

- **`ApiKeySecretValidator` is here, not in `Pod.Web.Authentication.ApiKeySecret`.** That sibling project only defines the `AuthenticationHandler` + interfaces; the implementation that actually checks HMACs against the DB lives here because it depends on `StationApiKeyService` (and thus `PodDbContext`).
- **EF migration runs in `Program.Main` before `host.RunAsync()`**, not in `Startup.Configure`. Doing it in `Configure` raced the hosted services (`ConnectionHealthService` queried tables before they existed and crashed the host on cold start). The initializer takes its own scope from `host.Services` and applies pending migrations + first-run seeds before the host accepts requests.
- **Three Authentication schemes** — `JwtBearer` (operators), `amx` (station REST via HMAC), `grpc-station` (station gRPC via per-call `(identity, password)` metadata). The `grpc-station` handler is `Pod.Grpc.Base.Server.GrpcMetadataAuthenticationHandler` and verifies via the pluggable `IGrpcStationCredentialVerifier` (default impl PBKDF2-checks against `Station.PasswordHash`). Default policy is JWT; controllers/services opt in via `[Authorize(AuthenticationSchemes = "amx")]` or `[Authorize(AuthenticationSchemes = "grpc-station")]`.
- **gRPC services live on the same Kestrel pipeline as REST**, mapped via `endpoints.MapGrpcService<ConnectHostServiceGrpc>()`, `endpoints.MapGrpcService<ShellHostServiceGrpc>()`, `endpoints.MapGrpcService<ShellApplicationServiceGrpc>()` in `Startup.Configure`. One port, one process, one TLS cert. HTTP/2 is required for gRPC; with LE-enabled it's negotiated via ALPN over TLS, with LE-disabled Kestrel falls back to h2c on `:80`.
- **Two Swagger documents:** `v1` is the public surface, `v1_internal` is the operator/admin/support surface. Selection is by `[ApiExplorerSettings(GroupName = "v1_internal")]` on the controller. Both are served from the same UI. Swashbuckle is on the new `Microsoft.OpenApi` 2.x flat namespace (`OpenApiDocument`/`OpenApiOperation`/`OpenApiInfo`/`OpenApiSecurityScheme`).
- **`AddIdentityCore` (not `AddIdentity`)** — chosen because we don't want cookies; everything is JWT-based. `SignInRequireConfirmedEmail = true` enforces the email-confirmation flow. Identity 10 added passkey support; `PodDbContext.OnModelCreating` calls `base.OnModelCreating(modelBuilder)` so the new passkey tables are migrated alongside the rest. Requires explicit `identityBuilder.AddSignInManager()` and a `services.TryAddScoped<IUserConfirmation<ApplicationUser>, DefaultUserConfirmation<ApplicationUser>>()` registration (which the legacy `AddIdentity` would have provided automatically).
- **MD5 of the request body is part of the `amx` signature** (see `ApiKeySecretValidator`). Body is buffered into memory (capped at 20 MB) and `Request.EnableBuffering()` is called so the controller can still read it. (`EnableRewind` was the ASP.NET Core 2.x API; the public 3.x+ equivalent is `EnableBuffering`.)
- **Wrong-password fallthrough** — `Pod.Services` `AddSignResult` previously had a silent bypass: `SignInResult` with `Succeeded == false && !IsLockedOut && !IsNotAllowed` was a no-op, and `AuthenticationService.GetTokenByLogin` then minted a JWT for the unauthenticated caller. Fixed in v1.0.0; the missing fall-through now flags `UserError.UserIdentityPasswordMismatch`.
- **`ConfigureLogging(...).ClearProviders()` in `Program.cs`** — NLog owns logging end-to-end. Settings in `appsettings.json:Logging` are ignored.
- **TLS certificates** — provided dynamically by `Pod.LetsEncrypt`'s ACME flow when `LetsEncryptOptions.IsEnabled=true`. No certs are copied at build time anymore; the historical `_Certificates/ssl create/` files are gone, only the cert-generation templates remain.
- **`DbSetupEmail` seeds the `EmailContentTemplate` rows from the `EmailTemplates/` folder.** Editing those HTML/TXT files only affects fresh DBs unless you also nuke the existing rows.

## Consumers

None — this is the host process. Everything else feeds into it.

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview + request flow
- [`docs/architecture/auth.md`](../../architecture/auth.md) — JWT vs `amx` HMAC details
- [`docs/architecture/grpc.md`](../../architecture/grpc.md) — gRPC contracts and channel setup
- [`docs/architecture/build-and-deploy.md`](../../architecture/build-and-deploy.md) — SDK pin, runtime layout
