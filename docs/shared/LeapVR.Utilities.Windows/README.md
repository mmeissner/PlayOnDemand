# LeapVR.Utilities.Windows

> Windows automation: process management, Task Scheduler, registry, USB device
> events, file utilities, AES string cipher, JSON helpers.

## Purpose

Sits one layer above `LeapVR.Shared.Lib.Win`. Where the lower lib exposes raw
WinAPI behind small wrappers, this project provides higher-level "do a thing"
utilities the kiosk uses every session: kill a not-responding process, register
a logon Task Scheduler job, read a registry key, listen for USB
plug/unplug events, monitor a tail of a log file, encrypt a string with AES,
serialise an object to JSON.

It is written explicitly for `.NET Framework 4.7.1` on Windows — the
`Microsoft.Win32.TaskScheduler` package and `System.Management` wire it tightly
to the OS.

## Tech

- **Target framework:** `.NET Framework 4.7.1`
- **Platforms:** `AnyCPU`, `x64`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Allows unsafe blocks:** yes
- **Key NuGet packages:**
  - `TaskScheduler` 2.8.1 — `Microsoft.Win32.TaskScheduler` wrapper around the
    Windows Task Scheduler 2.0 API
  - `Newtonsoft.Json` 12.0.2 — used by `JsonHelper`
  - `NLog` 4.5.11 — logging
  - `ini-parser` 2.4.0 — declared in packages.config (used by the Setup wizard
    helper paths historically; also pulled in transitively)
- **System references of interest:** `System.Management`, `System.Windows.Forms`,
  `PresentationCore`, `PresentationFramework`, `WindowsBase`
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib.Win`

## Responsibility

It IS responsible for:

- Process inspection and control beyond what `System.Diagnostics.Process`
  gives you out of the box (`ProcessUtilities.GetParentProcess`,
  `NotRespondingProcess`, `ProcessCpuWatchdog`, `SubsequentProcessTracker`,
  `ProcessExtentions`).
- Registry read/write helpers, hive-prefix-aware (`RegistryUtil`).
- Task Scheduler 2.0 task creation (`TaskSchedulerUtil.CreateUserLogOnTask`,
  the `TaskAction` set).
- USB device-arrival/removal notifications via a hidden message-only window
  (`UsbDeviceMessageOnlyWindow`).
- File and executable utilities (`FileUtil`, `ExecutableUtil`, the
  `FileProcessor` set including `LogFileMonitor`).
- AES symmetric encryption of strings (`StringCipher.Encrypt/Decrypt`).
- App config IO (`ConfigurationUtil`).
- A "clean screen" overlay helper (`CleanScreenHelper`) used during VR
  transitions.
- JSON serialisation helpers (`JsonHelper`).

It is NOT responsible for:

- Steam-specific logic — see `LeapVR.Utilities.Steam`.
- WPF-specific concerns — see `LeapVR.Shared.Lib.Wpf`.
- Cross-platform code — that's the job of `LeapVR.Shared.Lib`.

## Public API surface

| Type | Purpose |
|------|---------|
| `RegistryUtil` (static) | `GetValueData(key, value)`, hive-aware overloads, set/delete helpers. Does **not** support `CurrentUser` / `DynData` / `ClassesRoot` (see XML doc). |
| `TaskScheduler.TaskSchedulerUtil` (static partial) | `CreateUserLogOnTask(action, name)` — sets up a task that runs `action.Execute()` on user logon, `Hidden=false`, `Priority=AboveNormal`, registers as the current user via `WindowsIdentity.GetCurrent().Name`. |
| `TaskScheduler.ITaskInfo`, `TaskInfo`, `TaskAction` | Action descriptors consumed by `TaskSchedulerUtil`. |
| `Processes.ProcessUtilities` | `GetParentProcess()` via `NtQueryInformationProcess`. |
| `Processes.ParentProcessUtilities` | The marshalled struct used above. |
| `Processes.NotRespondingProcess` | Detect whether a window is responding (`SendMessageTimeout`). |
| `Processes.ProcessCpuWatchdog` | Watchdog that samples a process's CPU usage and fires when it stays above a threshold. |
| `Processes.ProcessExtentions` | Extension methods on `Process` (note spelling: `Extentions`, not `Extensions`). |
| `Processes.SubsequentProcessTracker` | Track child / sibling processes spawned after a launch. |
| `UsbDevice.UsbDeviceMessageOnlyWindow` | `NativeWindow` subclass that registers `RegisterDeviceNotification` for `GUID_DEVINTERFACE_USB_DEVICE` and exposes `MessageArrived` events. **Must be constructed on the UI thread.** |
| `FileProcessor.FileProcessor` | `Copy / Move / Delete` with retries and pre-checks. |
| `FileProcessor.LogFileMonitor` + `LogFileMonitorLineEventArgs` | Tail a log file and emit one event per appended line (used for monitoring external launchers). |
| `FileUtil` (static) | Path / size / extension helpers. |
| `ExecutableUtil` (static) + `Executable.Enums` | Identify and inspect `.exe`/`.dll` files. |
| `LogFileMonitorUtil` | Adapter around `LogFileMonitor`. |
| `ConfigurationUtil` | Read / write `app.config`-style values. |
| `JsonHelper.JsonHelper` (static) | `Serialize` / `Deserialize` shortcuts using `Newtonsoft.Json` with shared `JsonSerializerSettings`. |
| `StringCipher` (static) | AES-256 with PBKDF2 (`Rfc2898DeriveBytes`, 1000 iterations). Salt + IV are 32 bytes each, randomly generated and prepended to the ciphertext. |
| `CleanScreenHelper` | Briefly overlays a clean black/blank screen during VR transitions to hide reload flicker. |
| `AutoPlay.IQueryCancelAutoPlay` | COM interface to suppress Windows AutoPlay prompts for inserted media. |

## Internal structure

```
LeapVR.Utilities.Windows/
├── ConfigurationUtil.cs
├── CleanScreenHelper.cs
├── ExecutableUtil.cs / FileUtil.cs / LogFileMonitorUtil.cs
├── RegistryUtil.cs
├── StringCipher.cs
├── AutoPlay/
│   └── IQueryCancelAutoPlay.cs
├── Executable/
│   └── Enums.cs
├── FileProcessor/
│   ├── FileProcessor.cs
│   ├── LogFileMonitor.cs
│   └── LogFileMonitorLineEventArgs.cs
├── JsonHelper/
│   └── JsonHelper.cs
├── Processes/
│   ├── NotRespondingProcess.cs
│   ├── ParentProcessUtilities.cs
│   ├── ProcessCpuWatchdog.cs
│   ├── ProcessExtentions.cs           ← misspelled "Extentions"
│   ├── ProcessUtilities.cs
│   └── SubsequentProcessTracker.cs    ← present in folder, not in csproj
├── TaskScheduler/
│   ├── ITaskInfo.cs
│   ├── TaskAction.cs
│   ├── TaskInfo.cs
│   └── TaskSchedulerUtil.cs
└── UsbDevice/
    └── UsbDeviceMessageOnlyWindow.cs   ← namespace is .WindowsMessages
```

## Notable patterns / gotchas

- **`UsbDeviceMessageOnlyWindow`'s namespace is `LeapVR.Utilities.Windows.WindowsMessages`**,
  not `.UsbDevice` despite the folder name. Importers must match.
- **`RegistryUtil` does not handle `CurrentUser` / `DynData` / `ClassesRoot`** —
  the XML doc warns; it just won't resolve those hives. Use full-path keys
  starting with `HKEY_LOCAL_MACHINE\...`.
- **`TaskSchedulerUtil.CreateUserLogOnTask` registers the task with the
  current user identity**, so calling code must already be running as the user
  that should own the task. If launched elevated/SYSTEM the task ownership will
  be wrong.
- **`StringCipher` salt + IV are prepended to the ciphertext** as
  Base64-stitched parts — both sides must use the same protocol. PBKDF2 is
  fixed at 1000 iterations.
- **`Processes.ProcessExtentions` is misspelled** ("Extentions"). Don't fix
  the typo casually — it's a public namespace symbol.
- **`SubsequentProcessTracker.cs` exists on disk but is not listed in the
  csproj** — it's currently un-compiled. Add a `<Compile Include=...>` if you
  want to use it.
- **`NtQueryInformationProcess` is not officially supported API** — `ParentProcessUtilities`
  works on every Windows version we ship to but Microsoft reserves the right
  to break it.
- **`AllowUnsafeBlocks` is on** for direct memory access in the process /
  USB interop helpers.

## Consumers

- `LeapVR.Utilities.Steam` (registry + WMI lookup)
- `LeapVR.Shell` and most `LeapVR.Shell.*` (Modules, Managers, Controllers)
- `LeapVR.Shell.Setup` (registry + Task Scheduler in the wizard)
- `LeapVR.Content.Util`, `LeapVR.Content.Shared`

Not consumed by any `Pod.*` server project.

## Related docs

- [shared tier overview](../README.md)
- [`docs/client/README.md`](../../client/README.md) for how the Managers tier
  wires these utilities into the kiosk lifecycle.
