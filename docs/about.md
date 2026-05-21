# About PlayOnDemand

> The "what shipped and why" page. For the marketing front-door see
> [`README.md`](../README.md); for the illustrated walkthrough see
> [`docs/manual/`](manual/); for engineering detail see
> [`docs/architecture/`](architecture/) and the per-tier READMEs under
> [`docs/server/`](server/), [`docs/client/`](client/), and
> [`docs/content-creator/`](content-creator/).

## What this repo is

PlayOnDemand (PoD) is an open-source **VR-arcade and gaming-lounge management
platform**. It's the kind of stack a venue operator would deploy to run a
room full of VR PCs: a central server that authenticates operators and
stations, mints sessions, monitors heartbeats, and bills subscriptions, plus
a Windows kiosk binary that drives the VR runtime, launches games, and
reports state.

```
┌─────────────────────────────────────────────────────────────────┐
│  Pod.Web.Center        ASP.NET Core on .NET 10                  │
│   • REST API (JWT)        for operators / external API users    │
│   • Razor Pages           account portal                        │
│   • gRPC server (TLS)     station ↔ server protocol             │
│   • PostgreSQL via EF     persistence                           │
│   • Background services   email queue, connection health        │
└──────────────┬───────────────────────────────────┬──────────────┘
               │ gRPC over TLS                     │ HTTPS / JWT
       ┌───────▼────────┐                ┌─────────▼────────┐
       │ LeapPlay.Shell │                │ Operator UI       │
       │ WPF / .NET 4.7 │                │ Flutter web app   │
       │ Windows kiosk  │                │ (flutter_operator)│
       │ • OpenVR       │                └──────────────────┘
       │ • Game launcher│
       │ • LiteDB local │              ┌─────────────────────────┐
       │ • Multimedia   │              │ LeapPlay.Content.Creator│
       └────────────────┘              │ Authoring tool for      │
                                       │ .vbox game packages     │
                                       └─────────────────────────┘
```

## Status

This is a **one-shot open-source release** of a platform that was previously
operated as a closed-source commercial product. The server has been ported to
.NET 10 LTS; the kiosk and content creator still target .NET Framework 4.7.1
(WPF) and Windows. A minimal Flutter web operator UI ships alongside
(`flutter_operator/`) covering the v1.0.0 operator critical path — login,
station list, station detail, API-key mint + list. A richer mobile-friendly
Flutter app (`flutter_operator_mobile/`) ports the legacy field-ops client to
the current API.

The release is intended as a **reference implementation** that an arcade
operator could self-host. It runs end-to-end on a single Linux VM via Docker
Compose (3 containers: postgres + server + nginx-served operator UI), with
optional TLS via Let's Encrypt, and on a Windows kiosk machine via the
standalone installer produced by `LeapVR.Shell.Build/Build_Free.bat`.

## Heritage

The product shipped in production from ~2018 as **LeapVR** / **LeapPlay**, a
turnkey VR-arcade solution sold with custom hardware cabinets. The
screenshots in [`docs/manual/`](manual/) are real production UI captures from
that era. The codebase, design, and runtime artefacts have been preserved and
forward-ported into this open-source release:

- The server moved from ASP.NET Core 2.1 → .NET 10 LTS (auth, EF, gRPC stack
  migrated together).
- The kiosk and Content Creator stayed on .NET Framework 4.7.1 (the WPF
  runtime + OpenVR P/Invoke layer would have been a much larger port).
- The `LeapCertLicense` / hardware-certificate gating that locked the
  original product to vSpace's signed cabinets was excised; auth is now
  `(StationId, Password)` plus `(StationApiKey.PublicKey, Secret)` HMAC,
  documented in [`docs/architecture/auth.md`](architecture/auth.md).
- Production hostnames (`leap-play.com`, `leap-vr.cn`, …) and embedded
  certificate chains were replaced with `example.com` placeholders so a
  fresh clone is buildable without any vendor coupling.

## Quick start (server)

```sh
# 1. Clone
git clone https://github.com/<your-org>/PlayOnDemand
cd PlayOnDemand

# 2. Set secrets
cp .env.example .env
# edit .env: JWT_SECRET, POSTGRES_PASSWORD, ADMIN_EMAIL, ADMIN_PASSWORD, POD_HOSTNAME

# 3. Up (postgres + server + Flutter operator UI; all healthy after ~30s)
docker compose up -d

# 4. Verify backend
curl http://localhost/health                    # -> "Healthy"

# 5. Visit the operator UI at http://localhost:8080/
#    log in with ADMIN_USERNAME / ADMIN_PASSWORD from .env
```

The first run seeds an admin user from `ADMIN_EMAIL` / `ADMIN_PASSWORD` in
`.env` and migrates the Postgres schema. Subsequent runs are idempotent.

If you set `POD_HOSTNAME` to a real domain you control and point its DNS at
the server, the Let's Encrypt integration will issue a real TLS cert on first
request.

## Quick start (kiosk)

> The kiosk binary replaces `explorer.exe` as the Windows shell in production
> mode. **Always pass `-debug` when smoke-testing on a dev box** unless you
> know exactly what you're doing.

```cmd
:: Windows, with .NET 10 SDK 10.0.204+ (bundles MSBuild 18, required for the
:: kiosk's Microsoft.NET.Sdk.WindowsDesktop resolver), .NET Framework 4.7.1
:: Dev Pack, VS 2022 17.14+ Build Tools (for the C++ XInputInterface vcxproj),
:: Inno Setup 5
LeapVR.Shell.Build\Build_Free.bat
:: produces dist\LeapPlayInstaller.exe
LeapPlay.Shell.exe -debug
```

Run the Setup wizard once to register the station with the server (operator
account → create station → mint API key).

## Documentation map

Full architecture, project, and usage docs live under [`docs/`](.). The
orientation map is [`docs/README.md`](README.md).

The five most useful starting points for engineers:

- [`docs/architecture/overview.md`](architecture/overview.md) — system topology + request flow.
- [`docs/architecture/auth.md`](architecture/auth.md) — REST JWT vs `amx` HMAC vs gRPC metadata.
- [`docs/architecture/grpc.md`](architecture/grpc.md) — contracts, services, methods.
- [`docs/architecture/session-lifecycle.md`](architecture/session-lifecycle.md) — the core domain FSM.
- [`docs/architecture/build-and-deploy.md`](architecture/build-and-deploy.md) — how to build and ship.

For end-users (operators, content owners, station admins), start with the
illustrated [manual](manual/).

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md). Issues and PRs welcome.

## Security

See [SECURITY.md](../SECURITY.md) for the disclosure policy and operator
hardening notes.

## License

PlayOnDemand is licensed under the
[Apache License 2.0](../LICENSE). Bundled third-party software and native
runtime dependencies (FFmpeg, OpenVR, Unity, …) are listed under their
respective licenses in [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md).
