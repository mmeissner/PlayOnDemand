# Changelog

All notable changes to PlayOnDemand are recorded here. Format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); the project uses [Semantic Versioning](https://semver.org/).

> **Status: pre-release.** No version tag has been cut yet. The version-numbered sections below describe the work-in-scope for the eventual `v1.0.0` release; they are *milestone snapshots*, not released artefacts. The release tag will be created as the final action on the repo at the maintainer's call.

## v1.0.0 candidate — kiosk-runtime follow-ups (in progress)

Driven by the live `LeapPlay.Shell.exe -debug` walkthrough against the docker-compose server. All target the kiosk; the server stack is untouched. The full Phase-6 session round-trip (`Connect → SendLoginIntention → operator approves → SendLogoutRequest`) is verified end-to-end — see *Verified in this milestone* below.

### Fixed

- **Connect dialog race-close + missing Background.mp4** (`ac6c735`). On a clean dev box the kiosk's Connect dialog could close before the gRPC handshake finished; the default `Background.mp4` shipped to release output paths only in the obfuscated build, not the dev build, so the login splash had no video. Connect button stays disabled until the handshake completes; the asset is now copied to `bin\x64\Release_ShellClient\` unconditionally.
- **Modern Steam library schema** (`505611d`, `56d211d`). `libraryfolders.vdf` v2 (post-2021) is a flat `path/label/contentid/totalsize/update_clean_bytes_tally/time_last_update_corruption/apps` shape; the kiosk's parser still expected the v1 `Path` array. `GetSteamLibraryFolders` now handles both shapes, and `ScanAppManifestsInLibrary` uses indexer-assign (`_appInstallDirectories[appId] = path`) instead of `Add`, so the same app appearing under two library folders no longer throws. Unit tests cover both schemas.
- **Steam binary `appinfo.vdf` magic v0x28 + v0x29** (`e517d1e`). The Steam client started writing the v0x28 schema in 2022 and v0x29 in 2024; the kiosk's `AppinfoDecoder` rejected anything past 0x07564427. Rewrote the decoder against all four published magic versions, including the v0x28+ string-table layout (keys become uint32 indices into a table at the file's end-offset) and the v0x29 extra 20-byte SHA1 per app. 18 unit tests build synthetic fixtures for every variant + malformed / unknown-magic cases. 63 valid Steam games detected on the dev box (was: 0).
- **Steam Store API rate-limit storm** (`5b6c170`). The kiosk's `PlatformAppViewModel` fired `GetOrUpdateDisplayDataAsync` per app in parallel, which on a 63-game library blasts the Steam Store endpoint with 63 concurrent requests. Steam's ~200 req/5min/IP cap dropped them with `TaskCanceledException`. Now gated through `SemaphoreSlim(2,2)` + 150ms minimum dispatch gap + retry-with-backoff. The store endpoint URL was also flipped from plain HTTP to HTTPS — the HTTP variant now responds with a 302 redirect that the kiosk's HttpClient timed out before following.
- **`NullReferenceException` cascade from Steam `success:false` payloads** (`f666577`). Vendored `SteamWebAPI2.SteamStore.GetStoreAppDetailsAsync` dereferenced `appDetails.Data` without null-check; Steam returns 200 with `{"<appid>":{"success":false}}` for delisted, region-locked, family-share-only, or unauthenticated-query titles. Null-guard added in the vendored client; the retry catch in `SteamLib.GetAppDetailsAsync` broadened from `TaskCanceledException`/`HttpRequestException` to `Exception` so transient parse failures retry rather than escape; `SteamPlatformModule.GetOnlineDisplayInfoAsync` now bails cleanly on null `storeInfo` (was NREing inside `ToAppDisplayInfo.foreach(storeInfo.Categories)`).
- **Header-image download timeouts on Akamai cold-hits** (`f666577`). The 10s `WebRequestTimeout` was shared with the appdetails call; some image CDNs need longer on first hit. Header-image download now gets its own 20s budget + one retry; failures surface as Info instead of Debug so the cause is visible in the deployed log.
- **`SteamApplicationExecution` watchdog mis-detected launcher hand-off as a real Steam exit** (`f666577`). When Steam was already running on the box, the spawned `steam.exe -applaunch ...` delegated to the running instance and immediately exited; the watchdog interpreted that as Steam dying. `SteamSelf.SteamClientTimerCallback` now tries `RefreshSteamProcess()` whenever the tracked process exits (not just during Updating), and polls `HKCU\...\ActiveProcess\ActiveUser` directly each tick as a fallback for missed `RegistryWatcher` events — addresses the second failure mode where Steam writes `ActiveUser` before the watcher's kernel notification registers.
- **NLog `${exception}` layout truncated stack traces** (`f666577`). Switched to `${exception:format=tostring}` in `Release_Nlog.config` so kiosk failures land in the rolling log with a full stack trace, not just the message.
- **Per-launch-option "missing executable" log noise** (`4edab2f`). `SteamAppLaunchInfo` logged a Warn for every appinfo.vdf launch entry whose exe wasn't present locally; a 63-game library has ~10x alternate launch entries per game (Screen / VR / Oculus / Linux / playtest), so the log gained 590 Warn lines per run. Demoted to Debug — the higher-level `SteamAppInfo` ctor still emits an Info "Valid Game ..." for installed games and a Debug "Invalid Game ..." if all launch options fail to validate.
<!-- The "fix" that originally landed here (`SessionLimitReached` -> `StationLogout`) was reverted: a paying customer's session must never silently disappear from the kiosk, and `StationLogout` matches the user-clicked-Logout UX (quiet transition with no block screen). The actually-correct behavior is to show a block screen with the *right* reason; that requires plumbing the server's `Session.StopReason` to the kiosk via the gRPC contract and is tracked under "Known limitations" below + in `docs/usage/kiosk-known-issues.md`. -->

### Added

- **`LeapVR.Utilities.Steam.Test`** (`505611d` + `e517d1e`). New `net471` xunit project with `InternalsVisibleTo` from `LeapVR.Utilities.Steam`. 18 tests cover `GetSteamLibraryFolders` (legacy + post-2021 schema), `ScanAppManifestsInLibrary` (duplicate-appId tolerance), and `AppinfoDecoder` against synthetic binary fixtures for every published `appinfo.vdf` magic version (`0x26`/`0x27`/`0x28`/`0x29`) plus malformed/unknown-magic cases.
- **`flutter_operator_mobile/`** — full operator daily-ops web app. Modern Dart-3 / Flutter-3 port of the legacy `leap_play_x_app` (Flutter 1.7 / Dart 2 pre-null-safety, from a separate sibling repo), finished as originally envisioned and extended to cover every operator-facing REST endpoint on `Pod.Web.Center`. Wired into `docker-compose.yml` as the `operator-mobile` service on host port `:8081` (sibling to the minimal `operator` reference on `:8080`).

  Feature surface end-to-end against the v1.0.0 REST contract (every endpoint verified live):

  - **Login** (`POST /api/v1/auth/login`) with refresh-token rotation + auto-login on app start, persisted via `shared_preferences`.
  - **Stations list** (`GET /api/v1/stations`) with drawer navigation, stat bar (total / online / in-session), per-station card with state icons + mode icons + quick-action buttons.
  - **Create station** (`PUT /api/v1/stations`).
  - **Station detail** — four tabs, all backed by their own REST endpoint:
    - *Overview*: `GET /stations/{id}` + session lifecycle (`PUT /sessions`, `POST /sessions/current/update`, `POST /sessions/current/stop`) with a live remaining-time countdown.
    - *Sessions*: `GET /stations/{id}/sessions` — full history with expandable rows.
    - *API keys*: `GET / PUT / DELETE /stations/{id}/apikeys[/{publicKey}]` with one-shot secret-reveal card.
    - *Settings*: `GET / POST /stations/{id}/settings`, `POST /settings/mode`, `POST /settings/qrcode`, `POST /settings/password` (rename / control-mode picker / QR code URL / station-password rotation behind a danger UI).
  - **All sessions** (`GET /api/v1/stations/sessions`) — admin overview with All / Active / Ended filter chips.
  - **Account** (`POST /api/v1/accounts/password/change`).
  - **Logout** (`POST /api/v1/auth/logout`).
  - **Auto-polling** instead of refresh buttons: 5 s on the stations list, 3 s on station detail (catches kiosk-side `Requested → Delivered → Started → Ended` transitions within a tick), 8 s on session lists. Polling pauses when the tab is backgrounded (`WidgetsBindingObserver`). Pull-to-refresh kept as manual override.

  All 19 PNG/JPG image assets from the legacy app are used wholesale (sofa for Local mode, remote for Remote, QR for RemoteWithQrCode, sleeping monitor for idle, gamepad-on-monitor for active session, disconnected-plug for offline, etc.). The two operator apps are complementary: `flutter_operator/` is the minimal admin reference (login + list + mint), `flutter_operator_mobile/` is the full daily-ops tool. The legacy `leap_play_x_app/` source lives in a separate repo (not committed here) and was used as a one-time porting reference; the modernised port adds what the legacy never finished (station detail screen was a stub, the "extend session" button was misbound, no API-key management, no settings, no all-sessions view, no auto-refresh).

  Server-side bug uncovered during the build-out: my initial `StationControlMode` int constants were off-by-one — assumed `0 = Local` but `Pod.Enums.StationControlMode` actually starts at `1` (`0 = Undefined`, `1 = Local`, `2 = Remote`, `3 = RemoteWithQrCode`). Fixed in `lib/services/api_client.dart`.

### Verified in this milestone

- **Phase-6 session lifecycle round-trip** end-to-end against the docker-compose stack:
  - Operator authenticates: `POST /api/v1/auth/login` → JWT.
  - Kiosk launches with `-debug`; station transitions `Disconnected → Connected` within ~15s.
  - Operator sends login intention: `PUT /api/v1/stations/{id}/sessions` with `{reference, duration}`. Server returns the new `sessionId`. Kiosk's `RemoteSession.InitSession` receives it via gRPC streaming, `LoginViewModel.Handle` logs the intention id, and `SendLoginDecisionAsync` is dispatched. `ShellViewModel` transitions to `DashboardViewModel` on `login intention confirmed`.
  - Verified `state: "Started"` server-side, full session record populated with `requestedBy: "WebApi"`, `startedOnUtc`, `maxDurationLimit`.
  - Operator stops: `POST /api/v1/stations/{id}/sessions/current/stop` returns the session id. Kiosk's `RemoteServicesSet.UpdateSession` polls `GetSessionState`, sees `State == null`, dispatches `RequestStopSession(SessionStopReason.SessionLimitReached)`. `ShellViewModel.Handle(IUISessionStopedEvent)` shows the "session limit reached" block screen (a placeholder until the server's actual `Session.StopReason` is wired through — see *Known limitations* below). Server-side `latestState: "Ended"`, `stoppedBy: "RemoteLogout"`.

### Known limitations

- **Session-end reason isn't plumbed from server to kiosk.** When the server ends a session (operator stop, FSM expiry, admin action, etc.), `GET /api/v1/stations/{id}/sessions/current` returns null state and the kiosk has no way to know *why*. The kiosk falls back to `SessionStopReason.SessionLimitReached` so the user sees *some* block screen (a paying customer must never see their session silently disappear into a LoginView). The wording will be wrong for operator-stop / FSM-expiry / admin-action cases. Server-side `Session.StopReason` is recorded correctly (`UserLogout` / `RemoteLogout` / `Inactivity` / `LimitReached` / etc.) so the data exists; v1.x will plumb it through the gRPC contract and add a localized "ended by operator" / "ended by admin" / "max duration reached" block screen with per-reason copy. Tracked in `docs/usage/kiosk-known-issues.md`.

### Not in this milestone (deferred)

- Kiosk-driven Steam game launch still depends on the station's Steam credentials being valid (or Steam being pre-logged-in by the operator). The watchdog + `ActiveUser` polling work was verified; the launch-credentials handling is a station-configuration concern, not a code bug.

### Statistics

- Test totals (5 projects): **105 passing / 3 skipped / 0 failing**. Earlier in the v1.0.0 work the totals were 87/3/0 across 4 projects — the new `LeapVR.Utilities.Steam.Test` accounts for the +18.

## v1.0.0 candidate — core open-source migration

The bulk of the open-source preparation work, organised as one milestone for the eventual release tag.

This release ports the previously closed-source PlayOnDemand server from ASP.NET Core 2.1 (EOL) to .NET 10 LTS, replaces the legacy gRPC.Core hosting with `Grpc.AspNetCore` on a single Kestrel pipeline, ships a one-command Docker Compose deployment (postgres + server + Flutter operator UI), and excises the orphaned cert-licensing path that was never finished server-side. The kiosk and Content-Creator binaries are unchanged at runtime; only the kiosk-side wiring that talks to the cert-licensing path was removed.

End-to-end verified against `docker compose up -d` on a fresh stack: 3 containers healthy, login (good + bad creds) returns expected responses, station list + API-key mint + list flow works through the nginx reverse proxy in the operator container, secret leaked only at mint time. 8 production bugs (invisible to the in-process test harness) were caught by the live smoke and fixed before tagging.

### Added
- **Docker Compose deployment** with `postgres:16-alpine` + `Pod.Web.Center` server + `pod-operator:latest` (nginx-served Flutter web app) + named volumes for `letsencrypt-data` and `server-data`. First-run admin seed via env vars. `.env.example` documents every required secret. Healthcheck-gated container start order.
- **Flutter operator frontend** at `flutter_operator/`. Minimal Dart-3 web UI covering the v1.0.0 operator critical path: login (JWT, surfaces `userIdentityPasswordMismatch` on bad creds), station list, station detail, API-key mint (one-shot Secret reveal) + list. Two build paths: host-side `flutter build web --release` + offline `Dockerfile.prebuilt`, or fully-containerised via `ghcr.io/cirruslabs/flutter`. nginx (in the operator container) proxies `/api/*`, `/swagger/*`, `/health` to the server container so the SPA and the API share an origin — no CORS to configure.
- **`Pod.Web.Center` multi-stage `Dockerfile`** (`mcr.microsoft.com/dotnet/sdk:10.0` build → `aspnet:10.0` runtime, non-root `app` user, mounts at `/app/certs` and `/app/data`).
- **`/health` endpoint** backed by `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`. Reports liveness + DB connectivity. Wired into the docker healthcheck.
- **Open-source release plumbing**: top-level `LICENSE` (Apache 2.0), `THIRD_PARTY_NOTICES.md` (NuGet packages + kiosk-bundled libs + native runtime deps + Inno Setup + ConfuserEx + OpenSSL templates), `SECURITY.md` (disclosure + hardening), `CONTRIBUTING.md` (build/test instructions, kiosk `-debug` safety, skip-don't-disable convention), and a public-facing `README.md`.
- **`Pod.Web.Center.Test`** integration project. `PodWebApplicationFactory` boots the full pipeline against EF InMemory with a pre-seeded `ShellServer` row; covers anonymous endpoint reachability, `[Authorize]` 401 rejection, `/health` 200, and env-var binding (proves `AuthConfig__SecretKey` reaches `IConfiguration`).
- **`Pod.Grpc.Base.Server.Test`** project with characterization tests for the new `GrpcMetadataAuthenticationHandler` and `IGrpcStationCredentialVerifier` (15 tests).
- **`Pod.Data.Test/Fixtures/InMemoryDbContextFactory`** + matching helpers in `Pod.Services.Test` and `Pod.Web.Center.Test` (reflection-based, since `PodDbContext`'s constructor is internal).
- **EF Initial migration** regenerated against EF Core 10.0.4 — captures the full entity model (22+ tables including Identity 10's new passkey schema).
- **Per-deployment docs**: `docs/usage/server-deployment.md` (Docker Compose flow, `.env` reference, hardening), `docs/usage/admin-tasks.md` (rotate JWT, rotate admin password, force LE renewal, mint station via curl, Postgres backup/restore), `docs/usage/operator-frontend.md` (Flutter app state + path to v1.x), `docs/usage/kiosk-known-issues.md` (kiosk smoke procedure, VR runtime gap, MSBuild 18+ requirement, `-debug` safety).

### Changed
- **Server runtime**: ASP.NET Core 2.1 → ASP.NET Core on .NET 10 LTS. `global.json` pins `10.0.204` with `rollForward: latestFeature`.
- **gRPC hosting**: legacy `Grpc.Core.Server` on a separate port replaced with `Grpc.AspNetCore` 2.66 mapped via `endpoints.MapGrpcService<>()` on the same Kestrel pipeline as REST + Razor Pages. `Pod.Grpc.Base.Server.GrpcServer<T>` deleted.
- **gRPC station auth**: the per-call `(identity, password)` metadata handling is now a proper ASP.NET Core `AuthenticationHandler` with scheme `grpc-station` (`Pod.Grpc.Base.Server.GrpcMetadataAuthenticationHandler`), pluggable verifier interface (`IGrpcStationCredentialVerifier`), and the default `DefaultGrpcStationCredentialVerifier` that PBKDF2-checks against `Station.PasswordHash`.
- **JSON**: `Microsoft.AspNetCore.Mvc.NewtonsoftJson` for compatibility with existing DTO converters (`CamelCasePropertyNamesContractResolver` + `StringEnumConverter`). Newtonsoft.Json bumped from 11.x to 13.0.4 (CVE remediation).
- **OpenAPI**: Swashbuckle 4 → 10.1.7 + `Swashbuckle.AspNetCore.Annotations` 10.1.7 + `Swashbuckle.AspNetCore.Filters` 10.0.1. Filter signatures updated for the new `Microsoft.OpenApi` 2.x flat namespace (`OpenApiDocument`/`OpenApiOperation`/`OpenApiInfo`/`OpenApiSecurityScheme`). `swaggerDoc.Host`/`Schemes` replaced with `Servers`.
- **Rate limiting**: `AspNetCoreRateLimit` 5.0 (NuGet) replaces the vendored 3rd-party copy under `Pod.Web.Center.3rdParty/AspNetCoreRateLimit/`.
- **JWT Bearer**: `Microsoft.AspNetCore.Authentication.JwtBearer` is now a separate package (no longer in the `Microsoft.AspNetCore.App` framework reference).
- **Hosting**: `WebHost.CreateDefaultBuilder` → `Host.CreateDefaultBuilder().ConfigureWebHostDefaults(...)` (generic-host pattern). `IHostingEnvironment` → `IWebHostEnvironment`. `app.UseMvc()` → `UseRouting + UseAuthentication + UseAuthorization + UseEndpoints`.
- **EF Core**: 2.1 → 10.0.4. Npgsql provider bumped to 10.0.1. Initial migration regenerated.
- **Identity**: ASP.NET Identity bumped along with EF Core. Passkey schema (Identity 10) registered via `base.OnModelCreating(modelBuilder)` in `PodDbContext`. Token-provider subclasses updated to pass `ILogger` to `DataProtectorTokenProvider`.
- **`ContextInitializer.Initialize`** now guards migration execution with `Database.IsRelational()` so test scenarios on EF InMemory don't trip the relational-only `GetPendingMigrations()`.
- **Kiosk-side `IServerConfig`** registered as `ConfigFileRepository<ServerConfig>().Get()` instead of `StaticServerConfig`. Operators control the connect endpoint via `ServerConfig.json` next to the kiosk binary.
- **Kiosk + Content Creator csprojs migrated to SDK-style WPF**. `LeapVR.Shell.csproj` (1198 lines → ~130) and `LeapVR.Content.Creator.csproj` (520 lines → ~100) now use `Microsoft.NET.Sdk.WindowsDesktop` with `<UseWpf>true</UseWpf>`. Targets `net471` so the WPF runtime stays unchanged; the SDK shape just wires the MarkupCompile target chain cleanly under `dotnet build` (MSBuild 18 from the .NET 10 SDK). `LeapVR.Shell.csproj` keeps `Program.cs` as the entry point (App.xaml demoted to `<Page>`); Content Creator keeps App.xaml as `<ApplicationDefinition>`. Both: `AppendTargetFrameworkToOutputPath=false` so artefacts land at `bin\x64\Release_ShellClient\` (no TFM subfolder) — matches what `Build_Free.bat` + the Inno installer source expect. Legacy `<PostBuildEvent>` CMD-shell scripts rewritten as MSBuild `<Target>`s with `Copy` tasks (cross-shell, $(Configuration)-aware). Originals kept as `*.csproj.bak` for diff. Auto-globbed dead-code (`TabSystemViewModel.cs`, `StepScreen.cs`) excluded via `<Compile Remove>`.
- **`Build_Free.bat`** now routes the kiosk and Content-Creator builds through `dotnet build` instead of VS's MSBuild. The native XInputInterface vcxproj keeps using VS MSBuild via vswhere. This removes the previous "VS 2022 17.15+ required" caveat: VS 2022 17.14 (MSBuild 17.14) + .NET 10 SDK 10.0.204 (which bundles MSBuild 18) is now sufficient for the full server + kiosk + Content Creator + Inno installer build chain.

### Removed
- **Cert-based station-licensing dead code** (kiosk + shared): `LeapVR.Shell.Domain.Models/CertLicense/ILeapCertLicense.cs`, `LeapVR.Shell.Modules/ShellConfigurator/LeapCertLicense.cs`, `LeapVR.Shared.Lib/x509/*` (ClientIdentity, ClientRole, Crypto, CryptoHelpers, RSAParameterTraits), `LeapVR.Shell.Services.Interfaces/FileConfig/CredentialConfig.cs`, `StaticServerConfig` class (and its embedded production CA PEM + hardcoded `connect.leap-play.com` hostname), `IServerConfig.ClientCertificateChain` + `.PrivateKey` properties + `GetClientCertificate`/`GetClientPrivateKey` methods.
- **`Pod.Web.Center.ServicesHosted.GrpcServicesServer`** (legacy `GrpcServer<T>` wrapper). gRPC services now register via `services.AddGrpc()` + endpoint mapping. `GrpcHostedServer` was reduced to a `PublisherHub<ClientCommandType>.Disconnect` broadcast on host stop.
- **`Pod.Grpc.Base.Server.GrpcServer`** stub (had been kept as a documentation anchor in the previous commit; deleted now that `docs/architecture/grpc.md` documents the new wiring).
- **`Pod.Test.Sandbox`** (not a real test project per the audit).
- **`Pod.Web.Center.3rdParty/AspNetCoreRateLimit`** (replaced with the NuGet 5.0 package).
- **Committed TLS artefacts under `_Certificates/`**: 13 files (`*.crt`, `*.key`, `*.pem`, `*.srl`). Templates (`*.cnf`) and the `cert-create-*.bat` generators remain.
- **Production hostnames**: `leap-play.com`, `leap-arcade.com`, `vspace-tec.com`, `connect.leap-play.com`, `*.leap-vr.cn` swapped for `example.com` or `localhost`.

### Security
- All committed secrets in `appsettings.json` replaced with `<set-via-env-or-user-secrets>` placeholders (AuthConfig.SecretKey, LetsEncryptOptions.EncryptionPassword, ConfigSuperuser.Password, ConfigSuperuser.StationPassword, ConnectionStrings.PodApiContext password).
- `dotnet user-secrets` already wired via `UserSecretsId` in `Pod.Web.Center.csproj`.
- `.gitignore` extended for `_Certificates/**/*.{crt,key,pem,srl,p12,pfx}` to keep regenerated dev certs from being accidentally committed.

### Fixed (caught by live `docker compose up` smoke; would have shipped with the cut)
- **Migration race vs hosted services** — `ContextInitializer.Initialize` ran at the end of `Startup.Configure` after hosted services had already started, so `ConnectionHealthService` queried `ConnectionStates` before tables existed and crashed with `relation "ConnectionStates" does not exist`. Initializer moved to `Program.Main` inside a scope built from `host.Build()`, before `host.RunAsync()`.
- **Missing `IUserConfirmation<TUser>` registration** — `AddIdentityCore` (unlike full `AddIdentity`) doesn't register it; `SignInManager` needed it; the login endpoint returned 500. Added `identityBuilder.AddSignInManager()` plus an explicit `services.TryAddScoped<IUserConfirmation<ApplicationUser>, DefaultUserConfirmation<ApplicationUser>>()`.
- **Silent wrong-password bypass in `AddSignResult`** (real security bug). `SignInResult.Succeeded == false` with neither `IsLockedOut` nor `IsNotAllowed` was a no-op; `AuthenticationService.GetTokenByLogin` then minted a JWT for unauthenticated callers. Added the missing fall-through that flags `UserError.UserIdentityPasswordMismatch`. Existing characterization test (which documented the bug as "out of scope") rewritten as a positive regression assertion.
- **EF Core 10 LINQ projection rejection** — `ToStationCurrentStateVm.FromStation()` invoked `ToSessionViewVm.FuncFromSession(...)` (a compiled `Func<...>` captured in the expression body). EF Core 2.1 silently fell back to client eval; EF Core 10's memory-leak guard rejects. Inlined the `SessionViewModel` construction so the whole tree translates to SQL.
- **API-key secret leaked on list endpoint** (real security bug). `GET /api/v1/Stations/{id}/apikeys` returned `secret` for every key; the design says secret is mint-only. Split `ToStationApiKeyVm.FuncFromStationApiKey` into the mint-only full projection and a list-only `FuncFromStationApiKeyNoSecret`.
- **Kestrel HTTPS-without-cert crash** in the container. With `LETSENCRYPT_ENABLED=false`, `ConfigureKestrel` did nothing and Kestrel tried to bind `:443` from `ASPNETCORE_URLS=...;https://+:443` — but there's no dev cert in the runtime image. Made `ConfigureKestrel` handle both LE-on and LE-off cleanly (LE-off = `:80` HTTP/1.1 + h2c only).
- **NLog `/app/logs` denied** as non-root user. The aspnet runtime image has `WORKDIR /app` owned by root; the `app` user couldn't write log files. Pre-create + chown `/app/logs /app/certs /app/data` in the Dockerfile.
- **Docker healthcheck never went healthy** — no curl in the slim aspnet runtime image. apt-get install `curl` in the runtime stage. Operator container had a similar issue with `localhost` not resolving under musl; switched the healthcheck to `127.0.0.1`.

### Not in this release (deferred)
- **Kiosk runtime smoke against the new server** — see [docs/usage/kiosk-known-issues.md](docs/usage/kiosk-known-issues.md). The full build chain (server + kiosk + Content Creator + Inno installer) is verified via `LeapVR.Shell.Build/Build_Free.bat` end-to-end (0 errors, all artefacts produced at canonical paths). What still needs a human in the loop is the runtime walk-through of `Connect → SendLoginIntention → operator approves → SendLogoutRequest` against a running docker-compose server — only because launching `LeapPlay.Shell.exe` without `-debug` removes the Windows shell (replaces `explorer.exe`), so this is a manual acceptance test, not an automated one. The legacy `leap_play_x_app/` sibling repo (Dart 2 pre-null-safety) remains; the v1.0.0 operator UI under `flutter_operator/` is a fresh minimal Dart 3 implementation.
- **OpenVR/SteamVR runtime updates** — the VR module was last validated against ~2019-era SteamVR. PRs welcome.
- **Simulator tools removed** (`Pod.Grpc.ShellClient.Simulator`, `Pod.Web.Simulator.StressTest`). Both csprojs had been bumped to net10 syntactically but excluded from `PoD.sln` and never functionally ported (would have needed Terminal.Gui v0→v1 + Grpc.Core→Grpc.Net.Client code rewrite). Deleted in the cleanup pass; recoverable from git history if a future contributor wants to revive them.
- **Flutter operator UI screens beyond the v1.0.0 critical path** — active session monitor (needs SSE/SignalR server-side), session history, billing, admin settings, refresh-token-driven persistent login, multi-language. See [docs/usage/operator-frontend.md](docs/usage/operator-frontend.md).

### Statistics
- Test totals: **87 passing / 3 skipped / 0 failing** across 4 test projects.
- Server-side projects building clean: **28/28** via per-project `dotnet build`.
- 3 containers (postgres, server, operator) reach `healthy` on `docker compose up -d`; the full operator critical path (login → list stations → mint API key → list keys) runs end-to-end against this stack.
- 8 production bugs caught and fixed by the live smoke (see Fixed above).

<!-- Release-anchor links omitted: no version tag has been cut yet. They will be added at the same time as the eventual release. -->
