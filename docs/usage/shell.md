# LeapPlay.Shell.exe — Runtime Usage

> Operator-facing reference for running, configuring and troubleshooting the kiosk binary. For the *architecture* of the Shell see `docs/client/LeapVR.Shell/README.md`.

---

## Quick reference

| Command | What happens |
|---------|--------------|
| `LeapPlay.Shell.exe` | Boots the kiosk normally. If no valid disk config exists, the **Setup wizard auto-launches** instead. |
| `LeapPlay.Shell.exe -config` | Forces the Setup wizard (re-configure an already-configured station). |
| `LeapPlay.Shell.exe -uninstall` | Runs the uninstall pre-flow. **Only intended to be invoked by the Inno Setup uninstaller**, not interactively. |
| `LeapPlay.Shell.exe -uninstall -deletegames=true` | Same as above, but also deletes the games storage directory. |
| `LeapPlay.Shell.exe -d` (or `-debug`) | Starts in debug mode — skips the splash screen, otherwise normal. |
| `LeapPlay.Shell.exe -rdbg` | Pops a "Attach your remote debugger and press OK" message box at startup, blocks until OK. |

Flags can be combined (e.g. `-d -rdbg`). Matching is **case-insensitive** and uses *substring contains*, so `--config`, `/config`, etc. are also accepted.

Only **one instance** of `LeapPlay.Shell.exe` can run at a time per machine — enforced via a named mutex (`LeapVR.Shell`). Subsequent launches log "There is already an Instance of this application running!" and exit silently.

---

## Command-line parameters in detail

All parameter names are defined in `LeapVR.Shell.Domain.Models/GlobalConfig.cs` and `LeapVR.Shell.Setup/UninstallOptions.cs`. The matching is `arg.ToLowerInvariant().Contains(<token>)` — a substring match, not a strict equality check. That's why `--config`, `/config`, and `-config` all work.

### `-config`

Token: `-config`.

Forces the Setup wizard (`LeapVR.Shell.Setup.App` with `SetupType.Config`). Use this when the station is already configured but you want to change drives, network credentials, or other settings.

The Setup wizard also runs **automatically** (without this flag) if the on-disk `DiskConfig` is missing or invalid — see *Setup auto-trigger* below.

### `-uninstall`

Token: `-uninstall`.

Runs `LeapVR.Shell.Setup.App` with `SetupType.Uninstall`. Performs the kiosk's pre-uninstall housekeeping:
- Reverts Windows Defender exclusions (only if `-deletegames=true`).
- Removes the scheduled startup task.
- Re-enables Windows Error Reporting.
- Removes the on-disk custom configuration.

This flag is wired by the Inno Setup installer's uninstall flow (see the `[Code]` section in `LeapVR_Shell_Setup.iss`). **Do not run interactively** unless you understand what each step does.

### `-deletegames=<true|false>`

Token: `-deletegames=`. Only meaningful with `-uninstall`. Defaults to `false`.

When `true`:
- The kiosk deletes the games storage directory (`DiskConfig.StorageBaseDir`) along with the kiosk binaries.
- Windows Defender exclusions added by the installer are reverted (because no game files remain to whitelist).

When `false` (or absent), games are preserved so a re-install picks them up.

Parsing happens via `Boolean.TryParse` after stripping `-deletegames=`, so `=True`, `=true`, `=False`, `=false` all work; anything else is treated as `false`.

### `-d` / `-debug`

Tokens: `-d` (short) or `-debug` (long).

Skips the splash screen (one of `splashscreen_{1..4}.png` is normally shown). Otherwise runs normally. Use during development or when troubleshooting startup hangs.

### `-rdbg`

Token: `-rdbg`.

Shows a blocking message box at startup. Hold here, attach your IDE / WinDbg, then click OK to continue. Useful for catching very-early init bugs.

---

## Setup auto-trigger

Even without `-config`, the kiosk routes to the Setup wizard if `ShellConfigurator.HasValidDiskConfig` returns `false`. Validity check (`LeapVR.Shell.Modules/ShellConfigurator/ShellConfigurator.cs`):

1. `DiskConfig.SystemDrives` is non-null and has at least one entry.
2. `DiskConfig.StorageBaseDir` is non-empty.
3. The drive containing `StorageBaseDir` exists and reports `TotalSize > 0`.
4. The directory at `StorageBaseDir` exists (or can be created — the check creates it on the fly).

If any of those fail, the wizard auto-launches and the user is walked through picking a games drive and configuring credentials.

This means: deleting `%APPDATA%\LeapPlay\Config\LeapVR.Shell.Domain.Models.Customization.DiskConfig.json` is a hard reset to "first-run" state.

---

## Filesystem layout at runtime

Two roots. **Binaries** live next to the executable; **data + config + logs** live under `%APPDATA%`.

### `%LEAPVRINSTALLBASEDIRECTORY%\` (binary install root)

Set automatically to the directory containing `LeapPlay.Shell.exe` (defaulted on first run if the env var isn't already defined). Typical install path: `C:\Program Files\Leap Play\`.

```
LeapPlay.Shell.exe
LeapPlay.Content.Creator.exe
*.dll                                  ← managed assemblies
NLog.config                            ← runtime logging config (see "Logging" below)
LeapPlay.Shell.exe.nlog                ← per-environment NLog override (Debug_NLog.config or Release_Nlog.config copied at build time)
en-US/   zh-CN/                        ← satellite localisation assemblies
Media/                                 ← Background.mp4 + any operator-added media
License/                               ← bundled 3rd-party licence texts
vr_desktop/
   vrlounge_desktop.exe                ← in-VR home environment (Unity build)
   vrlounge_desktop_Data/
ffmpeg-related .dll files              ← avcodec-58, avformat-58, etc.
openvr_api.dll, SoundTouch.dll, XInputInterface.dll
```

### `%APPDATA%\LeapPlay\` (per-user persistent data)

Created on first run. Path expands from `GlobalConfig.PersistentDirectory = "%APPDATA%\LeapPlay"`.

```
LeapPlay.db                            ← LiteDB embedded NoSQL store (apps, stats, playlists)
DatabaseBackups/                       ← periodic LiteDB backups
Config/                                ← every JSON config file (see "Config files" below)
FileRepositories/                      ← shipped seeds + runtime file repositories
   OpenVrSettingsSet/
      _default/                        ← OpenVR config baseline
      _original/                       ← snapshot of the user's OpenVR config taken on first run
logs/                                  ← NLog output (deleted on uninstall)
```

If `%APPDATA%` resolves to something unusual (corporate roaming profiles, mandatory profiles), all of the above moves with it. Override at the OS level only.

---

## Config files

All config is stored as **JSON files** under `%APPDATA%\LeapPlay\Config\`. Each file maps 1:1 to a C# class extending `ConfigObject`. Default filename is the **fully-qualified type name** of the config class with `.json` appended:

```
LeapVR.Shell.Domain.Models.Customization.DiskConfig.json
LeapVR.Shell.Domain.Models.Customization.LoginConfig.json
LeapVR.Shell.Domain.Models.Customization.SecurityConfig.json
LeapVR.Shell.Domain.Models.Customization.ServerConfig.json
LeapVR.Shell.Domain.Models.Customization.StationConfig.json
LeapVR.Shell.Domain.Models.Customization.SystemConfig.json
LeapVR.Shell.Modules.FileConfig.OpenVrModuleConfig.json
LeapVR.Shell.Modules.FileConfig.VBoxPlatformModuleConfig.json
LeapVR.Shell.Modules.FileConfig.VrDesktopModuleConfig.json
LeapVR.Shell.Modules.FileConfig.XInputModuleConfig.json
```

Files are read **on demand and cached in memory** (`ConfigFileRepository<T>`). When a file doesn't exist, the kiosk creates one with default values and writes it. When it exists but is missing keys (e.g. a new field added in a later version), the missing keys are silently filled with defaults — see `DiskConfig.Initialize()` for the canonical pattern.

**To force a config reload after manual edit**: restart the kiosk. There is no live-reload.

**To reset a single config to defaults**: delete the corresponding `.json` file.

### `DiskConfig.json` — storage layout

The file the bootstrap depends on. Without a valid one, Setup auto-launches.

```jsonc
{
  "SystemDrives":             ["C:\\"],          // drives Windows / OS lives on (Setup populates)
  "StorageBaseDir":           "D:\\Games\\LeapPlay\\", // root for installed games
  "ReservedDiskSpaceRatio":   0.05,              // keep 5% free; refuse installs that violate this
  "ContentRelativeDirs": {
    "GameFiles":         "app",
    "HardwareTemplates": "hardware_template",
    "MediaFiles":        "media",
    "PufFiles":          "puf",
    "Metadata":          "metadata"
  }
}
```

`ContentRelativeDirs` keys are the `ContentType` enum values — under `StorageBaseDir`, each game gets a subfolder per content type. The defaults are pre-populated; if you add a new `ContentType` in code, `Initialize()` back-fills the missing key.

### `ServerConfig.json` — server connection (gRPC)

```jsonc
{
  "ConnectServerHost":       "connect.example.com",          // initial connect endpoint
  "ConnectServerPort":       443,
  "ServiceRootCert":         "License\\ca.crt"               // optional CA for a self-signed server cert
}
```

The host this points at is the `ServiceConnectHost` gRPC server — that endpoint redirects the kiosk to a `ServiceShellHost` for the actual session traffic.

Notes:

1. **`ServerConfig.json` is now live.** `Bootstrapper.cs` registers `ConfigFileRepository<ServerConfig>` (the file-backed config), not the historical `StaticServerConfig`. Edit this file to point a kiosk at your server. Defaults to `localhost:443`.

2. **`ServiceRootCert` resolves relative to `%APPDATA%\LeapPlay\`**, not the install directory. Drop the CA PEM at `%APPDATA%\LeapPlay\License\ca.crt` if your server uses a self-signed cert; for Let's Encrypt-issued certs leave the field unset (the kiosk trusts the Windows certificate store).

3. **Session authentication.** The kiosk authenticates each gRPC call by attaching `(stationId, password)` as gRPC **CallCredentials** via `GrpcChannelCredentialsHandler.SetChannelCredentials(stationId, password)` (`RemoteServiceFactory.cs:94-98`). The server reads those two headers and PBKDF2-verifies the password against `Station.PasswordHash`. See `docs/architecture/auth.md` § Scheme #3. The same kiosk also holds a `StationApiKey` `(PublicKey, Secret)` pair for its REST calls to `StationController` (scheme #2) — two channels, two secrets.

4. **Cert-based station-licensing is gone.** Earlier internal builds had a per-station x509 license cert mechanism (`ClientCertificateChain`, `PrivateKey`, `LeapCertLicense`, `ILeapCertLicense`, `ClientIdentity`). All of that was excised before v1.0.0; the `(StationApiKey.PublicKey, Secret)` model fully replaces it.

### `StationConfig.json` — station behavior

```jsonc
{
  "DefaultStationMode":         "Screen",   // StationMode enum: Screen | Vr | Hybrid (?)
  "ForceVrDriverRestart":       true,       // restart OpenVR runtime on session start
  "DisableVrDriverInteraction": true        // suppress SteamVR's own UI overlays
}
```

### `LoginConfig.json` — autologin / station credentials

```jsonc
{
  "StationId": null,        // Guid as string — station identity at the server side
  "Password":  null,        // Station password (or PIN)
  "AutoLogin": false        // skip the login screen at boot
}
```

⚠️ Setting `AutoLogin: true` with a real password committed to disk in plaintext is convenient but obviously a security trade-off. The Setup wizard prompts for these.

### `SecurityConfig.json` — admin / inactivity

```jsonc
{
  "SecurityCode":           "000000",   // PIN to enter the operator/admin area on the kiosk
  "SystemInactivityTimeout": "00:00:30" // 30s — TimeSpan format
}
```

`SystemInactivityTimeout` is read-only in the C# (no public setter) — to change, edit the JSON. Default 30 seconds.

### `SystemConfig.json` — OS integration / language

```jsonc
{
  "LeapVRTaskName":               "Start LeapPlay",        // scheduled-task name (set by Setup)
  "DefaultLanguage":              "en-US",
  "SupportedLanguageCultureNames": ["zh-CN", "en-US"]
}
```

### `OpenVrModuleConfig.json` — VR runtime

Controls how the kiosk talks to SteamVR / OpenVR. Most fields are constants the operator should never need to change.

```jsonc
{
  "IsOpenVrConfigReplacingEnabled": true,
  "IsOpenVrConfigReplaced":         false,   // kiosk-managed flag (don't edit)
  "OriginalOpenVrSettingsName":     "_original",
  "DefaultOpenVrSettingsName":      "_default",
  "VrMonitorProcessName":           "vrmonitor",
  "VrServerProcessName":            "vrserver",
  "VrCompositorProcessName":        "vrcompositor",
  "VrMonitorStartCommand":          "vrmonitor://compositor",
  "VrMonitorStopCommand":           "vrmonitor://quit",
  "VrMonitorConfigFilePath":        "%LOCALAPPDATA%\\openvr\\openvrpaths.vrpath",
  "VrMonitorLogFileName":           "vrmonitor.txt",
  "VrMonitorProcessNamesToExit":    ["vrmonitor", "vrcompositor", "vrserver", "vivelink"],
  "ConfigFilesDetails":             [ /* OpenVR file overrides — see source */ ]
}
```

The `_default/` and `_original/` folders mentioned above live under `%APPDATA%\LeapPlay\FileRepositories\OpenVrSettingsSet\`.

### `VrDesktopModuleConfig.json` — in-VR home

```jsonc
{
  "VrDesktopExecutableParameters":     "-popupwindow",
  "VrDesktopExecutableRelativeFilePath": "vr_desktop\\vrlounge_desktop.exe",
  "VrDesktopProcessName":              "vrlounge_desktop"
}
```

The path is relative to the install dir — points at the Unity-built environment binary that ships with the installer.

### `XInputModuleConfig.json` — gamepad

```jsonc
{
  "XInputDevicePollingPerSecond":                    25,
  "XInputDeviceResendDelay":                         700,   // ms before an autorepeat starts
  "XInputDeviceResendEachMs":                        200,   // ms between autorepeats
  "XInputCompositeButtonsForForceQuitingApps":       "Start,Back",
  "MillisecondsToHoldBeforeCompositeButtonsTakeEffect": 6000,
  "MillisecondsToWaitBeforeCompositeButtonsThrottleReOpen": 5000
}
```

The "force quit" combo (Start+Back held for 6s) is the operator's emergency exit during a session.

### `VBoxPlatformModuleConfig.json` — VBox app launcher

```jsonc
{
  "IsTerminateNotRespondingGamesEnabled": true,
  "MaximumLoopCountBeforeExecutionDetected": 30,
  "LoopIntervalBeforeExecutionDetected":     1000, // ms
  "TimeoutBeforeResumingFromNotResponding":  6000  // ms
}
```

Tunes the game-launch watchdog: how long to wait for a launched executable to actually start, and how long to wait before deciding a "not responding" process should be killed.

---

## Logging

NLog is the kiosk's logger.

- Configuration files (next to the executable):
  - `NLog.config` — base config (rarely present in installed builds — usually stripped during install staging).
  - `LeapPlay.Shell.exe.nlog` — overrides (`Release_Nlog.config` or `Debug_NLog.config` copied here at build time, depending on configuration).
- Output: `%APPDATA%\LeapPlay\logs\` (created on first log).
- Lifetime: log files are **deleted by the uninstaller** (the .iss `[UninstallDelete]` section). Back them up before uninstalling if you need the history.
- Debug builds log more verbosely; that's purely an NLog configuration choice — no separate code paths.

---

## Common operator tasks

### Re-pair a station to a different server

1. Edit `%APPDATA%\LeapPlay\Config\LeapVR.Shell.Domain.Models.Customization.ServerConfig.json` → set new `ConnectServerHost` / `ConnectServerPort` / `ServiceRootCert`.
2. Restart `LeapPlay.Shell.exe`.

### Wipe local credentials (force fresh login at boot)

1. Delete `LeapVR.Shell.Domain.Models.Customization.LoginConfig.json`.
2. Restart.

### Change games drive

1. Run `LeapPlay.Shell.exe -config` → walk through the wizard → pick the new drive.
2. Or: edit `DiskConfig.json` directly (`StorageBaseDir`) and restart. Existing games at the old path are not migrated; copy them yourself first.

### Reset everything to first-run state

1. Delete `%APPDATA%\LeapPlay\Config\` entirely.
2. Restart. Setup auto-launches.

### Get the kiosk into a debugger before main loop starts

```
LeapPlay.Shell.exe -rdbg
```

Attach IDE → click OK on the message box.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| Kiosk closes immediately, nothing visible. Log: "There is already an Instance of this application running!" | Existing instance holds the `LeapVR.Shell` mutex. | Kill `LeapPlay.Shell.exe` in Task Manager (also check `vrlounge_desktop.exe`) and retry. |
| Setup wizard launches every time. | `DiskConfig` invalid (missing drive, deleted folder, broken JSON). | Open the wizard, re-pick a drive. Check that `StorageBaseDir` exists and is on a present drive. |
| Splash screen never closes. | Bootstrapper threw during init. | Check `%APPDATA%\LeapPlay\logs\` for the latest log. |
| Connecting to wrong server after a config change. | Config is cached — needs a restart. | Restart the kiosk; check `ConfigFileRepository.Get()` doesn't have stale memory. |
| "Restart" exit code triggers a relaunch. | Component requested a restart via `TerminationSignal.Restart`. | This is expected behaviour — the launcher reuses the original command-line. Logged as "Application Throw during attempt of restart" if it fails. |
| Games drive change leaves orphan files. | Config edit doesn't migrate. | Move the games folder yourself before changing the path; or use the Setup wizard which has a migration option. |
| No log files appear under `%APPDATA%\LeapPlay\logs\`. | NLog config missing or invalid. | Verify `LeapPlay.Shell.exe.nlog` exists next to the executable; check it for syntax errors. |

---

## Related docs

- `docs/client/LeapVR.Shell/README.md` — architecture (Bootstrapper, IoC, view hierarchy).
- `docs/client/LeapVR.Shell.Setup/README.md` — what the Setup wizard actually does step by step.
- `docs/architecture/auth.md` — how the kiosk's gRPC traffic is authenticated.
- `docs/architecture/session-lifecycle.md` — what happens once the kiosk is connected.
- `docs/architecture/build-and-deploy.md` — installer layout and runtime prerequisites.
