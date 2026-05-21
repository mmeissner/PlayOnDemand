# LeapVR.Content.Shared

> Wire-format contract between Content Creator (producer) and the kiosk (consumer): container header DTOs, package metadata DTOs, execution/process-monitor DTOs, and the Protobuf + JSON serializers that read/write them.

## Purpose

This is the **only** code shared between the Creator side (which writes `.vbox` files) and the kiosk's `ContainerModule` (which reads them). Every type here implements an interface from `LeapVR.Shell.Domain.Models.Container` / `…App` / `…Disk` / `…Execution`. Splitting the shape (interfaces in Domain.Models) from the wire-format implementation (concrete DTOs here) lets both ends share the contract while the on-disk format ages independently.

Two serializer formats live side by side:

- **Protobuf** for the binary header (`.vbox`) — see `AppInstallationHeaderSerializer` (`v2`) plus the `[ProtoContract]` / `[ProtoMember]` annotations on `AppInstallationHeaderDto` and `PackageDataDto`. Sub-type registration happens in `BaseAppInstallationHeaderSerializer.ProtobufInitialization()` (idempotent, double-checked-locked at first instantiation).
- **JSON (Newtonsoft.Json)** for the metadata sidecars zipped inside the Metadata package (`displayData.json`, `platformData.json`) — see `ContainerJsonSerializer` and the generic `InterfaceConverter<TInterface, TImplementation>`.

**Anything you change here is a wire-format change** that must be deployed to both the Creator builds and every running kiosk. There is no version negotiation beyond the integer `AppInstallationHeaderDto.Version` (Creator currently emits `2`).

## Tech

- **Target framework:** .NET Framework 4.7.1, Library
- **Key NuGet packages:**
  - `protobuf-net` 2.3.7 — binary serialization of the header
  - `Newtonsoft.Json` 12.0.2 — JSON sidecar serialization
  - `NLog` 4.5.11 — logging in the serializer
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib.Win` — `IZipContainerHeader` + extension helpers (`LogJson`)
  - `LeapVR.Shell.Domain.Models` — the `I*Dto` interfaces these DTOs implement
  - `LeapVR.Shell.Modules.Interfaces`, `LeapVR.Shell.Modules` — `IAppInstallationHeaderSerializer` contract + `IPackageData` etc.
  - `Pod.Data.Infrastructure` — `LogJson()` extension used by error paths

## Responsibility

**It IS responsible for:**
- Defining every concrete type that crosses the `.vbox` boundary.
- Registering the Protobuf type/sub-type model exactly once per process (lazy, locked).
- Serializing/deserializing the header to/from `Stream` and `string filePath`.
- Providing a JSON serializer that knows how to round-trip interface-typed properties (Newtonsoft.Json can't otherwise hydrate `IDiskEntityDto` etc. from JSON).

**It is NOT responsible for:**
- Storing the actual game files — that's the zip-package side handled by `LeapVR.Shell.Modules.Container` (`ContainerModule`, `NewAppInstallationContainer`, `NewPackage`).
- Filesystem layout / temp-dir handling — that's `LeapVR.Content.Creator.Logic` on the producer side, `LeapVR.Shell.Modules.Container` on the consumer side.
- Validation. Producers and consumers validate independently.

## Public API surface

### Container DTOs (under `LeapVR.Content.Shared.Container`)

| Type | Implements | Wire role |
|------|------------|-----------|
| `AppInstallationHeaderDto` *(internal)* | `IAppInstallationHeader` | Top-level Protobuf message. Fields: `ApplicationGuid` (10), `Version` (20), `ThumbnailAsBytes` (40), `TotalFilesCount` (50), `TotalFilesSize` (60), `PackageDataFileOffsets` (70 — `Dictionary<IPackageData, long>`), `DisplayName` (80). |
| `PackageDataDto` *(internal)* | `IPackageData` | One entry per package inside the container. Fields: `PackageGuid`, `PackageVersion`, `ApplicationGuid`, `ContentType`, `TotalFilesCount`, `TotalFilesSize`. |
| `AppDisplayDataDto` | `IAppDisplayDataDto` | JSON sidecar (`database/displayData.json`). Fields: `ApplicationGuid`, `Name`, `Description`, `Category`, `MainPicture` (`IDiskEntityDto`), `Tags[]`. |
| `AppPlatformDataDto` | `IAppPlatformDataDto` | JSON sidecar (`database/platformData.json`). Fields: `ApplicationGuid`, `PlatformPluginId`, `IEnumerable<IProcessExecutionLogicDto> ExecutionLogicInstructions`. |
| `ProcessExecutionLogicDto` | `IProcessExecutionLogicDto` | One per launchable. Fields: `ApplicationGuid`, `PlatformPluginId`, `DisplayName`, `ExecutionFile` (`IDiskEntityDto`), `ExecutionParameters`, `RelativeWorkingDirectory`, `MonitorInstructions[]`, `ReguiredVrModuleGuid`, `RequiredModuleGuids[]`, `OptionalModuleGuids[]`. (Note the typo `Reguired` matches the interface in Domain.Models.) |
| `ProcessMonitorInstructionDto` | `IProcessMonitorInstructionDto` | One per process the kiosk should watch. Fields: `ExecutableRelativePathFileName`, `ProcessMonitorOption Instruction` (flags: `Ignore`, `KillOnExit`, `KillProcessOnNotResponding`, `IsMainExecutable`). |
| `DiskEntityDto` *(internal)* (in `Container/`) | `IDiskEntityDto` | Pointer to a file inside a package: `ApplicationGuid`, `PackageGuid`, `RelativePath`. |

There is also a **second `DiskEntityDto`** at the project root (`LeapVR.Content.Shared.DiskEntityDto`, public) — same fields, different namespace. The Container one is `internal`; the root one is what producer code uses (`LeapVrContainerCreation` aliases it: `using DiskEntityDto = LeapVR.Content.Shared.DiskEntityDto;`).

### Serializers

- `AppInstallationHeaderSerializer : BaseAppInstallationHeaderSerializer, IAppInstallationHeaderSerializer`
  - `IZipContainerHeader LoadFromFile(string filePath)`
  - `IZipContainerHeader LoadFromStream(Stream source)` — `Serializer.Deserialize<AppInstallationHeaderDto>(source)`
  - `bool SaveToFile(string filePath, IZipContainerHeader header)` — deletes existing file, writes new Protobuf payload
  - `bool SaveToStream(Stream destination, IZipContainerHeader header)`
  - All paths log + rethrow on exception. Returns `false` if the supplied `IZipContainerHeader` is not actually an `IAppInstallationHeader`.
- `abstract class BaseAppInstallationHeaderSerializer`
  - First-time-only `ProtobufInitialization()` registering sub-types: `IAppInstallationHeader → AppInstallationHeaderDto` (tag 20), `IPackageData → PackageDataDto` (tag 30), `IAppDisplayDataDto → AppDisplayDataDto` (tag 70). Double-checked locking on a static `Locker`.
- `static class ContainerJsonSerializer`
  - `T DeserializeObject<T>(string)` and `string SerializeObject(object, Formatting=Indented, JsonSerializerSettings=null)`
  - Bundles `InterfaceConverter<I, T>` for each of the 5 DTOs that show up as interface-typed properties.
- `class InterfaceConverter<TInterface, TImplementation> : JsonConverter`
  - Generic adapter so Newtonsoft.Json can both write any `TInterface` value and hydrate it back into a concrete `TImplementation`.

## Internal structure

```
LeapVR.Content.Shared/
├── BaseAppInstallationHeaderSerializer.cs   Protobuf type-model registration (lazy, locked)
├── DiskEntityDto.cs                         PUBLIC root-namespace variant (used by producer code)
├── Container/
│   ├── AppInstallationHeaderDto.cs          [ProtoContract] header — the binary payload
│   ├── AppInstallationHeaderSerializer.cs   v2 Protobuf serializer entry point
│   ├── PackageDataDto.cs                    [ProtoContract] per-package metadata
│   ├── AppDisplayDataDto.cs                 JSON: name/desc/category/tags/picture
│   ├── AppPlatformDataDto.cs                JSON: per-platform execution instructions
│   ├── ProcessExecutionLogicDto.cs          JSON: one launchable
│   ├── ProcessMonitorInstructionDto.cs      JSON: one watched process
│   ├── DiskEntityDto.cs                     INTERNAL Container-namespace variant
│   └── ContainerJsonSerializer.cs           JSON entry + InterfaceConverter<>
```

## Notable patterns / gotchas

- **Two `DiskEntityDto` types exist.** `LeapVR.Content.Shared.DiskEntityDto` is `public`; `LeapVR.Content.Shared.Container.DiskEntityDto` is `internal`. They are structurally identical. The Creator's `LeapVrContainerCreation.cs` deliberately disambiguates with a `using` alias. If you add a third copy you will pick a wrong one in JSON round-tripping.
- **Header version is integer + unmanaged.** `AppInstallationHeaderDto.Version` is just an `int` field — Creator hard-codes `2`. There is no negotiation: if you bump to `3`, every consumer must understand `3` or you must keep both serializers around.
- **Protobuf sub-type registration is global and one-shot.** `BaseAppInstallationHeaderSerializer` uses a `volatile bool _initialized` + `lock` pattern; once any serializer instance is constructed, `RuntimeTypeModel.Default` is mutated for the process lifetime. Don't try to register a second alternative sub-type later in the process.
- **`PackageDataFileOffsets` uses `IPackageData` as a dictionary key.** Equality is reference-based by default — Protobuf's deserializer materializes a fresh `PackageDataDto` per entry, which is fine for a one-pass read but means you can't blindly look up a package by reusing a different `IPackageData` instance.
- **`InterfaceConverter` uses `serializer.Populate`**, so any extra properties on the implementation that the interface doesn't expose will still be hydrated. This is intentional but can mask schema mismatches.
- **Field tags carry forward.** Don't renumber `[ProtoMember(N)]` — old kiosks in the wild will misread fields.
- **`ProcessExecutionLogicDto` and several other DTOs lack `[ProtoContract]`** — they only travel as JSON, never as Protobuf. The `using ProtoBuf;` lines in those files are leftover; only the JSON side (`ContainerJsonSerializer`) actually serializes them.
- **`AppPlatformDataDto.ExecutionLogicInstructions` is `IEnumerable<IProcessExecutionLogicDto>`** — the JSON converter handles the element type via `InterfaceConverter<IProcessExecutionLogicDto, ProcessExecutionLogicDto>`.

## Consumers

- **`LeapVR.Content.Creator.Logic`** — `LeapVrContainerCreation` constructs DTOs and serializes them to disk. `ContainerCreation` instantiates `AppInstallationHeaderSerializer` and hands it to `ContainerModule`.
- **`LeapVR.Content.Creator`** — references the assembly transitively; `SplitVBoxFileViewModel` constructs `AppInstallationHeaderSerializer` directly to open existing `.vbox` files.
- **Kiosk side (`LeapVR.Shell.Modules.Container.ContainerModule`)** — uses `IAppInstallationHeaderSerializer` to read the header at install/launch time.

## Related docs

- [`../README.md`](../README.md) — tier overview, full container format spec.
- [`../LeapVR.Content.Creator.Logic/README.md`](../LeapVR.Content.Creator.Logic/README.md) — producer that fills these DTOs.
- [`../../client/README.md`](../../client/README.md) — kiosk consumer side (when those docs are written).
