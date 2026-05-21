# Content Creator Tier

> WPF authoring tool that packages a folder of game files into a `.vbox` container the kiosk can install and run.

## What this tier produces

The Content Creator builds **`.vbox` container files** — self-describing archives that pair a binary header (Protobuf) with one or more zipped data packages (game files, media, metadata). Containers are the unit of distribution between the operator and the station: an arcade owner builds them once with the Creator, copies them to a station, and the kiosk's `ContainerModule` extracts and runs them.

```
┌────────────────────────────────────────────┐         ┌──────────────────────────────────────┐
│ LeapVR.Content.Creator (WPF)               │         │ LeapVR.Shell (kiosk WPF)             │
│   Wizard UI → builds AppInstallationHeader │ .vbox   │   ContainerModule consumes header,   │
│   + zipped packages → writes .vbox         ├────────►│   reads the ZIP package via          │
│                                            │ files   │   DotNetZip, spawns processes per    │
└────────────────────────────────────────────┘         │   execution logic, monitors per      │
                                                       │   ProcessMonitorOption flags         │
                                                       └──────────────────────────────────────┘
```

Both ends are **.NET Framework 4.7.1 WPF** apps and they share the wire-format types from `LeapVR.Content.Shared`.

---

## Project layout

| Project | Role |
|---------|------|
| **LeapVR.Content.Creator** | WPF entry point. Caliburn.Micro + SimpleInjector. Hosts the create / edit / split wizards. |
| **LeapVR.Content.Creator.Logic** | UI-decoupled packaging logic. `LeapVrContainerCreation.DoWork()` is the entry. |
| **LeapVR.Content.Creator.Language** | Localized strings (`en-US`, `zh-CN`) consumed via WPFLocalizeExtension. |
| **LeapVR.Content.Shared** | Container DTOs (`AppInstallationHeaderDto`, `AppDisplayDataDto`, `AppPlatformDataDto`, `ProcessExecutionLogicDto`, `ProcessMonitorInstructionDto`, `PackageDataDto`, `DiskEntityDto`) and serializers. **Wire-format contract between Creator and Kiosk.** |
| **LeapVR.Content.Util** | Archive (7zip) wrappers, game-engine fingerprinting, launcher (Steam-emulator) detection. **Used by the Creator wizard** to analyze and extract source archives the user drops in (`.zip`/`.rar`/`.7z`). The kiosk transitively pulls Util in but does NOT shell out to 7zip — its `ContainerModule` reads `.vbox` containers via `DotNetZip`. |
| **LeapVR.Content.3rdParty/bin/7zip** | Vendored 7zip CLI + DLL. Not a project; copied into Util's `BinaryDeps/7zip` at build time. Invoked by the Creator wizard during input-archive analysis. |

The reason **Creator** and **Creator.Logic** are split: the wizard view models (`LeapVrAppExecutableInfoViewModel`, `AppDetailInfoViewModel`, `SummaryViewModel` …) gather data and hand it off to `LeapVrContainerCreation.DoWork()`. The Logic project has zero references to WPF/Caliburn — only `LeapVR.Content.Shared`, `LeapVR.Shell.Modules` (for `ContainerModule`), and a handful of Domain.Models / Steam SDK references. A future CLI tool could host `LeapVrContainerCreation` directly without touching the WPF layer.

---

## Container format (`.vbox`)

A container is materialised as a header file plus one or more data files written next to it.

### Header — Protobuf

The header file (`*.vbox`) is a **Protobuf-serialized `AppInstallationHeaderDto`** (see `LeapVR.Content.Shared/Container/AppInstallationHeaderDto.cs`). Fields:

| Field | Meaning |
|-------|---------|
| `ApplicationGuid` | Stable per-application identifier (re-used across versions). |
| `Version` | Container schema version. Creator currently writes `2`. |
| `DisplayName` | Human-readable application name (also surfaced in the kiosk catalogue). |
| `ThumbnailAsBytes` | Raw bytes of the title image (PNG). |
| `TotalFilesCount`, `TotalFilesSize` | Aggregate metrics across all packages. |
| `PackageDataFileOffsets` | Map of `IPackageData → byte-offset` so the kiosk can seek to each package's payload. |

Serialization flows through `BaseAppInstallationHeaderSerializer.ProtobufInitialization()` which registers the Protobuf sub-types (`AppInstallationHeaderDto` at tag 20, `PackageDataDto` at tag 30, `AppDisplayDataDto` at tag 70).

### Packages — zipped payloads

`LeapVrContainerCreation.CreateContainerAsyncLogic()` always emits three packages:

1. **`ContentType.GameFiles`** — the entire app base directory (`AppBaseDirectory`).
2. **`ContentType.MediaFiles`** — the title image, copied to `images/mainPicture.png`.
3. **`ContentType.Metadata`** — JSON sidecars under `database/`:
   - `displayData.json` — `AppDisplayDataDto` (name, description, category, tags, main-picture pointer).
   - `platformData.json` — `AppPlatformDataDto` carrying `ExecutionLogicInstructions` (`ProcessExecutionLogicDto[]`).

JSON serialisation goes through `ContainerJsonSerializer`, which registers `InterfaceConverter<TInterface, TImplementation>` for every `I*Dto` so Newtonsoft.Json can round-trip the interface-typed properties.

### Execution / process-monitor model

For each launchable `IAppExecuteInstruction` the user defines in the wizard, the Creator writes one `ProcessExecutionLogicDto`:

- `ExecutionFile` — `DiskEntityDto` pointing at a file inside the GameFiles package.
- `ExecutionParameters`, `RelativeWorkingDirectory` — invocation specifics.
- `ReguiredVrModuleGuid` — VR module the kiosk must activate before launch (e.g. SteamVR).
- `MonitorInstructions[]` — one `ProcessMonitorInstructionDto` per process to watch. Each carries `ProcessMonitorOption` flags:
  - `IsMainExecutable` — when this process exits, the session ends.
  - `KillOnExit` — kill this process when the session ends.
  - `KillProcessOnNotResponding` — kill if it stops responding.
  - `Ignore` — present in the catalogue but not monitored.

This is the exact contract the kiosk's session/launcher code reads back at runtime.

### Install-time extraction

`.vbox` containers are standard ZIP files. The Creator writes them via `System.IO.Compression`; the kiosk reads them via `DotNetZip` (the `ZipContainer` family in `LeapVR.Shell.Modules/Container/`). 7zip is **not** invoked on the kiosk for `.vbox` extraction.

The 7zip binaries from `LeapVR.Content.3rdParty/bin/7zip` (invoked through `LeapVR.Content.Util/Archive/Archive.cs`) serve a different purpose: when the **Creator wizard** lets the user drop a `.zip`/`.rar`/`.7z` of a game, 7zip is shelled out to inspect and extract that source archive into the working directory before the wizard re-packages it as a `.vbox`. Output is parsed for entry tables and `Everything is Ok`.

---

## Editing an existing `.vbox` (partial-edit pipeline)

Containers can be 50 GB+ in practice, so the edit wizard never extracts + repacks the whole file. Edits are scoped to two surfaces:

- **Header-only**: `DisplayName`, `ThumbnailAsBytes` — rewrites the trailing Protobuf header + trailer (no package payload bytes touched).
- **Metadata-only**: `displayData.json` (Name, Description, Category, Tags, MainPicture pointer) + `platformData.json` (ExecutionLogicInstructions) — rebuilds the Metadata package's zip in memory, truncates the file at `metadataOffset`, appends the new zip, then re-writes the header + trailer.

The contract that makes this safe: **the Metadata package must always be the trailing package by file offset** (the Creator's `CreateContainerAsyncLogic` writes it last). Anything before the metadata offset is left byte-identical across edits — the GameFiles + MediaFiles packages, often the bulk of the container, are never re-read or re-written. Save is atomic via `File.Replace` against a `.vbox.editing` sibling.

### Edit call graph

```
ShellView "Edit" menu
  → ShellViewModel.GoEdit()
      → new LeapVrContainerEditor(vboxPath)        // open + load DTOs
      → EditAppDetailInfoViewModel(editor)         // XAML TwoWay-binds editor.DisplayData
      → EditPlatformDataViewModel(editor)
      → EditPackageViewModel(editor)
  → user mutates DTO properties via the wizard
  → user clicks Create / Save
      → WizardViewModel.Create()
          → await editor.DoWork()                  // → editor.Save()
              → BuildMetadataZip(DisplayData, PlatformData)
              → EditableAppInstallationContainer.StageMetadataPackageZip(bytes)
              → EditableAppInstallationContainer.Save()  // File.Replace atomic swap
```

`LeapVrContainerEditor` lives in `LeapVR.Content.Creator.Logic` (UI-decoupled). `EditableAppInstallationContainer` is the low-level primitive in `LeapVR.Shell.Modules/Container/`.

### Verifying the edit pipeline

Two reusable test harnesses live under `LeapVR.Shell.Modules.Container.Test/`:

- **`LeapVR.Shell.Modules.Container.Test.csproj`** (xUnit, 16 tests) — covers `EditableAppInstallationContainer` and `LeapVrContainerEditor` round-trips, including the 50 GB safety property (SHA-256 of bytes `0..metadataOffset` must match pre/post-edit).
- **`SampleVboxBuilder/`** — console exe that emits a tiny but realistic `.vbox` (GameFiles + MediaFiles + Metadata in canonical order) suitable for the WPF wizard's edit-open dialog.
- **`EndToEndEditVerifier/`** — console verifier that opens an existing `.vbox` via `LeapVrContainerEditor`, mutates `DisplayData.Description`, calls `Save()`, re-opens, and asserts byte-identity + persistence. Run it via `dotnet build -c Release_ShellClient -p:Platform=x64 EndToEndEditVerifier/EndToEndEditVerifier.csproj && EndToEndEditVerifier.exe <path-to-vbox>`. Exits 0 on PASS, 1 on FAIL.

---

## Where containers live

| Stage | Path |
|-------|------|
| Creator output | Selected by the user in `SummaryViewModel.BrowseOutputFilePath()`. Default = `ContentCreatorConfig.LastOutputPackageDirectory` + `<DisplayName>.vbox`. |
| Creator working temp | `%TEMP%\VrLeapContentCreator\<applicationGuid>\` (cleaned up after success). |
| Kiosk install location | Resolved by the kiosk's `ContainerModule` based on station configuration; details live in the kiosk docs. |

---

## Why Logic is decoupled from the UI

`LeapVR.Content.Creator.Logic` knows nothing about XAML, Caliburn screens, or the SimpleInjector container. It only knows:

- The user-facing intent expressed as `ContainerCreation` (abstract) → `LeapVrContainerCreation` (concrete) properties (`AppBaseDirectory`, `DisplayName`, `Executables`, `MainPictureFilePath`, `ContainerOutputFilePath`, …).
- The `IWizardModule` contract — a single `Task DoWork()` plus an `OccuredException` to read after.
- The shared DTOs from `LeapVR.Content.Shared` and the kiosk's `ContainerModule`.

The wizard view models live in the Creator project; they own the user dialogs, the path browsers, the validation, the busy spinner. Once the user clicks "Create", the wizard hands its filled `LeapVrContainerCreation` to `WizardViewModel.Create()` which simply `await`s `module.DoWork()`. **Wrapping the same call from a CLI** would mean: instantiate `LeapVrContainerCreation`, set its public properties, `await DoWork()`. No WPF dispatcher dependency on the hot path.

---

## Build configuration

All Content Creator projects target **`.NET Framework 4.7.1`**, x64-friendly, and are built under the **`Release_ShellClient`** configuration alongside the kiosk (see the root `Build_Free.bat` / `Build.bat`). The Creator project also has a separate `LeapVR.Content.Creator.ConfuserEx.crproj` and an `_OBFUSCATE.bat` script for optional ConfuserEx hardening.

Output assembly name for the entry project is **`LeapPlay.Content.Creator.exe`** (note the `LeapPlay.*` rebrand — the `LeapVR.*` namespace is a holdover).

---

## Related docs

- [`docs/README.md`](../README.md) — top-level repo index, glossary, build conventions.
- [`docs/client/README.md`](../client/README.md) — kiosk overview; the `ContainerModule` consumer side lives there.
- Per-project READMEs in this folder for deeper detail on each assembly.
