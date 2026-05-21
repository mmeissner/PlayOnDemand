# LeapVR.Shell.Domain.Models

> The shared vocabulary of the kiosk: domain entities, configuration POCOs, controller-message markers, the `IGlobalConfiguration` and `GlobalConfig` startup contract, and a handful of well-known constants. Pure contracts — no behaviour beyond DTO bookkeeping.

## Purpose

This is the lowest-coupling project in the client tier. Every other client project (controllers, modules, repositories, services, view-models, setup) references it. It contains:

- **Domain entities and interfaces** — apps, app installations, app categories, sessions, billing, hardware devices, multimedia playlists, platform/account info, station info.
- **Configuration POCOs** — strongly typed `*Config` classes that the `IConfigFileRepository<>` pattern persists as JSON: `DiskConfig`, `LoginConfig`, `SystemConfig`, `SecurityConfig`, etc.
- **Controller-message markers** — `IController`, `IRunLevelMsgReceiver`, `IExecutionMessageReceiver`, plus the `StationMessage` / `TerminationSignal` types that the `StationController` broadcasts.
- **Startup constants and the install-root environment variable** — `GlobalConfig.cs` defines the CLI flags (`-config`, `-uninstall`, `-d`, `-rdbg`), the `LEAPVRINSTALLBASEDIRECTORY` env var, the `LeapPlay` firewall group name, the `%APPDATA%\LeapPlay` persistent dir, and the singleton `IGlobalConfiguration` factory.

It deliberately holds *no* implementation logic worth speaking of: `GlobalConfig` resolves the install directory, ensures the persistent + config-files directories exist, and caches an `IGlobalConfiguration`. That's it.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Domain.Models.dll`)
- **Key NuGet packages:**
  - `NLog` 4.5.11 — `LoggingExtensions.cs` exposes a few formatters; `GlobalConfig` uses it for early-startup diagnostics
  - (no JSON.NET here — serialisation lives in projects that do the persisting)
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`
  - `Pod.Data.Infrastructure`

This project intentionally has the smallest reference graph in the client tier.

## Responsibility

**It IS responsible for:**
- Defining the *names* and *shapes* shared across the kiosk (entity interfaces, enum values, message types, marker interfaces).
- Owning the bootstrap-time singleton `GlobalConfig` / `IGlobalConfiguration` that everyone reads paths from.
- Owning the CLI-flag string constants so all parsers agree on `-config` / `-uninstall` / `-d`.

**It is NOT responsible for:**
- Persistence (no LiteDB, no JSON.NET serialiser in here — see `LeapVR.Shell.Repository` and `LeapVR.Shell.Modules.ConfigFileRepository`).
- gRPC types (those live in `Pod.Grpc.*`).
- Behaviour, scheduling, validation chains.

## Public API surface

Top-level constants and singletons:

| Type | Notes |
|---|---|
| `GlobalConfig` (static) | CLI flag strings, firewall group name, env-var name, persistent-dir template, factory `GetGlobalConfiguration()`. Static ctor sets `LEAPVRINSTALLBASEDIRECTORY` from the entry assembly's directory if not already in env. |
| `IGlobalConfiguration` / `GlobalConfiguration` | `PersistentDirectory`, `DatabaseFilePath`, `ShellExecutablePath`, `ConfigFilesDirectory` etc. |
| `IConfigFileRepository<T>` | (declared here, implemented in `LeapVR.Shell.Modules/ConfigFileRepository.cs`) — generic JSON-on-disk config repo. |
| `LoggingExtensions` | NLog formatting helpers used across the codebase. |

By folder, the salient namespaces:

| Folder | Highlights |
|---|---|
| `App/` | `IAppInstallationData`, `AppInstallationType` (`Container` / `Platform`), `IAppCategory`, app-related events. |
| `Authentication/` | `ILoginIntention`, `ILoginDecision`, `ILoginDecisionResult`, station credentials shape. |
| `Billing/` | `IntendAnonymousSessionResult`, billing-rate types used by sessions. |
| `CertLicense/` | License/cert payload shapes (used by `LeapCertLicense` in modules). |
| `Container/` | `IAppInstallationHeader`, `IAppInstallationHeaderSerializer`, `IContainerPackage`, container-package descriptors. |
| `Controllers/` | `IController`, `IRunLevelMsgReceiver`, `IExecutionMessageReceiver`, `StationMessage`, `TerminationSignal`, `ExecutionMessage`. The wiring contracts the `StationController` enumerates over. |
| `Customization/` | `*Config` classes: `DiskConfig`, `SystemConfig`, `SecurityConfig`, `LoginConfig`, `RpcClientConfig` shapes; theming customisation. |
| `Disk/` | `IDiskInfo`, drive-info abstractions. |
| `Execution/` | `IAppExecution`, run-level enums, execution-message types. |
| `Hardware/` | Hardware device contracts, `IHardwareDeviceTemplate`. |
| `Input/` | Input event/state contracts (gamepad-agnostic). |
| `Language/` | `ILanguageSelector`, language enum/culture wrappers. |
| `Module/` | `Migration` records used by `LeapVR.Shell.Repository.Database` migrations. |
| `Multimedia/` | `IMultimediaPlaylist`, `IMultimediaTrack`, multimedia settings. |
| `Platform/` | `IPlatform`, `IPlatformAccount`, `AccountType`, `InstallationType` flags. |
| `Station/` | `StationMode`, `IStationInfo`, station-shape contracts. |
| `Statistics/` | `IAppStatistic` shape. |
| `System/` | System-level enums (`StartupOptions` flags, etc.). |
| `UserInterface/` | `IUIMessageBroker`, `IShell`, `IUI*Event`, `EventMessages/`. The pub/sub backbone for UI events. |

## Internal structure

```
LeapVR.Shell.Domain.Models/
├── GlobalConfig.cs               Static — CLI flags, env var, paths, IGlobalConfiguration factory
├── LoggingExtensions.cs          NLog helpers
├── App/                          App identity, installation type, app category contracts
├── Authentication/               Login intent/decision contracts
├── Billing/                      Session billing types
├── CertLicense/                  Cert/license payload shapes
├── Container/                    Container-package descriptors + header serializer contract
├── Controllers/                  IController + message-receiver markers + StationMessage
├── Customization/                *Config POCOs
├── Disk/                         Disk-info contracts
├── Execution/                    Run-level + execution-message types
├── Hardware/                     Hardware device contracts
├── Input/                        Input event abstractions
├── Language/                     ILanguageSelector + culture types
├── Module/                       Migration descriptors
├── Multimedia/                   Playlist/track contracts
├── Platform/                     Platform + account contracts
├── Station/                      Station-state contracts
├── Statistics/                   Statistic contracts
├── System/                       System enums
├── UserInterface/                IUIMessageBroker + IShell + UI events (incl. EventMessages/)
├── Properties/AssemblyInfo.cs
└── packages.config
```

## Notable patterns / gotchas

- **`GlobalConfig` mutates process env on first touch.** Any code that reads `LEAPVRINSTALLBASEDIRECTORY` must either run after the static ctor has fired or be aware that it's set lazily. In practice everything goes through `GlobalConfig.GetGlobalConfiguration()`, which forces the static ctor.
- **Persistent directory uses `%APPDATA%\LeapPlay`** — `%APPDATA%` is per-user and roams. The kiosk runs as a Windows local user, so this is effectively a per-machine, per-user path. The wizard creates it during install; the runtime `GlobalConfig.GetGlobalConfiguration` also `Directory.CreateDirectory`s it on first call.
- **CLI flag constants live here, not in `Program.cs`.** Setup, the wizard, and the kiosk all read them from `GlobalConfig.*Parameter`. **Don't redeclare them locally.**
- **Most "interfaces" here are pure data shapes.** They look like Java POJO interfaces with only properties. Treat them as DTOs the rest of the system flows through, not as polymorphic call sites.
- **`Pod.Data.Infrastructure` is the single non-LeapVR dependency.** It provides `IResult<T>` (the server-side error pattern) so domain methods can speak the same error vocabulary as the gRPC server. Be aware: this is the only place the client tier transitively touches a "Pod.*" assembly besides the gRPC layer.

## Consumers

Effectively *everything* in the client tier:

- `LeapVR.Shell` — bootstraps types defined here, owns `IShell`/`IUIMessageBroker`.
- `LeapVR.Shell.Controllers` — implements `IController` and message-receiver interfaces; reads `StationMode`/`StationMessage`.
- `LeapVR.Shell.Modules` — implements `IConfigFileRepository<>`; consumes `IGlobalConfiguration`.
- `LeapVR.Shell.Modules.Interfaces` — references for shared types.
- `LeapVR.Shell.Repository` / `.Repository.Interfaces` — entity contracts (`IAppInstallationData`, `IMultimediaPlaylist`, …).
- `LeapVR.Shell.Services` / `.Services.Interfaces` — billing/auth contracts (`ILoginIntention`, `IntendAnonymousSessionResult`, …).
- `LeapVR.Shell.Managers` — `ILocalMachine` shape lives here.
- `LeapVR.Shell.Categories` — `IAppCategory` is here.
- `LeapVR.Shell.Language` — `ILanguageSelector` is here.
- `LeapVR.Shell.Setup` — reads CLI flags and `*Config`s.

## Related docs

- Sister projects: literally all of `docs/client/*`. Particularly close: [`LeapVR.Shell.Repository.Interfaces`](../LeapVR.Shell.Repository.Interfaces/README.md) (entities), [`LeapVR.Shell.Modules.Interfaces`](../LeapVR.Shell.Modules.Interfaces/README.md) (modules consume the configs/contracts here).
- Tier overview: [`docs/client/README.md`](../README.md)
- Architecture: `docs/architecture/data-model.md` (planned — server-side EF entities; the client-side equivalents live here).
