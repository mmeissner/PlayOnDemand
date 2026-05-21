# LeapVR.Utilities.Steam

> Steam library / appinfo scanner, Valve VDF parser, and a `SteamSelf`
> lifecycle helper for starting and quitting the Steam client.

## Purpose

Powers the kiosk's **Steam platform** module (`LeapVR.Shell.Modules/Platform/`).
At startup and on demand, the client needs to:

1. Discover which Steam libraries the user has (paths and which apps are in each
   one) — by reading `libraryfolders.vdf` and `appcache/appinfo.vdf` from
   the Steam install directory.
2. Translate Steam's binary VDF blobs into structured `SteamAppInfo` objects
   (display name, install dir, launch options, OpenVR support, supported OS)
   so the kiosk can show Steam games next to its own VBox containers.
3. Fetch additional metadata from the public Steam Web API (store details,
   media) via the vendored `SteamWebAPI2` library — used as a fallback when
   local appinfo cache is incomplete.
4. Drive the Steam client process itself: launch with credentials, wait for
   login confirmation (by polling for window classes like
   `BootstrapUpdateUIClass` and `vguiPopupWindow`), and quit it cleanly with
   `-shutdown`.

Despite the `LeapVR.Utilities.*` naming, this project sits one layer above the
shared base — it pulls in client-tier domain models for its return types.

## Tech

- **Target framework:** `.NET Framework 4.7.1`
- **Platforms:** `AnyCPU`, `x64`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Key NuGet packages:**
  - `NLog` 4.5.11 — logging
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib.Win` (WMI, ConsoleProcess, virtual keyboard)
  - `LeapVR.Utilities.Windows` (process management, registry)
  - `LeapVR.Shell.3rdParty/Steam.Models/Steam.Models.Net452` (vendored DTOs)
  - `LeapVR.Shell.3rdParty/SteamWebAPI2/SteamWebAPI2.Net452` (vendored client)
  - `LeapVR.Shell.Domain.Models` (`OsSupport`, `SteamAppLaunchInfo`, account models)
  - `LeapVR.Shell.Modules.Interfaces`
  - `Pod.Data.Infrastructure` (`Result<T>` pattern)

## Responsibility

It IS responsible for:

- Discovering the Steam install path (registry: `HKCU\Software\Valve\Steam`).
- Parsing Steam's text and binary VDF formats (the `Steam/VDF/` namespace).
- Enumerating installed apps (`SteamLib.AppInstallDirectories`,
  per-library `appmanifest_<id>.acf` reading).
- Returning structured `SteamAppInfo` blocks per app id.
- Querying the Steam Web API for store metadata (`SteamAppStoreInfo`).
- Starting / monitoring / quitting the Steam client (`SteamSelf`).
- Tracking SteamVR specifically (`SteamVRAppId = 250820`) so the kiosk can
  detect/install the runtime.

It is NOT responsible for:

- Running games — that's `LeapVR.Shell.Modules/Platform/`.
- Knowing what a "session" is — domain logic stays in the kiosk.
- Persisting Steam credentials — the `_username/_password` pair is passed in
  by the caller, not stored here.

## Public API surface

| Type | Purpose |
|------|---------|
| `SteamLib` | Static-state singleton-ish facade. `IsAvailable`, `SteamPath`, `AppInstallDirectories`, async methods to enumerate apps and resolve store info. Initialised lazily on first `IsAvailable` access. |
| `SteamAppInfo` | Per-app DTO: `AppId`, `DisplayName`, `InstallDirName`, `AppDirectory`, `LaunchInfo` (list), `OpenVrSupport`, `SupportedOs : OsSupport`, `IsValid`, `IsInstalled`. Built from a parsed `VdfData`. |
| `SteamAppStoreInfo` | Wraps `Steam.Models.SteamStore.StoreAppDetailsDataModel` from the Web API. |
| `SteamGame` | Lightweight per-game record. |
| `SteamSelf` | Starts Steam with given credentials (`-login`, `-applaunch`), monitors window classes for login state, can `Quit()` via `-shutdown`. Holds `_loginConfirmed` / `_steamExited` / `_quitRequested` `volatile bool`s for cross-thread coordination. |
| `AppState`, `StartState` | Enums describing app installation & startup lifecycle. |
| `Steam.VDF.VDFFile`, `VDFElement`, `NestedElement`, `NestedElementFile`, `SteamConfigFile`, `SteamAccount` | Text VDF reader/writer. |
| `Steam.VDF.Binary.AppinfoDecoder`, `AppinfoEncoder`, `VdfTxtDecoder`, `VdfTxtEncoder`, `VdfData`, `VDFBinaryUtils` | Binary VDF (`appinfo.vdf`) reader/writer. |

## Internal structure

```
LeapVR.Utilities.Steam/
└── Steam/
    ├── SteamLib.cs               ← entry point: discovery + enumeration
    ├── SteamSelf.cs              ← run / monitor / quit the Steam client
    ├── SteamAppInfo.cs           ← per-app DTO
    ├── SteamAppStoreInfo.cs      ← Web API store DTO wrapper
    ├── SteamGame.cs              ← lightweight per-game record
    ├── AppState.cs / StartState.cs   ← enums
    └── VDF/
        ├── VDFFile.cs / VDFElement.cs / NestedElement*.cs
        ├── SteamAccount.cs / SteamConfigFile.cs
        └── Binary/
            ├── AppinfoDecoder.cs / AppinfoEncoder.cs
            ├── VdfTxtDecoder.cs / VdfTxtEncoder.cs
            ├── VdfData.cs
            └── VDFBinaryUtils.cs
```

## Notable patterns / gotchas

- **`SteamLib` uses static state** (`_initialized`, `_appInstallDirectories`,
  `_steamPath`, `_steamStore`, `_isAvailable`). Constructing an instance just
  triggers `Initialize()` if not yet done. There's no thread-safe init lock —
  callers are expected to call once during boot. The `volatile bool _isAvailable`
  hints at a known race.
- **VDF parsing is hand-rolled.** Steam's binary format is undocumented. The
  parser handles only the dialect Valve was shipping at the time the code was
  written; new field types in newer Steam versions show up as parse failures.
  The `Logger.Trace("Analyze VdfData for game:{gamedData}",gameData.LogJson())`
  call (sic, typo "gamedData") was the canonical debug entry point when this
  broke last.
- **`SteamSelf` polls window classes** (`BootstrapUpdateUIClass` for the
  bootstrapper, `vguiPopupWindow` for app updaters) — Valve renames these
  unannounced; expect to tweak.
- **`SteamSelf` uses `LeapVR.Shared.Lib.Win.VirtualKeyboard`** to send synthetic
  keystrokes into the Steam window during automated login flows.
- **Web API calls have a 10-second timeout** (`WebRequestTimeout`); they fail
  silently into `null` on timeout (logged at `Warn`).
- **Once Steam has been started and exited**, `SteamSelf` cannot restart it
  in the same process — the class comment explicitly warns about this.
- **`SteamVRAppId = 250820`** is hard-coded — the kiosk treats this app id
  specially for VR runtime detection.

## Consumers

- `LeapVR.Shell` (the kiosk; resolves `SteamLib` via SimpleInjector at boot)
- `LeapVR.Shell.Modules` (the Steam Platform module)

Not consumed by Content Creator, Setup, or any `Pod.*` server project.

## Related docs

- [shared tier overview](../README.md)
- [`docs/client/README.md`](../../client/README.md) for the Platform module
  registration.
- [`docs/3rdParty/README.md`](../../3rdParty/README.md) for the vendored
  `Steam.Models` and `SteamWebAPI2` projects.
