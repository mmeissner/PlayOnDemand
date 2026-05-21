# Open-Source Readiness — v1.0.0 release checklist

> Original audit performed 2026-05-16. This page now records the as-shipped state of the v1.0.0 cut. The original checklist has been ticked off below; remaining advisories are at the bottom.

## What shipped

- **License**: [Apache 2.0](../LICENSE) at the repo root. Applies to all server-side and kiosk-side code authored in this repo. Third-party libraries keep their upstream licenses ([THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md)).
- **Server code**: `Pod.Web.Center` + supporting `Pod.*` projects, migrated to .NET 10 LTS (`global.json` pin: `10.0.204`, `rollForward: latestFeature`). 87 passing + 3 skipped tests, 0 failing at v1.0.0 (rose to 105/3 across 5 projects at v1.0.1 with the addition of `LeapVR.Utilities.Steam.Test`).
- **Kiosk code**: `LeapPlay.Shell.exe` + `LeapPlay.Content.Creator.exe`, still on .NET Framework 4.7.1 (WPF). Cert-licensing dead code excised; `(StationId, Password)` + `(StationApiKey.PublicKey, Secret)` is the canonical auth model.
- **Deployment**: [docker-compose.yml](../docker-compose.yml) + [Dockerfile](../Pod.Web.Center/Dockerfile) + [.env.example](../.env.example). One-command up; see [docs/usage/server-deployment.md](usage/server-deployment.md).
- **Docs**: full as-built docs under [docs/](.). Every architecture page (`architecture/*.md`) is current as of v1.0.0. Usage pages under [docs/usage/](usage/) cover deployment, kiosk build, operator-frontend status, admin-tasks.

## Release checklist (was open, now closed)

### Secrets and committed credentials — DONE
- `Pod.Web.Center/appsettings.json` no longer contains live secrets. Every sensitive field is a `<set-via-env-or-user-secrets>` placeholder. Override via `dotnet user-secrets` (dev) or env vars (`DOTNET_<Section>__<Key>` or `<Section>__<Key>`; the latter is what docker-compose passes).
- An `appsettings.Development.json.example` is tracked; the real `appsettings.Development.json` is gitignored.
- `Pod.Data/appsettings.DesignTime.json` had production hostnames swapped for `example.com`.
- All committed TLS artefacts under `_Certificates/` were removed (13 files: `*.crt`, `*.key`, `*.srl`, `*.pem`). The `*.cnf` OpenSSL templates and `cert-create-all.bat` / `cert-create-server.bat` remain so developers can regenerate dev CAs locally.

### Production hostnames — DONE
- `leap-play.com`, `leap-arcade.com`, `vspace-tec.com`, `connect.leap-play.com`, `*.leap-vr.cn` all replaced with `example.com` placeholders (or `localhost` for dev defaults). The hardcoded production CA embedded in `StaticServerConfig` (kiosk) was deleted along with the class itself.

### Cert-based licensing (orphaned design) — REMOVED
- `LeapCertLicense`, `ILeapCertLicense`, `LicenseRole`, `ClientIdentity`, `ClientRole`, `StaticServerConfig`, `IServerConfig.ClientCertificateChain/PrivateKey`, the entire `LeapVR.Shared.Lib/x509/` folder, and `CredentialConfig.cs` are gone. `LeapVR.Shell/Bootstrapper.cs` now registers `IServerConfig` as a file-backed `ConfigFileRepository<ServerConfig>().Get()` instead of an embedded constant.

### LICENSE + NOTICES + SECURITY + CONTRIBUTING — DONE
- [LICENSE](../LICENSE), [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md), [SECURITY.md](../SECURITY.md), [CONTRIBUTING.md](../CONTRIBUTING.md) all present.
- Top-level [README.md](../README.md) rewritten as public-facing entry (status, quickstart, doc index).

### .gitignore — DONE
- Extended with `_Certificates/**/*.{crt,key,pem,srl,p12,pfx}` so future regenerated dev certs don't accidentally land in commits.

### FFmpeg redistribution — handled via build-time download
- `LeapVR.Shell.Build/Build_Free.bat` downloads FFmpeg 4.x LGPL win64-shared on first run. The GPL FFmpeg binaries that the original audit found in `LeapVR.Shell.3rdParty/bin/` are not part of the v1.0.0 distribution; they are pulled at build time by the build script, which is also when the developer/operator implicitly accepts FFmpeg's LGPL terms. The kiosk redistribution boundary is therefore the installer (`LeapPlay_Setup.exe`) — it carries the FFmpeg binaries the build pulled, under LGPL, with full attribution in [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md).

## Remaining advisories

Items that are documented as future work, not blocking for v1.0.0:

- **Flutter operator frontend** (`leap_play_x_app` sibling repo) — partial reference implementation, predates the server migration, needs Dart 3 + provider 6 + regenerated Swagger client. See [docs/usage/operator-frontend.md](usage/operator-frontend.md). The v1.0.0 server stack ships without bundling a built Flutter artefact.
- **Kiosk smoke against the new server** — manual acceptance test described in [docs/usage/kiosk-known-issues.md](usage/kiosk-known-issues.md). Requires VS 2022 17.15+ to build (older MSBuild versions can't resolve the net10 SDK for the netstandard2.0 sibling projects).
- **VR runtime gap** — `LeapVR.Shell.OpenVR.Wrapper` was last validated against ~2019-era SteamVR. Current SteamVR may have controller-pose differences for post-2020 firmware. Fix path is in the kiosk-known-issues doc.
- **Simulators removed.** `Pod.Grpc.ShellClient.Simulator` and `Pod.Web.Simulator.StressTest` were deleted in the cleanup pass after being orphaned from `PoD.sln` and judged not worth the Terminal.Gui v0→v1 + Grpc.Core→Grpc.Net.Client port work. Recoverable from git history if needed.

## Reading list before publishing publicly

1. Review the rewritten [README.md](../README.md) and [docs/usage/server-deployment.md](usage/server-deployment.md) for any leftover internal references.
2. Spot-check [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md) — the bundled-library list is best-effort. If you find a library not represented, file an issue.
3. Decide on the public security contact in [SECURITY.md](../SECURITY.md) — currently `security@example.com`.
4. Run a fresh `docker compose up -d --build` against a clean clone to confirm the deployment story still holds end-to-end.

That's the release gate. Once those are green, tag `v1.0.0` and publish.
