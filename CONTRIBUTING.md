# Contributing to PlayOnDemand

PlayOnDemand is an open-source reference implementation of a VR-arcade kiosk + management server. Contributions are welcome.

## Before you start

- Read `docs/README.md` for the orientation map. Skim the relevant `docs/architecture/*.md` for the area you're touching.
- Open an issue describing the change before writing significant new code. For drive-by fixes (typos, small bugs, doc tweaks), feel free to send a PR directly.
- This repo doesn't enforce a CLA; we accept contributions under the project's Apache 2.0 license. By submitting a PR you affirm you have the right to license the contribution under that license. A `Signed-off-by:` line (the DCO) is welcome but not required.

## Repository layout

| Path | What it is |
|------|-----------|
| `Pod.*` | Server side (ASP.NET Core on .NET 10, EF Core 10 + Postgres). |
| `LeapVR.Shell.*` | Kiosk (WPF on .NET Framework 4.7.1). |
| `LeapVR.Content.*` | Authoring tool for `.vbox` game packages (WPF on .NET Framework 4.7.1). |
| `LeapVR.Shared.*`, `LeapVR.Utilities.*` | Cross-process libraries. |
| `docs/` | Architecture + per-project + usage docs. |
| `_Certificates/` | OpenSSL config templates for local dev TLS (generated artefacts are gitignored). |
| `leap_play_x_app/` | Flutter operator frontend (separate repo, referenced as a sibling). |

## Building

### Server (.NET 10)

```sh
# From the repo root
dotnet build Pod.Web.Center/Pod.Web.Center.csproj
dotnet test  Pod.Data.Test/Pod.Data.Test.csproj
dotnet test  Pod.Services.Test/Pod.Services.Test.csproj
dotnet test  Pod.Grpc.Base.Server.Test/Pod.Grpc.Base.Server.Test.csproj
dotnet test  Pod.Web.Center.Test/Pod.Web.Center.Test.csproj
```

Don't run `dotnet build PoD.sln` — the solution includes .NET-Framework 4.7.1 kiosk projects that need MSBuild + the .NET-Framework Developer Pack. Build server projects individually, or use the kiosk build script (below) for the full WPF stack.

### Kiosk + Content Creator + installer (.NET Framework 4.7.1, Windows)

```cmd
LeapVR.Shell.Build\Build_Free.bat
```

Prerequisites: MSBuild 16+, the .NET-Framework 4.7.1 Developer Pack, Inno Setup 5 on `PATH`, and the FFmpeg 4.x LGPL build dropped into `LeapVR.Shell.3rdParty/ffmpeg/bin/` (see `docs/architecture/build-and-deploy.md`).

### Kiosk runtime safety

**`LeapPlay.Shell.exe` only runs safely with `-debug`.** In production mode it replaces `explorer.exe` as the Windows shell. If you're poking at the kiosk on a developer machine, always pass `-debug`. There is no "I'll be careful" exception.

## Coding style

- Match the surrounding code's brace, indent, and naming style. There is no autoformatter currently; aim for consistency rather than reformatting unrelated lines.
- Server: prefer the `IResult<T>` pattern over throwing exceptions for business-level errors. The pattern is defined in `Pod.Data.Infrastructure`.
- gRPC contract changes: edit the `.proto` files in `Pod.Grpc.Base/`, regenerate (the SDK's `Grpc.Tools` package handles that on build), then update both the server-side `Pod.Grpc.*HostServer` services and the client-side `LeapVR.Shell.Services` consumers.

## Tests

- Server projects with characterization tests: `Pod.Data.Test`, `Pod.Services.Test`, `Pod.Grpc.Base.Server.Test`, `Pod.Web.Center.Test`. All four must stay green on `dotnet test`.
- Use the `InMemoryDbContextFactory` helpers in each test project for unit-scope persistence. Hit the full pipeline via `PodWebApplicationFactory` for integration tests.
- Skip don't disable: if a test is temporarily broken, mark it `[Fact(Skip = "reason: ...")]` with a sentence explaining what would re-enable it. Don't delete or comment out the body.

## Commit + PR style

- Conventional-commit prefixes (`feat:`, `fix:`, `docs:`, `chore:`, …) are welcome but not enforced. A clear one-line subject and a body explaining the *why* is what we care about.
- Keep PRs small. If you're touching the kiosk + server + docs in one change, that's fine; if you're refactoring three unrelated things, please split.
- CI is currently manual. Run the full server test suite locally before pushing.

## Areas where contributions are especially welcome

See `docs/usage/kiosk-known-issues.md` and any open issues. In particular:

- OpenVR / SteamVR runtime updates against the current binding (`LeapVR.Shell.OpenVR.Wrapper`).
- New game platforms (Epic, GOG) under `LeapVR.Shell.Modules/Platform/`.
- Flutter operator-frontend flows (`leap_play_x_app`).
- Hardening the Docker Compose deployment for non-Linux hosts.

Thanks for reading.
