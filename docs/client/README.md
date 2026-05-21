# Client tier — `LeapVR.Shell.*`

> The kiosk software that runs on every VR station: a WPF application on .NET Framework 4.7.1 that launches games, drives SteamVR, plays background media, and reports state to the central server over gRPC.

This document is the entry point for the client side. Read it before drilling into per-project READMEs.

---

## What "client" means here

A *client* is a deployed copy of `LeapPlay.Shell.exe` running on an arcade VR PC. It is **not** a developer tool, **not** a web client, and **not** a thin terminal. It is the full kiosk operating shell: it owns the screen, hides the Windows taskbar, owns the GPU during VR sessions, manages the local game library, and arbitrates all user interaction. The web portal (`Pod.Web.Center`) only orchestrates; the client *executes*.

A single client process consumes ~15 sibling assemblies. They are split into three concentric rings:

```
  ┌───────────────────────────────────────────────────────────────────────┐
  │ Composition root                                                      │
  │   • LeapVR.Shell           WPF entry, Bootstrapper (SimpleInjector)   │
  │   • LeapVR.Shell.Setup     -config wizard / -uninstall flow            │
  ├───────────────────────────────────────────────────────────────────────┤
  │ Implementations (referenced only by the composition root)             │
  │   • LeapVR.Shell.Controllers   business orchestration                 │
  │   • LeapVR.Shell.Modules       feature subsystems (VR/XInput/Steam…)  │
  │   • LeapVR.Shell.Repository    LiteDB-backed local persistence        │
  │   • LeapVR.Shell.Services      gRPC client wrappers                   │
  │   • LeapVR.Shell.Managers      OS-level managers (USB, machine info)  │
  │   • LeapVR.Shell.Categories    app categorisation + icon dictionary   │
  │   • LeapVR.Shell.Language      WPFLocalizeExtension localisation      │
  │   • LeapVR.Shell.OpenVR.Wrapper  Valve openvr_api.dll P/Invoke        │
  ├───────────────────────────────────────────────────────────────────────┤
  │ Pure contracts (referenced by everyone)                               │
  │   • LeapVR.Shell.Domain.Models       domain entities + interfaces     │
  │   • LeapVR.Shell.Modules.Interfaces  IPlatformModule, IVrModule, …    │
  │   • LeapVR.Shell.Repository.Interfaces  IAppDisplayRepository, …      │
  │   • LeapVR.Shell.Services.Interfaces    ISessionServiceOutgoing, …    │
  └───────────────────────────────────────────────────────────────────────┘
```

**Rule:** an implementation project never references a sister implementation directly. It imports the `.Interfaces` project. The composition root in `LeapVR.Shell/Bootstrapper.cs` is the only place that wires concrete types to interfaces.

---

## Runtime startup sequence

The startup path is non-trivial because the same `LeapPlay.Shell.exe` binary is also the setup wizard and the uninstaller. Source: `LeapVR.Shell/Program.cs` and `LeapVR.Shell/Bootstrapper.cs`.

1. **`Program.Main`** runs first.
   - `SingleInstanceGuard.TryAcquire("LeapVR.Shell")` — refuses to launch a second process.
   - Hooks `AppDomain.CurrentDomain.UnhandledException`.
   - Inspects `Environment.GetCommandLineArgs()` for switches defined in `LeapVR.Shell.Domain.Models/GlobalConfig.cs`:
     - `-config` → first-run wizard (`LeapVR.Shell.Setup`)
     - `-uninstall` → uninstaller (`LeapVR.Shell.Setup`)
     - `-d` / `-debug` → run windowed, taskbar visible
     - `-rdbg` → show "Attach Remote Debugger" message-box gate
   - Splash screen (`Resources/Images/Splashscreens/splashscreen_{1..4}.png`) shown for normal launches.
   - Builds a `ShellConfigurator(new ConfigFileRepository<DiskConfig>())` and checks `HasValidDiskConfig`. **No valid disk config => the Setup wizard runs even without `-config`.** This is the first-run trigger.
   - Branches: `new Setup.App(SetupType.Config | SetupType.Uninstall)` *or* `new App()` (the kiosk shell).

2. **`App.xaml` is loaded.** It is a thin `Application` whose `ResourceDictionary.MergedDictionaries` instantiates `Bootstrapper` as a XAML resource:
   ```xml
   <ResourceDictionary>
     <local:Bootstrapper x:Key="Bootstrapper" />
   </ResourceDictionary>
   ```
   That construction triggers `BootstrapperBase.Initialize()`, which is Caliburn.Micro's standard hook.

3. **`Bootstrapper.Configure`** (called by Caliburn.Micro before the first view is shown) wires SimpleInjector:
   - `RegisterHostSystem` — `IApplicationHost` (this), `IGlobalConfiguration`, `IEventAggregator`, `IWindowManager`, `IUIMessageBroker`, `ILanguageSelector`.
   - `RegisterConfigurations` — open generic `IConfigFileRepository<>` plus singletons for `RpcClientConfig`, `UiConfig`, `IServerConfig` (Static), `SystemConfig`, `SecurityConfig`.
   - `RegisterConcreteRepositories` — every `LeapVR.Shell.Repository.*` repo as a singleton, plus a hand-built `OpenVrSettingsSetRepository`.
   - `RegisterRPCServices` — `RemoteServiceFactory` and a singleton `IRemoteServiceSet` produced by it (the gRPC client surface).
   - `RegisterControllers` — every controller from `LeapVR.Shell.Controllers`, with several controllers added to two `RegisterCollection` groups (`IRunLevelMsgReceiver`, `IExecutionMessageReceiver`) so the `StationController` can broadcast to them.
   - `RegisterModules` — modules from `LeapVR.Shell.Modules` (VR, XInput, container, multimedia, network, USB, Steam lib, app-info processor, two `IPlatformModule`s — `VBoxPlatformModule` + `SteamPlatformModule` — and the `IBaseModule` collection).
   - `RegisterViewModels` — `IShell`, `ILoginViewModel`, `IDashboardViewModel`, `IAdministrationViewModel`, status bar, keypad, USB sticks bar, all administration sub-tabs, statistics sub-tabs, app-management sub-tabs (registered by reflection on marker interfaces like `ITabItemSystemScreen`).
   - `Container.Verify()` — SimpleInjector eager validation. Misconfiguration crashes here with a fatal log.

4. **`Bootstrapper.OnStartup`** — calls `ISystemController.Initialize()` and grabs `IStationController` (so Alt+F4 can still trigger a clean shutdown).

5. **`Bootstrapper.ShowGUI`** (invoked through the Caliburn `IApplicationHost` interface) hides the Windows taskbar (`Taskbar.Hide()`), hides the cursor, then calls `DisplayRootViewFor<IShell>(settings)` — Caliburn.Micro resolves `ShellViewModel` from the IoC container, looks up the matching view by name convention (`UI/Shell/Views/ShellView.xaml`), and shows it.

6. **`ShellViewModel`** is a Caliburn `Conductor.OneActive<IScreen>`. It conducts:
   - `LoginViewModel` (entry / heartbeat / billing) — `UI/Shell/Login/`
   - `DashboardViewModel` (game grid) — `UI/Shell/Dashboard/`
   - `AdministrationViewModel` (admin panel with sub-tabs) — `UI/Shell/SystemAdministration/`
   - `BlockShellViewModel` (modal blocker during long ops) — `UI/Shell/Blocker/`
   - `ConnectViewModel` (offline / reconnect) — `UI/Shell/Connect/`
   It implements `IHandle<>` for half a dozen UI events from `IUIMessageBroker`.

---

## Cross-cutting tech

| Concern | Choice | Rationale / Notes |
|---|---|---|
| UI framework | WPF (.NET Framework 4.7.1, x64) | Required by SteamVR overlay rendering and existing native deps. |
| MVVM | Caliburn.Micro 3.2 | View ↔ ViewModel auto-binding by name (`FooViewModel` → `FooView`). |
| IoC | SimpleInjector 4.2.2 | All registration is in `Bootstrapper.cs`. `Container.Verify()` enforces correctness at startup. |
| Logging | NLog 4.5.11 | `Debug_NLog.config` / `Release_Nlog.config` are copied to `LeapPlay.Shell.exe.nlog` by the post-build step. |
| Local DB | LiteDB 4.1.2 (embedded) | One file at `%APPDATA%\LeapPlay\LeapPlay.db`. Not Sql, not Sqlite. See `LeapVR.Shell.Repository`. |
| Localisation | WPFLocalizeExtension 3.1.2 + `.resx` | Currently EN-US and ZH-CN. See `LeapVR.Shell.Language`. |
| Materials/icons | MaterialDesignThemes 2.5.1, FontAwesome.WPF 4.7, LiveCharts 0.9.6 | Visual styling. |
| Media | Unosquare FFME (vendored) + FFmpeg.AutoGen + native FFmpeg 4.0.2 | Video/audio playback in Login background and admin tabs. |
| VR | Valve OpenVR via `LeapVR.Shell.OpenVR.Wrapper` | Bundled `openvr_api.dll`. |
| RPC | gRPC + Protobuf, server-cert TLS, `(identity, password)` metadata per call | Wrapped in `LeapVR.Shell.Services`. The kiosk attaches its `StationId` and `Station.Password` as plain `identity`/`password` gRPC metadata via a `CallCredentials` interceptor; server PBKDF2-verifies against `Station.PasswordHash`. **No HMAC, no mutual TLS.** See [`docs/architecture/auth.md`](../architecture/auth.md). |

---

## How a fresh agent should read this tier

1. **`LeapVR.Shell/`** — entry, bootstrapper, top-level views.
2. **`LeapVR.Shell.Domain.Models/`** — vocabulary. Every other project references this.
3. **`LeapVR.Shell.Controllers/`** — what orchestrates what. Controllers are the verbs.
4. **`LeapVR.Shell.Modules/`** — pluggable subsystems behind the verbs.
5. **`LeapVR.Shell.Services/`** — outbound gRPC; the only place that talks to `Pod.Web.Center`.
6. **`LeapVR.Shell.Repository/`** — local persistence (LiteDB).
7. The remaining smaller projects on demand.

For build/run mechanics, see `docs/architecture/build-and-deploy.md` (planned). For the gRPC contract surface, see `docs/architecture/grpc.md` (planned). For session state, `docs/architecture/session-lifecycle.md` (planned).

---

## Cross-references

- Server tier — [`docs/server/README.md`](../server/README.md)
- Repo overview — [`docs/README.md`](../README.md)
- Content authoring tool — [`docs/content-creator/README.md`](../content-creator/README.md)
