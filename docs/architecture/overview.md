# Architecture Overview

> System topology, request flow, and the call graph that ties the three deployable artefacts together.

---

## The three deployables

| Artefact | What it is | Where it runs |
|----------|------------|---------------|
| **Pod.Web.Center** | ASP.NET Core on .NET 10 LTS: REST API + Razor Pages + gRPC server (single Kestrel pipeline on `:443`) | Docker Compose stack: `server` + `postgres:16-alpine` + a `letsencrypt-data` volume. See [build-and-deploy.md](build-and-deploy.md) / [server-deployment.md](../usage/server-deployment.md). |
| **LeapPlay.Shell.exe** | WPF kiosk app | Per-station Windows PC (one per VR rig). Native deps: OpenVR, FFmpeg, vrlounge_desktop.exe, XInputInterface.dll. |
| **LeapPlay.Content.Creator.exe** | WPF authoring tool | Operator's workstation. Builds `.vbox` containers, distributed via shared storage / external means. |

Bundled with the Shell installer, but operationally independent. The Creator does not call the server; the Shell does.

---

## Call graph (high level)

```
                                       ┌─ HTTPS / JWT ───┐
                                       │                 │
       Operator browser ───────────────► REST API        │
       (Swagger UI / SPA)               (Pod.Web.Center) │
                                                ▲       │
                                                │       │
                                    Pages + Identity    │
                                    flow (Razor)        │
                                                        │
                                       ┌────────────────┘
                                       ▼
                                ┌──────────────┐
                                │ Pod.Services │  business logic
                                └─┬──────────┬─┘
                                  │          │
                                  ▼          ▼
                         ┌──────────┐  ┌──────────┐
                         │ Pod.Data │  │ MailEng. │
                         │ EF Core  │  └──────────┘
                         │ Postgres │
                         └──────────┘
                                  ▲
                                  │ scoped DbContext
                                  │
                         ┌────────┴─────────┐
                         │ Pod.Grpc.*Server │  gRPC services
                         └────────┬─────────┘
                                  │ TLS (server cert only) +
                                  │ (StationId, Password) as gRPC
                                  │ CallCredentials metadata
                                  ▼
                       ┌──────────────────────┐
                       │ LeapPlay.Shell       │
                       │ (per-station kiosk)  │
                       └──────────────────────┘
```

---

## Tier-by-tier responsibilities

### Server (`Pod.*`)

- **`Pod.Web.Center`** is the entry. Hosts everything: middleware pipeline, REST controllers, Razor pages (`Pages/Account/`), gRPC service registration, background hosted services (`SendEmailServiceHosted`, `ConnectionHealthService`), Swagger.
- **`Pod.Services`** is where business logic lives. Services like `AuthenticationService`, `SessionService`, `StationService`, `ApplicationService` are scoped and injected into both REST controllers and gRPC services. **No repository layer** — services use `PodDbContext` directly via DI.
- **`Pod.Data`** wraps EF Core. The `PodDbContext` extends `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`. Entity configuration is via Fluent API, declared in nested `ModelConfig` classes.
- **`Pod.Grpc.*`** is split: `Pod.Grpc.Base` holds the `.proto` files + generated stubs. `Pod.Grpc.ConnectHost.Server` and `Pod.Grpc.ShellHost.Server` implement the contracts. The base/client/server split mirrors typical gRPC project layout.
- **Cross-cutting libraries**: `Pod.MailEngine` (templated email), `Pod.LetsEncrypt` (custom ACME for SSL automation), `Pod.Web.Authentication.ApiKeySecret` (the `amx` HMAC-SHA256 scheme for **REST** machine clients — *not* used by the kiosk's gRPC traffic; see [auth.md](auth.md)).

### Client (`LeapVR.Shell.*`)

- **`LeapVR.Shell`** is the WPF entry. `Bootstrapper.cs` wires the IoC container (SimpleInjector + Caliburn.Micro). `App.xaml.cs` parses the command line; `-config` routes into `LeapVR.Shell.Setup` (the first-run wizard) instead of the main shell.
- **Modules** (`LeapVR.Shell.Modules`): pluggable feature units. Each module owns a slice of station capability:
  - `OpenVrModule` — VR runtime lifecycle, `HmdActivityWatchdog` watches headset presence.
  - `XInputModule` — Xbox controller input, drives the `BehaviorController` for controller→action mapping.
  - `VBoxPlatformModule` / `SteamPlatformModule` — platform abstraction, both implementing `IPlatformModule`.
  - `ContainerModule` — extracts `.vbox` files (standard ZIP via DotNetZip 1.10.1).
  - `MultimediaModule` / `PlaylistModule` — ambient background media (uses ffmediaelement).
  - `VrDesktopModule` — bridge to the prebuilt `vrlounge_desktop.exe` Unity binary.
- **Repositories** (`LeapVR.Shell.Repository`): LiteDB-backed local stores: `AppDisplayRepository`, `AppInstallationRepository`, `AppStatisticsRepository`, `MultimediaPlaylistRepository`, etc. Built on `GenericRepository<T>` + `GenericCache<T>`.
- **Services** (`LeapVR.Shell.Services`): gRPC client wrappers. They speak to `Pod.Web.Center`'s gRPC services over **server-cert TLS** (no mTLS), with `(StationId, Station.Password)` attached as `identity`/`password` gRPC CallCredentials metadata on every call. Server-side `VerifyCredentials` PBKDF2-checks the password against `Station.PasswordHash`.
- **Controllers** (`LeapVR.Shell.Controllers`): higher-level orchestrators stitching modules + services + repositories together. `SessionController`, `PlatformController`, `VrController`, `FirewallController`, `DiskController`, `SecurityController`, `BehaviorController`.

### Content Creator (`LeapVR.Content.*`)

Standalone WPF tool. Produces `.vbox` containers (ZIP + JSON header). The `LeapVR.Content.Creator.Logic` project is **deliberately UI-decoupled** — `LeapVrContainerCreation.DoWork()` is invoked from the WPF view-model but could equally be wrapped in a CLI. The kiosk consumes the same containers via `LeapVR.Content.Shared` (DTO layer) and `ContainerModule`.

---

## Two distinct request flows

### REST (operator UI)

```
Browser ─► HTTPS ─► Pod.Web.Center (Kestrel)
                  │
                  ├─ JWT bearer middleware (validates Identity-issued tokens)
                  ├─ AspNetCoreRateLimit (vendored — global + per-client buckets)
                  ├─ Routing → REST controllers (api/v1/...)
                  │   └─ controllers call Pod.Services
                  │       └─ services touch PodDbContext (Pod.Data)
                  └─ Swashbuckle (Swagger UI at /swagger)
```

### gRPC (station ↔ server)

```
LeapPlay.Shell ─► server-cert TLS ─► Pod.Web.Center :gRPC port
                          │
                          ├─ ASP.NET Core gRPC pipeline
                          ├─ CallContextUtil.ToClientCredentials(context):
                          │     reads "identity" + "password" metadata headers,
                          │     parses identity as Guid, returns ClientCredentials
                          │     { StationId, Password }; throws RpcException
                          │     (Unauthenticated) if missing or malformed.
                          │
                          │  ⚠ Pod.Web.Authentication.ApiKeySecret (the "amx"
                          │     HMAC scheme) is REST middleware - it does NOT
                          │     run on gRPC calls. See auth.md.
                          ├─ Service dispatch (ServiceShellHost / ServiceConnectHost)
                          │   └─ method body opens with:
                          │       var result = await clientCredentials
                          │           .VerifyCredentials<TResponse>(_podContext);
                          │       // loads Station by Id, then PBKDF2-verifies
                          │       // password against Station.PasswordHash.
                          │       result.ArgNotNull(...).ArgTrue(...);
                          │       if (result.HasError()) return result;
                          └─ on relevant events, services Publish() to
                            PublisherHub<T> / StationResponseHub so REST
                            controllers can subscribe and stream realtime
                            updates back to the operator UI.
```

The `IResult<T>` pattern means **business-rule failures are not exceptions**. Exceptions are reserved for genuine programming errors / infra failures.

---

## Why two auth schemes coexist

- **Humans** (operators, admins) — JWT Bearer. Token lifetimes: access 2h, refresh 20y, plus one-shot tokens for email confirmation and password reset. Issued/validated in `AuthenticationService` + `ConfigureJwtBearerOptions`.
- **Machines** (stations on gRPC) — `(StationId, Station.Password)` attached as plain `identity` / `password` gRPC metadata headers per call. Server reads them, loads the `Station`, and PBKDF2-verifies the password against `Station.PasswordHash`. Confidentiality comes from the TLS channel, not from a signature. JWT would have been wrong here because (a) stations don't log in interactively, they're issued long-term credentials, and (b) gRPC streaming over multi-hour sessions doesn't fit well with rotating bearer tokens.
- **Machines** (stations hitting the station-facing REST surface, plus any future machine clients) — `amx` HMAC-SHA256 scheme (`Pod.Web.Authentication.ApiKeySecret`). Uses the per-station `StationApiKey` (`PublicKey`+`SecretKey`) to sign each REST request. Today this gates every method on `StationController` (`/api/v1/Station/*`) — the kiosk uses it for non-gRPC operations under its own identity. Available for any future endpoint decorated with `[Authorize(AuthenticationSchemes = "amx")]`.

An **originally intended cert-based licensing scheme** (per-station x509 certs, `LicenseId` encoded in CN) was excised in v1.0.0 — server-side issuance was never finished and the kiosk-side plumbing (`LeapCertLicense`, `ClientIdentity`, `License\*-License.{crt,key}` files) is gone. The `(StationApiKey.PublicKey, Secret)` model above is the canonical station identity. [auth.md](auth.md) has the longer history.

Full detail: [auth.md](auth.md).

---

## Background work

- `SendEmailServiceHosted` — drains the `EmailSendOrder` queue (DB-backed) and sends transactional mail via `Pod.MailEngine`.
- `ConnectionHealthService` — monitors station heartbeats and updates `Station.IsOnline` state.
- `Pod.LetsEncrypt` — periodic ACME renewal job for the server's own TLS cert.

All implemented as `IHostedService`.

---

## Storage

| Side | Engine | Purpose |
|------|--------|---------|
| Server | PostgreSQL via EF Core 10 (Identity 10 with passkey schema, Guid keys) | All operational state: users, stations, sessions, billing, email templates, audit |
| Client | LiteDB (embedded NoSQL, single file) | Local app library, statistics, playlists, multimedia config — all per-station |

The two stores are **eventually consistent at best**. Stations push state updates to the server via gRPC; nothing in the kiosk waits on DB sync.

---

## What you should not assume

- The kiosk does **not** stream telemetry continuously. It checkpoints state on session events + periodic heartbeats. Don't add features that assume realtime metrics from kiosk to server.
- There is **no central asset store** for `.vbox` containers. Distribution is operator-managed (USB, network share, manual copy). Don't build server endpoints around container management.
- `Pod.Services` is **not** stateless across requests in a strong sense — it depends on the request-scoped `PodDbContext`. Background work uses scoped service factories.

---

## Read next

- [auth.md](auth.md) — both auth schemes, in detail.
- [grpc.md](grpc.md) — proto layout, services, methods, the result pattern.
- [session-lifecycle.md](session-lifecycle.md) — the core domain FSM.
- [data-model.md](data-model.md) — entities and relationships.
- [build-and-deploy.md](build-and-deploy.md) — what to build, in what order, with what tools.
