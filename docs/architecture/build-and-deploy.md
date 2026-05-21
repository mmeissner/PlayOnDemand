# Build & Deploy

> How the three deployables are produced and how they hit production.

---

## Three pipelines, one repo

| Deployable | Build tool | Target framework | Output |
|------------|-----------|------------------|--------|
| **`Pod.Web.Center`** (server) | `dotnet publish` via Docker multi-stage `Dockerfile` | `net10.0` | Container image `pod-web-center:latest` |
| **`LeapPlay.Shell.exe`** (kiosk) | MSBuild via `LeapVR.Shell.Build/Build_Free.bat` | `net471`, `x64`, Configuration `Release_ShellClient` | Windows installer `LeapPlay_Setup.exe` (Inno Setup) |
| **`LeapPlay.Content.Creator.exe`** (authoring tool) | Same as kiosk (`Build_Free.bat` builds both) | `net471`, `x64` | Bundled in the same installer |

They don't share a build invocation. The kiosk path needs MSBuild + the .NET-Framework 4.7.1 Developer Pack on a Windows machine; the server path needs `dotnet` 10 SDK + Docker.

> **Don't `dotnet build PoD.sln`.** The solution includes legacy .NET-Framework projects that `dotnet`'s bundled MSBuild can't build (resx / AL task incompatibilities). Build server projects individually with `dotnet build <project>.csproj`; build the kiosk via `Build_Free.bat`.

---

## Server pipeline (`docker compose up -d`)

`Pod.Web.Center/Dockerfile` is a two-stage build:

1. **Build stage** — `mcr.microsoft.com/dotnet/sdk:10.0`
   - Restores against the curated dep graph (server projects only — kiosk and shared .NET-Framework projects are not copied into the container context).
   - Runs `dotnet publish -c Release -o /app/publish` against `Pod.Web.Center.csproj`.
2. **Runtime stage** — `mcr.microsoft.com/dotnet/aspnet:10.0`
   - Copies `/app/publish` from the build stage.
   - Runs as the pre-existing non-root `app` user (UID 1654).
   - Mounts two named volumes: `letsencrypt-data` at `/app/certs` (ACME account key + issued chain), `server-data` at `/app/data` (future use).
   - Exposes `:80` (ACME challenge) and `:443` (REST + gRPC on the same Kestrel pipeline).

`docker-compose.yml` glues the server image to a `postgres:16-alpine` container with healthcheck-gated start ordering. Configuration flows through env vars only — see [`docs/usage/server-deployment.md`](../usage/server-deployment.md) for the full `.env` reference and the "10 minutes from clone to running" walkthrough.

### Manual server build (outside of Docker)

If you want to run the server directly on a host:

```sh
dotnet publish Pod.Web.Center/Pod.Web.Center.csproj -c Release -o ./out
cd out
# Provide secrets via env vars or appsettings.Production.json. See appsettings.Development.json.example.
ConnectionStrings__PodApiContext="..." \
AuthConfig__SecretKey="..." \
ConfigSuperuser__Password="..." \
... \
dotnet Pod.Web.Center.dll
```

This is essentially what the runtime container does; the Dockerfile is the canonical recipe.

---

## Kiosk pipeline (`LeapVR.Shell.Build/Build_Free.bat`)

A single self-anchored batch script:

```cmd
LeapVR.Shell.Build\Build_Free.bat
```

What it does:
1. Locates VS MSBuild via `vswhere` (used for the native `XInputInterface.vcxproj`) and verifies the .NET 10 SDK is on `PATH` (used for the kiosk + Content Creator csprojs).
2. Downloads FFmpeg 4.x LGPL win64-shared into `LeapVR.Shell.3rdParty/bin/ffmpeg-*` if not already present.
3. Restores + builds:
   - `XInputInterface.vcxproj` via VS MSBuild (`-p:Configuration=Release -p:Platform=x64`).
   - `LeapVR.Shell.csproj` (Configuration=`Release_ShellClient`, Platform=`x64`) via `dotnet build`.
   - `LeapVR.Content.Creator.csproj` (same config) via `dotnet build`.
4. Runs Inno Setup against `LeapVR.Shell.Installer/LeapVR_LeapPlay_Setup_<version>.iss` to produce `dist/LeapPlay_Setup.exe`.

Why the split: both kiosk csprojs are SDK-style WPF (`Microsoft.NET.Sdk.WindowsDesktop`, still `net471`) and resolve through the .NET 10 SDK's MSBuild 18 — VS 2022 17.14's MSBuild 17 can't load them. `dotnet build` picks up MSBuild 18 from the SDK install. The native vcxproj still goes through VS MSBuild because the C++ toolset isn't part of the .NET SDK. When VS 2022 17.15+ (MSBuild 18) becomes ubiquitous this split can collapse back to a single `%MSBUILD%` invocation.

No obfuscation, no code signing (the "Free" in the filename). For commercial builds the original repo had a `Build.bat` that ran ConfuserEx + signtool on top — that path is not part of v1.0.0.

**Don't invoke `LeapVR.Shell.csproj` directly without `Build_Free.bat`.** The kiosk csproj's `PodCopyVrDesktopAndNlogConfig` MSBuild target reads `$(SolutionDir)` to find the 141 MB `vrlounge_desktop.exe` Unity binary; when invoking `dotnet build` on the csproj directly, set `SolutionDir` explicitly: `SolutionDir="$(pwd)/" dotnet build LeapVR.Shell/LeapVR.Shell.csproj -c Release_ShellClient -p:Platform=x64`.

---

## Prerequisites (build box)

### Server
- Docker Engine 24+ + Docker Compose v2 (`docker compose`, not `docker-compose`).
- For non-Docker development: .NET 10 SDK (`global.json` pins 10.0.204; `rollForward: latestFeature` allows newer patch revisions).

### Kiosk + Content Creator (Windows only)
- .NET 10 SDK (`global.json` pins 10.0.204; `dotnet build` for the SDK-style WPF kiosk csprojs).
- .NET Framework 4.7.1 SDK + Developer Pack / targeting pack (the kiosk targets `net471`).
- Visual Studio 2022 17.14+ Build Tools, with:
  - "Desktop development with C++" (for the native `XInputInterface.vcxproj`)
  - ".NET desktop development" (for the WPF designer; optional if you only build via `Build_Free.bat`)
- Inno Setup 5 at `_Tools/Inno_Setup_5/ISCC.exe`.
- `_Tools/nuget.exe` for restoring the packages.config-style 3rd-party kiosk sibling csprojs (FFME, QRCoder, SteamWebAPI2 etc.).
- ~3 GB free under `LeapVR.Shell.3rdParty/bin/` for the FFmpeg + Unity Lounge binaries.

---

## Production runtime layout

### Docker Compose (canonical)

```
<host>
├── docker-compose.yml             ← from this repo
├── .env                           ← operator-provided, gitignored
└── (named volumes)
    ├── postgres-data              ← /var/lib/postgresql/data
    ├── letsencrypt-data           ← /app/certs (ACME account key + chain)
    └── server-data                ← /app/data
```

The server reaches the database via the service-network alias `postgres`. The Let's Encrypt cert auto-renews ~20 days before expiry (`LetsEncryptOptions.DaysBefore`). Operator first contact is at `https://<POD_HOSTNAME>/`.

### Kiosk install (one per VR PC)

```
C:\Program Files\LeapPlay\
├── LeapPlay.Shell.exe             ← WPF kiosk
├── LeapPlay.Content.Creator.exe   ← Content authoring tool (optional)
├── ServerConfig.json              ← connect endpoint
├── StationConfig.json             ← (StationId, Station.Password)
├── ApiKeyConfig.json              ← (StationApiKey.PublicKey, StationApiKey.Secret)
├── vr_desktop\                    ← Unity in-VR home environment
├── ffmpeg-*\                      ← Unosquare ffmediaelement native deps
├── openvr_api.dll                 ← OpenVR runtime
├── XInputInterface.dll            ← Xbox controller native wrapper
└── License\                       ← Bundled NOTICES (third-party attributions)
```

On first launch the operator runs `LeapPlay.Shell.exe -config` to start the Setup wizard which writes the three `*Config.json` files. After that the daemon-style `LeapPlay.Shell.exe` (no `-config`) is what runs in production. **Don't run it without `-debug` on a developer machine** — see [`docs/usage/kiosk-known-issues.md`](../usage/kiosk-known-issues.md).

---

## Troubleshooting

| Symptom | Cause / fix |
|---------|-------------|
| `dotnet build` says SDK 10.0.204 not found | Install .NET 10 SDK or edit `global.json` `rollForward` to allow a newer/older patch. |
| `dotnet build PoD.sln` produces 3000+ errors | You're trying to build .NET-Framework projects with `dotnet`. Build server projects individually (see top of this page). |
| `Build_Free.bat` errors out at vswhere step | VS 2022 not installed, or installed without "Desktop development". Run `vswhere -latest` manually to confirm. |
| `Build_Free.bat` builds but kiosk has missing `vrlounge_desktop.exe` | `$(SolutionDir)` wasn't resolved correctly — invoke via `Build_Free.bat` from the repo root, not in a sub-shell. |
| Docker `server` container restarts on a loop | Check `docker compose logs server` for the actual exception. Common causes: missing required `.env` value, can't reach `postgres` service, Let's Encrypt port 80 blocked. |
| Kiosk launches but immediately exits | Missing `ServerConfig.json` or `StationConfig.json`, or the kiosk can't reach the server. Check `%LOCALAPPDATA%\LeapPlay\logs\`. |
