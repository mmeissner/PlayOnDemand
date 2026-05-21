# LeapVR.Shell.Managers

> OS-adjacent state managers: the local-machine info provider (`ILocalMachine` — VBox fingerprint, CPU/GPU/RAM details, software version), the USB storage devices manager (USB-device arrival/removal events + drive enumeration + safe folder access), and the lightweight `WindowData` window-position helper.

## Purpose

This project hosts the small handful of managers that don't fit cleanly into "controller" (orchestration) or "module" (subsystem) but still need lifetime-managed instances. They are typically:

- **Computed at startup, cached for the process lifetime** — e.g. `LocalMachine.VBoxFingerprint` is hashed once from CPU + disk WMI data via `Lazy<T>`.
- **Event-driven OS hooks** — e.g. `UsbStorageManager` opens a hidden message-only window via `LeapVR.Utilities.Windows.WindowsMessages.UsbDeviceMessageOnlyWindow` and listens for `WM_DEVICECHANGE` to keep its `ObservableCollection<IUsbStorage>` current.

They don't broadcast on `IUIMessageBroker`; they are pulled by controllers and view models on demand.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Managers.dll`)
- **Key NuGet packages:**
  - `NLog` 4.5.11 — logging
  - `Microsoft.VisualBasic` (BCL ref) — `ComputerInfo` for total physical memory (used in `LocalMachine`)
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Win`
  - `LeapVR.Shell.Domain.Models`
  - `LeapVR.Shell.Modules.Interfaces`
  - `LeapVR.Utilities.Windows`

## Responsibility

**It IS responsible for:**
- Building and exposing the per-station hardware fingerprint (`VBoxFingerprint`) used by the server to identify the machine.
- Caching CPU / GPU / RAM strings shown in the admin UI.
- Tracking USB storage drives (insertion / removal / current contents) and serving a filtered, observable list to the dashboard's "USB Sticks Bar".
- Providing the `WindowData` snapshot helper used by some view-position logic.

**It is NOT responsible for:**
- Persistence (no DB).
- gRPC.
- VR / input / multimedia (those are modules).

## Public API surface

### LocalMachine (`LocalMachine/`)

| Type | Purpose |
|---|---|
| `LocalMachine` | Implements `ILocalMachine` (interface in `LeapVR.Shell.Domain.Models.Station`). Lazy-loads a `LocalMachineData` blob on first property access. Exposes `SoftwareVersion`, `VBoxFingerprint`, `CpuDetails`, `VgaDetails`, `RamDetails`. |
| `LocalMachineData` (private nested) | Reads CPU/GPU/RAM via `Wmi.Query` (from `LeapVR.Shared.Lib.Win`), reads disks, builds an MD5/SHA fingerprint string. |
| `Enums.cs` | Local enums (e.g. machine-state markers). |
| `ILocalMachineManager` (in `Interfaces/`) | Higher-level local-machine query surface. |

### UsbStorage (`UsbStorage/`)

| Type | Purpose |
|---|---|
| `UsbStorageManager` | Implements `IUsbDevicesManager`. Owns an `ObservableCollection<IUsbStorage> UsbDrives`. Hooks `WM_DEVICECHANGE` (`DBT_DEVICEARRIVAL` 0x8000 / `DBT_DEVICEREMOVECOMPLETE` 0x8004). Must be constructed on the UI thread. |
| `UsbStorage` | Implements `IUsbStorage`. Wraps a single `DriveInfo` and exposes `IUsbStorageAccess`. |
| `UsbStorageAccess` | Implements `IUsbStorageAccess`. Walks the drive for content. |
| `Folder` | Implements `IFolder`. Lazy directory walker with the file-search filter applied. |
| `FileSearchFiltersAttribute` | Attribute that scopes which file extensions a folder enumeration returns. Used to surface only `.vbox` packages on installation USB sticks. |
| `Interfaces/IAppInstallationFile.cs` | Marker for installation-payload files. |
| `Interfaces/IFile.cs` / `IFolder.cs` / `IUsbStorage.cs` / `IUsbStorageAccess.cs` / `IUsbDevicesManager.cs` | Public contracts. |

### Window (`Window/`)

| Type | Purpose |
|---|---|
| `WindowData` | Plain DTO carrying window-position info. Used by view-positioning helpers. |

## Internal structure

```
LeapVR.Shell.Managers/
├── LocalMachine/
│   ├── Enums.cs
│   ├── LocalMachine.cs                 ILocalMachine implementation
│   └── Interfaces/
│       └── ILocalMachineManager.cs
├── UsbStorage/
│   ├── FileSearchFiltersAttribute.cs   Attribute-based file-extension filter
│   ├── Folder.cs                        IFolder implementation
│   ├── UsbStorage.cs                    IUsbStorage implementation
│   ├── UsbStorageAccess.cs              IUsbStorageAccess implementation
│   ├── UsbStorageManager.cs             IUsbDevicesManager implementation (WM_DEVICECHANGE)
│   └── Interfaces/                      Public contracts (IFile/IFolder/IUsbStorage/…)
├── Window/
│   └── WindowData.cs
├── Properties/AssemblyInfo.cs
├── packages.config / app.config
└── LeapVR.Shell.Managers.csproj
```

## Notable patterns / gotchas

- **`LocalMachine.VBoxFingerprint` is hashed from CPU `Name` + `ProcessorId` + GPU + total RAM + disk model/serial/size.** Anything that changes those fields shifts the fingerprint. WMI failures (e.g. headless VM with missing video controller) cause early-startup pain; `LocalMachineData.InitializeData` is the place to debug.
- **`UsbStorageManager` constructor must be called from the UI thread.** It creates a hidden message-only window to receive `WM_DEVICECHANGE`. The constructor comment is explicit. SimpleInjector resolves it during `Bootstrapper.RegisterModules` while the WPF UI thread is current — don't move the registration off the dispatcher.
- **`ObservableCollection<IUsbStorage> UsbDrives`** is the public hook the dashboard's `UsbSticksBarViewModel` data-binds to.
- **`IsDisposed` uses `Interlocked` semantics** (`int _isDisposed; // 0 = false, 1 = true`) to guarantee idempotent dispose under arrival-event races.
- **`ILocalMachine` interface lives in `LeapVR.Shell.Domain.Models.Station`** — not here. Everything depends on `Domain.Models`; that's where `ILocalMachine` is consumable from any tier.
- **`IUsbDevicesManager` interface lives in this project** under `UsbStorage/Interfaces/`. It is consumed by `LeapVR.Shell` view-models directly; controllers don't typically touch it.

## Consumers

- `LeapVR.Shell` — registers `ILocalMachine` (`Singleton<ILocalMachine, LocalMachine>`) and `IUsbDevicesManager` (`Singleton<IUsbDevicesManager, UsbStorageManager>`). View models (`UsbSticksBarViewModel`, `StationDetailsViewModel`) inject these.
- `LeapVR.Shell.Controllers` — `StationController` reads `ILocalMachine` for hardware fingerprint to send to the server.
- `LeapVR.Shell.Modules` — `LeapVR.Shell.Categories` and `LeapVR.Shell.Modules` depend on this for window data and machine info indirectly via `LeapVR.Shell.Domain.Models`.
- `LeapVR.Shell.Services` — `RemoteServiceFactory` accepts `ILocalMachine` to embed station identity in request metadata.

## Related docs

- Sister projects: [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md) (defines `ILocalMachine`), [`LeapVR.Shell.Controllers`](../LeapVR.Shell.Controllers/README.md) (`StationController` consumer)
- Shared lib: `LeapVR.Shared.Lib.Win` (provides `Wmi.Query`, `Taskbar`, `User32`, the UsbDeviceMessageOnlyWindow)
- Tier overview: [`docs/client/README.md`](../README.md)
