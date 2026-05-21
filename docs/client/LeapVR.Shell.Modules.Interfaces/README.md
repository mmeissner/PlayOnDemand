# LeapVR.Shell.Modules.Interfaces

> Contracts for every kiosk subsystem (VR, XInput, platforms, multimedia, network, container, hardware, repositories used *by modules*). Pure interfaces — no behaviour. The implementations live one tier up, in `LeapVR.Shell.Modules`.

## Purpose

This project defines the *shape* of every module the kiosk uses. Controllers, view models, the setup wizard, and even other modules talk to subsystems through these interfaces — never against the implementation classes. This is the single most-referenced contract project in the client tier (only `LeapVR.Shell.Domain.Models` is referenced more often).

It also re-exposes a small set of *repository* interfaces needed by modules (specifically `IConfigFileRepository<T>`, `IHardwareDeviceTemplateRepository`, `IOpenVrSettingsSetRepository`). These intentionally live here, not in `LeapVR.Shell.Repository.Interfaces`, because they are *module-internal* persistence rather than shared LiteDB entities. (A module owns its config; the kiosk-wide repositories live in the dedicated `Repository.Interfaces` project.)

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Modules.Interfaces.dll`)
- **Key NuGet packages:** none — pure contracts.
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`
  - `LeapVR.Shell.Domain.Models`

That short reference list is intentional: this project must compile against minimal surface so it can be referenced from every other client project without creating cycles.

## Responsibility

**It IS responsible for:**
- Declaring `IBaseModule` (the marker every module satisfies) and the per-domain module contracts derived from it.
- Declaring DTOs / event types / state types that flow across the module-consumer boundary (e.g. `XInputButtonArgs`, `IControllerButtonActionEvent`, `DeviceData`).
- Declaring exceptions thrown at module boundaries (`Exceptions.cs`).

**It is NOT responsible for:**
- Implementations (in `LeapVR.Shell.Modules`).
- Domain entities (in `LeapVR.Shell.Domain.Models`).
- LiteDB-backed entity repos (in `LeapVR.Shell.Repository.Interfaces`).

## Public API surface

`IBaseModule` is the root marker:

```csharp
public interface IBaseModule
{
    Guid ModuleId { get; }
    string ModuleName { get; }
}
```

Every other module interface inherits from it (directly or transitively). Module instances are identified by `ModuleId` so that platform configs, statistics, and remote-service state can correlate across runs.

By folder:

| Folder | Notable contracts |
|---|---|
| `Execution/` | `IApplicationExecution`, `IProcessExecution`, `IProcessExecutionManager`, `IOptionalBehavior`, `ProcessIdentifier` — the abstraction over "launch and supervise a process". |
| `Hardware/` | `IHardwareDevice`, `IHardwareDeviceModule`, `IHardwareDeviceUtil`, `DeviceData`. |
| `Multimedia/` | `IMultimediaModule`, `IMultimediaProvider`, `IMultimediaPlaylist`, `IPlaylistModule`. |
| `Network/` | `INetworkModule`. |
| `Platform/` | `IPlatformModule` (the largest contract — install/uninstall/launch/account flows), `IPlatformStateProvider`, plus `Steam/` for Steam-specific shapes. |
| `Repositories/` | `IConfigFileRepository<T>` (open generic, JSON-on-disk), `IHardwareDeviceTemplateRepository`, `IOpenVrSettingsSetRepository`. These are module-private persistence surfaces. |
| `Utilities/WinApi/` | Tiny WinAPI shape hooks shared between projects. |
| `Vr/` | `IVrModule`, `IOpenVrModule`, `IVrDesktopModule`, `IHmdActivityWatchdog`, `IOpenVrEvent`, `IControllerButtonActionEvent`, `IHmdActivityEvent`, `IOpenVrSettingsSet`, `IOpenVrSettingFile`, `IOpenVrSettingEntityDetails`, `ITransparencyArea`, plus `Enums.cs`. |
| `XInput/` | `IXInputModule`, `InputState`, `XInputButtonArgs`, `XInputCompositeArgs`, `XInputButtonState`, `XInputButtons`. |

Top-level files:

- `IBaseModule.cs` — root marker.
- `Exceptions.cs` — module-level exception types.

## Internal structure

```
LeapVR.Shell.Modules.Interfaces/
├── IBaseModule.cs                Root marker for every module
├── Exceptions.cs                 Module-tier exceptions
├── Execution/                    Process-launch + supervision contracts
├── Hardware/                     Hardware-device contracts + DTO
├── Multimedia/                   Media + playlist contracts
├── Network/                      INetworkModule
├── Platform/                     IPlatformModule + IPlatformStateProvider + Steam/
├── Repositories/                 Module-private persistence (config files, OpenVR settings, device templates)
├── Utilities/WinApi/             Cross-cut WinAPI shapes
├── Vr/                           VR-subsystem surface (IVrModule + many event/state types)
├── XInput/                       XInput-subsystem surface
├── Properties/AssemblyInfo.cs
└── LeapVR.Shell.Modules.Interfaces.csproj
```

## Notable patterns / gotchas

- **Module-private repositories deliberately live here, not in `Repository.Interfaces`.** The split is by ownership, not by storage technology: `IConfigFileRepository<T>` is JSON-on-disk and only consumed by module-tier or controller-tier code; `IAppDisplayRepository` etc. live in `LeapVR.Shell.Repository.Interfaces` because they're shared LiteDB entities.
- **`IPlatformModule` is the largest interface** — it covers app discovery, install, uninstall, launch, account management and supported-feature reporting. Read the full interface before adding a third platform implementation; the surface is wide.
- **Keep this project zero-dependency-noise.** No NuGets, no third-party. If you find yourself wanting to add a package here, consider whether the type belongs in an implementation project instead.
- **Several VR types have `Dto` siblings** (e.g. `IOpenVrSettingsSet` ↔ `OpenVrSettingsSetDto` in `LeapVR.Shell.Modules`). The DTOs are concrete; the interfaces are the public surface.
- **No `IContainerModule` here.** That contract was kept inside the `LeapVR.Shell.Modules/Container/` namespace itself (see `IContainerModule.cs` in the implementation project). It's an internal-ish surface — only `LeapVR.Shell.Modules` consumes it via `Bootstrapper`.

## Consumers

Effectively every implementation project plus the composition root:

- `LeapVR.Shell.Modules` — implements every contract here.
- `LeapVR.Shell.Controllers` — depends on the contracts to call into modules without referencing the implementation project.
- `LeapVR.Shell.Repository.Interfaces` — references this for shared types (e.g. `IMultimediaPlaylistData`).
- `LeapVR.Shell.Services` and `.Services.Interfaces` — references for execution / platform shapes referenced by remote DTOs.
- `LeapVR.Shell.Setup` — uses `IPlatformModule`-collection for the uninstall path.
- `LeapVR.Shell.Managers` — references for `ILocalMachine`-related types.
- `LeapVR.Shell` — registers every implementation against these interfaces in `Bootstrapper.RegisterModules`.

## Related docs

- Sister implementation: [`LeapVR.Shell.Modules`](../LeapVR.Shell.Modules/README.md)
- Closely related: [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md), [`LeapVR.Shell.Repository.Interfaces`](../LeapVR.Shell.Repository.Interfaces/README.md)
- Tier overview: [`docs/client/README.md`](../README.md)
