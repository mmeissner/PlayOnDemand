# LeapVR.Content.Creator

> WPF entry-point and wizard UI for building, editing, and splitting `.vbox` container files.

## Purpose

This is the executable end of the Content Creator tier. It hosts a Caliburn.Micro `ShellViewModel` from which the operator picks one of three flows — **Create** a new container, **Edit** an existing one, or **Split** a `.vbox` into per-package zips. Each flow drives a `WizardViewModel<IStepScreenWizard>` that walks the user through screens, validates input, and finally hands off to a `IWizardModule` (the actual packaging logic from `LeapVR.Content.Creator.Logic`).

The project deliberately does no packaging itself. It is purely glue: file/folder pickers, validation, progress display, language switching (EN/ZH-CN), and view-models that populate properties on a `ContainerCreation` / `LeapVrContainerEditor` object. When the user clicks **Create**, control flows out of the WPF code into `IWizardModule.DoWork()` — see [`../LeapVR.Content.Creator.Logic/README.md`](../LeapVR.Content.Creator.Logic/README.md).

The output assembly is named **`LeapPlay.Content.Creator.exe`** (the `LeapPlay.*` brand; `LeapVR.*` namespaces are a historical holdover).

## Tech

- **Target framework:** .NET Framework 4.7.1, `WinExe`, x64-friendly
- **Key NuGet packages:**
  - `Caliburn.Micro` 3.2 — MVVM framework (Conductor/Screen/IShell pattern)
  - `SimpleInjector` 4.2.2 — DI container (registered in `AppBootstrapper`)
  - `WPFLocalizeExtension` 3.1.2 + `XAMLMarkupExtensions` — XAML-side localization (`{lex:Loc ...}`)
  - `FontAwesome.WPF` 4.7 — icon font
  - `SharpVectors` 1.0 — SVG rendering for vector resources
  - `Windows7APICodePack-Shell` 1.1 — `CommonOpenFileDialog` (folder picker)
  - `System.Reactive` 4.1.5 — Rx subjects driving wizard `WhenBusyRequested` / `WhenRevalidated` streams
  - `NLog` 4.5.11 — logging (`Debug_NLog.config` / `Release_Nlog.config`, copied by post-build)
  - `Newtonsoft.Json` (transitive)
- **Project references (in this repo):**
  - `LeapVR.Content.Creator.Language`, `LeapVR.Content.Creator.Logic`, `LeapVR.Content.Shared`, `LeapVR.Content.Util`
  - `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Wpf`
  - `LeapVR.Shell.Categories`, `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Managers`, `LeapVR.Shell.Modules`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.OpenVR.Wrapper`, `LeapVR.Shell.Repository`
  - `LeapVR.Utilities.Steam`, `LeapVR.Utilities.Windows`
  - 3rdParty: `Steam.Models.Net452`, `SteamWebAPI2.Net452`

## Responsibility

**It IS responsible for:**
- WPF application bootstrap (`App.xaml` references `AppBootstrapper` as a `ResourceDictionary` key, which initializes Caliburn).
- DI composition root (`AppBootstrapper.Configure()`).
- Hosting the wizard step screens (Create / Edit / Split).
- Persisting the `ContentCreatorConfig` (last-used paths, available platforms, language).
- Localization runtime switching via `WPFLocalizeExtension.LocalizeDictionary.Instance.Culture`.
- Catching unhandled exceptions and logging them via NLog (`OnApplicationDispatcherUnhandledException`, `OnCurrentDomainUnhandledException`).

**It is NOT responsible for:**
- The actual `.vbox` build. That lives in `LeapVR.Content.Creator.Logic` (`LeapVrContainerCreation.DoWork()`).
- Container DTOs / serializers — those are in `LeapVR.Content.Shared`.
- Archive/7zip wrangling — that's `LeapVR.Content.Util`.

## Public API surface

The Creator is a `WinExe`, so "public API" is mostly internal MVVM types, but the contracts that other projects (and the wizard internals) bind to are:

- `IShell : IScreen` — the root window. Implemented by `ShellViewModel`.
- `IStepScreenWizard : IScreen` — base for any wizard step. Defines `Previous`, `Next`, `CanGoNext/Previous/Exit`, and an `IObservable<BusyCancelableViewModel> WhenBusyRequested` stream so steps can pop a cancellable progress dialog.
- `IStepScreenCreate : IStepScreenWizard` — exposes the `ContainerCreation PackageCreation` being built.
- `IStepScreenEdit : IStepScreenWizard` — exposes the `LeapVrContainerEditor ContainerEditor` being mutated.
- `ContentCreatorConfig : ConfigObject` — persisted user settings (paths, language, available platforms, etc.). Stored via `IConfigFileRepository<ContentCreatorConfig>` from `LeapVR.Shell.Modules.FileConfig`.
- `AppBootstrapper : Caliburn.Micro.BootstrapperBase` — wires SimpleInjector into Caliburn's `IoC`.

## Internal structure

```
LeapVR.Content.Creator/
├── App.xaml / App.xaml.cs            Application root; merges resource dictionaries; instantiates AppBootstrapper
├── AppBootstrapper.cs                DI registration, root view display, exception handlers
├── ContentCreatorConfig.cs           Persisted user settings
├── IShell.cs / IStepScreen*.cs       Core wizard interfaces (see above)
├── ShellView.xaml                    Shell window markup
├── Debug_NLog.config / Release_Nlog.config
├── Resources/
│   ├── Converters.xaml
│   ├── Images/, Vectors/             PNG/SVG/XAML asset dictionaries
│   └── Styles/                       Clickable, ComboBox, Expander, TextElement, Templates, Styles
├── UI/
│   ├── ValidatingScreen.cs           Base Screen w/ IDataErrorInfo + WhenRevalidated Rx subject
│   ├── ViewModels/
│   │   ├── ShellViewModel.cs         Root VM. Hosts language switcher + Go{Create,Edit,Split} dispatch
│   │   ├── WizardViewModel.cs        Conductor<IStepScreenWizard>.Collection.OneActive — Back/Next/Create/Exit
│   │   ├── BusyCancelableViewModel.cs   Cancellable busy modal
│   │   ├── CompleteViewModel.cs      Success/error dialog after DoWork() completes
│   │   ├── Category/                 CategoryViewModel
│   │   ├── Create/                   ApplicationModeVM, AppDetailInfoVM, AppExecuteInstructionVM,
│   │   │                             LeapVrAppExecutableInfoVM, SummaryVM
│   │   ├── Edit/                     EditAppDetailInfoVM, EditExecutionLogicVM, EditPackageVM, EditPlatformDataVM
│   │   └── Split/                    SplitVBoxFileVM (.vbox → per-package .zip)
│   └── Views/                        Matching XAML views (Caliburn.Micro convention-based)
├── _OBFUSCATE.bat                    ConfuserEx wrapper
└── LeapVR.Content.Creator.ConfuserEx.crproj
```

## Notable patterns / gotchas

- **`AppBootstrapper` is referenced from XAML**, not from `Main()`. `App.xaml` adds it as `<local:AppBootstrapper x:Key="Bootstrapper" />` inside `Application.Resources`, and Caliburn's `BootstrapperBase` constructor calls `Initialize()`. `App.xaml.cs` is empty.
- **Caliburn `IoC` + SimpleInjector**: `GetInstance` falls back to a reflective name lookup when `service == null` — Caliburn passes `(null, "ViewModelName")` for view-first resolution.
- **`Release_ShellClient` configuration** is the one used by the master build script. `Release` and `Debug` post-build steps copy the appropriate NLog config; everything else is a no-op via the labelled `goto :exit`.
- **Pre-build hook deletes any stale `NLog.config`** in the output dir to force regeneration.
- **Steam-platform create flow throws `NotImplementedException`** — `ShellViewModel.GoCreate()` only wires up `PlatformType.LeapVr`. The platform enum (in `LeapVR.Content.Creator.Logic/Enums.cs`) lists `LeapVr = 1` and `Steam = 2` but only the LeapVR flow is wired.
- **Edit flow has unfinished branches** — `ContainerInfo` and `EditableExecutionLogic` (in Logic) `throw new NotImplementedException()` for the v2 container migration. `GoEdit()` does run, but downstream Edit screens may be incomplete.
- **OpenVR dependency at startup** — `AppBootstrapper.Configure()` registers `OpenVrModule` as `IOpenVrModule`/`IVrModule`, so `openvr_api.dll` and friends must be reachable at runtime even though the Creator doesn't render VR. This is to share VR-module discovery code with the kiosk.
- The Categories resource dictionary is loaded **by reference order** from `LeapVR.Shell.Categories` — the comment in `App.xaml` warns: *"If the Order changes, we will not be able to identify categories in code"*.

## Consumers

None — this is the executable. Nothing references it.

## Related docs

- [`../README.md`](../README.md) — Content Creator tier overview, container format, where containers live.
- [`../LeapVR.Content.Creator.Logic/README.md`](../LeapVR.Content.Creator.Logic/README.md) — what `WizardViewModel.Create()` actually invokes.
- [`../LeapVR.Content.Creator.Language/README.md`](../LeapVR.Content.Creator.Language/README.md) — strings used across the wizard.
- [`../../client/README.md`](../../client/README.md) — kiosk consumer side (when those docs are written).
