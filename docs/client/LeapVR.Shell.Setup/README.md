# LeapVR.Shell.Setup

> First-run configuration wizard and uninstaller, both hosted inside the `LeapPlay.Shell.exe` binary. Activated by `LeapPlay.Shell.exe -config` / `-uninstall` or implicitly when no valid `DiskConfig` exists.

## Purpose

`LeapVR.Shell.Setup` is a Caliburn.Micro WPF "mini-application" that ships **inside the same executable** as the kiosk. It is not a separate installer — Inno Setup handles binary placement; this project handles *configuration*: where to store games, which credentials the station authenticates to the server with, which language to use, which Windows tweaks to apply (auto-start task, Windows Error Reporting suppression, Windows Defender exclusions, firewall rules, etc.).

The same project also contains the *uninstaller* logic — invoked as `LeapPlay.Shell.exe -uninstall` from the Inno Setup `[UninstallRun]` section. The uninstaller reverses the wizard's changes (removes scheduled task, re-enables WER, deletes installed games, drops the Defender exclusion, removes firewall rules) before Inno Setup deletes the binaries.

It is registered as `OutputType=Library` (it produces a DLL referenced by `LeapVR.Shell`) and contains its own `App` (`Setup.App`) instantiated in `Program.Main` when the right CLI flag is present. Setup uses its own `Bootstrapper`/IoC `Container` separate from the kiosk's.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Setup.dll`), namespace `LeapVR.Shell.Setup`
- **Key NuGet packages:**
  - `Caliburn.Micro` 3.2.0 — separate `BootstrapperBase` for the wizard
  - `SimpleInjector` 4.2.2 — wizard IoC (different container instance from kiosk)
  - `MaterialDesignThemes` 2.5.1 / `MaterialDesignColors` 1.1.3 — wizard styling
  - `NLog` 4.5.11 — logging
  - `Microsoft.PowerShell.3.ReferenceAssemblies` 1.0.0 — used by `SetupHelper` to invoke `Get-MpPreference` / `Set-MpPreference` for Defender exclusions
  - `Windows7APICodePack-Core/-Shell` 1.1.0 — shell helpers
  - `WPFLocalizeExtension` 3.1.2 — localised wizard text
- **Project references (in this repo):**
  - `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Repository.Interfaces`
  - `LeapVR.Shell.Controllers`, `LeapVR.Shell.Modules`, `LeapVR.Shell.Repository`
  - `LeapVR.Shell.Categories`, `LeapVR.Shell.Language`
  - `LeapVR.Shared.Lib.Wpf`, `LeapVR.Utilities.Windows`

## Responsibility

**It IS responsible for:**
- Showing the wizard UI when no usable `DiskConfig` exists or when `-config` is on the command line.
- Persisting `DiskConfig` (game-storage location), `LoginConfig` (station credentials), `SystemConfig` (language, supported cultures, scheduled task name).
- Enabling/disabling the LeapVR Windows Scheduled Task (`SetupHelper.ChangeAutoStart`).
- Toggling Windows Error Reporting (`SetupHelper.SetWer`).
- Adding/removing the storage directory to/from Windows Defender exclusions via PowerShell (`SetupHelper.GetExcludedFoldersFromWinDefender` etc.).
- Driving the uninstall sequence (`Uninstaller.StartUninstall`) when `-uninstall` is passed.

**It is NOT responsible for:**
- Installing files or registry entries on the machine — that is Inno Setup's job (`LeapVR.Shell.Installer/`).
- Speaking gRPC to register the station — that flow lives in the kiosk shell (`StationController` / `RemoteServiceController`). The wizard only stores credentials locally.
- Network-level changes — firewall rules go through `IFirewallController` from `LeapVR.Shell.Controllers`.

## Public API surface

| Type | Purpose |
|---|---|
| `App : Application` (`App.cs` + `App.xaml.cs`) | WPF `Application` for setup mode. Constructor takes `SetupType` (`Config` or `Uninstall`). Instantiated by `LeapVR.Shell/Program.cs`. |
| `SetupType` enum | `Config`, `Uninstall`. |
| `Bootstrapper : BootstrapperBase` | Separate Caliburn bootstrapper. Two registration paths: `SetupContainerForConfig` and `SetupContainerForUninstall` (the uninstall path registers `Dummies.cs` stand-ins for heavy services it doesn't need). |
| `SetupHelper` | All the OS-level mutations: PowerShell-driven Defender exclusion add/remove, WER on/off, scheduled-task lookup/create/delete via `LeapVR.Utilities.Windows.TaskScheduler`. |
| `Uninstaller` | Orchestrates uninstall steps in order, returning an exit code. Driven by `UninstallOptions`. |
| `UninstallOptions` | Boolean flags: `RemoveStartupTask`, `EnableWer`, `DeleteGames`, `RemoveWindowsDefenderExclusion`, … |
| View models in `UI/ViewModels/`: `ConfigViewModel`, `SettingsWizViewModel`, `RegisterAccountViewModel`, `ApplyChangesViewModel`, plus dialogs (`WarnStorageNotEmptyViewModel`, `WarnStorageChangeViewModel`) and `WorkTask/WorkTaskViewModel`. |
| `Dummies.cs` | `CategoryProviderDummy`, `AppInfoProcessorDummy`, `VirtualRealityControllerDummy`, `LocalMachineDummy` — minimal stubs registered in uninstall mode so that the controllers/repositories still resolve. |

## Internal structure

```
LeapVR.Shell.Setup/
├── App.xaml / App.xaml.cs / App.cs    Application + SetupType enum
├── Bootstrapper.cs                    Wires Config or Uninstall flavour into a SimpleInjector container
├── SetupHelper.cs                     PowerShell, Registry, TaskScheduler glue
├── Uninstaller.cs                     Sequenced uninstall steps
├── UninstallOptions.cs                Step toggles
├── Dummies.cs                         Stub implementations for heavy interfaces during uninstall
├── packages.config                    NuGet manifest
├── app.config
└── UI/
    ├── IWizardPage.cs                 Wizard page contract (Title, Next/Back gating)
    ├── ViewModels/
    │   ├── ConfigViewModel.cs                       Top-level wizard conductor
    │   ├── SettingsWizViewModel.cs                  Storage path / behaviour wizard page
    │   ├── RegisterAccountViewModel.cs              Station credential entry page
    │   ├── ApplyChangesViewModel.cs                 "Click Apply" page that runs the work tasks
    │   ├── Dialog/
    │   │   ├── WarnStorageNotEmptyViewModel.cs      Confirm wiping a non-empty target
    │   │   └── WarnStorageChangeViewModel.cs        Confirm changing storage on existing install
    │   └── WorkTask/WorkTaskViewModel.cs            Single async step + progress UI
    └── Views/
        ├── ConfigView.xaml                          Wizard window chrome
        ├── SettingsWizView.xaml
        ├── RegisterAccountView.xaml(.cs)
        ├── ApplyChangesView.xaml(.cs)
        ├── Dialog/
        │   ├── WarnStorageNotEmptyView.xaml
        │   └── WarnStorageChangeView.xaml
        └── WorkTask/WorkTaskView.xaml(.cs)
```

## Notable patterns / gotchas

- **Two IoC sets, one project.** `SetupContainerForConfig` registers the wizard view-models and just enough services to write configs. `SetupContainerForUninstall` registers controllers + repositories + `Dummies` so it can iterate installed apps and uninstall them via `IPlatformController.Uninstall`. Don't add wizard-only deps to the uninstall path or you waste startup time on a process that's about to be deleted.
- **The wizard window is `Topmost=true`** with `SizeToContent=WidthAndHeight` — designed to stay on top of any leftover SteamVR or game window.
- **Delete-games loop is synchronous-spinning.** `Uninstaller.StartUninstall` calls `_platformController.Uninstall` for each installed app, then `Thread.Sleep(500)` while `GetLockedApplications().Any()` returns non-empty. Uninstall can take minutes on stations with many games.
- **PowerShell must be installed.** `SetupHelper.IsPowerShellInstalled` reads `HKLM\SOFTWARE\Microsoft\PowerShell\3\Install`. If absent, Defender-exclusion calls quietly no-op (returning `false`) — they do **not** fall back to alternative APIs. Stations on stripped Windows images may need PowerShell installed first.
- **`SetupApp` is captured in a static constructor** (`static Bootstrapper() { SetupApp = (App)Application.Current; }`). This is fragile: if `Application.Current` is not set when the JIT first touches `Bootstrapper`, the cast throws. In practice it works because `Program.Main` constructs the `App`, hooks events, and only then `application.Run()` causes XAML to instantiate the bootstrapper.
- **Exit code is the uninstaller's success signal.** `Application.Shutdown(retval)` propagates the exit code back to `Program.Main`'s `finally`, which Inno Setup then reads.
- **No `IEventAggregator` sharing with the kiosk** — the wizard's `EventAggregator` is registered fresh in this container. UI events from setup never reach the (not-yet-running) kiosk.

## Consumers

- `LeapVR.Shell` (`Program.Main`) instantiates `Setup.App(SetupType.Config | SetupType.Uninstall)` and `application.Run()`s it.
- `LeapVR.Shell.Installer` (Inno Setup) calls `LeapPlay.Shell.exe -uninstall` from the `[UninstallRun]` section before deleting binaries.

## Related docs

- Sister projects: [`LeapVR.Shell`](../LeapVR.Shell/README.md) (entry that decides which mode to launch), [`LeapVR.Shell.Controllers`](../LeapVR.Shell.Controllers/README.md) (uninstaller drives `IPlatformController` + `IDiskController` + `IFirewallController`)
- Tier overview: [`docs/client/README.md`](../README.md)
- Build/install pipeline: `docs/architecture/build-and-deploy.md` (planned)
