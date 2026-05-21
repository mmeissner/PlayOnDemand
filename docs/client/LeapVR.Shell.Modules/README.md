# LeapVR.Shell.Modules

> Concrete subsystems behind the controller tier: VR (OpenVR), input (XInput), platform plug-ins (VBox custom containers + Steam library), container packaging (ZIP-based), multimedia (FFmpeg), network state, hardware-device probing, the JSON-on-disk `ConfigFileRepository`, and the bootstrap helper `ShellConfigurator`.

## Purpose

Modules are the *nouns* under the controllers. Where `LeapVR.Shell.Controllers` says "launch this app", a module knows how to *actually do it*: which executable to start under Steam, where to extract a `.vbox` archive, how to call into `openvr_api.dll`, how to subscribe to XInput poll events, how to enumerate USB-attached HMD vendors.

Every module implements either `IBaseModule` directly or one of the more specific interfaces in `LeapVR.Shell.Modules.Interfaces` (`IPlatformModule`, `IVrModule`, `IXInputModule`, `IContainerModule`, `IMultimediaProvider`, `IPlaylistModule`, `INetworkModule`, etc.). They are registered as singletons in `LeapVR.Shell/Bootstrapper.cs#RegisterModules`. Several are also added to the `IBaseModule` collection so the platform-state provider can introspect them.

This project is also the home of two cross-cutting helpers that don't fit a single subsystem: `ConfigFileRepository<T>` (the generic JSON-on-disk config repo wired up via `Bootstrapper.RegisterConfigurations`) and `ShellConfigurator` (the bootstrap precondition checker invoked from `Program.Main` to decide whether to launch the wizard).

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Modules.dll`)
- **Key NuGet packages:**
  - `DotNetZip` 1.10.1 — the `.vbox` container archive format (despite the user-facing perception of "7zip"; the actual implementation is ZIP)
  - `Newtonsoft.Json` 12.0.2 — config persistence and container header serialisation
  - `NLog` 4.5.11 — logging
  - `Polly` 5.7.0 — retry policies for fragile platform calls
  - `System.Reactive` 4.1.5 — observables in HMD watchdog and XInput pump
- **Project references (in this repo):**
  - Contracts: `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Repository.Interfaces`
  - Native: `LeapVR.Shell.OpenVR.Wrapper` (Valve `openvr_api.dll` bindings)
  - Vendored 3rd-party (file refs in `LeapVR.Shell.3rdParty/`): `XInputDotNetPure`, `Unosquare.FFME.Common`, `Unosquare.FFME.Windows`
  - Other client projects: `LeapVR.Shell.Categories`, `LeapVR.Shell.Managers`
  - Shared: `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Win`, `LeapVR.Utilities.Steam`, `LeapVR.Utilities.Windows`, `Pod.Data.Infrastructure`

## Responsibility

**It IS responsible for:**
- Concrete implementations of every `IBaseModule`-derived interface.
- Hosting OpenVR / SteamVR process lifecycle (`OpenVrModule`, `OpenVrProcessHandler`, `OpenVRFilesHandler`, `HmdActivityWatchdog`, `VrDesktopModule`).
- Decoding/launching `.vbox` containers (`Container/`).
- Wrapping XInput polling (`XInput/XInputModule`).
- Discovering and launching Steam apps (`Platform/Steam/SteamPlatformModule`, `SteamPlatformProvider`, `SteamApplicationExecution`, `SteamProcessExecutionLogic`, `SteamProcessMonitorInstruction`).
- Discovering and launching VBox container apps (`Platform/VBox/VBoxPlatformModule`, `VBoxApplicationExecution`).
- Background media playback (`Multimedia/MultimediaModule`, `MultimediaProvider`, `PlaylistModule`).
- Exposing network state (`Network/NetworkModule`).
- Enumerating attached HMDs (`Hardware/HardwareDeviceModule`).
- Persisting per-module configs as JSON (`ConfigFileRepository<T>`, `FileConfig/*ModuleConfig.cs`).
- The first-boot precondition check (`ShellConfigurator`).

**It is NOT responsible for:**
- Business orchestration (controllers do that).
- Drawing UI (no XAML in this project).
- Speaking gRPC (services do that).
- LiteDB persistence (`LeapVR.Shell.Repository` does that — config files here are JSON-on-disk, separate concern).

## Public API surface — every module, grouped

### VR

| Class | Interface | Notes |
|---|---|---|
| `OpenVrModule` | `IOpenVrModule` (≡ `IVrModule`) | Owns the SteamVR lifecycle. Reads `OpenVrModuleConfig`, applies `IOpenVrSettingsSet` from `IOpenVrSettingsSetRepository`. Pulls Steam install path from `LeapVR.Utilities.Steam.SteamLib`. |
| `OpenVrProcessHandler` | (internal helper) | Static handler that owns the actual `vrserver.exe` / `vrmonitor.exe` lifecycle. |
| `OpenVRFilesHandler` | (internal helper) | Reads/writes `steamvr.vrsettings` and friends. |
| `HmdActivityWatchdog` | `IHmdActivityWatchdog` | Periodically polls headset proximity sensor; emits `IHmdActivityEvent`s used by `VirtualRealityController` to detect "user took the headset off → end session". |
| `VrDesktopModule` | `IVrDesktopModule` | Launches and supervises `vrlounge_desktop.exe` (the Unity-built in-VR home environment). |
| `OpenVrSettingsSet` / `OpenVrSettingsSetDto` / `OpenVrSettingsSetRepository` | `IOpenVrSettingsSet`, `IOpenVrSettingsSetRepository` | A "settings set" is a named bundle of `steamvr.vrsettings` overrides; the repository persists multiple sets and applies the active one. |
| `OpenVrSettingFile` / `OpenVrSettingFileDto` / `OpenVrSettingEntityDetails` / `SteamVrConfigFile` | (DTOs) | Strongly typed Steam VR settings file shapes. |
| `ControllerButtonActionEvent` / `HmdActivityEvent` | `IControllerButtonActionEvent`, `IHmdActivityEvent` | Event types emitted by VR subsystem. |
| `VrConstants` | (static) | Process names, registry paths, well-known GUIDs. |

### Input

| Class | Interface | Notes |
|---|---|---|
| `XInputModule` | `IXInputModule` | Polling loop over `XInputDotNetPure`. Emits `XInputButtonArgs`, `XInputCompositeArgs`, exposes `InputState`. Consumed by `BehaviorController` and `GamepadController`. |
| `XInputBinding` / `XInputGesture` / `XInputEventArgs` | (event types) | Higher-level event shapes. |

### Platform — VBox

| Class | Interface | Notes |
|---|---|---|
| `VBoxPlatformModule` | `IPlatformModule` | The "in-house" platform: reads `.vbox` containers from disk, presents them as installable apps. Used as the canonical `IPlatformModule` for first-party content. |
| `VBoxApplicationExecution` | `IApplicationExecution` | Process-launch logic for a specific VBox app (uses `ApplicationExecutionBase` infrastructure). |

### Platform — Steam

| Class | Interface | Notes |
|---|---|---|
| `SteamPlatformModule` | `IPlatformModule` | Scans the Steam library via `LeapVR.Utilities.Steam`, projects installed apps. |
| `SteamPlatformProvider` | `IPlatformStateProvider` | Reports Steam-side health state (Steam running? library reachable?). |
| `SteamAppDisplayInfo` | (DTO) | Steam-specific display info shape. |
| `SteamApplicationExecution` | `IApplicationExecution` | Launches an app via `steam://rungameid/...`. |
| `SteamProcessExecutionLogic` / `SteamProcessMonitorInstruction` / `DiskEntity` | (helpers) | Steam-process supervision (Steam fans off children, so monitor logic is non-trivial). |

Shared platform helpers in `Platform/`:

- `ApplicationExecutionBase`, `BaseApplicationExecution` — common process-execution infrastructure.
- `PlatformStateProvider` — generic state-provider implementation.

### Container

| Class | Interface | Notes |
|---|---|---|
| `ContainerModule` | `IContainerModule` | Entry point. Recognises `.vbox` extension, dispatches to `AppInstallationContainer`. |
| `AppInstallationContainer` | `IAppInstallationContainer<IContainerPackage>` | Reads a `.vbox` header, exposes the contained zip package. |
| `NewAppInstallationContainer` / `NewAppInstallationHeader` / `NewPackage` | (builder side) | Used by the Content Creator-built containers when they enter the kiosk. |
| `AppInstallationFile` | (DTO) | One file inside a container package. |
| `ZipContainer` (abstract) / `ZipReadablePackage` | | Underlying ZIP read/write — uses `DotNetZip`. **Note:** common shorthand is "7zip extraction"; the actual format is standard ZIP. |
| `InternalFileInfo` | (DTO) | File-entry metadata pulled from the archive. |

### Multimedia

| Class | Interface | Notes |
|---|---|---|
| `MultimediaModule` | `IMultimediaModule` | Wraps Unosquare FFME for video playback. |
| `MultimediaProvider` | `IMultimediaProvider` | Singleton; serves video files to view models (login background loop, admin previews). |
| `PlaylistModule` | `IPlaylistModule` | Manages the active playlist; persistence delegated to `IMultimediaPlaylistRepository`. |
| `MultimediaPlaylist` / `MultimediaPlaylistData` | (DTOs) | Playlist + entry shapes. |

### Network

| Class | Interface | Notes |
|---|---|---|
| `NetworkModule` | `INetworkModule` | Detects connection state; uses Network List Manager via `NetworkListManagerEvents` (COM). Used by `RemoteServiceController` to decide if an offline UI is needed. |

### Hardware

| Class | Interface | Notes |
|---|---|---|
| `HardwareDeviceModule` | `IHardwareDeviceModule` | Enumerates attached hardware devices via WMI. |
| `HardwareDevice` | `IHardwareDevice` | Single device descriptor. |
| `HardwareDeviceUtil` | `IHardwareDeviceUtil` | Helpers around WMI queries. |

### Cross-cutting helpers in this project

| Class | Notes |
|---|---|
| `ConfigFileRepository<T>` | Generic JSON-on-disk repository implementing `IConfigFileRepository<T>` from `LeapVR.Shell.Domain.Models`. Stores under `IGlobalConfiguration.ConfigFilesDirectory`. Registered as open generic singleton in `Bootstrapper.RegisterConfigurations`. |
| `ShellConfigurator` (`ShellConfigurator/ShellConfigurator.cs`) | Reads `DiskConfig` via `IConfigFileRepository<DiskConfig>` and exposes `HasValidDiskConfig`. Used by `LeapVR.Shell/Program.Main` to decide whether to short-circuit into the setup wizard. |
| `LeapCertLicense` (`ShellConfigurator/LeapCertLicense.cs`) | Bundle of certificate/license payload utilities. |
| `AppInfoProcessor` (under `Platform/`) | `IAppInfoProcessor` implementation that joins persisted display info with platform info to produce the UI-facing `AppDisplayInfo`. |
| `ExtensionMethods.cs` | Tiny utility extensions used across modules. |
| `Utilities/WinApi/` | Local WinAPI P/Invokes that didn't warrant a sister project. |

### Per-module config classes (`FileConfig/`)

`OpenVrModuleConfig.cs`, `VBoxPlatformModuleConfig.cs`, `VrDesktopModuleConfig.cs`, `XInputModuleConfig.cs` — POCOs persisted via `ConfigFileRepository<T>` so each module owns its tunables.

## Internal structure

```
LeapVR.Shell.Modules/
├── ConfigFileRepository.cs           Generic JSON-on-disk IConfigFileRepository<T>
├── ExtensionMethods.cs
├── Container/                        IContainerModule + .vbox / ZIP support
├── FileConfig/                       Per-module config POCOs
├── Hardware/                         WMI-driven hardware-device enumeration
├── Multimedia/                       FFME-based video + playlist
├── Network/                          Network List Manager wrapping
├── Platform/                         Steam + VBox platform modules + shared execution helpers
│   ├── Steam/                        SteamPlatformModule + provider + execution + monitor
│   └── VBox/                         VBoxPlatformModule + execution
├── ShellConfigurator/                ShellConfigurator (bootstrap precondition) + LeapCertLicense
├── Utilities/WinApi/                 P/Invoke helpers
├── Vr/                               OpenVrModule, VrDesktopModule, HmdActivityWatchdog, settings repo, Steam VR settings file types
├── XInput/                           XInputModule + binding/gesture types
├── Resources/                        Embedded resources (icons etc.)
├── Properties/AssemblyInfo.cs
├── packages.config / app.config
└── LeapVR.Shell.Modules.csproj
```

## Notable patterns / gotchas

- **`IPlatformModule` is a *collection*-style registration.** `Bootstrapper.RegisterModules` builds singletons for `VBoxPlatformModule` and `SteamPlatformModule` and registers them as `IEnumerable<IPlatformModule>`. `PlatformController` enumerates the collection. Adding a third platform = create a new `IPlatformModule`, register it into the collection, no other code changes.
- **Container format is ZIP, not 7zip.** The user-facing description "7zip extraction" is a misnomer — `ZipContainer` is built on `DotNetZip`. Do not introduce 7zip without a separate decision.
- **`OpenVrModule` has hard runtime dependencies on Steam.** It reads Steam's install path from `LeapVR.Utilities.Steam.SteamLib`. Stations without Steam installed will fail to initialise SteamVR; this is by design — the kiosk targets SteamVR.
- **`vrlounge_desktop.exe` is owned by `VrDesktopModule`.** The binary is built out-of-tree (Unity) and copied from `LeapVR.Shell.3rdParty/bin/vr_desktop/` into the `LeapVR.Shell` post-build directory. This module supervises its lifecycle but does not build it.
- **`HmdActivityWatchdog` polls; it does not subscribe to events.** OpenVR exposes pose data; the watchdog samples it periodically and decides "active vs idle" based on movement deltas. Its threshold lives in `OpenVrModuleConfig`.
- **`ConfigFileRepository<T>` is generic-singleton-registered.** The same instance answers `IConfigFileRepository<DiskConfig>` and `IConfigFileRepository<SystemConfig>` etc. via SimpleInjector's open-generic registration — there's not a separate file per `T` until you write one. Files land at `IGlobalConfiguration.ConfigFilesDirectory/<TypeName>.json`.
- **`ShellConfigurator.HasValidDiskConfig` is the *only* hook between `Program.Main` and module-tier code.** Touching it has knock-on effects on first-run UX.
- **No view models here.** Everything is testable, headless. UI lives in `LeapVR.Shell/UI/`.

## Consumers

- `LeapVR.Shell` — registers everything in `Bootstrapper.RegisterModules` / `RegisterConfigurations`.
- `LeapVR.Shell.Controllers` — direct project reference; `PlatformController` enumerates `IEnumerable<IPlatformModule>`, `VirtualRealityController` consumes `IVrModule` + `IHmdActivityWatchdog`, `BehaviorController` consumes `IXInputModule`.
- `LeapVR.Shell.Setup` — uses `ShellConfigurator` to decide first-run, uses `VBoxPlatformModule` during uninstall.
- `LeapVR.Shell.Repository` — none directly (other direction: modules consume `LeapVR.Shell.Repository.Interfaces`).

## Related docs

- Sister contract: [`LeapVR.Shell.Modules.Interfaces`](../LeapVR.Shell.Modules.Interfaces/README.md)
- Closely related: [`LeapVR.Shell.OpenVR.Wrapper`](../LeapVR.Shell.OpenVR.Wrapper/README.md) (native VR P/Invoke), [`LeapVR.Shell.Controllers`](../LeapVR.Shell.Controllers/README.md) (consumer), [`LeapVR.Shell.Repository.Interfaces`](../LeapVR.Shell.Repository.Interfaces/README.md) (multimedia + OpenVR-settings repos)
- Vendored 3rd parties under `LeapVR.Shell.3rdParty/`: `ffmediaelement` (Unosquare FFME), `XInputDotNetPure`, `FFmpeg.AutoGen`, `QRCoder`, `openvr_api.dll` (used via `LeapVR.Shell.OpenVR.Wrapper`)
- Tier overview: [`docs/client/README.md`](../README.md)
