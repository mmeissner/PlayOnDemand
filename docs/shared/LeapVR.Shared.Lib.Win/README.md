# LeapVR.Shared.Lib.Win

> Windows-only counterpart to `LeapVR.Shared.Lib`: WinAPI P/Invoke wrappers,
> single-instance guard, image processing, virtual keyboard / mouse, WMI,
> taskbar, DPI, version reporting.

## Purpose

Where `LeapVR.Shared.Lib` is `netstandard2.0` and platform-agnostic, this
project is the place for code that talks directly to Windows. Pretty much every
`[DllImport(...)]` call in the kiosk lives here — User32, GDI32, PsApi,
Kernel32, Shell32 — and is then wrapped behind small static utility classes.

It also vendors a fork of WindowsInput's `InputSimulator` (under
`VirtualKeyboard/`) for sending synthetic key/mouse events into games and the
in-VR overlay, an `ImageProcessor` for screen pattern-matching, and small
helpers used during boot (`SingleInstanceGuard`, `MD5Algorithm`, `Wmi`,
`VersionProvider`).

## Tech

- **Target framework:** `.NET Framework 4.7.1`
- **Platforms:** `AnyCPU`, `x64`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient` (x86 not built)
- **Allows unsafe blocks:** yes (image processing operates on bitmap pixel data)
- **Key NuGet packages:**
  - `NLog` 4.5.11 — logging
  - `Newtonsoft.Json` 12.0.2 — used in helpers that serialise diagnostic state
  - `Microsoft.PowerShell.5.ReferenceAssemblies` 1.1.0 — pulls in
    `System.Management.Automation`; some helpers run inline PowerShell
- **System references of interest:** `PresentationCore`, `PresentationFramework`,
  `System.Drawing`, `System.Management`, `System.Windows.Forms`, `WindowsBase`
- **Project references (in this repo):** none directly. (Consumers separately
  reference `LeapVR.Shared.Lib`, but this project doesn't.)

## Responsibility

It IS responsible for:

- Marshalling Win32 API calls (User32 / GDI32 / Kernel32 / PsApi / Shell32) into
  C#-friendly static utility classes.
- Synthesising keyboard and mouse input via `SendInput` (the `VirtualKeyboard`
  folder).
- Screen capture + pattern matching (`ImageProcessor.FindImageOnPrimaryScreen`).
- Single-instance application enforcement via a global named `Mutex`
  (`SingleInstanceGuard`).
- WMI ad-hoc querying (`Wmi.Query`).
- Reading the entry-assembly version (`VersionProvider.SoftwareVersion`).
- MD5 hashing of files (`MD5Algorithm.GetFileHash`).
- Hiding/showing the Windows taskbar and start menu (`Taskbar`).
- Running an external console process and capturing stdout/stderr (`ConsoleProcess.Run`).

It is NOT responsible for:

- Any WPF-specific UI helpers — see `LeapVR.Shared.Lib.Wpf`.
- Process management beyond launching a console process —
  see `LeapVR.Utilities.Windows.Processes.*`.
- Registry, Task Scheduler, USB device events — see `LeapVR.Utilities.Windows`.

## Public API surface

| Type | Purpose |
|------|---------|
| `ConsoleProcess` (static) | `Run(exe, args, out stdout, out stderr, workingDir)`. |
| `MD5Algorithm` (static) | `GetFileHash(path)` → lowercase hex string. |
| `Wmi` (static) | `Query("SELECT * FROM ...")` → `List<ManagementBaseObject>`. |
| `Taskbar` (static) | `Hide()` / `Show()` for explorer's taskbar + start menu. |
| `VersionProvider` (static) | `SoftwareVersion` (entry-assembly version, cached). |
| `ImageProcessor` | Screen / bitmap pattern matching with colour + fault tolerance. (Note: namespace is `LeapVR.Shared.Lib.Win.ImagreProcessor` — typo preserved.) |
| `SingleInstanceGuard : IDisposable` | Global Mutex-based single-instance check. |
| `IInputSimulator`, `InputSimulator`, `KeyboardSimulator`, `MouseSimulator`, `VirtualKeyCode`, `MouseButton`, `InputBuilder` | Synthetic input — fork of `WindowsInput`/`InputSimulator`. |
| `WinApi.Win32.User32`, `Kernel32`, `GDI32`, `PsApi`, `Shell32` | Raw P/Invoke signatures, structs, constants. |
| `WinApi.User32WindowUtil`, `Kernel32Util`, `DpiUtil`, `TrayApplicationUtil` | Higher-level wrappers around the raw P/Invoke. |
| `Structs.DpiInfo`, `Structs.ClipboardData` | Marshal helpers / DTOs. |
| `Classes.SingleInstanceGuard` | (see above) |

## Internal structure

```
LeapVR.Shared.Lib.Win/
├── ConsoleProcess.cs
├── MD5.cs                        ← MD5Algorithm
├── TaskbarHelper.cs              ← static class Taskbar
├── VersionProvider.cs
├── Wmi.cs
├── Classes/
│   └── SingleInstanceGuard.cs
├── ImageProcessor/
│   └── ImageProcessor.cs         ← namespace ImagreProcessor (typo)
├── Structs/
│   ├── ClipboardData.cs
│   └── DpiInfo.cs
├── VirtualKeyboard/              ← fork of InputSimulator
│   ├── IInputSimulator.cs / InputSimulator.cs
│   ├── IKeyboardSimulator.cs / KeyboardSimulator.cs
│   ├── IMouseSimulator.cs / MouseSimulator.cs
│   ├── InputBuilder.cs / InputProcessor.cs
│   ├── VirtualKeyCode.cs / MouseButton.cs
│   ├── WindowsInputDeviceStateAdaptor.cs
│   ├── WindowsInputMessageDispatcher.cs
│   └── Native/                   ← INPUT, KEYBDINPUT, MOUSEINPUT, flags
└── WinApi/
    ├── DpiUtil.cs / Kernel32Util.cs / TrayApplicationUtil.cs / User32WindowUtil.cs
    └── Win32/
        ├── User32.cs / GDI32.cs / Kernel32.cs / PsApi.cs / Shell32.cs
```

## Notable patterns / gotchas

- **`AllowUnsafeBlocks` is on for every config** — needed by `ImageProcessor`
  for direct bitmap pixel access.
- **The `ImageProcessor` namespace is `ImagreProcessor`** (typo in the source).
  Consumer `using` directives must match.
- **Version reading uses `Assembly.GetEntryAssembly()`** — meaning unit-test
  hosts and tools that don't have an entry assembly will null-ref. The
  Build_Free.bat workflow uses `LeapVR.Utilities.VersionInfo.exe` instead for
  the same reason.
- **`Taskbar` walks process windows by class name** — fragile across Explorer
  rewrites; if a Windows update changes class names this breaks silently.
- **`SingleInstanceGuard` uses `Global\` prefix + EveryoneSid ACL** so the
  same name blocks across user sessions (e.g. fast user switching).
- **The `VirtualKeyboard` folder is a fork** of WindowsInput / InputSimulator
  with the namespace rewritten to `LeapVR.Shared.Lib.Win.VirtualKeyboard`.
  Treat as upstream and avoid editing — except where Windows version drift
  forces an update.

## Consumers

- `LeapVR.Utilities.Windows`, `LeapVR.Utilities.Steam`
- `LeapVR.Shell` and most `LeapVR.Shell.*` sub-projects (Modules, Managers,
  Controllers, Services, Repository)
- `LeapVR.Content.Util`, `LeapVR.Content.Shared`

Not consumed by `LeapVR.Shell.Setup` (the wizard avoids the heavy WinAPI
surface) or any `Pod.*` server project.

## Related docs

- [shared tier overview](../README.md)
- [`docs/architecture/build-and-deploy.md`](../../architecture/build-and-deploy.md) —
  the kiosk build copies the FFmpeg / openvr / soundtouch DLLs into the same
  folder where this assembly lives.
