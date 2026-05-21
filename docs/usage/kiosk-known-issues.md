# Kiosk known issues + deferred maintenance

This page lists the open items between "the server stack is shippable on .NET 10" (which it is) and "an operator can install the LeapPlay kiosk against the new server and run an end-to-end session". The work below is environmental / tooling-bound and the v1.0.0 cut consciously stops at the boundary so the server side can ship without being held hostage to a kiosk-machine build loop.

PRs welcome.

## ⚠️ Always pass `-debug`

`LeapPlay.Shell.exe` without `-debug` swaps the Windows shell entry in `winlogon` and replaces `explorer.exe` for the current user. That's the intended kiosk behaviour, but it is **not what you want on a developer machine**. Every smoke-test invocation below uses `-debug`. If you skip it and end up with a station that won't log out of kiosk mode, recover by booting into Safe Mode and editing `HKLM\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell` back to `explorer.exe`.

## Build status

v1.0.0 verified end-to-end via `LeapVR.Shell.Build\Build_Free.bat`: server DLL, kiosk EXE (`LeapPlay.Shell.exe`), Content Creator EXE (`LeapPlay.Content.Creator.exe`), and unsigned Inno Setup installer all produced. 0 errors on a box with VS 2022 17.14 + .NET 10 SDK 10.0.204 + .NET Framework 4.7.1 Developer Pack.

How that works: `LeapVR.Shell.csproj` and `LeapVR.Content.Creator.csproj` are SDK-style WPF (`Microsoft.NET.Sdk.WindowsDesktop` + `<UseWpf>true</UseWpf>`, still targeting `net471`). `Build_Free.bat` routes both kiosk csprojs through `dotnet build` so MSBuild 18 (bundled with the .NET 10 SDK) drives the WPF target chain (`MarkupCompilePass1`, `PresentationBuildTasks`). The native `XInputInterface.vcxproj` still goes through VS's MSBuild via `vswhere`.

Tooling requirements:

- **.NET 10 SDK 10.0.204+** (provides MSBuild 18 and the `Microsoft.NET.Sdk.WindowsDesktop` resolver).
- **.NET Framework 4.7.1 Developer Pack** (the kiosk targets `net471`).
- **Visual Studio 2022 17.14+ Build Tools** for the C++ XInputInterface vcxproj (`Desktop development with C++` workload).
- **Inno Setup 5** at `_Tools/Inno_Setup_5/ISCC.exe` for the installer.
- **nuget.exe** at `_Tools/nuget.exe` for the legacy packages.config-style 3rd-party sibling csprojs (FFME, QRCoder, SteamWebAPI2 etc. still consume packages.config).

Other known sharp edges:

- **Resx + AL task incompatibility under dotnet's MSBuild.** Culture-specific `.resx` resources produce satellite assemblies via the `AL` (Assembly Linker) task; the .NET 10 SDK ships an incomplete `AL` task implementation (MSB4063/MSB4064 on `ToolPath`/`ToolExe`/`EnvironmentVariables`). For each kiosk project with culture-specific resx (`LeapVR.Shell.Language`, `LeapVR.Shell.Categories`, …) v1.0.0 wraps the culture resx items in `Condition="'$(MSBuildRuntimeType)' != 'Core'"` — they're skipped under `dotnet msbuild` but processed unconditionally by full MSBuild. The neutral `Resources.resx` ships either way; WPFLocalizeExtension's fall-back chain handles missing culture variants gracefully.
- **System.Resources.Extensions reference.** Pre-serialized .resx writers (required when the resx carries non-string content) ship in `System.Resources.Extensions`. Each affected kiosk csproj now has an explicit `<Reference Include="System.Resources.Extensions">` resolved from the NuGet cache (`$(NuGetPackageRoot)system.resources.extensions\9.0.0\lib\net462`).
- **Grpc.Core bump (1.19 → 2.46.6).** `LeapVR.Shell.Services` previously referenced Grpc.Core 1.19 which is incompatible with the server-side Grpc.Core.Api 2.x surface that `Pod.Grpc.Base.Client` ships. Packages.config + csproj References bumped to 2.46.6; explicit `System.Memory` 4.5.3 + `System.Buffers` 4.5.0 references added for the gRPC 2.x BufferWriter / Span<T> surface that .NET-Framework 4.7.1 needs to satisfy at compile time.
- **FFmpeg pull.** `Build_Free.bat` downloads the FFmpeg 4.x LGPL binaries on first run. The download URL is hardcoded; mirror locally if your build box is offline.
- **Unity-built `vrlounge_desktop.exe`.** The 141 MB Unity binary lives under `LeapVR.Shell.3rdParty/bin/vr_desktop/` and is copied to the kiosk output via `LeapVR.Shell.csproj`'s post-build event. If your build box doesn't have that path populated, the kiosk binary will still build but VR mode won't have a home environment. The PostBuildEvent reads `$(SolutionDir)`, so when invoking MSBuild on `LeapVR.Shell.csproj` directly you must pass `-p:SolutionDir=<repo-root>/`.

## v1.0.0 server-side changes the kiosk needs to absorb

Behaviour-level (no proto contract changes):

- The kiosk no longer ships an embedded production CA cert. `ServerConfig.GetServerRootCert(basePath)` returns null by default; the kiosk trusts the Windows certificate store. To talk to a development server with a self-signed cert, drop the CA PEM next to the kiosk binary and point `ServerConfig.ServiceRootCert` at it (the field accepts a path relative to the kiosk install dir).
- `StaticServerConfig` is gone. The connect-server endpoint is read from `ServerConfig.json` next to the kiosk binary. Defaults: `localhost:443`. Set `ConnectServerHost` / `ConnectServerPort` to your deployed server.
- The kiosk no longer presents an X.509 client cert during the TLS handshake — production never did mTLS (`GrpcServerConfig.ForceClientCertificate=false`); the orphan `ClientCertificateChain`/`PrivateKey` fields are removed.
- Authentication still flows through `(StationId, Station.Password)` gRPC metadata. No change.
- REST station traffic still uses the `amx` HMAC scheme with `(StationApiKey.PublicKey, StationApiKey.Secret)`. No change.

The Setup wizard (`LeapPlay.Shell.exe -config`) does **not** need updating: it never asked for cert files (the old install path mounted them next to the binary via `LeapVR.Shell.Build/Build_Free.bat` resource copies).

## End-to-end smoke procedure (manual, deferred)

Until this is automated, the v1.0.0 acceptance test is:

```cmd
:: 1. Build the kiosk on a Windows machine with VS 2022 17.14+ Build Tools
::    + .NET 10 SDK + .NET Framework 4.7.1 Developer Pack.
LeapVR.Shell.Build\Build_Free.bat

:: 2. Bring up the server somewhere reachable from the kiosk box.
::    docker compose up -d  (on a Linux VM or Docker Desktop)

:: 3. Install the kiosk and configure it AGAINST the server.
::    a. Run LeapPlay_Setup.exe (produced under LeapVR.Shell.Build\dist\)
::    b. Open ServerConfig.json next to the installed kiosk and set:
::         "ConnectServerHost": "<server-DNS-or-IP>"
::         "ConnectServerPort": 443
::    c. Open the operator UI at https://<server>/, log in, create a station,
::       mint an api-key pair (operator UI -> station -> API keys -> Create).
::    d. Open StationConfig.json next to the installed kiosk and paste in:
::         "StationId": "<guid-from-operator-UI>"
::         "Password": "<station-password-from-operator-UI>"

:: 4. Launch the kiosk with -debug.
"C:\Program Files\LeapPlay\LeapPlay.Shell.exe" -debug

:: 5. Walk through the session lifecycle from the operator UI:
::    a. Confirm station shows as Connected.
::    b. Operator: send a login intention.
::    c. Kiosk: confirm receipt + accept.
::    d. Operator: see session move to Active.
::    e. Kiosk: launch a game (if you have a .vbox installed).
::    f. Operator OR kiosk: request logout.
::    g. Confirm session moves to Ended.

:: 6. Capture the kiosk log (default %LOCALAPPDATA%\LeapPlay\logs\) for any
::    warnings/errors during the walkthrough.
```

If anything in step 5 breaks, the fix likely lives in `LeapVR.Shell.Services/Factory/RemoteServiceFactory.cs` or one of the gRPC service implementations under `LeapVR.Shell.Services/RpcServices/`. The proto contract is unchanged from the pre-net10 baseline.

## Post-v1.0.0 findings from the live `-debug` walkthrough (folded into `v1.0.1`)

Driving `LeapPlay.Shell.exe -debug` against the docker-compose stack on a real Steam-installed dev box turned up half a dozen kiosk-side bugs the build-only check couldn't catch — plus the full Phase-6 session lifecycle round-trip is now verified end-to-end. All landed in `v1.0.1`; see `CHANGELOG.md` → `[1.0.1]` for the full list. Highlights:

- **Steam catalog ingestion**: the kiosk's `libraryfolders.vdf` parser was still on the v1 schema (Steam moved to v2 in 2021), and `AppinfoDecoder` rejected `appinfo.vdf` magic past `0x07564427` (Steam shipped `0x28` in 2022 and `0x29` in 2024). Fixed; 63 valid Steam games are detected on the dev box where the v1.0.0 build saw zero.
- **Steam Store API rate-limit**: `PlatformAppViewModel` fanned out `GetOrUpdateDisplayDataAsync` concurrently per game; Steam's ~200 req / 5 min / IP cap dropped most of them. Now gated through `SemaphoreSlim(2,2)` + 150ms minimum gap + retry-with-backoff, and the store endpoint switched from plain HTTP (which now responds 302) to HTTPS.
- **Connect dialog race-close**: the Connect button could close the dialog before the gRPC handshake completed; now stays disabled until the connection resolves.
- **`SteamApplicationExecution` watchdog**: incorrectly treated the spawned `steam.exe -applaunch` launcher's hand-off exit as Steam itself dying when Steam was already running. The watchdog now tries `RefreshSteamProcess()` on any tracked-process exit and polls `HKCU\...\ActiveProcess\ActiveUser` directly each tick (the `RegistryWatcher` was missing the `0 → user-id` flip when Steam wrote it before the kernel notification registered).
- **Session-end reason isn't plumbed from server to kiosk** (still open, **kept as `SessionLimitReached` placeholder by design**). When the server ends a session for *any* reason that the kiosk didn't initiate itself (operator stop, FSM expiry, admin action, connection loss recovery), `GET /api/v1/stations/{id}/sessions/current` returns null state and `RemoteServicesSet.UpdateSession` falls back to `SessionStopReason.SessionLimitReached`. The wording shown to a paying customer is sometimes wrong (the *actual* server-side `Session.StopReason` may be `RemoteLogout`, `Inactivity`, etc.) but a misleading block screen is strictly better than a silent transition back to LoginView — a customer who paid for a session must always see *something* explaining why it ended. The clean fix is to plumb the server's `Session.StopReason` to the kiosk via the gRPC contract and add a localized block-screen variant per reason; deferred to v1.x.

### Phase-6 session lifecycle — VERIFIED

The full `Connect → SendLoginIntention → operator approves → SendLogoutRequest` round-trip works end-to-end against the docker-compose stack. Driven via REST from the operator side and observed via the kiosk's NLog output:

```
[operator]  POST /api/v1/auth/login → JWT
[kiosk]     LeapPlay.Shell.exe -d → station goes Connected within ~15s
[operator]  PUT  /api/v1/stations/{id}/sessions   → sessionId returned (server: state=Started)
[kiosk]     "LoginIntention Event arrived with Intention ID: <sessionId>"
[kiosk]     "View switched to DashboardViewModel on login intention confirmed"
[operator]  POST /api/v1/stations/{id}/sessions/current/stop → 200
[kiosk]     "View switched to LoginViewModel due to session stopped. StopReason: SessionLimitReached"
            (block screen shown - placeholder wording, see "Session-end reason" above)
[server]    GET /api/v1/stations/{id}/sessions → latestState: "Ended", stoppedBy: "RemoteLogout"
```

**Open kiosk item:** the watchdog + `ActiveUser` polling have been verified live, but starting a game from the kiosk also requires the station's stored Steam credentials to actually log Steam in. On a dev box where Steam Guard 2FA is enabled or saved credentials don't auto-resolve, the kiosk's `-login {user} {pass} -applaunch` will land Steam at the login screen with `ActiveUser=0`. Workaround: launch Steam manually first, then trigger Play from the kiosk dashboard — the existing-Steam-adoption path works end-to-end. A fully autonomous Steam-launch flow on cold boot is a separate station-configuration concern.

## VR runtime gap

The kiosk's `OpenVRModule` was last validated against SteamVR runtime as it shipped circa 2019. SteamVR has had several beta/stable channel rotations since. The OpenVR ABI is stable enough that most things still work, but expect:

- Controller pose events may report through a slightly different path on Index controllers (post-2020 firmware).
- The Unity-built `vrlounge_desktop.exe` (the in-VR "home" environment) was built against Unity 2018 LTS and may render with different colours / lighting on current SteamVR.

If you take the time to validate it on current SteamVR, please file a confirmation issue. If you fix something, the relevant code is in `LeapVR.Shell.OpenVR.Wrapper/` (C# bindings around `openvr_api.dll`) and `LeapVR.Shell.Modules/Vr/`.
