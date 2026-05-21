# PlayOnDemand (PoD) / LeapPlay — Documentation Index

> **Read this first.** This folder is the canonical onboarding doc. Every doc here is written so an AI agent can understand the project from these pages alone, without grepping the codebase.
>
> **Looking for the illustrated manual?** It lives at [`manual/`](manual/) — screenshot-driven walkthrough of every screen in the kiosk, the Content Creator, and the Flutter operator app. Start there if you're an operator or content owner rather than an engineer.
>
> **Looking for the marketing front-door?** Top-level [`../README.md`](../README.md). The "what shipped, why, status" version of that page is now [`about.md`](about.md).

---

## What is PoD?

A **VR-arcade / gaming-lounge management platform**. Arcade operators deploy VR PCs ("stations"), control sessions, manage billing, supervise hardware — all from a central server. The kiosk launches games, drives an OpenVR/SteamVR environment, runs background media, and reports state via gRPC to the server.

Three deployable artefacts:

```
┌─────────────────────────────────────────────────────────────────┐
│  Pod.Web.Center        ASP.NET Core on .NET 10 (server side)    │
│   • REST API (JWT)        for operator UI / external consumers  │
│   • Razor Pages           account portal                        │
│   • gRPC Server (TLS)     station ↔ server protocol             │
│   • PostgreSQL via EF     persistence                           │
│   • Background services   email queue, connection health        │
└──────────────┬───────────────────────────────────┬──────────────┘
               │ gRPC over TLS                     │ HTTPS / JWT
       ┌───────▼────────┐                ┌─────────▼───────┐
       │ LeapVR.Shell   │                │ Web Clients      │
       │ WPF .NET FW 4.7│                │ Swagger UI / SPA │
       │ Kiosk binary   │                └─────────────────┘
       │ • OpenVR       │
       │ • Game launcher│              ┌─────────────────────────┐
       │ • LiteDB local │              │ LeapVR.Content.Creator  │
       │ • Multimedia   │              │ WPF tool to package     │
       └────────────────┘              │ games into .vbox files  │
                                       └─────────────────────────┘
```

---

## Glossary

| Term | Meaning |
|------|---------|
| **Station** | A physical VR PC running `LeapPlay.Shell.exe`. Identified by `StationId` (Guid). |
| **Session** | A timed/paid VR play instance on a station. Has a state machine: `Requested → Delivered → Started → Ended` (happy path), with `Canceled`, `DeliveryTimeout`, `ResponseTimeout` failure terminals. See [session-lifecycle.md](architecture/session-lifecycle.md). |
| **Operator** | Human user (arcade staff, admin) of the web portal. Authenticated via JWT. |
| **ApiKey / Secret** | Per-station credential pair (`StationApiKey` entity, `PublicKey`+`SecretKey`) used by the **REST `amx` HMAC scheme** — *not* by the kiosk's gRPC traffic. The kiosk authenticates gRPC calls with `(StationId, Station.Password)` as plain metadata headers over TLS. See [auth.md](architecture/auth.md). |
| **Container / .vbox** | Game package format. ZIP archive + JSON header (`AppInstallationHeader`). Built by Content Creator, installed on stations. |
| **Platform** | Plug-in abstraction over a game source. Today: **VBox** (custom containers) and **Steam** (library scan + launch). |
| **Module** | Pluggable kiosk subsystem (VR, XInput, Container, Multimedia, …). Lives in `LeapVR.Shell.Modules`. |
| **Heartbeat** | Periodic gRPC ping from station → server keeping a session alive. |
| **Setup wizard** | `LeapPlay.Shell.exe -config` launches a separate WPF flow (`LeapVR.Shell.Setup`) for first-run configuration. |
| **VRLounge** | Pre-built Unity binary (`vrlounge_desktop.exe`) shipped with the kiosk for the in-VR home environment. Lives in `LeapVR.Shell.3rdParty/bin/vr_desktop/` (141 MB, copied at build time). |

---

## How to navigate this docs tree

```
docs/
├── README.md                       ← you are here
├── architecture/                   ← cross-cutting topics (read after this README)
│   ├── overview.md                 ← system topology + request flow
│   ├── auth.md                     ← three schemes: REST JWT, REST `amx` HMAC, gRPC `(identity,password)` metadata
│   ├── grpc.md                     ← contracts, .proto layout, IResult<T> pattern
│   ├── session-lifecycle.md        ← state machine + heartbeat protocol
│   ├── data-model.md               ← EF entities + relationships
│   └── build-and-deploy.md         ← Build_Free.bat, prerequisites, runtime layout
│
├── server/        ← Pod.* projects (ASP.NET Core on .NET 10 + supporting libs)
│   ├── README.md                   ← server-tier overview
│   ├── Pod.Web.Center/             ← entry point (Startup, controllers, Razor pages)
│   ├── Pod.Services/               ← business logic services
│   ├── Pod.MailEngine/             ← templated transactional email
│   ├── Pod.LetsEncrypt/            ← custom ACME client for SSL automation
│   ├── Pod.Enums/                  ← enums shared across server projects
│   ├── Pod.DtoModels/              ← REST request/response DTOs
│   ├── Pod.ViewModels/             ← Razor / Swagger view models
│   ├── Pod.ViewModels.Expressions/ ← expression-tree helpers for VMs
│   ├── Pod.Web.Authentication.ApiKeySecret/  ← `amx` HMAC scheme for REST machine clients (NOT for gRPC)
│   ├── Pod.Web.Client.Rest/        ← public REST client SDK
│   ├── Pod.Web.Client.Rest.Internal/  ← internal REST client variant
│   ├── data/                       ← persistence layer
│   │   ├── Pod.Data/               ← DbContext + EF config + migrations
│   │   ├── Pod.Data.Models/        ← entity classes
│   │   └── Pod.Data.Infrastructure/  ← Result<T> pattern, extensions
│   ├── grpc/                       ← gRPC layer
│   │   ├── Pod.Grpc.Base/          ← .proto + generated stubs
│   │   ├── Pod.Grpc.Base.Server/   ← server hosting helpers
│   │   ├── Pod.Grpc.Base.Client/   ← client connection helpers
│   │   ├── Pod.Grpc.Const/         ← shared constants (channel options, header names)
│   │   ├── Pod.Grpc.Messages/      ← protobuf ↔ DTO converters
│   │   ├── Pod.Grpc.Utilities/     ← error → gRPC status mapping
│   │   ├── Pod.Grpc.ConnectHost.Server/  ← service: station registration
│   │   └── Pod.Grpc.ShellHost.Server/    ← service: session lifecycle
│   ├── 3rdParty/AspNetCoreRateLimit/  ← vendored rate-limiter
│   ├── tests/                      ← unit/integration tests
│   └── tools/                      ← simulator, stress test
│
├── client/        ← LeapVR.Shell.* (WPF kiosk on .NET Framework 4.7.1)
│   ├── README.md                   ← client-tier overview
│   ├── LeapVR.Shell/               ← WPF entry, Bootstrapper (SimpleInjector + Caliburn.Micro)
│   ├── LeapVR.Shell.Setup/         ← first-run wizard (-config flag)
│   ├── LeapVR.Shell.Controllers/   ← business controllers (Platform, Session, VR, Disk, Firewall)
│   ├── LeapVR.Shell.Domain.Models/ ← domain interfaces & models
│   ├── LeapVR.Shell.Modules/       ← feature modules (VR, XInput, Container, Multimedia)
│   ├── LeapVR.Shell.Modules.Interfaces/  ← module contracts
│   ├── LeapVR.Shell.Repository/    ← LiteDB local repos (apps, stats, playlists)
│   ├── LeapVR.Shell.Repository.Interfaces/  ← repo contracts
│   ├── LeapVR.Shell.Services/      ← gRPC client wrappers
│   ├── LeapVR.Shell.Services.Interfaces/  ← service contracts
│   ├── LeapVR.Shell.Managers/      ← local machine + USB + scheduling managers
│   ├── LeapVR.Shell.Categories/    ← app categorization + multilingual labels
│   ├── LeapVR.Shell.Language/      ← localization resources (EN/ZH-CN)
│   └── LeapVR.Shell.OpenVR.Wrapper/  ← C# bindings for openvr_api.dll
│
├── content-creator/   ← LeapVR.Content.* (WPF authoring tool)
│   ├── README.md
│   ├── LeapVR.Content.Creator/     ← WPF entry
│   ├── LeapVR.Content.Creator.Logic/  ← packaging logic (decoupled from UI)
│   ├── LeapVR.Content.Creator.Language/  ← localization
│   ├── LeapVR.Content.Shared/      ← container DTOs (header schemas)
│   └── LeapVR.Content.Util/        ← archive, game-discovery, launcher-detection helpers
│
├── shared/        ← cross-process libraries
│   ├── LeapVR.Shared.Lib/          ← collections, crypto, sanity checks
│   ├── LeapVR.Shared.Lib.Win/      ← WinAPI helpers, image processing
│   ├── LeapVR.Shared.Lib.Wpf/      ← WPF converters & UI helpers
│   ├── LeapVR.Utilities.Steam/     ← Steam library scan, app info
│   ├── LeapVR.Utilities.Windows/   ← process mgmt, registry, USB, task scheduler
│   └── LeapVR.Utilities.VersionInfo/  ← build-time CLI: extract assembly version
│
└── 3rdParty/                       ← single page summarising vendored libs
    └── README.md
```

---

## Reading order for a fresh agent

1. **This file** — gets the big picture.
2. **`architecture/overview.md`** — request flow, what calls what.
3. **`architecture/auth.md` and `architecture/grpc.md`** — the load-bearing protocols.
4. **`architecture/session-lifecycle.md`** — the core domain concept.
5. **`architecture/data-model.md`** — entities and their relationships.
6. **Tier overviews** — `server/README.md`, `client/README.md`, `content-creator/README.md`.
7. **Per-project READMEs as needed** — when changing or extending a specific area.
8. **`architecture/build-and-deploy.md`** — when packaging or shipping.

---

## Tech stack at a glance

| Layer | Tech |
|-------|------|
| Server runtime | ASP.NET Core on .NET 10 LTS |
| Server lang | C# 7.1 |
| Client runtime | WPF on .NET Framework 4.7.1, x64 |
| Database (server) | PostgreSQL via EF Core 10 with Identity (passkey schema enabled) |
| Database (client) | LiteDB (embedded NoSQL) |
| RPC | gRPC + Protobuf over server-cert TLS (no mTLS — `ForceClientCertificate=false`) |
| Auth (humans) | JWT Bearer (Identity-issued) |
| Auth (stations, gRPC) | `(StationId, Station.Password)` as plain `identity`/`password` metadata headers; server PBKDF2-verifies password vs `Station.PasswordHash` |
| Auth (stations, REST) | `amx` HMAC-SHA256 (`Pod.Web.Authentication.ApiKeySecret`) — used by every `StationController` endpoint. Kiosk signs with the `StationApiKey.Secret` provisioned via `PUT /api/v1/Stations/{id}/apikeys`. |
| IoC server | ASP.NET Core built-in DI |
| IoC client | SimpleInjector |
| MVVM | Caliburn.Micro 3.2 |
| Logging | NLog 4.x (client) + `Microsoft.Extensions.Logging` (server) |
| API docs | Swashbuckle / Swagger UI |
| VR | OpenVR (SteamVR) |
| Input | XInput (Xbox controller, via custom C++ wrapper) |
| Media | Unosquare ffmediaelement + FFmpeg.AutoGen + FFmpeg 4.0.2 native DLLs |
| Installer | Inno Setup 5 |
| Obfuscation | ConfuserEx (optional — `Build.bat`; not in `Build_Free.bat`) |

---

## Repo conventions worth knowing

- **`Pod.*` = server side**, **`LeapVR.*` = client side**. Strict.
- **Interface separation**: most modules have `<Project>.Interfaces` siblings (`LeapVR.Shell.Repository` ↔ `LeapVR.Shell.Repository.Interfaces`). Always reference the `.Interfaces` project from upstream code, never the implementation directly.
- **`IResult<T>` pattern (server)**: business-level errors propagate as `Result.HasError()` rather than exceptions. Controllers and gRPC services chain `result.ArgNotNull(...)` style validators.
- **`global.json` pins the SDK to 10.0.204** at the repo root, with `rollForward: latestFeature`. Server projects target `net10.0`; kiosk/Content-Creator stay on `net471` (WPF, .NET-Framework Developer Pack required to build).
- **Configuration `Release_ShellClient`**: client/Content-Creator use this configuration (not plain `Release`) so post-build copies and obfuscator-ready output behave correctly.
- **Native dependencies**: FFmpeg 4.0.2 win64-shared DLLs and the Unity-built `vrlounge_desktop.exe` are copied into the client bin via post-build events. Both must exist before the client build succeeds — see [build-and-deploy.md](architecture/build-and-deploy.md).

---

## Where to look for X

| If you want to … | Go to |
|---|---|
| Add a new REST endpoint | `server/Pod.Web.Center/`, then `server/Pod.DtoModels/` for the DTO |
| Add a new entity | `server/data/Pod.Data.Models/`, add migration in `server/data/Pod.Data/` |
| Change a gRPC contract | `server/grpc/Pod.Grpc.Base/` (the `.proto` files), then re-gen + adjust services |
| Add a new kiosk module | `client/LeapVR.Shell.Modules/`, register in `Bootstrapper.cs` |
| Add a new game platform (e.g. Epic, GOG) | `client/LeapVR.Shell.Modules/Platform/` — implement `IPlatformModule` |
| Tweak the in-VR home experience | rebuild Unity project (out of repo); drop new `vrlounge_desktop.exe` into `LeapVR.Shell.3rdParty/bin/vr_desktop/` |
| Change a session state transition | `server/data/Pod.Data.Models/Shell/SessionDetails.cs` |
| Adjust email templates | DB-backed (`EmailContentTemplate` entity) — operator UI; engine in `server/Pod.MailEngine/` |
| Trace a station-to-server call | start at `client/LeapVR.Shell.Services/` → `Pod.Grpc.Base.Client` → server `Pod.Grpc.ShellHost.Server` |

---

*Documentation last regenerated together with the codebase at `Pod.Web.Center` build version 2019.6.x.*
