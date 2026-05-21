# Vendored 3rd-party libraries

> Why this folder exists, and a one-screen summary of every vendored lib.

The repo carries two folders of "vendored" 3rd-party code that is built
*from source* alongside the rest of the solution rather than pulled from NuGet:

- `LeapVR.Shell.3rdParty/` — everything the WPF kiosk needs that we either had
  to fork, modify, or pin to a specific version.
- `Pod.Web.Center.3rdParty/` — server-side equivalents (currently only
  `AspNetCoreRateLimit`).

A third folder exists with mostly the same shape, `LeapVR.Content.3rdParty/`,
but in the current snapshot it only contains a `bin/` cache and ships no
projects of its own — Content Creator reuses the libraries under
`LeapVR.Shell.3rdParty/` directly.

---

## Why vendored?

Several reasons, depending on the lib:

1. **Pinned to a specific version that NuGet no longer offers cleanly.**
   `FFmpeg.AutoGen` 4.0.0.3 must match the FFmpeg 4.0.2 native DLLs we ship
   in `LeapVR.Shell.3rdParty/bin/ffmpeg-4.0.2-win64-shared/`. Likewise
   `ffmediaelement` (FFME) 4.0.260-era source matches that FFmpeg version.
2. **Fork with local modifications.** `Steam.Models` and `SteamWebAPI2` are
   re-targeted to `net452` so they can be referenced from the kiosk's
   net471 build (the upstream NuGet packages target newer frameworks the
   client deliberately doesn't follow). `XInputDotNet` is forked to keep the
   C++ `XInputInterface` project building under modern Visual Studio
   toolsets (v143 + Windows SDK overrides — see
   `docs/architecture/build-and-deploy.md`).
3. **No NuGet equivalent at the time.** Some helper libraries
   (`SoundTouch`, the Unity-built `vrlounge_desktop.exe`, vendored COM
   interop DLLs) ship as raw binaries.
4. **Internalised to allow obfuscation.** The kiosk has an optional
   ConfuserEx step (`Build.bat`); shipping these libraries as project
   references rather than NuGet packages keeps them under the same
   obfuscation regime.
5. **Build-time DLL copy convenience.** The native FFmpeg/SoundTouch DLLs are
   referenced as `<Content CopyToOutputDirectory="PreserveNewest"/>` from
   within the vendored projects so they end up next to the kiosk binary
   without an explicit post-build copy.

Modifications, when they exist, are kept small and live next to the original
upstream files. Unless otherwise noted below, the source is unchanged from
upstream apart from `.csproj` retargeting.

---

## Contents

### `LeapVR.Shell.3rdParty/` (client tier)

| Folder | Library / version | Why vendored | Notes |
|--------|-------------------|--------------|-------|
| `ffmediaelement/` | Unosquare **FFME** (WPF MediaElement backed by FFmpeg) — 4.0.260-era source, assembly name `ffme.win` | Pinned to a build that pairs with FFmpeg 4.0.2; integrates with our obfuscated kiosk build pipeline | Three sub-projects: `Unosquare.FFME.Common` (cross-platform core), `Unosquare.FFME.Windows` (the `MediaElement` control), `Unosquare.FFME.MacOS` (unused). Csproj copies all FFmpeg DLLs (`avcodec-58.dll`, `avformat-58.dll`, `avutil-56.dll`, `swresample-3.dll`, `swscale-5.dll`, `postproc-55.dll`, `avdevice-58.dll`, `avfilter-7.dll`) plus `SoundTouch.dll` to the output directory at build time. Targets `net471`, AnyCPU. Has a `Release_ShellClient` configuration matching the kiosk. |
| `FFmpeg.AutoGen/` | **Ruslan-B/FFmpeg.AutoGen** v4.0.0.3 (C# bindings for FFmpeg native libs) | Required dependency of the FFME version above; pinned for ABI compatibility | Signed assembly using `FFmpeg.AutoGen.snk`. `AllowUnsafeBlocks=true`. Includes `FFmpeg.AutoGen.CppSharpUnsafeGenerator` and `FFmpeg.AutoGen.Example` projects (the generator is unused at build, the example is reference-only). Targets `net471`. |
| `QRCoder/` | **codebude/QRCoder** (MIT, by Raffael Herrmann) — pre-1.x source | Used by the station-pairing flow to display a QR code the operator scans with a phone to bind the station to a tenant | Multi-target (the folder contains four `.csproj`s: NET40, NET35, NET Core 2.0, Portable, plus a Unity-flavoured one with a vendored `UnityEngine.dll` for `UnityQRCode.cs`). Repo also contains the upstream `QRCoderTests`, `QRCoderConsole`, `QRCoderDemo`, `QRCoderDemoUWP` projects but only `QRCoder.csproj` is referenced. License: MIT (`LICENSE.txt`). |
| `Steam.Models/` | **JustinSkiles/SteamWebAPI2** companion DTO assembly (`Steam.Models`) — assembly v1.0.0.0, copyright 2016 | Re-targeted to `net452` to match the kiosk's net471 build profile | Three csproj variants: `Steam.Models` (multi-target), `Steam.Models.Net451`, `Steam.Models.Net452`. The kiosk references **`Steam.Models.Net452`**. Output goes to `..\bin\<Configuration>\net452\` so multiple parallel builds don't collide. |
| `SteamWebAPI2/` | **JustinSkiles/SteamWebAPI2** main client | Same retargeting reason as above | Same three-csproj layout (`SteamWebAPI2`, `.Net451`, `.Net452`). Kiosk references `SteamWebAPI2.Net452`. Auto-mapper-based wrapping of the public Steam Web API endpoints. |
| `XInputDotNet/` | **speps/XInputDotNet** (MIT) — managed wrapper for XInput | We ship the C++ `XInputInterface.dll` from this fork because building it cleanly under modern VS requires platform toolset and Windows SDK overrides we'd rather keep as repo state | Contains: `XInputDotNetPure/` (managed C# assembly) plus `XInputInterface/` (a **C++ vcxproj**) plus pre-built `BinariesX64/` for fallback. Builds across `Win32`/`x64` and `Debug`/`Release`/`Release_ShellClient`. Demo / Reporter / Unity4 / Unity5 projects in the folder are reference-only. License: MIT (`MITLicense.txt`). See `docs/architecture/build-and-deploy.md` for the v143 + Windows SDK overrides required to compile the C++ project on a fresh dev box. |
| `bin/ffmpeg-4.0.2-win64-shared/` | **Zeranoe FFmpeg** Windows 64-bit shared build, version 4.0.2 (GPL + version3 + sdl2) | Native runtime DLLs — no NuGet equivalent for arbitrary FFmpeg builds | The 8 `av*-NN.dll` / `sw*-N.dll` / `postproc-55.dll` files plus `ffmpeg.exe`, `ffplay.exe`, `ffprobe.exe`. Referenced as `<Content>` items by `ffmediaelement/Unosquare.FFME.Windows/Unosquare.FFME.Windows.csproj` for build-time copy. |
| `bin/soundtouch_dll-2.1.1/` | **SoundTouch** 2.1.1 audio time-stretch / pitch-shift native DLLs (LGPL — `COPYING.TXT`) | Required by FFME for `SpeedRatio` audio playback; no NuGet | Includes both x86 and x64 builds. **Local modification:** see `LEAP_README.txt` — `SoundTouch.dll` was renamed from the original `SoundTouch_x64.dll` (and the original x86 DLL renamed to `SoundTouch_x86.dll`) because FFME's loader looks for the unsuffixed name and the kiosk runs as x64. |
| `bin/vr_desktop/` | **Pre-built Unity binary `vrlounge_desktop.exe`** — the in-VR home environment | Not a managed project; just a 22 MB binary asset (folder total ~141 MB including `vrlounge_desktop_Data/`) | Built out-of-repo from a separate Unity project (not in this repo). Files: `vrlounge_desktop.exe`, `vrlounge_desktop_Data/` (Unity data folder), `app.vrmanifest` (OpenVR registration), `vrbox.vrhome.vrappconfig` (kiosk config glue), `cover_square_image.jpg`. Copied into the kiosk install folder at build time. To replace the in-VR home: rebuild the Unity project externally and drop a fresh `vrlounge_desktop.exe` here. |
| `bin/openvr_api.dll` | **Valve OpenVR** native runtime | Paired with `LeapVR.Shell.OpenVR.Wrapper` for SteamVR integration | Single DLL, copied at build time. |
| `bin/Newtonsoft.Json.dll` (+ .xml) | **Newtonsoft.Json** | Pre-NuGet vintage copy kept for the few places that load it via reflection / direct binding redirect | The active csproj references use the NuGet package `Newtonsoft.Json` 12.0.2; this binary is a fallback. |
| `bin/FirewallAPI.dll` | Microsoft `NetFwTypeLib` interop assembly | COM interop wrapper for the Windows Firewall API; kiosk uses it to whitelist itself | Pre-generated TLB import. |
| `bin/Interop.NETWORKLIST.dll` | Microsoft `NetworkListManager` COM interop | Used to detect network connectivity changes for the heartbeat fallback paths | Pre-generated TLB import. |
| `bin/Interop.NetFwTypeLib.dll` | Same firewall typelib as above, alternate name | Historical duplicate; both are referenced in different sub-projects | — |
| `bin/System.Windows.Interactivity.dll` | **Blend** for Visual Studio's `System.Windows.Interactivity` | XAML behaviour support; pre-NuGet vintage kept for binary-identical kiosk builds | Used by `LeapVR.Shell` XAML triggers. |

### `Pod.Web.Center.3rdParty/` (server tier)

| Folder | Library / version | Why vendored | Notes |
|--------|-------------------|--------------|-------|
| `AspNetCoreRateLimit/` | **stefanprodan/AspNetCoreRateLimit** v3.0.5 (MIT, by Stefan Prodan + Cristi Pufu) — IP / Client-ID rate limiting middleware for ASP.NET Core | Source-included so we can apply local fixes (counter-key builders, async key locking strategy) without forking on GitHub; we also need to track v3 breaking changes against our 2.1 ASP.NET Core target | Single `Microsoft.NET.Sdk` csproj targeting `netstandard2.0` with `LangVersion=7.3`. Has its own per-project doc at [`docs/server/3rdParty/AspNetCoreRateLimit/`](../server/3rdParty/AspNetCoreRateLimit/). Subfolders: `Core/`, `Middleware/`, `Models/`, `Net/`, `Resolvers/`, `Store/`, `CounterKeyBuilders/`, `AsyncKeyLock/`. |

---

## Source-of-truth pointers

If a vendored lib needs an update, the upstream repo is:

| Vendored | Upstream |
|----------|----------|
| `ffmediaelement` | https://github.com/unosquare/ffmediaelement |
| `FFmpeg.AutoGen` | https://github.com/Ruslan-B/FFmpeg.AutoGen |
| `QRCoder` | https://github.com/codebude/QRCoder |
| `Steam.Models`, `SteamWebAPI2` | https://github.com/babelshift/SteamWebAPI2 (Justin Skiles) |
| `XInputDotNet` | https://github.com/speps/XInputDotNet |
| FFmpeg native | https://ffmpeg.org/ — Zeranoe builds (now defunct; use BtbN or gyan.dev as replacements when re-pinning) |
| SoundTouch native | https://www.surina.net/soundtouch/ |
| `AspNetCoreRateLimit` | https://github.com/stefanprodan/AspNetCoreRateLimit |
| `vrlounge_desktop.exe` | (private Unity project — not in this repo) |

When refreshing any of these, watch out for:

- **Configuration name parity** — every vendored .NET project must define
  `Release_ShellClient` to participate in the kiosk build (`Build_Free.bat`
  builds the whole solution under that configuration).
- **Native DLL copy paths** — the `<Content Include="..\..\bin\...">` items in
  `ffmediaelement/Unosquare.FFME.Windows.csproj` use repo-relative paths;
  upstream csproj refresh wipes them.
- **Strong-name signing** — `FFmpeg.AutoGen` is signed with `FFmpeg.AutoGen.snk`;
  re-vendoring requires the snk to remain present.

## Related docs

- [`docs/architecture/build-and-deploy.md`](../architecture/build-and-deploy.md)
  for build prerequisites, the v143 / Windows SDK overrides for
  `XInputInterface.vcxproj`, and the `Release_ShellClient` configuration.
- [`docs/shared/LeapVR.Utilities.Steam/README.md`](../shared/LeapVR.Utilities.Steam/README.md)
  for how the kiosk consumes `Steam.Models.Net452` + `SteamWebAPI2.Net452`.
- [`docs/server/3rdParty/AspNetCoreRateLimit/`](../server/3rdParty/AspNetCoreRateLimit/)
  for the rate-limiter's own per-project doc.
