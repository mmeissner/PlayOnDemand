# LeapVR.Shell.OpenVR.Wrapper

> Thin C# bindings for Valve's `openvr_api.dll`. A single auto-generated file (`openvr_api.cs`) plus build-event packaging that copies the native DLL next to the consuming binaries. The kiosk's only direct touchpoint with the OpenVR/SteamVR ABI.

## Purpose

OpenVR is shipped as a native C++ DLL. Valve provides an auto-generated C# header (`openvr_api.cs`) that exposes the same vtable structs/enums to managed code via `[StructLayout]` + `[UnmanagedFunctionPointer]` delegates. This project is essentially that one file plus the MSBuild glue to make the native DLL travel with consumers.

It is consumed by `LeapVR.Shell.Modules` (specifically `OpenVrModule`, `OpenVrProcessHandler`, `HmdActivityWatchdog`, `VrDesktopModule`). No other project references it directly.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.OpenVR.Wrapper.dll`) — note the assembly name and root namespace are **`LeapVR.OpenVR.Wrapper`**, not `LeapVR.Shell.OpenVR.Wrapper`. Only the *project* directory and project file carry the `Shell.` prefix.
- **Key NuGet packages:** none. Pure managed wrapper.
- **Project references (in this repo):** none — leaf project.
- **Native dependency (file-referenced):**
  - `..\LeapVR.Shell.3rdParty\bin\openvr_api.dll` — included as `<Content Include="..." Link="openvr_api.dll" CopyToOutputDirectory="PreserveNewest" />` so consumers get the DLL next to their `.exe`.

## Responsibility

**It IS responsible for:**
- Re-exposing Valve's `IVRSystem`, `IVRCompositor`, `IVRChaperone`, `IVROverlay`, `IVRRenderModels`, etc. as C# structs with delegate-typed function pointers.
- Re-exposing every OpenVR enum (`EVREye`, `EVRApplicationType`, `ETrackedDeviceClass`, `EVREventType`, `EVRButtonId`, …) and POD struct (`HmdMatrix44_t`, `HmdMatrix34_t`, `HmdQuaternion_t`, `TrackedDevicePose_t`, `VREvent_t`, …).
- Ensuring `openvr_api.dll` is copied into the consuming `bin/` folder.

**It is NOT responsible for:**
- Calling OpenVR — that's `LeapVR.Shell.Modules.Vr.OpenVrModule`'s job.
- Process lifecycle of `vrserver.exe` / `vrmonitor.exe` — `OpenVrProcessHandler` (in modules) handles that.
- The Unity-built in-VR home environment (`vrlounge_desktop.exe`) — that's a separate native binary loaded by `VrDesktopModule`.

## Public API surface

Effectively *every public type in `openvr_api.cs`*. Highlights:

- **Structs (vtable surface):** `IVRSystem`, `IVRChaperone`, `IVRChaperoneSetup`, `IVRCompositor`, `IVROverlay`, `IVRRenderModels`, `IVRApplications`, `IVRSettings`, `IVRScreenshots`, `IVRResources`, `IVRTrackedCamera`, `IVRDriverManager`, `IVRInput`, `IVRIOBuffer`, `IVRSpatialAnchors`, `IVRDebug`.
- **POD structs:** `HmdMatrix34_t`, `HmdMatrix44_t`, `HmdVector3_t`, `HmdVector4_t`, `HmdQuaternion_t`, `HmdQuad_t`, `HmdRect2_t`, `Texture_t`, `VRTextureBounds_t`, `TrackedDevicePose_t`, `VREvent_t`, `VRControllerState_t`, `VRControllerAxis_t`, etc.
- **Enums:** `EVREye`, `ETextureType`, `EColorSpace`, `ETrackingResult`, `ETrackedDeviceClass`, `ETrackedControllerRole`, `ETrackingUniverseOrigin`, `ETrackedDeviceProperty`, `ETrackedPropertyError`, `EVRSubmitFlags`, `EVRState`, `EVREventType`, `EDeviceActivityLevel`, `EVRButtonId`, `EVRMouseButton`, `EDualAnalogWhich`, `EShowUIType`, `EVRComponentProperty`, `EVRInputError`, `EIOBufferError`, `EIOBufferMode`, `EVROverlayInputMethod`, `EVROverlayTransformType`, `VROverlayFlags`, `VRMessageOverlayResponse`, `EGamepadTextInputMode`, `EGamepadTextInputLineMode`, `EHiddenAreaMeshType`, `EVRControllerAxisType`, `EVRControllerEventOutputType`, `ECollisionBoundsStyle`, `EVROverlayError`, `EVRApplicationType`, `EVRFirmwareError`, `EVRNotificationError`, `EVRSkeletalMotionRange`, `EVRSkeletalTrackingLevel`, `EVRInitError`, `EVRScreenshotType`, `EVRScreenshotPropertyFilenames`, `EVRTrackedCameraError`, `EVRTrackedCameraFrameType`, `EVSync`, `EVRMuraCorrectionMode`, `EVRApplicationError`, `EVRApplicationProperty`, `EVRSceneApplicationState`, `EVRApplicationTransitionState`, `EVRCompositorError`, `EVRCompositorTimingMode`, `VRCompositorError`, `EVRFirmwareError`, `EVRNotificationStyle`, `EVRSettingsError`, `EVRRenderModelError`, `EVRComponentProperty`, etc.
- **Static `OpenVR` facade class** (toward the end of the file) — exposes top-level `Init`, `Shutdown`, `GetGenericInterface`, the singleton-style accessors `OpenVR.System`, `OpenVR.Compositor`, `OpenVR.Chaperone`, etc.

(All names match Valve's `openvr.h`/`openvr_api.h` C++ headers verbatim.)

## Internal structure

```
LeapVR.Shell.OpenVR.Wrapper/
├── openvr_api.cs                  Auto-generated bindings (single file, ~thousands of lines)
├── Properties/AssemblyInfo.cs
└── LeapVR.Shell.OpenVR.Wrapper.csproj
```

The `.csproj` references `..\LeapVR.Shell.3rdParty\bin\openvr_api.dll` as `<Content>` with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` and `<Link>openvr_api.dll</Link>`, so the native DLL flows into every consumer's output folder.

## Notable patterns / gotchas

- **DO NOT EDIT `openvr_api.cs` MANUALLY.** The header at the top is explicit: "This file is auto-generated, do not edit it." When upgrading OpenVR, drop in Valve's freshly-generated file, replace `openvr_api.dll` in `LeapVR.Shell.3rdParty/bin/`, and rebuild. Manual edits will be silently overwritten next upgrade.
- **Namespace mismatch is intentional.** Project folder is `LeapVR.Shell.OpenVR.Wrapper`, but the assembly + root namespace is `LeapVR.OpenVR.Wrapper`. Don't be tempted to "fix" this — the namespace matches Valve's documentation conventions and consumers (in `LeapVR.Shell.Modules`) `using LeapVR.OpenVR.Wrapper`.
- **x64 only.** `openvr_api.dll` is 64-bit. The kiosk is x64 by global convention; cross-compiling x86 will fail at runtime with a BadImageFormatException.
- **Function pointers are delegates, not direct P/Invoke.** Valve's vtable shape requires the `OpenVR.GetGenericInterface(...)` indirection. Read Valve's "Using OpenVR from Managed Code" doc before extending the bindings.
- **The DLL ships through this project, but is also referenced by `LeapVR.Shell` indirectly.** If you build `LeapVR.Shell.Modules` standalone (without `LeapVR.Shell.3rdParty` populated), the build fails because the `<Content>` glob can't be resolved. See `docs/architecture/build-and-deploy.md` (planned).

## Consumers

- `LeapVR.Shell.Modules` — only direct consumer. `OpenVrModule`, `OpenVrProcessHandler`, `OpenVRFilesHandler`, `HmdActivityWatchdog`, `VrDesktopModule` all `using LeapVR.OpenVR.Wrapper;`.
- `LeapVR.Shell` — transitive consumer (the DLL travels into the executable's output dir).

## Related docs

- Sister projects: [`LeapVR.Shell.Modules`](../LeapVR.Shell.Modules/README.md) (the only direct consumer; see the `Vr/` subfolder there)
- Vendored 3rd-party: `LeapVR.Shell.3rdParty/bin/openvr_api.dll` — Valve's binary. Source of truth at <https://github.com/ValveSoftware/openvr>.
- Tier overview: [`docs/client/README.md`](../README.md)
