# LeapVR.Content.Util

> Archive (7zip) wrappers and game-engine fingerprinting. Used by the
> Creator's wizard and reused on the kiosk for install-time extraction.

## Purpose

A grab-bag of pre-packaging utilities. The original use-case is the
**Creator's "import a game folder/archive" path**: an operator points the
wizard at a folder full of game files (potentially still inside `.zip` /
`.rar` / `.7z` archives), and the Creator needs to:

1. Drive `7z.exe` to inspect / extract the archive.
2. Identify the engine (Unity, Unreal, CryEngine, Esenthel, SDL) and the
   game's main executable.

The 7zip binaries themselves live in
**`LeapVR.Content.3rdParty/bin/7zip/`** at the repo root and are linked
into this project's `BinaryDeps/7zip/` folder via `<Content Include>`
items in the csproj. They're copied to the build output of any project
that references `LeapVR.Content.Util` — including the kiosk transitively
— but the **kiosk does not currently shell out to 7zip**. The kiosk's
`ContainerModule` reads `.vbox` files via `DotNetZip` (since `.vbox` is a
standard ZIP archive). The 7zip CLI is invoked exclusively by the
**Creator wizard** when a user drops a source `.zip`/`.rar`/`.7z` of a
game and the wizard analyzes / extracts it before re-packaging.

## Tech

- **Target framework:** .NET Framework 4.7.1, Library
- **Key NuGet packages:**
  - `NLog` 4.5.11 — logging
  - (No INI parser package referenced here despite the project using INI files — `Util/IniParser.cs` is hand-rolled.)
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib.Win` — `Kernel32Util.DriveFreeBytes`, `ConsoleProcess.Run` (for shelling out to `7z.exe`)
  - `LeapVR.Shell.Domain.Models` — used for `GlobalConfig` access by sibling projects; this one's surface is small.
- **Native dependencies (not NuGet):** 7zip 18.x — `7z.exe`, `7z.dll`, `7-zip.dll`, `7-zip32.dll`, `7z.sfx`, `7zCon.sfx`, plus the full `Lang/*.txt` set. All linked from `..\LeapVR.Content.3rdParty\bin\7zip\` into `BinaryDeps\7zip\`. **The csproj also has a `BinaryDeps/Readme.md`** warning that anything in BinaryDeps is *not* automatically referenced — it relies on the `<Content Include … Link=…>` items to copy to output.

## Responsibility

**It IS responsible for:**

- Wrapping `7z.exe` — listing archive contents (`Archive.Analyze`), extracting with password / filter support and free-disk-space pre-check (`Archive.Extract`).
- Engine detection by directory fingerprint — Unity (`*_Data/Managed,Mono,Plugins,Resources`), Unreal (`Engine/` + `<name>/Binaries/Win64`), CryEngine (`engine/*.pak` + `bin/`), Esenthel (`bin/*.pak`), SDL (root `sdl.dll` / `sdl2.dll`).
- Locating game executables under those layouts, filtering out installers (`setup.exe`, `install.exe`, `uninstall.exe`, `remove.exe`) and generic helper launchers (`launcher_*`, `*_launcher.exe`, `loader.exe`).
- Filtering multipart archives (`*.part1.rar` series, `*.7z.001` series, `*.r01` series) down to the first part for handing to 7zip.
- A few miscellaneous helpers: `DirectoryAnalyzer`, `DirectoryCleaner`, `DuplicateFinder`, `Extractor`, `LookUpFile`, `IniParser`.

**It is NOT responsible for:**

- Building `.vbox` containers (that's `LeapVR.Content.Creator.Logic`).
- Knowing what `.vbox` is — this project has no reference to `LeapVR.Content.Shared`.
- Any UI.

## Public API surface

### Archive layer (`LeapVR.Content.Util.Archive`)

- `class Archive` — constructed with `(FileInfo sevenZipExe, FileInfo archive)`. Properties: `HasError`, `CompressionType Compression`, `ulong PhysicalSize`, `string SevenZipType`, `IReadOnlyList<ArchiveContent> Contents`, `bool ExtractionSuccess`. Methods: `bool Extract(string targetPath, string archivePassword=null, string filter=null)`, `ArchiveReport GetReport()`.
- `class ArchiveContent` — single entry inside an archive (path, sizes, modified date).
- `class ArchiveFile`, `class ArchiveReport` — analysis output.
- `class ContentDirectory` (with nested `RootType { Undefined, Archive, Directory }`) — a recursive directory representation backed by either a real `DirectoryInfo` or an `Archive`. `GameRoot` extends it.

### Game detection (`LeapVR.Content.Util.Game`)

- `class GameInfo` — `EngineType Engine`, `List<ContentFile> GameExes`, `GameRoot Root`. Static factory: `GameInfo ScanRoot(GameRoot)` — runs the engine detectors in order and returns the first match.
- `class GameRoot : ContentDirectory` — `bool IsGameRoot(ignoreUrlFiles, ignoreTxtFiles, ignoreLaunchers)`.

### Utility helpers (`LeapVR.Content.Util.Util`)

- `class DirectoryAnalyzer` — `(sevenZipExe, rootDir, scanDepth=1, lookForGames=true, lookForArchives=true, blacklist=null)`. Raises `FoundEvent` for each game directory or archive found. Static helper `GetAllExe(ContentDirectory, includeSubDirectories=true, filterLauncherLoader=true, filterInstallExes=true)`. `[Flags] enum FolderType { Unknown, CompressedArchive, GameRoot }`.
- `class DirectoryCleaner` — `static bool Clean(DirectoryInfo)`; deletes files matching `Library.FileExclusions`.
- `class DuplicateFinder` — groups `GameInfo` records by exe filename, returns `List<GameFileCollection>`.
- `class Extractor` — orchestrates a 7zip extraction with optional "rebase root" logic (rename the extraction directory to the detected game name when the archive root layout is awkward).
- `static class FilePathNameExtensions` — `T[] SubArray<T>`, `DirectoryInfo.ToContentDirectory()`. Plus `TypeLoaderExtensions.GetLoadableTypes(this Assembly)` (handles `ReflectionTypeLoadException`) and `DictonaryExtensions.MergeWithoutDuplicates<TKey,TValue>` (sic — typo in source).
- `class IniParser` — hand-rolled INI reader/writer (no `INIFileParser` NuGet here despite a similar dependency in sibling projects).
- `class LookUpFile` — generic key→value lookup file with a hard-coded default category set (`Casual`, `Education`, `Music`, `Puzzle`, `Room Escape`, `Shooter`, `Sport`).

### Enums (`LeapVR.Content.Util.Enums`)

- `enum CompressionType { Zip, Rar, SevenZip, Rar5, SplitArchive, ... }`
- `enum EngineType { Unity3D, UnrealEngine4, CryEngine, Esenthel, SDL, Custom, Unknown }`
- `enum SearchMode { ... }`

### Library (`LeapVR.Content.Util.Library`, root)

- Static config holder. Loads `archives.lst`, `excluded.lst`, `renameDir.lst` from `Environment.CurrentDirectory` if present, merging with hard-coded defaults.
- `List<string> ArchivesFileExtension` (default: `.zip .rar .7zip .r01 .7z`), `List<string> FileExclusions` (`.txt .url .nfo .info`), `List<string> DirectoryExclusions` (`game`).

## Internal structure

```
LeapVR.Content.Util/
├── Library.cs                 Static config holder
├── Archive/
│   ├── Archive.cs             7z.exe wrapper (list / extract / parse output)
│   ├── ArchiveContent.cs      One file entry inside an archive
│   ├── ArchiveFile.cs
│   ├── ArchiveReport.cs       Analysis output
│   ├── CompressionType.cs
│   └── ContentDirectory.cs    Recursive directory abstraction (filesystem OR archive)
├── Game/
│   ├── GameInfo.cs            ScanRoot() → engine + main exe detection
│   └── GameRoot.cs            Root with "is this really a game?" heuristics
├── Enums/
│   ├── CompressionType.cs     (duplicate of Archive/CompressionType.cs — see gotcha)
│   ├── EngineType.cs
│   └── SearchMode.cs
├── Util/
│   ├── DirectoryAnalyzer.cs   Scans for archives + game folders, raises FoundEvent
│   ├── DirectoryCleaner.cs
│   ├── DuplicateFinder.cs
│   ├── Extensions.cs          Misc extension methods
│   ├── Extractor.cs           Higher-level extraction wrapper around Archive
│   ├── IniParser.cs           Hand-rolled INI read/write
│   └── LookUpFile.cs
├── BinaryDeps/Readme.md       ⚠ "things included in BinaryDeps are nowhere referenced" — copy-only
└── (Linked) ../LeapVR.Content.3rdParty/bin/7zip/  → BinaryDeps/7zip/
```

## Notable patterns / gotchas

- **`ContentDirectory` straddles both filesystem and archive sources.** `RootType` distinguishes `Directory` vs `Archive`; downstream code (`GameInfo.ScanRoot`) treats both uniformly, so a game can be analysed *without* extracting first.
- **Engine-detection order matters** — `ScanRoot` returns the first match from `Unity → CryEngine → Unreal → Esenthel → SDL`. The Unreal detector is the most permissive (it just needs an `engine/` directory), so it must run after CryEngine to avoid false positives.
- **Two `CompressionType` enums exist** — one in `Archive/CompressionType.cs`, one in `Enums/CompressionType.cs`. The `Archive` namespace is the one the rest of the project uses.
- **`Archive.Analyze` parses 7zip's plain-text `l` output** — looks for the "----" separator, scans for `Type = `, `Total Physical Size = `, `Physical Size = `. The file-listing parser hard-codes column offsets (`line.Substring(0, 38)`, `line.Substring(0, 51)`, `line.Substring(53)`). 7zip output format changes will silently break detection.
- **Free-disk-space check uses `Kernel32Util.DriveFreeBytes`** before calling `7z.exe x`. If the drive lookup fails it returns `false` (i.e. refuses to extract).
- **Multipart archive detection** in `DirectoryAnalyzer.FilterMultiPartArchives` is critical — without it 7zip would be invoked on every `.r01`, `.r02`, `.r03` of the same archive set. Logic: look at the second-to-last `.`-segment for `partN`, or the last segment for numeric `001/002/...` after a `7z`/`zip` segment, or `r##`. Then merge consecutive parts and emit only the first.
- **No `LeapVR.Content.Shared` reference** — this project predates the container format and is reused at install-time on the kiosk to extract the GameFiles package; keeping it `.vbox`-agnostic preserves that reuse.
- **`Library` resolves config paths from `Environment.CurrentDirectory`** — meaning behaviour depends on which working dir the host process started in. The Creator and the kiosk both `Environment.CurrentDirectory` to their install dir very early.

## Consumers

- `LeapVR.Content.Creator` — wizard's import-from-archive flow, game-detection. **Only consumer that actually shells out to `7z.exe`.**
- `LeapVR.Content.Creator.Logic` — *transitively*; doesn't reference Util directly.
- **Kiosk side (`LeapVR.Shell.*`)** — pulls in this project transitively (so the 7zip binaries land in the kiosk install folder), but **does not invoke them**. The kiosk's `ContainerModule` reads `.vbox` files via `DotNetZip`. If you remove unused 7zip binaries to slim the install, verify no future kiosk feature needs them first.

## Related docs

- [`../README.md`](../README.md) — tier overview; explains the Creator-wizard vs kiosk-side distinction for `.vbox` extraction.
- [`../LeapVR.Content.Creator/README.md`](../LeapVR.Content.Creator/README.md) — wizard host that drives `DirectoryAnalyzer`.
