# LeapVR.Shell.Repository.Interfaces

> Pure contracts for every kiosk-side LiteDB repository, plus the small set of entity-shape interfaces that live exclusively in the persistence layer (`AppDisplayData`, `AppHardwareData`, `AppStatisticsData`, `PlatformAccountData`, `IDbFileInfo`).

## Purpose

This is the consumer-facing contract for `LeapVR.Shell.Repository`. Every project that needs to read/write kiosk-local persistence references this — not the implementation. The split keeps controllers and modules from accidentally pulling in LiteDB or any other persistence detail.

It also owns a few entity shapes that don't belong in `LeapVR.Shell.Domain.Models` because they are *only* meaningful in the persistence boundary (e.g. `IDbFileInfo` describes a tracked DB file; `AppHardwareData` is the persisted form of hardware-fingerprint info). Domain-tier interfaces (`IAppInstallationData`, `IMultimediaPlaylist`, `IMultimediaPlaylistData`) live in `LeapVR.Shell.Domain.Models` or `LeapVR.Shell.Modules.Interfaces`; this project re-exposes them through repo interfaces.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Repository.Interfaces.dll`)
- **Key NuGet packages:** none — pure contracts.
- **Project references (in this repo):**
  - `LeapVR.Shell.Domain.Models`
  - `LeapVR.Shell.Modules.Interfaces`

## Responsibility

**It IS responsible for:**
- One `I*Repository` per persisted collection.
- Persistence-only entity shapes that have no meaning outside the DB layer.
- The `IRepository`, `IGenericRepository`, `IGenericFileRepository`, `IGenericCache`, `IGenericCacheProvider` cross-cutting contracts.

**It is NOT responsible for:**
- Implementations (in `LeapVR.Shell.Repository`).
- Domain entity contracts (in `LeapVR.Shell.Domain.Models` and `LeapVR.Shell.Modules.Interfaces`).
- Module-private persistence (`IConfigFileRepository<T>`, `IOpenVrSettingsSetRepository`, `IHardwareDeviceTemplateRepository` — those live in `LeapVR.Shell.Modules.Interfaces` because they're owned by modules).

## Public API surface

### Repositories (`Interfaces/`)

| Interface | Stores | Implementation |
|---|---|---|
| `IAppDisplayRepository` | UI-facing app display info (sort order, icon path, hidden flag) | `AppDisplayRepository` |
| `IAppInstallationRepository` | App-install records keyed by `ApplicationGuid` + `PlatformPluginGuid` | `AppInstallationRepository` |
| `IAppPlatformRepository` | Per-platform state (VBox, Steam) | `AppPlatformRepository` |
| `IAppPlatformAccountRepository` | Platform-account credentials | `AppPlatformAccountRepository` |
| `IAppStatisticsRepository` | Per-app cumulative play time / launch counts | `AppStatisticsRepository` |
| `IAppFileRepository` | Generic per-app blob store (icons, screenshots) | (file-system backed) |
| `IMultimediaPlaylistRepository` | Saved playlists | `MultimediaPlaylistRepository` |
| `IMultimediaSettingsRepository` | Multimedia tunables + active playlist | `MultimediaSettingsRepository` |
| `IStoredPackageRepository` | `.vbox` packages staged on disk | `StoredPackageRepository` |
| `IRepository` | Marker | — |
| `IGenericRepository` | Synchronous CRUD shape | `GenericRepository` |
| `IGenericFileRepository` | File-blob persistence | `GenericFileRepository` |
| `IGenericCache` / `IGenericCacheProvider` | In-memory cache contract | `GenericCache<T>` / `GenericCacheProvider` |
| `IMultimediaPlaylistData` | Playlist-data shape (used by the multimedia repository contract) | (DTO only) |

### Entity shapes (`Entities/`)

| Type | Notes |
|---|---|
| `IEntity` | Marker — has `Id` etc. for LiteDB-managed types. |
| `IDbFileInfo` | Describes a DB file the migration system tracks. |
| `AppDisplayData` | DTO for UI display info. |
| `AppHardwareData` | DTO for hardware fingerprint info. |
| `AppStatisticsData` | DTO for per-app statistics. |
| `PlatformAccountData` | DTO for platform-account credentials. |

(Note: these entity shapes here are *DTOs*, not interfaces — they are intentionally non-polymorphic because they cross only this single persistence boundary.)

## Internal structure

```
LeapVR.Shell.Repository.Interfaces/
├── Entities/                Persistence-only shapes (IEntity, IDbFileInfo, *Data DTOs)
├── Interfaces/              I*Repository contracts + IGenericCache + IRepository + IGenericFileRepository + IMultimediaPlaylistData
├── Properties/AssemblyInfo.cs
└── LeapVR.Shell.Repository.Interfaces.csproj
```

## Notable patterns / gotchas

- **Module-tier persistence is *not* here.** `IConfigFileRepository<T>`, `IOpenVrSettingsSetRepository`, and `IHardwareDeviceTemplateRepository` live in `LeapVR.Shell.Modules.Interfaces` — they're owned by modules, not by the kiosk-wide repository tier. Don't move them.
- **DTOs vs interfaces.** Repository contracts return `IAppInstallationData` (interface, in `Domain.Models`) but `AppDisplayData` / `AppHardwareData` etc. (DTO classes, in `Entities/`). The mix is historical: domain interfaces flow up into UI; persistence-only DTOs don't need polymorphism.
- **No methods take a `LiteDB.*` type.** That's the contract: nothing leaks. If you find yourself wanting to expose a `Query<T>` from LiteDB through one of these interfaces, refactor instead.

## Consumers

- `LeapVR.Shell` — `Bootstrapper.RegisterConcreteRepositories` binds every interface here to its implementation.
- `LeapVR.Shell.Controllers` — `PlatformController`, `StatisticsController`, `AppInfoProcessor`, and others inject these interfaces directly.
- `LeapVR.Shell.Modules` — `MultimediaProvider`, `PlaylistModule` consume the multimedia repos.
- `LeapVR.Shell.Setup` — references for the uninstall flow (drives `IPlatformController`, which uses these underneath).

## Related docs

- Sister implementation: [`LeapVR.Shell.Repository`](../LeapVR.Shell.Repository/README.md)
- Closely related: [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md) (domain-entity interfaces), [`LeapVR.Shell.Modules.Interfaces`](../LeapVR.Shell.Modules.Interfaces/README.md) (module-private repos)
- Tier overview: [`docs/client/README.md`](../README.md)
