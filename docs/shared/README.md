# Shared / Utility tier

> Cross-process libraries: helpers, WinAPI wrappers, WPF converters, Steam scanning,
> Windows automation, and a build-time CLI for stamping versions.

This folder collects every project under the repo whose namespace begins with
`LeapVR.Shared.*` or `LeapVR.Utilities.*`. None of them are deployable artefacts
on their own; they are linked into the kiosk (`LeapVR.Shell`), the authoring tool
(`LeapVR.Content.Creator`), or — in the case of `LeapVR.Utilities.VersionInfo`
— invoked from the build script.

The server (`Pod.*`) does **not** depend on any of these libraries. Cross-tier
sharing happens via gRPC contracts (`Pod.Grpc.*`) and DTO assemblies (`Pod.DtoModels`),
not via these shared libs.

---

## What lives where

| Project | Target | Platform | Role |
|---------|--------|----------|------|
| `LeapVR.Shared.Lib` | `netstandard2.0` | cross-platform | Pure C# helpers: collections, sanity rules, expressions, x509/RSA. No WinAPI, no UI. |
| `LeapVR.Shared.Lib.Win` | `net471` | Windows | WinAPI wrappers (User32/GDI32/PsApi/Kernel32), `SingleInstanceGuard`, image processing, virtual keyboard / mouse, WMI, taskbar, DPI. |
| `LeapVR.Shared.Lib.Wpf` | `net471` (WPF) | Windows + WPF | `IValueConverter` set, visual-tree helpers, `EnumBindingSourceExtension`, `LocalizedDescriptionAttribute`. |
| `LeapVR.Utilities.Steam` | `net471` | Windows | Steam library/appinfo scanner, VDF binary+text parser, SteamWebAPI2 wrapper, `SteamSelf` lifecycle helper. |
| `LeapVR.Utilities.Windows` | `net471` | Windows | Process management, registry, USB device-arrival window, file utilities, Task Scheduler, AES-string cipher, JSON helper. |
| `LeapVR.Utilities.VersionInfo` | `net471` (Exe) | Windows | Build-time CLI: prints the `AssemblyVersion` of a managed `.exe`/`.dll` to stdout. Used by `Build.bat` / `Build_Free.bat`. |

---

## Consumer matrix

| Library | Kiosk (`LeapVR.Shell.*`) | Content Creator (`LeapVR.Content.*`) | Setup wizard | Server (`Pod.*`) |
|---------|:---:|:---:|:---:|:---:|
| `LeapVR.Shared.Lib`             | yes | yes | yes | no |
| `LeapVR.Shared.Lib.Win`         | yes | yes | no  | no |
| `LeapVR.Shared.Lib.Wpf`         | yes | yes | yes | no |
| `LeapVR.Utilities.Windows`      | yes | yes | yes | no |
| `LeapVR.Utilities.Steam`        | yes | no  | no  | no |
| `LeapVR.Utilities.VersionInfo`  | called by build script (extracts version stamp from compiled `LeapPlay.Shell.exe`) | | | |

`LeapVR.Shared.Lib` is the only `netstandard2.0` library here — the rest are
strictly `net471 / Windows`. Content Creator + Kiosk re-use the bottom three rows
heavily; the Setup wizard stays small and only references `Lib` + `Lib.Wpf`.

---

## Internal dependency direction

```
                LeapVR.Shared.Lib            (netstandard2.0, no deps)
                       ▲
              ┌────────┴────────┐
              │                 │
   LeapVR.Shared.Lib.Win   LeapVR.Shared.Lib.Wpf  (net471)
              ▲                 ▲
              │                 │
   LeapVR.Utilities.Windows     │
              ▲                 │
              │                 │
   LeapVR.Utilities.Steam       │
                                │
   ─── consumed by LeapVR.Shell.* / LeapVR.Content.* ───
```

Note: `LeapVR.Utilities.Steam` also references **client-tier** projects
(`LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`,
`Pod.Data.Infrastructure`) — so despite the `LeapVR.Utilities.*` naming,
it lives one layer above the shared base.

---

## Conventions

- Every project ships under both `Release` and `Release_ShellClient` configurations.
  The kiosk build uses `Release_ShellClient`, which routes outputs to a folder the
  ConfuserEx step (Build.bat) and Inno Setup script can pick up.
- All `net471` projects also build the `x64` platform variant. The `netstandard2.0`
  one builds AnyCPU only.
- Logging is via NLog 4.5.x. `LeapVR.Shared.Lib` additionally references
  `Microsoft.Extensions.Logging` 2.1.1 for libraries that prefer the
  `ILogger<T>` abstraction.
- Source files copied from third-party projects keep their original namespaces
  (e.g. the `VirtualKeyboard` folder under `LeapVR.Shared.Lib.Win` is a vendored
  fork of `InputSimulator`).

---

## Per-project READMEs

- [LeapVR.Shared.Lib](LeapVR.Shared.Lib/README.md)
- [LeapVR.Shared.Lib.Win](LeapVR.Shared.Lib.Win/README.md)
- [LeapVR.Shared.Lib.Wpf](LeapVR.Shared.Lib.Wpf/README.md)
- [LeapVR.Utilities.Steam](LeapVR.Utilities.Steam/README.md)
- [LeapVR.Utilities.Windows](LeapVR.Utilities.Windows/README.md)
- [LeapVR.Utilities.VersionInfo](LeapVR.Utilities.VersionInfo/README.md)

## Related docs

- [`docs/architecture/build-and-deploy.md`](../architecture/build-and-deploy.md)
  for how `LeapVR.Utilities.VersionInfo.exe` plugs into the build.
- [`docs/3rdParty/README.md`](../3rdParty/README.md) for vendored libraries
  (FFmpeg.AutoGen, ffmediaelement, Steam.Models, SteamWebAPI2, XInputDotNet,
  QRCoder, AspNetCoreRateLimit).
