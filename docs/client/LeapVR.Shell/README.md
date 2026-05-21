# LeapVR.Shell

> The kiosk WPF entry point — `LeapPlay.Shell.exe`. Owns the IoC composition root, the splash screen, the Caliburn.Micro `ShellView`, and every top-level view (login, dashboard, administration, blocker, connect dialog).

## Purpose

This project produces the executable that runs on every VR station. It is the *composition root*: it is the only project allowed to reference all sibling implementation projects (`LeapVR.Shell.Controllers`, `.Modules`, `.Repository`, `.Services`, `.Managers`, `.Categories`, `.Language`, `.Setup`) and to wire them into interface dependencies. Every other project in the client tier consumes only `.Interfaces` projects.

Beyond IoC bootstrapping, the project owns the *visual shell*: a `ShellView` (full-screen window, no chrome, taskbar hidden) hosting one of several conducted views. Caliburn.Micro convention auto-resolves view ↔ viewmodel by namespace match (`UI/Shell/Login/ViewModels/LoginViewModel.cs` ↔ `UI/Shell/Login/Views/LoginView.xaml`).

The same binary additionally serves three secondary roles via command-line arguments handled in `Program.Main`: setup wizard (`-config`), uninstaller (`-uninstall`), debug-windowed mode (`-d` / `-debug`). The first-run wizard fires automatically when no valid `DiskConfig` is present, even without `-config`.

## Tech

- **Target framework:** .NET Framework 4.7.1 (`<TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>`), x64
- **Output:** `WinExe`, assembly name `LeapPlay.Shell` (the on-disk binary is `LeapPlay.Shell.exe`, *not* `LeapVR.Shell.exe`)
- **Key NuGet packages:**
  - `Caliburn.Micro` 3.2.0 — MVVM, view auto-binding, event aggregator, `BootstrapperBase`
  - `SimpleInjector` 4.2.2 — IoC container; `Container.Verify()` runs at startup
  - `NLog` 4.5.11 — file logging (config copied per build configuration)
  - `MaterialDesignThemes` 2.5.1 + `MaterialDesignColors` 1.1.3 — styling
  - `FontAwesome.WPF` 4.7.0.9 — icon font
  - `LiveCharts` 0.9.6 + `LiveCharts.Wpf` — statistics charts in admin tab
  - `Newtonsoft.Json` 12.0.2 — config serialisation
  - `Humanizer.Core` + `Humanizer.Core.zh-CN` — text humanisation, Chinese locale
  - `WPFLocalizeExtension` 3.1.2 — runtime culture switching
  - `System.Reactive` 4.1.5 — observable patterns inside view models
  - `Windows7APICodePack-Core/-Shell` 1.1.0 — taskbar / shell helpers
- **Project references (in this repo):**
  - Contracts: `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Repository.Interfaces`, `LeapVR.Content.Shared`
  - Implementations: `LeapVR.Shell.Controllers`, `LeapVR.Shell.Modules`, `LeapVR.Shell.Repository`, `LeapVR.Shell.Services`, `LeapVR.Shell.Managers`, `LeapVR.Shell.Categories`, `LeapVR.Shell.Language`, `LeapVR.Shell.Setup`
  - Shared libs: `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Win`, `LeapVR.Shared.Lib.Wpf`, `LeapVR.Utilities.Steam`, `LeapVR.Utilities.Windows`, `Pod.Data.Infrastructure`, `Pod.Enums`
  - Vendored: `Unosquare.FFME.Common/.Windows`, `FFmpeg.AutoGen`, `QRCoder.NET40`

## Responsibility

**It IS responsible for:**
- Process lifecycle: single-instance guard, exit codes, restart-via-`Process.Start`, dispatcher exception handler.
- IoC composition: registering every concrete implementation and view model.
- Routing the executable to the right entry mode (kiosk shell / setup / uninstall).
- Hosting the `ShellView` window and the conducted top-level view models.
- The visual theme dictionaries, image dictionaries, splash screens, sound effects, fonts.
- Copying NLog and the Unity `vrlounge_desktop` payload via build events.

**It is NOT responsible for:**
- Business logic — that lives in `LeapVR.Shell.Controllers`.
- Hardware abstraction — that lives in `LeapVR.Shell.Modules`.
- Persistence — that lives in `LeapVR.Shell.Repository`.
- gRPC — that lives in `LeapVR.Shell.Services`.
- Domain types — those live in `LeapVR.Shell.Domain.Models`.

## Public API surface

This project produces an `.exe`; it has no library API. The interesting types are entry points and the visible view-model surface:

| Type | Purpose |
|---|---|
| `Program` (`Program.cs`) | Static entry. Parses CLI flags, owns `SingleInstanceGuard`, dispatches to setup or shell `App`. |
| `App : Application` (`App.xaml.cs`) | Trivial WPF `Application`. The `App.xaml` resource dictionary instantiates the `Bootstrapper` which is what actually does the work. |
| `Bootstrapper : BootstrapperBase, IApplicationHost` (`Bootstrapper.cs`) | Caliburn.Micro bootstrapper. Owns the SimpleInjector `Container`. Methods: `Configure`, `OnStartup`, `OnExit`, plus `Register*` helpers. Implements `ShowGUI`, `Restart`, `Shutdown`, `RequestPoweroff` from `IApplicationHost`. |
| `NLogLogger` (`NLogLogger.cs`) | Adapter so Caliburn.Micro's `LogManager.GetLog` flows into NLog (only attached if the `CaliBurnLog` app-setting is true — Caliburn's per-call logging is otherwise too noisy). |
| `ShellView` / `ShellViewModel` (`UI/Shell/`) | Top-level conductor `(Conductor<IScreen>.Collection.OneActive)`. Listens to UI events from `IUIMessageBroker` and switches between `LoginViewModel`, `DashboardViewModel`, `AdministrationViewModel`. |
| `ViewModelFactory` (`UI/ViewModelFactory.cs`) | Convenience wrapper to build VMs that need runtime-only state (e.g. KeypadViewModel). |

## Internal structure

```
LeapVR.Shell/
├── App.xaml                    Application resource dictionary; instantiates Bootstrapper
├── App.xaml.cs                 Trivial Application subclass
├── Program.cs                  STA Main; flag parsing; SingleInstanceGuard; chooses Setup or normal App
├── Bootstrapper.cs             SimpleInjector wiring + Caliburn lifecycle (OnStartup/OnExit/Configure)
├── NLogLogger.cs               Caliburn.Micro → NLog adapter (opt-in)
├── Debug_NLog.config / Release_Nlog.config   Renamed at build to LeapPlay.Shell.exe.nlog
├── NLog.xsd                    Schema reference
├── app.config                  Standard .NET appSettings (incl. `CaliBurnLog`)
├── app.manifest                Win32 manifest (DPI awareness, requestedExecutionLevel)
├── favicon.ico                 Window icon
├── _OBFUSCATE_DEBUG.bat        ConfuserEx invocations (used by full Build.bat, not Build_Free.bat)
├── _OBFUSCATE_RELEASE.bat
├── FileConfig/
│   ├── UiConfig.cs                    Persisted UI tweakables (themes, image paths)
│   ├── ColorConfig.cs / FontConfig.cs / ImagesConfig.cs / PathConfig.cs
│   ├── GuardViewsThemingConfig.cs / SessionViewsThemingConfig.cs
│   └── Models/
├── Resources/
│   ├── Themes/Default/Package.xaml    Merged dictionary entry
│   ├── Images/                        Splashscreens, icons, thumbnails
│   ├── Styles/                        UserControl style packs
│   └── ButtonClick.wav
├── Properties/
│   └── AssemblyInfo.cs                Title `LeapPlay.Shell`; version `2019.6.*`
└── UI/
    ├── ViewModelFactory.cs
    ├── Base/                          Base view-models (Conductor specialisations, InputControllerConductor)
    ├── Core/                          PlatformProvider, PasswordBoxHelper convention, ViewInputHandler
    ├── Interfaces/                    IShell, IBlockShellViewModel, IViewInputHandler, ITabItemSystemScreen, …
    ├── Usercontrols/                  Reusable XAML user controls
    ├── Universal/                     Cross-screen views (StationDetails, Platform pickers, common widgets)
    └── Shell/
        ├── Views/ShellView.xaml(.cs)        Root window; mouse-down workaround for drag exception
        ├── ViewModels/ShellViewModel.cs     Root conductor
        ├── Login/                            LoginView + LoginViewModel
        ├── Dashboard/                        DashboardView + DashboardViewModel (game grid)
        ├── Blocker/                          Modal blocker shown during long ops
        ├── Connect/                          Reconnect / offline dialog
        └── SystemAdministration/
            ├── ViewModels/AdministrationViewModel.cs   Admin shell (top-level admin tab host)
            ├── Views/                                  Admin XAML
            ├── Accounts/   Applications/   Exit/      PowerOff/   Security/
            ├── Hardware/   Multimedia/     Settings/  Statistics/  Updates/
```

The view auto-binding works because Caliburn.Micro is configured with the default convention manager — folder layout effectively *is* the routing table. A new top-level admin tab is added simply by creating `ITabItemSystemScreen`-marked types under `SystemAdministration/`; `Bootstrapper.RegisterViewModels` discovers them by reflection.

## Notable patterns / gotchas

- **Two startup paths.** `App.xaml`'s resource dictionary instantiates `Bootstrapper` as a `<local:Bootstrapper x:Key="Bootstrapper" />`. That is what triggers Caliburn.Micro's `Initialize` chain — *not* the normal WPF `App.Startup` event. If you delete that line, the app boots a window but no IoC.
- **`-config` is sticky if there's no DiskConfig.** `Program.Main` builds a `ShellConfigurator`; `!HasValidDiskConfig` forces `Setup.App(SetupType.Config)` regardless of arguments. This is how first-run after install transparently shows the wizard.
- **`Bootstrapper.GetInstance(Type, string key)` falls back to assembly scan** when `service` is null but `key` is supplied — this is how Caliburn.Micro resolves a view by string name when no IoC mapping exists.
- **`Container.Verify()` is destructive at startup.** Any unresolved dependency or registration error is fatal-logged and rethrown. Add new registrations *before* this call.
- **Two `RegisterCollection`s carry cross-controller messages**: `IRunLevelMsgReceiver` (Vr, Gamepad, Platform, RemoteService) and `IExecutionMessageReceiver` (Statistics, Vr). The `StationController` enumerates these to broadcast state-machine events. If you add a controller that needs those signals, add it to the right collection.
- **Splash screen randomisation.** `new Random().Next(1, 5)` picks one of `Resources/Images/Splashscreens/splashscreen_{1..4}.png`. Suppressed in debug and uninstall modes.
- **Single-instance guarding** is via the named `SingleInstanceGuard` from `LeapVR.Shared.Lib.Win.Classes`, key `"LeapVR.Shell"`. Releasing it is in `finally` after `application.Run()`.
- **Restart mechanism.** `Bootstrapper.Restart` calls `Application.Current.Shutdown((int)TerminationSignal.Restart)`. After `Run()` returns, `Program.Main`'s `finally` block reads `Environment.ExitCode`, switches on `TerminationSignal.Restart`, and `Process.Start`s the same exe with the same args.
- **`ShellView` swallows mouse-down drag exceptions** intentionally (commented "Related to UC-52"). Do not remove that try/catch without re-investigating the original WPF drag crash.
- **`PasswordBoxHelper` convention** is registered in `Bootstrapper.RegisterViewModels` so Caliburn.Micro can two-way bind `PasswordBox.Password` (which is normally not a DependencyProperty).

### Post-build event (build/deploy gotcha)

The `<PostBuildEvent>` in `LeapVR.Shell.csproj` does two important things:

```bat
xcopy "$(SolutionDir)LeapVR.Shell.3rdParty\bin\vr_desktop\*" "$(TargetDir)vr_desktop\" /i /s /e /h
```

This copies the Unity-built `vrlounge_desktop.exe` (~141 MB) and its assets into `bin/.../vr_desktop/`. **`$(SolutionDir)` must be defined** — building a single project from MSBuild without a solution context will fail this step. Use `Build_Free.bat` or build via `PoD.sln`. (See repo overview for the documented workaround.)

The same script then copies either `Debug_NLog.config` or `Release_Nlog.config` to `$(TargetName).exe.nlog` depending on `$(ConfigurationName)`. Configurations recognised: `Debug`, `Offline_Debug`, `Home_Debug`, `Release_ShellClient`. Anything else falls through and the binary will start without an NLog config.

A sister `<PreBuildEvent>` kills any running `vrlounge_desktop.exe`, blasts the existing `vr_desktop\` copy, and removes a stale `NLog.config` so the post-build copy wins.

## Consumers

None inside the repo — this is the executable. The Inno Setup installer at `LeapVR.Shell.Installer/` packages the entire `bin/x64/Release_ShellClient/` output of this project plus its post-build payload.

## Related docs

- Sister projects:
  - Contracts the bootstrapper wires: [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md), [`LeapVR.Shell.Modules.Interfaces`](../LeapVR.Shell.Modules.Interfaces/README.md), [`LeapVR.Shell.Repository.Interfaces`](../LeapVR.Shell.Repository.Interfaces/README.md), [`LeapVR.Shell.Services.Interfaces`](../LeapVR.Shell.Services.Interfaces/README.md)
  - Implementations the bootstrapper plugs in: [`LeapVR.Shell.Controllers`](../LeapVR.Shell.Controllers/README.md), [`LeapVR.Shell.Modules`](../LeapVR.Shell.Modules/README.md), [`LeapVR.Shell.Repository`](../LeapVR.Shell.Repository/README.md), [`LeapVR.Shell.Services`](../LeapVR.Shell.Services/README.md), [`LeapVR.Shell.Managers`](../LeapVR.Shell.Managers/README.md)
  - Setup/uninstall flow: [`LeapVR.Shell.Setup`](../LeapVR.Shell.Setup/README.md)
  - Localisation + categories: [`LeapVR.Shell.Language`](../LeapVR.Shell.Language/README.md), [`LeapVR.Shell.Categories`](../LeapVR.Shell.Categories/README.md)
- Tier overview: [`docs/client/README.md`](../README.md)
- Architecture: [`docs/architecture/`](../../architecture/) — read `overview.md` first.
