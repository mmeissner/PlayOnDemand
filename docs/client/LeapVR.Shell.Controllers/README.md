# LeapVR.Shell.Controllers

> Business orchestration tier for the kiosk. Each controller owns one cross-cutting concern (platform, station, VR, firewall, disk, security, behaviour, statistics, system, gamepad, remote service, UI message broker) and is the layer that view models and other controllers consume — not the modules underneath.

## Purpose

Controllers are the *verbs* of the kiosk. Where `LeapVR.Shell.Modules` provides hardware-/library-/protocol-level subsystems (e.g. `OpenVrModule`, `XInputModule`, `SteamPlatformModule`), controllers translate those subsystems into business operations: "launch this app", "block input until session is paid", "enforce firewall rules for this game", "publish heartbeat to the server", "watch for HMD activity and toggle session state".

Controllers are registered as singletons in `LeapVR.Shell/Bootstrapper.cs#RegisterControllers`. Several are also added to two `RegisterCollection` groups so the `StationController` can broadcast messages to them: `IRunLevelMsgReceiver` (Vr, Gamepad, Platform, RemoteService) and `IExecutionMessageReceiver` (Statistics, Vr).

The `Interfaces/` folder contains the **same project's** controller contracts (`IPlatformController`, `IStationController`, …). These are not split into a separate `.Interfaces` sister project — controllers are referenced directly via interface from view models and other controllers. The interface marker `IController` and message-receiver markers like `IRunLevelMsgReceiver`, `IExecutionMessageReceiver` come from `LeapVR.Shell.Domain.Models.Controllers`.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Controllers.dll`)
- **Key NuGet packages:**
  - `Caliburn.Micro` 3.2.0 — only `MyEventAggregator.cs` depends on it
  - `Newtonsoft.Json` 12.0.2 — config + DTO conversion
  - `NLog` 4.5.11 — logging
  - `System.Reactive` 4.1.5 — observables in long-running watchers
  - `Polly` (transitively) — retry policies
- **COM interop (vendored):**
  - `Interop.NetFwTypeLib` — Windows Firewall (`HNetCfg.FwPolicy2`)
  - `Interop.NETWORKLIST` — Network List Manager events
- **Project references (in this repo):**
  - Contracts: `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Repository.Interfaces`
  - Implementations it consumes directly: `LeapVR.Shell.Modules`, `LeapVR.Shell.Repository`, `LeapVR.Shell.Managers`, `LeapVR.Shell.Categories`
  - Shared: `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Win`, `LeapVR.Utilities.Windows`, `Pod.Data.Infrastructure`, `Pod.Enums`
  - Content shared with server: `LeapVR.Content.Shared`

## Responsibility

**It IS responsible for:**
- Coordinating modules to fulfil business operations.
- Holding session-spanning state (current platform, current app, current user, current VR config).
- Acting as the bridge between view models / `RemoteServiceController` and modules / repositories.
- Implementing the message-receiver contracts that the `StationController` broadcasts on (run-level changes, execution events).

**It is NOT responsible for:**
- UI logic — view models (in `LeapVR.Shell/UI/...`) consume controller interfaces.
- Direct hardware or platform calls — modules do that.
- Persistence — repositories do that.
- gRPC frame handling — `LeapVR.Shell.Services` does that; `RemoteServiceController` adapts those services to local message flow.

## Public API surface — every controller

All controllers implement `IController` (a marker from `LeapVR.Shell.Domain.Models.Controllers`).

| Interface | Implementation | Concern (one line) |
|---|---|---|
| `IBehaviorController` | `Behavior/BehaviorController.cs` | Watches `XInput` for shortcut chord patterns (`ControllerShortcutWatch`, `ControllerKeyActionWatch`) and triggers configured key actions; also publishes user-activity signals. |
| `IDiskController` | `Disk/DiskController.cs` | Resolves the `StorageDirectory`, summarises `WholeDiskUsage`, and exposes `DiskEntity` snapshots used by uninstall + free-space checks. |
| `IFirewallController` | `Firewall/FirewallController.cs` | Adds/removes Windows Firewall rules in the `LeapPlay` group via `INetFwPolicy2`; enforces inbound/outbound rules per `FirewallDirection`. |
| `IGamepadController` | `GamePad/GamepadController.cs` | Owns the `XInputModule` lifecycle and routes raw button events into UI-input pipelines. |
| `IPlatformController` | `Platform/PlatformController.cs` (+ `Platform/Installation/InstallationManager.cs`, `PlatformInstallationProcess.cs`, `UninstallationProcess.cs`) | Aggregates all `IPlatformModule` instances (VBox + Steam) into a unified app catalogue. Methods: `GetInstalledApplications`, `GetAvailableApplications`, `Install`, `Uninstall`, `Start`, `Stop`, `GetApplicationInstallationData`, `GetLockedApplications`. Also receives run-level messages. |
| `IStationController` | `Station/StationController.cs` (+ `NetworkListManagerEvents.cs`, `ShellClientDisplayInfo.cs`) | The kiosk's central state machine — owns `StationMode`, `ForceVrDriverRestart`, broadcasts `StationMessage`s to `IRunLevelMsgReceiver` collection, dispatches `TerminationSignal`s. Also requests shutdown/restart. |
| `IStatisticsController` | `Statistics/StatisticsController.cs` (+ `AppStatistic.cs`) | Tracks per-app play time and aggregates statistics. Subscribes to `IExecutionMessageReceiver` events. |
| `ISecurityController` | `Security/SecurityController.cs` | Enforces hidden-cursor / locked-keyboard policies. Reads `SecurityConfig`. |
| `ISessionController` | (interface only — no concrete in-tree; sessions are split between `StationController` and `RemoteServiceController` in this build) | Defines the session-control contract for future refactors. |
| `ISystemController` | `System/SystemController.cs` (+ `QueryCancelAutoPlay.cs`, `RunningObjectTableEntry.cs`) | OS hardening at startup: cancels Windows Autoplay (`IQueryCancelAutoPlay` COM), polls Running Object Table, hides shell ornaments, performs initial environment checks. Called from `Bootstrapper.OnStartup`. |
| `IVirtualRealityController` | `VirtualReality/VirtualRealityController.cs` (+ `SelectableVrType.cs`) | Owns the `IVrModule` (today: `OpenVrModule`), starts/stops SteamVR, applies `OpenVrSettingsSet`, watches for HMD activity events to publish into the message broker. Also receives execution events. |
| `IUpdateController` (interface) / `IUpdateProcess` / `UpdateState` / `VersionInfoDto` | (interface only) | Future hook for shell-side update orchestration. |
| `RemoteServiceController` (no `I*` here — see `RemoteService/Interfaces/`) | `RemoteService/RemoteServiceController.cs` | Adapts `IRemoteServiceSet` (gRPC client surface) into local controller-friendly notifications (`RemoteNotifications`, `IRpcConnection`). Receives run-level messages so it can suspend/resume connections during state transitions. |

Other notable types:

- `Platform/Account/AccountManager.cs` + `PlatformAccount.cs` + `AccountAccess.cs` + `AccountChangeInfo.cs` — manages per-platform credentials (Steam logins).
- `Platform/Installation/InstallationManager.cs` — singleton that coordinates concurrent install/uninstall jobs.
- `Platform/Installation/PlatformInstallationProcess.cs`, `InstallationProcessBase.cs`, `UninstallationProcess.cs`, `InstallProcessData.cs`, `UninstallProcessData.cs` — long-running install/uninstall state machines.
- `Platform/Installation/UIAppInstalledEvent.cs`, `UIAppUninstalledEvent.cs`, `DtoConverter.cs`, `AppInstallationData.cs` — UI-facing notifications and DTO bridging.
- `Platform/AppPlatformInfo.cs`, `AppDisplayInfo.cs`, `AppExecutionInfo.cs`, `AppExecutablesUpdate.cs`, `AppDisplayUpdate.cs` — view-model-shaped projections of platform data.
- `Platform/ProcessExecutionLogic.cs`, `EditableProcessExecutionLogic.cs`, `ProcessMonitorInstruction.cs`, `Executeable.cs`, `Platform.cs` — process-launch primitives shared by VBox and Steam paths.
- `Platform/AppInfoProcessor.cs` — implements `IAppInfoProcessor`; computes display info (icons, categorisation) for installed apps. Registered as a singleton in `Bootstrapper.RegisterModules`.
- `MyEventAggregator.cs` — Caliburn.Micro `EventAggregator` subclass with extra logging.
- `UserInterface/UIMessageBroker.cs` — `IUIMessageBroker` implementation. The single pub/sub channel for UI events (session started/stopped, admin access, app install lifecycle, connect dialog).
- `Behavior/ControllerShortcutWatch.cs`, `ControllerShortcutCondition.cs`, `ControllerKeyActionWatch.cs`, `ControllerKeyAction.cs`, `ControllerKeyState.cs` — chord/shortcut detection for gamepads (e.g. "hold three buttons to exit a session").
- `RemoteService/Interfaces/` — local-only service contracts: `IRemoteServiceSet`, `IRpcConnection`, `IConnectionInfo`, `ILoginDecisionResult`, `IServiceErrorInfo`, `IDto`, `RemoteNotifications`.

## Internal structure

```
LeapVR.Shell.Controllers/
├── Behavior/                Shortcut/chord watchers + BehaviorController
├── Disk/                    DiskController, WholeDiskUsage, DiskEntity
├── FileConfig/              BehaviorConfig (JSON-persisted shortcut bindings)
├── Firewall/                FirewallController (NetFwTypeLib COM)
├── GamePad/                 GamepadController
├── Interfaces/              IController contracts for the entire project
├── MyEventAggregator.cs     Caliburn EventAggregator subclass
├── Platform/                The biggest sub-namespace — see surface table
│   ├── Account/             Platform credential management
│   ├── Installation/        Install/uninstall state machines + UI events + DTO converter
│   └── *.cs                 Platform.cs / PlatformController.cs / process-execution helpers
├── RemoteService/           RemoteServiceController + local service contracts
├── Security/                SecurityController
├── Station/                 StationController + Network List Manager glue
├── Statistics/              StatisticsController + AppStatistic
├── System/                  SystemController + Autoplay/ROT helpers
├── UserInterface/           UIMessageBroker
├── VirtualReality/          VirtualRealityController + SelectableVrType
├── Properties/              Annotations.cs (JetBrains nullability) + AssemblyInfo.cs
├── packages.config / app.config
└── LeapVR.Shell.Controllers.csproj
```

## Notable patterns / gotchas

- **Controllers as message receivers.** Several controllers implement `IRunLevelMsgReceiver` and/or `IExecutionMessageReceiver` (defined in `LeapVR.Shell.Domain.Models.Controllers`). The `StationController` enumerates the `RegisterCollection<IRunLevelMsgReceiver>` set and pumps each `StationMessage` through every member. **A new controller that needs run-level signals must be added to the right collection in `Bootstrapper.RegisterControllers`.**
- **Platform controller fan-out.** `PlatformController` aggregates a *collection* of `IPlatformModule` instances (today: `VBoxPlatformModule` + `SteamPlatformModule`). Adding a new platform (Epic, GOG, …) means adding an `IPlatformModule` implementation in `LeapVR.Shell.Modules` and registering it into the collection in `Bootstrapper.RegisterModules` — no change here.
- **Installation is async + lockable.** `InstallationManager` coordinates concurrent installs; `IPlatformController.GetLockedApplications()` returns the set currently in flight. The `LeapVR.Shell.Setup` uninstaller spin-waits on this list (see [`LeapVR.Shell.Setup`](../LeapVR.Shell.Setup/README.md)).
- **`UIMessageBroker` ≠ `IEventAggregator`.** The Caliburn `IEventAggregator` exists too, but UI-domain events (session started, app installed, connect dialog) flow through `UIMessageBroker`. Treat them as separate channels — Caliburn aggregator is Caliburn-internal; `UIMessageBroker` is the application's pub/sub.
- **COM interop dependencies are file-referenced**, not NuGet'd: see `<Reference Include="Interop.NetFwTypeLib">` pointing to `LeapVR.Shell.3rdParty/bin/`. They're embedded with `<EmbedInteropTypes>True</EmbedInteropTypes>` so the consumer doesn't need a separate DLL.
- **No `LeapVR.Shell.Services` reference.** Controllers do *not* know about gRPC. They consume `IRemoteServiceSet` (local interface in `RemoteService/Interfaces/`), and `Bootstrapper.RegisterRPCServices` binds it to the `RemoteServiceFactory` from `LeapVR.Shell.Services`. Controllers stay protocol-agnostic.

## Consumers

- `LeapVR.Shell` — wires every controller into IoC; view-models inject controller interfaces.
- `LeapVR.Shell.Setup` — uses `IPlatformController`, `IDiskController`, `IFirewallController`, `ISecurityController`, `ISystemController` during install/uninstall. Uses `Dummies.cs` for things it doesn't need (e.g. `IVirtualRealityController`).

## Related docs

- Sister projects: [`LeapVR.Shell.Modules`](../LeapVR.Shell.Modules/README.md) (the subsystems controllers orchestrate), [`LeapVR.Shell.Modules.Interfaces`](../LeapVR.Shell.Modules.Interfaces/README.md), [`LeapVR.Shell.Repository`](../LeapVR.Shell.Repository/README.md), [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md) (provides `IController`, message-receiver markers, `StationMessage`)
- Tier overview: [`docs/client/README.md`](../README.md)
- Architecture: `docs/architecture/session-lifecycle.md` (planned), `docs/architecture/grpc.md` (planned)
