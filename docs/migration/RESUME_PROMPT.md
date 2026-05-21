# Resume-migration prompt (paste this into the next session)

> Save this file. When you start a new Claude Code session, paste the section between the `--- BEGIN PROMPT ---` and `--- END PROMPT ---` lines as the first user message. It's self-contained: future-Claude will read the repo, verify state, and resume the migration without re-asking the planning questions we already settled.

---

## --- BEGIN PROMPT ---

You are resuming a partially-completed migration of the PlayOnDemand (PoD / LeapPlay) VR-arcade platform from ASP.NET Core 2.1 (EOL) to .NET 10 LTS, with a Flutter operator frontend, Docker-compose deployment, and a tagged v1.0.0 release as the final outcome. This is a **one-shot final open-source release** — there is no "fix it later".

**Repo:** `D:\Repository\Software\PlayOnDemand\PlayOnDemand`
**Current branch:** `migration/dotnet10`
**Don't push to remote.** Local commits only.
**Don't ask planning questions.** All scope decisions are settled (see `docs/migration/DECISIONS.md` if you want the rationale; the short version is below).

### Step 0 — read these before doing anything else, in this order
1. `docs/migration/RESUME_PROMPT.md` (this file) — full plan, gotchas, references.
2. `git log --oneline migration/dotnet10` — see what's already committed.
3. `docs/README.md` and the `docs/architecture/*` set — the as-built docs.
4. `docs/open-source-readiness.md` — the cert/secret/cleanup checklist (still partly open).
5. Run `dotnet test Pod.Data.Test Pod.Services.Test Pod.Grpc.Base.Server.Test` to confirm the 74 baseline tests still pass on your machine. **If they don't, fix that first** — your environment may have drifted from the baseline.

### Hard constraints (do not violate)
- **Kiosk (`LeapPlay.Shell.exe`) only run with `-debug`.** In production mode it removes the Windows shell (replaces explorer.exe). For any kiosk smoke-testing during this migration, always pass `-debug`. There is no "I'll be careful" exception.
- **No remote push.** `git push origin` is forbidden unless the user explicitly tells you to.
- **No destructive ops without checking** — `git reset --hard`, `git rebase -i`, branch deletes, force-push, `rm -rf` outside scoped folders. The conversation's "what's already done" list is authoritative; never re-do migrated projects without verifying via `git log`.
- **Don't bump SDK pin.** `global.json` is on `10.0.204` (`rollForward: latestFeature`). Don't move it.
- **Server-side projects target `net10.0`. Kiosk-shared projects stay `netstandard2.0`** — that boundary lets the .NET-Framework 4.7.1 kiosk keep working without rebuilding. The split is documented in this file under *State as of last session*.
- **TDD discipline.** Before changing a system, write a characterization test that pins its current behaviour. Run tests continuously (`dotnet watch test --project <test-project>` or via test runner per change). Never commit a non-green test project — skip failing tests with a clear `[Fact(Skip="reason: ...")]` and document why.

### Locked decisions (don't re-litigate)
- Target: **.NET 10 LTS** server, **Flutter web** operator frontend, **Docker Compose** as canonical deployment.
- gRPC: **`Grpc.AspNetCore`** in a single Kestrel pipeline (same port 443 for REST + gRPC), `Grpc.Core` 2.46.6 kept on netstandard2.0 projects for kiosk consumption only.
- ACME: **`Certes` 3.x** library — kept (not replaced).
- Cert-based licensing: **delete it entirely** (`LeapCertLicense`, `ILeapCertLicense`, `ClientIdentity`, `ClientRole`, `ClientCertificateChain`/`PrivateKey` fields in `ServerConfig`/`CredentialConfig`, `StaticServerConfig`'s embedded production CA + hostname, the Setup-wizard cert pages, `_Certificates/grpc client*/`). Orphaned by design — replaced by the `(StationApiKey.PublicKey, Secret)` model.
- FFmpeg: **build script downloads** (not committed). README states the GPL implication for the binaries.
- Top-level LICENSE: **Apache 2.0**.
- Kiosk runtime: **minimum-touch**. Don't migrate WPF runtime, don't restructure modules.
- VR runtime decision (fix-up vs document gap): **defer to after Phase 7** (Flutter frontend) per user instruction.

### State as of last session

> **Current state overlay (2026-05-17):** All planned phases (1-8) are complete. **No version tag has been cut yet** — the release is intentionally deferred to the *final* action on the repo at the maintainer's call (per repo convention; do not pre-emptively `git tag`). The Phase-6 round-trip (`Connect → SendLoginIntention → operator approves → SendLogoutRequest`) has been verified end-to-end live against the docker-compose stack; all kiosk-runtime bugs uncovered by the live `LeapPlay.Shell.exe -debug` walkthrough have been fixed. The body of this prompt below documents the migration journey; for what's actually *open* see `CHANGELOG.md`'s top section. The remaining open kiosk items are: end-to-end Steam *launch* verification on a box where the station's Steam credentials let Steam log in automatically (station-config concern, not a code bug), and the proper-fix path for session-end-reason plumbing from server to kiosk (documented limitation, v1.x).

**Branches** (all local, no remote):
- `migration/dotnet10` — main migration branch, current HEAD `f666577`
- `migration/dotnet10-services` — agent-S work, **already merged**
- `migration/dotnet10-grpc` — agent-G work, **already merged**

**Latest commit on `migration/dotnet10`** (verify with `git log -1`):
- `f666577 fix(steam): robustness fixes uncovered by the live -d walkthrough`

**Phase 1 status: COMPLETE.** All 12 of the original Phase-1 plan tasks are done.

**Phase 2 status: COMPLETE.** EF Initial migration regenerated (`Pod.Data/Migrations/20260516055939_Initial.cs`).

**Phase 3 status: server-side portion COMPLETE.** Kiosk cert-licensing excision (LeapCertLicense, ClientIdentity/ClientRole, ServerConfig cert fields, StaticServerConfig, Setup wizard cert pages) still pending; touches .NET-Framework kiosk that this environment can't smoke-test, so it should be done with care and ideally followed up by a kiosk `-debug` smoke run.

**Phase 4 status: COMPLETE.** LICENSE (Apache 2.0), THIRD_PARTY_NOTICES.md, SECURITY.md, CONTRIBUTING.md, public-facing README.md, sanitised appsettings.json (placeholders for every secret), appsettings.Development.json.example, .gitignore extended for `_Certificates/**/*.{crt,key,pem,srl,p12,pfx}`, all 13 committed TLS artefacts under `_Certificates/` removed (templates `*.cnf` + `cert-create-all.bat` + `cert-create-server.bat` retained).

**Cert file cleanup status:** server-side done. Kiosk side (deleting the corresponding `<None Include="..\_Certificates\...">` entries from `LeapVR.Shell.csproj` etc.) is folded into the kiosk cert-licensing excision task.

**Server-side build status:** 28/28 server projects build clean via `dotnet build <project>.csproj`.

**Note:** `dotnet build PoD.sln` will NOT work — the solution includes .NET-Framework 4.7.1 kiosk projects that require MSBuild + .NET-Framework Developer Pack (built via `LeapVR.Shell.Build/Build_Free.bat`). Treat per-project `dotnet build` as the canonical check for the server stack.

**Server projects (all green, do NOT re-touch unless you have a reason and a test):**
| Project | Status |
|---|---|
| `Pod.Enums` | netstandard2.0 |
| `Pod.Data.Infrastructure` | netstandard2.0 + Newtonsoft 13.0.4 |
| `Pod.Data.Models` | netstandard2.0 |
| `Pod.DtoModels` | netstandard2.0 |
| `Pod.ViewModels` | netstandard2.0 + Newtonsoft 13.0.4 |
| `Pod.ViewModels.Expressions` | netstandard2.0 |
| `Pod.MailEngine` | netstandard2.0 |
| `Pod.Data` | net10.0 + EF Core 10.0.4 + Npgsql 10.0.1 + regenerated Initial migration (20260516055939) |
| `Pod.LetsEncrypt` | net10.0 + Certes 3.0.4 |
| `Pod.Web.Authentication.ApiKeySecret` | net10.0 |
| `Pod.Services` | net10.0 |
| `Pod.Test.Utilities` | net10.0 |
| `Pod.Data.Test` | net10.0 (12 passing, 0 skipped) |
| `Pod.Services.Test` | net10.0 (48 passing, 3 skipped — Postgres-fixture-dependent, see file Skip messages) |
| `Pod.Grpc.Const` (csproj name `Pod.Grpc.Base.Const`) | netstandard2.0 + Grpc.Core 2.46.6 |
| `Pod.Grpc.Messages` | netstandard2.0 + Google.Protobuf 3.27 |
| `Pod.Grpc.Utilities` | netstandard2.0 + Grpc.Core 2.46.6 |
| `Pod.Grpc.Base.Client` | netstandard2.0 + Grpc.Core 2.46.6 |
| `Pod.Grpc.Base` | multi-target netstandard2.0/net10.0 + Grpc.Tools 2.66 |
| `Pod.Grpc.Base.Server` | net10.0 + Grpc.AspNetCore + `GrpcMetadataAuthenticationHandler` ("grpc-station" scheme) — legacy GrpcServer.cs deleted |
| `Pod.Grpc.ConnectHost.Server` | net10.0 |
| `Pod.Grpc.ShellHost.Server` | net10.0 |
| `Pod.Grpc.Base.Server.Test` | net10.0 (15 passing) |
| `Pod.Web.Center` | **net10.0 — COMPLETE.** Grpc.AspNetCore endpoints, Swashbuckle 10.1.7 + Microsoft.OpenApi 2.4.1 (flat namespace), AspNetCoreRateLimit 5.0, Newtonsoft via AddNewtonsoftJson, generic-host Program/Startup. `GrpcServicesServer.cs` deleted; `GrpcHostedServer.cs` reduced to a PublisherHub-shutdown coordinator. Microsoft.EntityFrameworkCore.Design 10.0.4 added so `dotnet ef migrations` resolves the startup project. |
| `Pod.Web.Center.Test` | **net10.0 — NEW.** xunit + Mvc.Testing + EF InMemory + Grpc.Net.Client + FluentAssertions. `PodWebApplicationFactory` boots the full pipeline against InMemory + pre-seeded ShellServer. 4 smoke tests passing (2 unit, 2 integration). |
| `Pod.Web.Client.Rest`, `Pod.Web.Client.Rest.Internal` | netstandard2.0 |

**Deleted:**
- `Pod.Test.Sandbox/` (not a real test project)
- `Pod.Web.Center.3rdParty/AspNetCoreRateLimit/` (replaced with NuGet 5.0)
- `Pod.Web.Center/ServicesHosted/GrpcServicesServer.cs` (legacy `GrpcServer<T>` wrapper)
- `Pod.Grpc.Base.Server/GrpcServer.cs` (`[Obsolete]` documentation stub)

**Removed entirely** (csprojs + sources + per-project docs, after they sat orphan in `PoD.sln` and judged not worth the Terminal.Gui v0→v1 + Grpc.Core→Grpc.Net.Client port effort):
- `Pod.Grpc.ShellClient.Simulator/`
- `Pod.Web.Simulator.StressTest/`
- `docs/server/tools/` (held only the two simulator READMEs)

**Test baseline (current):** 12 + 48 + 15 + 4 = **79 passing / 3 skipped / 0 failing**.

**Pod.Data behaviour change worth noting:** `ContextInitializer.Initialize` now guards `GetPendingMigrations` with `Database.IsRelational()` so InMemory test scenarios don't trip a relational-only method. The migration step is unchanged for Postgres.

**Server entrypoint behaviour change worth noting:** `Pod.Web.Center.Startup` no longer takes `ILogger<Startup>` (the field was never used). This was required to let `WebApplicationFactory<Program>` activate Startup under the test host without a pre-built logger.

**Remaining Phase 3 — kiosk-side (deferred, needs `-debug` kiosk smoke test):**
- Delete `LeapVR.Shell.Domain.Models/CertLicense/`, `LeapVR.Shell.Modules/ShellConfigurator/LeapCertLicense.cs`.
- Verify no live callers of `ClientIdentity`/`ClientRole` in `LeapVR.Shared.Lib/x509/`, then delete.
- Strip `ClientCertificateChain`/`PrivateKey` from `LeapVR.Shell.Domain.Models/Customization/ServerConfig.cs` + `LeapVR.Shell.Services.Interfaces/FileConfig/CredentialConfig.cs`.
- Replace `LeapVR.Shell/Bootstrapper.cs` `StaticServerConfig` registration with `ConfigFileRepository<ServerConfig>` and delete `StaticServerConfig`.
- Drop the `<None Include="..\_Certificates\grpc client\…">` blocks from `LeapVR.Shell.csproj`, `LeapVR.Shell.Setup.csproj` (or wherever they appear) since those files no longer exist.
- Update kiosk Setup wizard pages to drop cert-file collection.

**Phases 5-8: COMPLETE.**
- Phase 5: Docker Compose deployment shipped (commit `d3facfa`, with follow-ups `5713562`, `2bb1383`). `Pod.Web.Center/Dockerfile` (multi-stage sdk:10.0 → aspnet:10.0, non-root `app` user), root `docker-compose.yml` with `postgres:16-alpine` + server + operator-nginx, `.env.example`, first-run admin seed via env vars, `/health` endpoint backed by EF Core health checks. End-to-end verified live: 3 containers reach `healthy`; full operator critical path (login → station list → mint API key → list keys) runs through the nginx proxy.
- Phase 6: **CLOSED in `v1.0.1`** (round-trip verified live; one known-and-documented limitation around block-screen wording stays open as a v1.x item). Full `Connect → SendLoginIntention → operator approves → SendLogoutRequest` verified end-to-end against the docker-compose stack. The kiosk's session-end block-screen wording is a `SessionLimitReached` placeholder for any non-station-initiated session end, because the server's actual `Session.StopReason` isn't plumbed through the gRPC `GetSessionState` return — see `docs/usage/kiosk-known-issues.md` and `CHANGELOG.md` → `[1.0.1]` → *Known limitations*.
- Phase 7: Flutter operator frontend committed at `flutter_operator/` (commit `6511718`). Fresh Dart 3 implementation under this repo (not the legacy `leap_play_x_app` sibling). Covers v1.0.0 critical path: login, station list/detail, API-key mint + list.
- Phase 6.5: VR runtime fix decision still deferred — documented as "VR runtime gap" in `docs/usage/kiosk-known-issues.md`.
- Phase 8: doc sweep + `CHANGELOG.md` + `v1.0.0` tag landed in `5178f1f`/`a1bcbcf`/`de2c312`. Tag was subsequently moved forward to `505611d` to include the first batch of kiosk-runtime fixes uncovered by the live walkthrough.

### The actual work, in order

#### Phase 1 finish — Pod.Web.Center code migration (estimated: most of one focused session)

The `Pod.Web.Center.csproj` is on net10 with all new packages. The code now fails to compile in ~30-50 places. Tackle by category:

1. **JwtBearer namespace** — `Microsoft.AspNetCore.Authentication.JwtBearer` is now a separate package (already in csproj). Add `using Microsoft.AspNetCore.Authentication.JwtBearer;` where needed. Affects: every controller, Startup, `Config/ConfigureJwtBearerOptions.cs`.

2. **Swashbuckle 4 → 7 (OpenAPI types)** — `SwaggerDocument` → `OpenApiDocument`, `Operation` → `OpenApiOperation`, `IParameter` → `OpenApiParameter`, `Response` → `OpenApiResponse`, `ApiKeyScheme` → `OpenApiSecurityScheme`. Filters now have `Apply(OpenApiDocument, DocumentFilterContext)` signatures. Affects: `Swagger/EnumDocumentFilter.cs`, `Swagger/AuthorizationOperationFilter.cs`, `Swagger/ApiExplorerGroupPerVersionConvention.cs`, `Swagger/OnlyApiResponseAndRequestFilterOrdered.cs`, `Swagger/SchemaIdStrategy.cs`, `Startup.cs` (AddSwaggerGen options).

3. **Kestrel-internal API removal** — `Authentication/ApiKeySecretValidator.cs` uses `Microsoft.AspNetCore.Http.Internal` (`EnableRewind()`). Replace with `Request.EnableBuffering()`. Mirror the fix I already did in `Pod.Web.Authentication.ApiKeySecret/ApiKeySecretHandler.cs` for the IHeaderDictionary pattern.

4. **Rewrite.Internal removal** — `Presenter/ResultPresenter.cs` uses `Microsoft.AspNetCore.Rewrite.Internal`. Use the public `Microsoft.AspNetCore.Mvc.IUrlHelper` / `LinkGenerator` instead.

5. **`GrpcServer<T>` removal** — `ServicesHosted/GrpcServicesServer.cs` is an `IHostedService` that used the legacy `Pod.Grpc.Base.Server.GrpcServer<T>` (which agent-G turned into an `[Obsolete]` empty stub). **Delete `GrpcServicesServer.cs` entirely.** Replace with endpoint registration in `Startup.Configure`:
   ```csharp
   app.UseEndpoints(endpoints => {
       endpoints.MapGrpcService<Pod.Grpc.ConnectHost.Server.ConnectHostServiceGrpc>();
       endpoints.MapGrpcService<Pod.Grpc.ShellHost.Server.ShellHostServiceGrpc>();
       endpoints.MapGrpcService<Pod.Grpc.ShellHost.Server.ShellApplicationServiceGrpc>();
       endpoints.MapControllers();
       endpoints.MapRazorPages();
   });
   ```

6. **Program.cs rewrite** — `WebHost.CreateDefaultBuilder` → either keep the Startup pattern with `Host.CreateDefaultBuilder().ConfigureWebHostDefaults(...).UseStartup<Startup>()` OR switch to the minimal-hosting `WebApplication.CreateBuilder(args)` pattern. **Recommendation: keep Startup.cs** (less invasive). The Kestrel `ServerCertificateSelector` callback for LetsEncrypt stays unchanged.

7. **Startup.cs** —
   - `ConfigureServices`:
     - `services.AddMvc()` → `services.AddControllers().AddNewtonsoftJson(...)` + `services.AddRazorPages()`.
     - `services.AddGrpc();` (NEW — from `Grpc.AspNetCore`).
     - Auth registration: add `.AddGrpcStationMetadata()` to the chain after JwtBearer + `amx` (the extension method is in `Pod.Grpc.Base.Server.GrpcMetadataAuthenticationExtensions`).
     - Register `IGrpcStationCredentialVerifier` → `DefaultGrpcStationCredentialVerifier` (or `services.AddSingleton<IGrpcStationCredentialVerifier, DefaultGrpcStationCredentialVerifier>()`).
     - `AspNetCoreRateLimit` 5.x: API changed slightly — `services.AddInMemoryRateLimiting()` is now `services.AddInMemoryRateLimiting()` plus `services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>()`.
     - Swashbuckle: `AddSwaggerGen` options have shifted — the `c.SwaggerDoc(...)` calls now take `OpenApiInfo` instead of `Info`.
     - `services.Configure<IISServerOptions>(...)` if you need the request body size limit raised for the 20MB `amx` hash.
   - `Configure`:
     - `IHostingEnvironment` → `IWebHostEnvironment`.
     - Replace `app.UseMvc()` with `app.UseRouting()` + `app.UseAuthentication()` + `app.UseAuthorization()` + `app.UseEndpoints(...)`.
     - Inside `UseEndpoints`: map controllers, Razor pages, and the 3 gRPC services.
     - `app.UseIpRateLimiting()` stays (5.x is backward-compatible at this call site).
   - Drop the registration of `Pod.Web.Center.ServicesHosted.GrpcServicesServer` (deleted in #5).

8. **Build iteratively**: after each category, `dotnet build Pod.Web.Center/Pod.Web.Center.csproj` and fix the next batch. Target: 0 errors.

9. **Wire `Pod.Web.Center.Test` into the solution** (`dotnet sln add Pod.Web.Center.Test/Pod.Web.Center.Test.csproj`) and run its smoke tests. Add a real `WebApplicationFactory<Program>`-based test for `GET /api/v1/account/login` (anonymous endpoint) and one for `[Authorize]` rejection.

10. **Regenerate EF migrations**: `dotnet ef migrations add Initial -p Pod.Data -s Pod.Web.Center`. One fresh migration that captures the current entity model. Commit it.

11. **Re-enable the skipped tests** in `Pod.Services.Test` (the gRPC integration test) and `Pod.Data.Test` (the API-key one). Wire them against the new `PodWebApplicationFactory` from `Pod.Web.Center.Test/Fixtures/`.

12. **Resurrect `SessionFsmTests`** in `Pod.Data.Test` against the `InMemoryDbContextFactory` (saving the entity first so `StationId` is populated). 13+ FSM transition tests covering Requested → Delivered → Started → Ended plus all failure terminals.

**Done when:** `dotnet build` of the whole solution = 0 errors, `dotnet test` of all test projects = 0 failures, 0 unexpected skips.

#### Phase 3 — dead-code excision (parallelizable with #4 if you spawn an agent)

- Delete `LeapVR.Shell.Domain.Models/CertLicense/ILeapCertLicense.cs`.
- Delete `LeapVR.Shell.Modules/ShellConfigurator/LeapCertLicense.cs`.
- Delete `ClientIdentity`, `ClientRole` enum (in `LeapVR.Shared.Lib/x509/`) **only after grep confirms no live caller** (the gRPC code does NOT use them; verify).
- Strip `ClientCertificateChain` + `PrivateKey` from `LeapVR.Shell.Domain.Models/Customization/ServerConfig.cs` and `LeapVR.Shell.Services.Interfaces/FileConfig/CredentialConfig.cs`.
- Replace `LeapVR.Shell/Bootstrapper.cs:223` `StaticServerConfig` registration with file-backed `ServerConfig` (registered via `ConfigFileRepository<ServerConfig>`). Delete `StaticServerConfig` (the class with the embedded production CA cert + hostname).
- Delete `_Certificates/grpc client/`, `_Certificates/grpc client internal/`.
- Delete `_Certificates/ssl create/{server,ca}.{crt,key,srl}` (keep `*.cnf` + `cert-create-all.bat`).
- Update kiosk Setup wizard pages that ask for cert files — remove those pages, simplify the wizard to just collect `(StationId, password)` + `(StationApiKey.PublicKey, StationApiKey.Secret)`.
- Delete `Pod.Grpc.Base.Server/GrpcServer.cs` (the `[Obsolete]` stub). It's served its documentation purpose; anyone reading old docs will land on `docs/architecture/grpc.md`.
- Sweep simulator HintPaths in `Pod.Grpc.ShellClient.Simulator/Pod.Grpc.ShellClient.Simulator.csproj` + `Pod.Web.Simulator.StressTest/Pod.Web.Simulator.StressTest.csproj` — replace with PackageReferences (same pattern I used in Phase 1).
- Migrate `Pod.Grpc.ShellClient.Simulator` + `Pod.Web.Simulator.StressTest` to net10.0 (they're dev tools, optional but the audit found broken HintPaths in both).
- Strip `Pod.Web.Center.csproj`'s remaining hardcoded `C:\Repositories\pod\PoD\` `DocumentationFile` path (cosmetic, but should be repo-relative).

**Done when:** every grep for orphan-cert classes returns zero hits in source (only in deletion-history commits + docs that explain "previously had X").

#### Phase 4 — secrets / licensing / hygiene

- **`Pod.Web.Center/appsettings.json`**: replace every committed secret with placeholder (`<set via dotnet user-secrets or env DOTNET_X__Y>`). Specifically:
  - `AuthConfig.SecretKey`
  - `LetsEncryptOptions.EncryptionPassword`
  - `ConfigSuperuser.Password`, `ConfigSuperuser.StationPassword`
  - `ConnectionStrings.PodApiContext` (password segment)
  - Replace `leap-play.com`, `leap-arcade.com`, `vspace-tec.com`, `connect.leap-play.com` with `example.com`.
- Add `appsettings.Development.json.example` with realistic-but-fake values.
- Document `dotnet user-secrets` setup in `docs/usage/server-deployment.md`.
- **`_Certificates/`**: delete every `*.crt`, `*.key`, `*.pem`, `*.srl` from working tree (you'll do history scrub later — or document that the user does it). Keep `*.cnf` + `cert-create-all.bat`.
- **`.gitignore`**: extend with `_Certificates/**/*.{crt,key,pem,srl,p12,pfx}`.
- **FFmpeg**: rewrite `Build_Free.bat` (and add a `setup.sh` for Linux/Docker) to download FFmpeg 4.x or current LTS LGPL build on first run, with a one-line acknowledgement prompt. Don't commit binaries.
- **Top-level `LICENSE`**: write Apache 2.0 text.
- **`THIRD_PARTY_NOTICES.md`**: aggregate `LeapVR.Shell.Build/License/*` entries + every NuGet package's license + Unity runtime EULA + OpenVR BSD + FFmpeg LGPL pointer. Each entry: name, version, license, upstream URL.
- **`SECURITY.md`**: vulnerability reporting address (use `example.com` if user hasn't provided one).
- **`CONTRIBUTING.md`**: build instructions, test instructions, DCO/CLA stance.
- **Root `README.md`**: rewrite as public-facing. Sections: what is this, screenshot/demo (placeholder), quickstart (`docker compose up`), architecture diagram (ASCII), full docs link, license, status statement (production-deployable self-hosted reference impl).

**Done when:** no committed secrets in any tracked file; `git grep -i "password\s*=\|SecretKey" appsettings.json` returns only placeholders; LICENSE + THIRD_PARTY_NOTICES + SECURITY + CONTRIBUTING exist at root.

#### Phase 5 — Docker Compose deployment (parallelizable with #6)

- `Dockerfile` in `Pod.Web.Center/`: multi-stage. Stage 1 = `mcr.microsoft.com/dotnet/sdk:10.0` for restore + publish. Stage 2 = `mcr.microsoft.com/dotnet/aspnet:10.0` for runtime. Expose 443 (gRPC + REST). Run as non-root.
- `docker-compose.yml` at repo root:
  - `postgres:16-alpine` service with named volume.
  - `server` service from the Dockerfile.
  - `letsencrypt-data` named volume for cert persistence.
  - Healthcheck endpoint.
  - Environment from `.env`.
- `.env.example` with: `POD_HOSTNAME`, `LETSENCRYPT_EMAIL`, `LETSENCRYPT_ACCEPT_TOS=true`, `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `JWT_SECRET`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.
- First-run bootstrap: container starts → wait for Postgres → run `dotnet ef database update` (or use the in-app `ContextInitializer` already in `Pod.Data`) → seed admin user from env vars → start app.
- Add `/health` endpoint via ASP.NET Core HealthChecks (`AddHealthChecks().AddDbContextCheck<PodDbContext>()`).
- README "Deploy in 10 minutes" section.

**Done when:** `docker compose up -d` on a fresh VM (or even a clean local Docker) → server responds at `https://localhost/api/v1/account/login` with a 400 (bad payload — meaning the endpoint is reachable and authed correctly). Bonus: deploy to a real domain you control, get a real LE cert.

#### Phase 6 — kiosk compat (after #5 — needs a running server)

⚠️ **`LeapPlay.Shell.exe` only with `-debug` flag**. Production-mode removes Windows shell.

- Build kiosk via `Build_Free.bat` (already works on Phase 0 baseline).
- Point its `ServerConfig.json` at the docker-compose server (`localhost:443`).
- Run with `-debug`.
- Provision a station via the new REST flow (operator login → create station → mint StationApiKey → get PublicKey+Secret).
- Walk through: `Connect` → `SendLoginIntention` → operator approves → station picks up via `GetLoginIntention` → `SendLoginResponse(true)` → session Active → `SendLogoutRequest`.
- Fix what breaks. Minimum-touch — don't restructure modules. The proto contract is unchanged so most things should just work; expected pain points: TLS validation (LE cert vs StaticServerConfig embedded CA — now gone after Phase 3), connection string in kiosk's `ServerConfig.json` placeholder defaults.

**Done when:** end-to-end session lifecycle works through the modernized server. Document any unfixable VR-runtime gap in `docs/usage/kiosk-known-issues.md`.

#### Phase 7 — Flutter operator frontend

- First step: read the existing partial app at `D:\Repository\Software\PlayOnDemand\leap_play_x_app`. Note state-management choice, routing, API-client structure. Continue in their style.
- Generate Dart API client from the new Swagger (use `openapi-generator-cli` or `swagger_parser`).
- Build flows in this order:
  1. Login (JWT)
  2. Station list + create new station
  3. Station detail (settings, mode)
  4. ApiKey management (create + show Secret once + revoke)
  5. Active session monitor (real-time via SSE/SignalR if the server has it, otherwise poll)
  6. Session history
  7. Billing view (SubscriptionState, recent orders)
  8. Basic admin settings
- Test against the docker-compose server. Use Flutter web (not mobile) — operator UI doesn't need mobile.
- Build artefact: `flutter build web` → static files. Add an nginx sidecar to the docker-compose for serving them, OR embed in `Pod.Web.Center`'s wwwroot.
- Update root README screenshot.

**Done when:** an operator can run `docker compose up`, navigate to `https://example.com`, log in, create a station, mint its keys, see live session traffic, all without touching anything else.

#### Phase 6.5 — VR runtime fix decision (after Phase 7)

Try the kiosk with current SteamVR. If `OpenVrModule` works → great. If broken: either spend a session fixing the binding update OR document the gap as "PRs welcome" with a clear scope in `docs/usage/kiosk-known-issues.md`. User's call which.

#### Phase 8 — final docs + release tag

- Audit every `docs/architecture/*.md` against the new architecture (no `Grpc.Core` server references, no `LeapCertLicense`, no `StaticServerConfig`, no `WebHost.CreateDefaultBuilder`, etc.).
- `docs/usage/server-deployment.md` (Docker Compose flow).
- `docs/usage/operator-frontend.md` (Flutter web flow).
- `docs/usage/admin-tasks.md` (CLI tasks: create operator, seed admin, rotate JWT secret, regen LE cert).
- `docs/architecture/build-and-deploy.md` rewritten for net10 + Docker.
- `docs/open-source-readiness.md` → reframed as "release checklist complete" with every item ticked.
- Update `docs/server/Pod.Web.Center/README.md`, `docs/server/Pod.Services/README.md`, `docs/server/data/Pod.Data/README.md` for the new architecture.
- Top-level `README.md` finalized.
- `CHANGELOG.md` for v1.0.0.
- **Tag `v1.0.0`** (`git tag -a v1.0.0 -m "Initial open-source release"` — do not push).

**Done when:** every doc reflects reality, tag exists locally, the user can review and push when they're ready.

### Workflow rules

- **Parallel via worktrees** (use `Agent` tool with `isolation: "worktree"`): when remaining work has 3+ independent chunks, spawn agents. Especially useful for: cross-cutting doc sweeps, simultaneous controller fixes, Phase 3 cert-licensing excision in parallel with Phase 4 secrets cleanup.
- **Commit cadence**: every logical unit (one project migration done, one phase complete, one bug fixed). Aim for 10-30 line diffs per commit when feasible. Conventional-commits style is fine but not strict.
- **Test continuously**: after every meaningful code change, `dotnet test` the affected project. Before committing, `dotnet test` everything.
- **When stuck on an API change**: WebFetch the official .NET docs first (don't guess at API renames — your training data is from before the latest .NET 10 release).
- **Use `Agent` tool with `Plan` subagent type** when designing a non-trivial refactor before writing code.
- **`docs/` is canonical** — when you change behaviour, update the relevant `docs/architecture/*.md` and `docs/server/*/README.md` in the same commit.
- **Skip don't disable**: failing test → `[Fact(Skip="reason")]`. Never delete or comment out a test to make the build pass.
- **End every session with**: a clean commit, a status update at the top of this file (`State as of last session` section), and the agent worktrees cleaned up.

### Definition of done (the whole thing)

- `git tag -l` shows `v1.0.0`.
- `dotnet build` (whole solution) = 0 errors.
- `dotnet test` (all test projects) = 0 failures, <5 skipped with explicit reasons.
- `docker compose up` on a fresh VM → working server with TLS, admin can log in.
- Flutter web bundle deployed alongside, operator can complete the full flow.
- A kiosk built from `Build_Free.bat` and configured against the server runs a full session end-to-end (with `-debug` only).
- Every doc in `docs/` matches reality.
- Top-level `README.md`, `LICENSE` (Apache 2.0), `THIRD_PARTY_NOTICES.md`, `SECURITY.md`, `CONTRIBUTING.md` all exist and are accurate.
- No committed secrets, no hardcoded production hostnames, no orphan dead code.

Until you reach Definition of Done, loop. Don't ask questions; the reference is the application's existing behaviour. Make the best technical call when forced to choose, document the call in the relevant doc, and keep moving.

When you genuinely complete it: append a final `### Release notes for v1.0.0` section to this file summarising what shipped.

## --- END PROMPT ---

---

## Notes for the user (not part of the prompt)

- Verify the **branch + last commit** in step 0. If you've made changes between sessions, the next Claude should know.
- The `.claude/worktrees/` folder will contain agent-leftover worktrees from the previous session. Safe to `git worktree remove` each one, or just delete the directory.
- The user-secrets ID committed in `Pod.Web.Center.csproj` (`8d0f9b82-...`) is fine to keep — it's not a secret, just an identifier for the `dotnet user-secrets` store on disk.
- If you'd rather have me write a `DECISIONS.md` file alongside this prompt that captures *why* we picked .NET 10 / Flutter web / Docker / Apache 2.0 (so future-Claude doesn't re-litigate), say the word.

---

## Release notes for v1.0.0

Tagged locally on 2026-05-16 as `v1.0.0`. Currently at commit `505611d` (the tag was moved forward to include the first batch of kiosk-runtime fixes uncovered by the live `-debug` walkthrough: SDK-migrate of `LeapVR.Shell.*` WPF library projects so BAML lands at runtime, the kiosk Steam library-folders parser handling the post-2021 `libraryfolders.vdf` schema, etc.). Not pushed. **Five additional fixes have landed since the current tag location** (commit `56d211d` → `f666577`) — see `CHANGELOG.md` → `[Unreleased]`. The user controls whether to move the `v1.0.0` tag again or cut a `v1.0.1` patch tag for those.

### What this release is
The first open-source cut of PlayOnDemand. Server stack ported from ASP.NET Core 2.1 (EOL) to .NET 10 LTS with `Grpc.AspNetCore` on a single Kestrel pipeline, Docker Compose deployment, all secrets sanitised, full licensing/attribution paperwork in place. The kiosk is unchanged at runtime; only the orphaned cert-licensing plumbing was excised. The Flutter operator frontend ships as a separate concern (sibling repo, documented as v1.x deferred work).

### Verifiable acceptance criteria
- `git tag -l` shows `v1.0.0`.
- Per-project `dotnet build` of all 28 server-side projects: 0 errors.
- `dotnet test` across all 4 test projects: 12 + 49 + 15 + 11 = **87 passing / 3 skipped (Postgres-dependent, documented) / 0 failing**.
- `LeapVR.Shell.Build\Build_Free.bat` runs end-to-end: server DLL, kiosk EXE, Content Creator EXE, and unsigned Inno installer all produced at canonical paths. 0 errors.
- `docker compose up` smoke: 3 containers reach `healthy`; SPA loaded in a real browser at `http://localhost:8080/` makes authenticated `/api/v1/*` calls through the nginx proxy. Verified flows: login (good + bad creds), station list, API-key mint (Secret returned once), API-key list (Secret field absent).
- Top-level `LICENSE` (Apache 2.0), `THIRD_PARTY_NOTICES.md`, `SECURITY.md`, `CONTRIBUTING.md`, `CHANGELOG.md`, `README.md` all present and current.
- Zero committed secrets in any tracked file (every `appsettings.json` secret replaced with `<set-via-env-or-user-secrets>`).
- Zero hardcoded production hostnames (`leap-play.com`, `leap-arcade.com`, `vspace-tec.com`, `*.leap-vr.cn` all replaced with `example.com` / `localhost`).
- Zero committed TLS artefacts under `_Certificates/` (13 files removed; templates `*.cnf` + `cert-create-*.bat` retained).
- Zero references to the excised cert-licensing classes (whole-tree grep clean for `LeapCertLicense`, `ILeapCertLicense`, `StaticServerConfig`, `ClientIdentity`, `ClientRole`, `LicenseRole`, `CryptoHelpers`).
- `Pod.Web.Center/Dockerfile`, `docker-compose.yml`, `.env.example` all present at repo root or in `Pod.Web.Center/`.

### Verified in this release
- `docker compose up -d` on a real Docker host (WSL2 Ubuntu 24.04). 3 containers (postgres-16-alpine, server, operator nginx) reach `healthy`. Full operator critical path verified live: login → station list → mint API key (Secret returned once) → list keys (Secret absent). Verified via curl AND via a real Chrome browser pointing at `http://localhost:8080/` (the Flutter web SPA executes `fetch` against the proxied `/api/v1/*` endpoints and gets the expected responses, including 400 + Identity-policy-message on a bad password).
- Full `LeapVR.Shell.Build\Build_Free.bat` end-to-end. The kiosk and Content Creator csprojs were SDK-migrated to `Microsoft.NET.Sdk.WindowsDesktop` so the WPF target chain loads cleanly under MSBuild 18 (bundled with .NET 10 SDK). VS 2022 17.14 + .NET 10 SDK 10.0.204 is now sufficient; the previous "VS 17.15+" caveat is removed.
- Flutter operator frontend (`flutter_operator/`) is the v1.0.0 reference implementation: Dart 3, fresh repo. The legacy `leap_play_x_app` sibling repo is not part of v1.0.0.

### NOT verified by this release (deferred / environmental)
- End-to-end kiosk session smoke (the runtime walk-through `Connect → SendLoginIntention → operator approves → SendLogoutRequest`). Documented as a manual acceptance test in `docs/usage/kiosk-known-issues.md`. **Always launch with `-debug` on a developer machine** — production mode replaces `explorer.exe` as the Windows shell, so this is intentionally never automated.

### What changed at a glance
See `CHANGELOG.md` for the full Added/Changed/Removed/Security breakdown. Commit graph (newest first):

```
c997c72 phase 3+6 finalise: kiosk + Content Creator on SDK-style WPF (net471)
65db4e0 phase 3+6: kiosk-side migration completes for 7 of 9 sub-projects under dotnet msbuild
de2c312 docs: refresh README + CHANGELOG + usage docs for the v1.0.0 cut
6511718 phase 7: Flutter operator frontend - minimal v1.0.0 reference + nginx integration
2bb1383 phase 5: docker compose actually works - 3 more bugs found by `docker compose up`
5713562 phase 5+: fix StationApiKey secret leak on list endpoint
d3facfa phase 5: docker-compose deployment
8b6d420 phase 3: kiosk-side cert-licensing excision (the (StationId, Password) model wins)
8d4b086 phase 4: open-source release plumbing
f02e5d2 phase 3: server-side dead code excision + simulator net10 csproj bump
7e7461a phase 1: re-enable Pod.Data.Test station-apikey test on InMemory + sharpen remaining skip messages
6dcf43a phase 2: regenerate EF Initial migration for net10 + EF Core 10
33220d9 phase 1: Pod.Web.Center.Test wired up + WebApplicationFactory smoke green
53058d1 phase 1: Pod.Web.Center compiles on net10.0
b3ddb86 phase 1+2: Pod.Web.Center csproj migrated; code migration in progress
9fbb5b7 merge: gRPC stack migrated to Grpc.AspNetCore + Pod.Grpc.Base.Server.Test (agent-G)
b79e947 merge: Pod.Services characterization tests + fixtures + docs (agent-S round 2)
3db6ea1 phase 1 (partial): migrate 4 server projects to .NET 10
```

### What a new contributor should read first
1. [README.md](../../README.md)
2. [docs/usage/server-deployment.md](../usage/server-deployment.md)
3. [docs/architecture/overview.md](../architecture/overview.md) → [auth.md](../architecture/auth.md) → [grpc.md](../architecture/grpc.md)
4. [CONTRIBUTING.md](../../CONTRIBUTING.md)

### To push the release
The user runs `git push origin migration/dotnet10 v1.0.0` when they're ready. This release was tagged locally per the original prompt's "no remote push" constraint.
