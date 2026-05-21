# LeapVR.Shell.Repository

> LiteDB-backed local persistence for the kiosk: app installation records, app display info, app statistics, app platform/account state, multimedia playlists & settings, hardware-device templates, stored packages. One file per station: `%APPDATA%\LeapPlay\LeapPlay.db`.

## Purpose

This project owns *everything* that survives a process restart on the kiosk side, **except** JSON-on-disk module configs (those live in `LeapVR.Shell.Modules/ConfigFileRepository.cs`). It uses [LiteDB](https://www.litedb.org/) 4.x — a single-file embedded NoSQL document store — accessed through a static `Database` facade with a process-wide lock. Per-collection repositories implement the interfaces declared in `LeapVR.Shell.Repository.Interfaces` and convert between domain types (interfaces in `LeapVR.Shell.Domain.Models`) and on-disk DB types (`Entities/*Db.cs`).

Schema migration is supported through `Database/Migrations/` — invoked at startup by the static `Database` initialiser when an existing DB file is opened.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Repository.dll`)
- **Key NuGet packages:**
  - `LiteDB` 4.1.2 — embedded NoSQL store (loaded from `..\packages\LiteDB.4.1.2\lib\net40\LiteDB.dll`)
  - `NLog` 4.5.11 — logging
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`
  - `LeapVR.Shell.Domain.Models`
  - `LeapVR.Shell.Repository.Interfaces`

## Responsibility

**It IS responsible for:**
- Owning the LiteDB connection lifecycle (`Database/Database.cs` — static, locked, lazily-initialised).
- Backing up the DB file on schema migration (`Database/DatabaseBackup.cs` → `DatabaseBackups/`).
- Running migrations from `Database/Migrations/` on startup.
- Reading/writing every entity collection.
- Mapping DB-row types (`Entities/*Db.cs`) ↔ domain interfaces (`LeapVR.Shell.Domain.Models`).
- Per-repo error mapping into `RepositoryGetDbException` / etc. (`Exception/Exceptions.cs`).
- Two cross-cutting helpers used by other tiers: `GenericFileRepository<T>` (file-system blob storage, distinct from `ConfigFileRepository<T>`) and `GenericCacheProvider` / `GenericCache<T>` (in-memory cache used by `IGenericCacheProvider`).

**It is NOT responsible for:**
- Module configuration (JSON via `ConfigFileRepository<T>` in `LeapVR.Shell.Modules`).
- Any server-side persistence (Postgres + EF Core lives in `Pod.Data`).
- Schema definitions for Pod.* tables — these are *separate* collections inside the same LiteDB file but are kiosk-local.

## Public API surface

Every concrete repository is registered as a singleton in `LeapVR.Shell/Bootstrapper.cs#RegisterConcreteRepositories`.

| Class | Interface (in `.Interfaces`) | Collection contents |
|---|---|---|
| `AppDisplayRepository` | `IAppDisplayRepository` | `AppDisplayDataDb` — UI-facing app metadata (name, sort order, icon path, hidden flag). |
| `AppInstallationRepository` | `IAppInstallationRepository` | `AppInstallationDataDb` — install state per `ApplicationGuid`/`PlatformPluginGuid`. Backbone of "what is installed". |
| `AppPlatformRepository` | `IAppPlatformRepository` | `AppPlatformDataDb` — known platforms (VBox, Steam) and their per-station state. |
| `AppPlatformAccountRepository` | `IAppPlatformAccountRepository` | `PlatformAccountDataDb` — Steam-account credentials etc. |
| `AppStatisticsRepository` | `IAppStatisticsRepository` | `AppStatisticsDataDb` — per-app cumulative play time / launch counts. |
| `MultimediaPlaylistRepository` | `IMultimediaPlaylistRepository` | `MultimediaPlaylistDataDb` — saved playlists. |
| `MultimediaSettingsRepository` | `IMultimediaSettingsRepository` | `MultimediaSettingsDataDb` — current playlist selection + multimedia tunables. |
| `StoredPackageRepository` | `IStoredPackageRepository` | `StoredPackageDataDb` — `.vbox` packages staged on disk awaiting install. |
| `HardwareDeviceTemplateRepository` | `IHardwareDeviceTemplateRepository` (lives in `LeapVR.Shell.Modules.Interfaces`) | `HardwareDeviceDataTemplateDb` — known-good hardware-device fingerprints. |
| `OpenVrSettingsSetRepository` (lives under `LeapVR.Shell.Modules/Vr/`) | `IOpenVrSettingsSetRepository` | Saved VR settings sets (this repo lives in the modules project for namespace reasons, but uses the same DB). |

Cross-cutting helpers:

| Class | Interface | Notes |
|---|---|---|
| `GenericRepository` (abstract base) | — | Shared synchronous CRUD shape. |
| `GenericFileRepository<T>` | `IGenericFileRepository<T>` | Persists items as discrete files (e.g. binary blobs) under `IGlobalConfiguration.PersistentDirectory`. Distinct from `ConfigFileRepository`. |
| `GenericCache<T>` + `GenericCacheProvider` | `IGenericCache<T>`, `IGenericCacheProvider` | In-memory caches keyed by type; injected wherever expensive lookups happen (e.g. `AppInfoProcessor`). |
| `ServiceConfigFileRepository` | (concrete) | Specialised file-config helper for service configs that the modules' generic repo isn't a fit for. |
| `EntityConverter` | (static) | Domain ↔ DB row conversion centralised. |

The static `Database` facade exposes `QueryDatabase<TResult, TEntity>(Func<LiteCollection<TEntity>, TResult> action)` — every repository is just thin error-mapping around this method.

## Internal structure

```
LeapVR.Shell.Repository/
├── Database/
│   ├── Database.cs                     Static LiteDB facade — connection, lock, init, migration trigger
│   ├── DatabaseBackup.cs               Pre-migration backup into DatabaseBackups/
│   ├── DBFileInfo.cs                   File-info shape used by migrations
│   └── Migrations/                     Versioned migration steps
├── Entities/                           DB row types — *Db.cs files mirror Domain.Models contracts
│   ├── AppDisplayDataDb.cs / AppHardwareDataDb.cs / AppInstallationDataDb.cs
│   ├── AppPlatformDataDb.cs / AppStatisticsDataDb.cs
│   ├── DiskEntityDb.cs / HardwareDeviceDataTemplateDb.cs
│   ├── MultimediaPlaylistDataDb.cs / MultimediaSettings.cs / MultimediaSettingsDataDb.cs
│   ├── PlatformAccountDataDb.cs / ProcessExecutionLogicDb.cs / ProcessMonitorInstructionDb.cs
│   ├── StoredPackageDataDb.cs
│   └── EntityConverter.cs              Static converters (DB ↔ domain)
├── Exception/Exceptions.cs              RepositoryGetDbException, RepositoryStoreDbException, …
├── AppDisplayRepository.cs              One file per repository
├── AppInstallationRepository.cs
├── AppPlatformRepository.cs
├── AppPlatformAccountRepository.cs
├── AppStatisticsRepository.cs
├── HardwareDeviceTemplateRepository.cs
├── MultimediaPlaylistRepository.cs
├── MultimediaSettingsRepository.cs
├── ServiceConfigFileRepository.cs
├── StoredPackageRepository.cs
├── GenericRepository.cs                 Shared base
├── GenericFileRepository.cs             File-blob persistence
├── GenericCache.cs / GenericCacheProvider.cs   In-memory caches
├── Properties/AssemblyInfo.cs
├── packages.config / app.config
└── LeapVR.Shell.Repository.csproj
```

## Notable patterns / gotchas

- **Static `Database` is process-wide and locked.** All access flows through `Database.QueryDatabase(Func<LiteCollection<T>, R>)`. The single `DBLock` serialises every operation. This is fine for kiosk traffic but expect contention if you start parallelising.
- **DB file location is computed lazily** from `GlobalConfig.GetGlobalConfiguration().DatabaseFilePath`. That path resolves to `%APPDATA%\LeapPlay\LeapPlay.db` (see `GlobalConfig.DbFileName` + `PersistentDirectory` constant). The wizard creates the parent directory.
- **`isNewDb = !File.Exists(_databaseFile)` drives migration vs init.** A new DB skips migrations and just installs the latest schema. An existing DB is run through `Database/Migrations/` after a backup snapshot.
- **`BsonMapper` is exposed publicly** via `Database.Mapper` so other code can register custom field mappings (used by some converters).
- **Every read wraps `LiteDB.LiteException` and rethrows as `RepositoryGetDbException`** with a `nameof(method)` prefix — error messages tell you the failing method without a debugger. Preserve this pattern when adding new repos.
- **Closing the DB is one-way** (`_isClosed = true`). There's no reopen. The kiosk treats DB closure as a terminal failure.
- **Entity `*Db.cs` types implement the corresponding `Domain.Models` interface** so consumers can iterate `IEnumerable<IAppInstallationData>` even though the underlying objects are `AppInstallationDataDb`. Don't expose `*Db` types outside this project.
- **`MultimediaSettings.cs` (no `Db` suffix)** is intentional — it's a non-persisted helper used by the settings repository. Don't conflate with `MultimediaSettingsDataDb.cs`.

## Consumers

- `LeapVR.Shell` — registers every repository in `Bootstrapper.RegisterConcreteRepositories`.
- `LeapVR.Shell.Controllers` — `PlatformController`, `StatisticsController`, `AppInfoProcessor` directly inject the repos.
- `LeapVR.Shell.Modules` — `MultimediaProvider`, `PlaylistModule` consume the multimedia repos; `OpenVrSettingsSetRepository` (which lives in modules) calls into the same `Database` facade.
- `LeapVR.Shell.Setup` — uses `AppInstallationRepository`, `AppPlatformRepository`, `AppPlatformAccountRepository`, `StoredPackageRepository` during uninstall.

## Related docs

- Sister contract: [`LeapVR.Shell.Repository.Interfaces`](../LeapVR.Shell.Repository.Interfaces/README.md)
- Closely related: [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md) (entity domain interfaces), [`LeapVR.Shell.Modules`](../LeapVR.Shell.Modules/README.md) (multimedia + OpenVR settings consumer)
- Tier overview: [`docs/client/README.md`](../README.md)
- Architecture: `docs/architecture/data-model.md` (planned — server-side EF schema; the kiosk-side LiteDB collections live here).
