# LeapVR.Content.Creator.Logic

> UI-decoupled packaging logic. The wizard hands a populated `LeapVrContainerCreation` here and calls `DoWork()`; this project writes the `.vbox` file.

## Purpose

This is the only project in the Content Creator tier that knows how to build a `.vbox` container. It exposes a small contract — `IWizardModule.DoWork()` — and a concrete `LeapVrContainerCreation` whose public properties are filled in by the wizard view models, then `await`ed.

There are **no WPF or Caliburn references**. The references are limited to `LeapVR.Content.Shared` (the wire-format DTOs and serializers), `LeapVR.Shell.Modules` (for `ContainerModule` / `INewApplicationInstallationContainer`), Domain.Models, and a few utility libraries. This separation is intentional: a CLI tool wanting to build the same containers needs only to instantiate `LeapVrContainerCreation`, set its properties, and `await DoWork()`.

A second concrete module, `LeapVrContainerEditor`, opens an existing `.vbox` for editing; it is partially implemented and currently throws `NotImplementedException` from its `DoWork()`.

A `SteamApiDataAcquisition` helper fetches title/description/header-image from the Steam Store API for the "create from Steam ID" wizard branch. (The Steam *create* path itself is not yet wired in `ShellViewModel.GoCreate()` — the helper exists for future use.)

## Tech

- **Target framework:** .NET Framework 4.7.1, Library
- **Key NuGet packages:**
  - `Newtonsoft.Json` 12.0.2 — JSON for `displayData.json` / `platformData.json` sidecars (via `ContainerJsonSerializer` in Shared)
  - `INIFileParser` 2.4.0 — INI files (mapping & launcher config in Util are read via this)
  - `NLog` 4.5.11 — logging
  - `System.Reactive` 4.1.5 — `IObservable<Empty>` progress streams (`WhenContainerCreationStarted`, `WhenProgressChanged`, `WhenContainerCreationEnded`)
- **Project references (in this repo):**
  - `LeapVR.Content.Shared` — DTOs + serializers (the wire format)
  - `LeapVR.Content.Creator.Language` — localized strings used in `Enums.cs` `LocalizedDescription` attributes
  - `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Wpf`
  - `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Managers`, `LeapVR.Shell.Modules`, `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Repository`, `LeapVR.Shell.Repository.Interfaces`
  - 3rdParty: `Steam.Models.Net452`, `SteamWebAPI2.Net452` (for `SteamApiDataAcquisition`)

## Responsibility

**It IS responsible for:**
- Driving `ContainerModule` to build a new `INewApplicationInstallationContainer`, add the GameFiles / MediaFiles / Metadata packages, and persist the header.
- Translating wizard-supplied `IAppExecuteInstruction` / `Executable` records into `ProcessExecutionLogicDto` + `ProcessMonitorInstructionDto[]`, including OR-ing the right `ProcessMonitorOption` flags (`IsMainExecutable`, `KillOnExit`, `KillProcessOnNotResponding`).
- Generating the temp working directory under `%TEMP%\VrLeapContentCreator\<applicationGuid>\`, copying the main picture, writing the metadata JSON files, and triggering the final `SaveToFiles()` on a background `Task.Run`.
- Reporting progress and lifecycle (started/progress/ended) as Rx streams that the wizard subscribes to.
- Acquiring Steam Store metadata for a given AppId (`SteamApiDataAcquisition`).

**It is NOT responsible for:**
- Defining the DTO shapes — those are in `LeapVR.Content.Shared`.
- Performing the actual zip/protobuf I/O — `ContainerModule` (in `LeapVR.Shell.Modules`) does that.
- 7zip extraction during the wizard's import-from-source-archive flow — that's `LeapVR.Content.Util` invoked by the Creator UI. (The kiosk extracts `.vbox` containers via DotNetZip in `ContainerModule`, not 7zip.)
- Any UI concern.

## Public API surface

- `abstract class ContainerCreation : IWizardModule` — base for all platform-specific creators.
  - Properties wizards fill in: `SteamId`, `MainPictureFilePath`, `ContainerOutputFilePath`, `DisplayName`, `Description`, `Category`, `Tags`, `List<IAppExecuteInstruction> Executables`.
  - Progress: `TotalFilesCount`, `DoneFilesCount`, `TotalFilesSize`, `DoneFilesSize`, `WasContainerCreationStarted`, `IsContainerCreationEnded`, `OccuredException`.
  - Streams: `WhenContainerCreationStarted`, `WhenProgressChanged`, `WhenContainerCreationEnded` (all `IObservable<Empty>`).
  - Holds a protected `IContainerModule ContainerModule` constructed with `AppInstallationHeaderSerializer` (the v2 protobuf serializer).
- `class LeapVrContainerCreation : ContainerCreation, IDisposable` — the only concrete implementation today.
  - **Entry point: `override async Task DoWork()`** — wraps `CreateContainerAsyncLogic()` and pumps the lifecycle subjects.
  - `string AppBaseDirectory { get; set; }` — the source directory whose contents become the `GameFiles` package.
  - Hard-coded constants worth knowing:
    - `VrLeapPlatformGuid = aa14f747-5d15-4b06-a9c0-7187f0e206d3`
    - `Container Version = 2`
    - `MainPictureFileName = "mainPicture.png"` (TODO in source: enforce this name)
    - Package layout: GameFiles at `""`, media at `images/`, metadata at `database/`.
- `class LeapVrContainerEditor : IWizardModule` — opens an existing `.vbox` (header + sibling `.vboxdata`); `IsValid` reports whether both files exist; `ContainerInfo` exposes the parsed structure. **`DoWork()` currently throws `NotImplementedException`.**
- `interface IWizardModule` — `Task DoWork()` + `Exception OccuredException { get; }`. Implemented by `ContainerCreation`, `LeapVrContainerEditor`, and the wizard's `SplitVBoxFileViewModel` (which lives in the Creator project, not here, but implements the same contract).
- `interface IAppExecuteInstruction` + `class Executable` + `class VRSelectableModule` — the user-side description of "this is one launchable, here are the processes that go with it, here's the VR module it needs".
- `enum PlatformType { None=0, LeapVr=1, Steam=2 }` — decorated with `LocalizedDescription` attributes (`Resources` from the Language project).
- `class SteamApiDataAcquisition : IDisposable` — `Acquire(steamId, language, countryCodes)` static factory; populates `Title`, `Description`, `ImagePath`. Tries each country code in turn; downloads `details.HeaderImage` to `%TEMP%\VrLeapContentCreator\WebDownload\<steamId>`.
- `class PackageDataValidator` — currently empty (placeholder).
- `ContainerEdit.cs` — defines `ContainerInfo`, `EditableAppPlatformData`, `EditableExecutionLogic`, `DiskEntityNotifiable`, `AppDisplayDataNotifiable`. Several constructors `throw new NotImplementedException()` waiting for the v2 container shape to settle.

## Internal structure

```
LeapVR.Content.Creator.Logic/
├── ContainerCreation.cs        Abstract base + the protected ContainerModule wiring
├── LeapVrContainerCreation.cs  THE entry point (DoWork → CreateContainerAsyncLogic)
├── ContainerEdit.cs            INotifyPropertyChanged shells over the editable container shape
├── LeapVrContainerEditor.cs    Open-existing-vbox flow (DoWork unimplemented)
├── SteamApiDataAcquisition.cs  Steam Store API helper
├── PackageDataValidator.cs     (empty placeholder)
├── IAppExecuteInstruction.cs   IAppExecuteInstruction, Executable, VRSelectableModule
├── IWizardModule.cs            Task DoWork() + OccuredException
└── Enums.cs                    PlatformType (LeapVr / Steam)
```

## Notable patterns / gotchas

- **`DoWork()` is the only entry**. Everything in `LeapVrContainerCreation` flows from there; the wizard awaits it once.
- **Subjects vs. observables**: each of the three lifecycle subjects is a `ReplaySubject` or `Subject` privately, exposed as `IObservable<Empty>` publicly. `LeapVrContainerCreation` also subscribes the inner container's started/progress streams *into* its own subjects (`newContainer.WhenContainerCreationStarted.Subscribe(_whenContainerCreationStartedSubject)`), so subscribers see both the container's events and the lifecycle ending.
- **GUIDs are minted per call** — `appGuid = Guid.NewGuid()` inside `CreateContainerAsyncLogic`. There is no upsert path; every Create produces a new application identity.
- **Hard-coded LeapVR platform GUID** — `aa14f747-5d15-4b06-a9c0-7187f0e206d3`. The kiosk's platform-plugin loader must know this same GUID.
- **JSON metadata is written to disk first, then `metadataPackage.AddDirectory(tempMetadataDir, "")`** — the package builder zips files from disk; there is no in-memory streaming path.
- **ProcessMonitorOption is OR-flagged**, not assigned: `instruction.Instruction |= ProcessMonitorOption.IsMainExecutable;` etc. So a single executable can be e.g. `IsMainExecutable | KillOnExit`.
- **Tags parsed from a single string** by `TagStringToArray`, splitting on `,`, `;`, or space.
- **Working directory rewriting** — if the user-supplied working directory is empty, `RelativeWorkingDirectory = null`; otherwise computed as relative-to-AppBaseDirectory via `QuickLeap.GetRelativePath`.
- **Steam helper swallows per-country exceptions silently** (try/catch around `GetStoreAppDetailsAsync` with a comment "// swallow"). Logs at Debug level.

## Consumers

- `LeapVR.Content.Creator` — instantiates `LeapVrContainerCreation` in `ShellViewModel.GoCreate()` and `LeapVrContainerEditor` in `ShellViewModel.GoEdit()`. Awaits `DoWork()` from `WizardViewModel.Create()`.

Nothing else depends on this project. The kiosk consumes the *output* (`.vbox` files via `ContainerModule`), not this assembly.

## Related docs

- [`../README.md`](../README.md) — tier overview, full container format spec.
- [`../LeapVR.Content.Shared/README.md`](../LeapVR.Content.Shared/README.md) — DTOs this project serializes.
- [`../LeapVR.Content.Creator/README.md`](../LeapVR.Content.Creator/README.md) — wizard host that calls `DoWork()`.
