# Pod.LetsEncrypt

> Self-contained ACME v2 client (built on Certes) packaged as ASP.NET Core middleware + hosted service. When enabled, automatically issues and renews Let's Encrypt TLS certs for the Kestrel listener with zero ops involvement.

## Purpose

`Pod.Web.Center` runs on a public domain (`api.example.com` or whatever the operator wires up via `POD_HOSTNAME` in `.env`) and needs a valid TLS certificate for HTTPS. Rather than relying on an external acme client (`certbot`, `win-acme`, …) wired into the deploy pipeline, the system ships its own: this project. When `LetsEncryptOptions.IsEnabled = true`, the server itself talks to Let's Encrypt's ACME v2 endpoint, performs the HTTP-01 challenge against itself (port 80), receives the cert, and hands it to a custom `ServerCertificateSelector` that Kestrel uses for the HTTPS listener (port 443).

The whole thing is one library, no external CLI, no PowerShell, no scheduled task. The renewal loop is a `Timer`-driven `IHostedService` that ticks every 12 hours and renews any cert within `DaysBefore` of expiry (default 15).

The assembly name is `Pod.LetsEncryptMiddleware` (note: differs from the project name) — historical and not worth changing.

## Tech

- **Target framework:** `net10.0`
- **AssemblyName:** `Pod.LetsEncryptMiddleware`
- **Version:** 1.4.0.0 (in csproj)
- **Key NuGet packages:**
  - `Certes` 3.0.4 — pure-C# ACME v2 client (does the actual protocol work)
  - ASP.NET Core 10 framework reference — for `IApplicationBuilder`, `IHostedService`, `KestrelServerOptions`, `IConfigureOptions<>`
- **Project references (in this repo):** **none.** This library is fully standalone — no `Pod.*` deps.

## Responsibility

**Is responsible for:**
- ACME v2 account creation + key persistence (PEM file in the cache folder, or supplied via config)
- Periodic certificate renewal (every 12 h, renew if `<= DaysBefore` days to expiry)
- HTTP-01 challenge: storing tokens in `IHttpChallengeResponseStore` and serving them at `/.well-known/acme-challenge/<token>` via `HttpChallengeResponseMiddleware`
- Providing the obtained `X509Certificate2` to Kestrel via a `CertificateSelector` exposed through `ServiceLocator` (the host wires it into `httpsOptions.ServerCertificateSelector` in `Program.cs`)
- Configuring Kestrel HTTP/HTTPS listeners (`KestrelOptionsSetup`) when ACME is enabled
- Exposing optional `ILetsEncryptHook` extension points so the host can react to refresh stages (e.g. send an alert email on failure)

**Is NOT responsible for:**
- Choosing **whether** to enable ACME — that's `Pod.Web.Center`'s call (`LetsEncryptOptions.IsEnabled` from config)
- Actually binding ports — `Pod.Web.Center/Program.cs` does that, conditional on the same flag
- Storing certs anywhere other than the configured `CacheFolder` (a folder under `ContentRootPath`)

## Public API surface

| Type | Purpose |
|---|---|
| `Extensions.AddLetsEncrypt(this IServiceCollection)` | Register all the engine's services into DI. Called from `Startup.ConfigureServices` when `IsEnabled`. |
| `Extensions.UseLetsEncrypt(this IApplicationBuilder)` | Maps the challenge middleware under `/.well-known/acme-challenge/`. Called from `Startup.Configure` (inside the `MapWhen` branch for ACME requests). |
| `Config.LetsEncryptOptions` | Bound to the `LetsEncryptOptions` config section. `IsEnabled`, `EmailAddress`, `Hosts[]`, `DaysBefore`, `CacheFolder`, `EncryptionPassword`, `UseStagingServer`, `AcceptTermsOfService`. |
| `Config.LetsEncryptConst` (in `ConstantsChallengeRoute.cs`) | `ChallengePath` — the path the host needs to route uniquely. |
| `Config.KestrelOptionsSetup` | `IConfigureOptions<KestrelServerOptions>` — sets up endpoints when ACME is enabled. |
| `Services.CertificateSelector` | The thread-safe per-host cert dictionary Kestrel queries. `Use(domain, cert)` updates it. `GetCertificatesAboutToExpire()` drives the renewal loop. |
| `Services.AccountManager` | Persists the ACME account key (PEM file in `CacheFolder`, filename `account`). |
| `Services.CertificateRequestService` | The `IHostedService` that runs the renewal `Timer`. Heart of the renewal loop. |
| `Services.CertificateBuilderService` | Generates the CSR + finalises the order to receive the actual cert bytes. |
| `Services.OrderInfo` | DTO bundling an active ACME `IOrderContext` + its HTTP challenge + domain name (passed between renewal stages). |
| `Services.ServiceLocator` | Static holder for the `CertificateSelector` so `Program.cs` can read it from outside DI when configuring Kestrel. |
| `IHttpChallengeResponseStore` (`InMemoryHttpChallengeResponseStore`) | Pluggable challenge-token store. Default impl is in-memory. |
| `ILetsEncryptHook` | Optional extension. Implement and register multiple instances; `LetsEncryptEventArgs.SendArgs(...)` dispatches to all of them. |
| `Middleware.HttpChallengeResponseMiddleware` | Serves `<token>` → `<key-authorization>` for ACME validation. |

## Internal structure

```
Pod.LetsEncrypt/
├── Extensions.cs                            UseLetsEncrypt() + AddLetsEncrypt() — public entry
├── ILetsEncryptHook.cs                      hook interface + LetsEncryptEventArgs + LetsEncryptStage enum
├── IHttpChallengeResponseStore.cs           store interface (token -> OrderInfo)
├── InMemoryHttpChallengeResponseStore.cs    default impl: ConcurrentDictionary
│
├── Config/
│   ├── LetsEncryptOptions.cs                the bind-target POCO
│   ├── KestrelOptionsSetup.cs               IConfigureOptions<KestrelServerOptions>
│   └── ConstantsChallengeRoute.cs           "/.well-known/acme-challenge"
│
├── Middleware/
│   └── HttpChallengeResponseMiddleware.cs   answers ACME HTTP-01 challenges
│
└── Services/
    ├── CertificateRequestService.cs         the IHostedService — 12 h Timer drives RefreshCertificates
    ├── CertificateBuilderService.cs         CSR + order finalisation
    ├── CertificateSelector.cs               thread-safe per-host cert dict (Kestrel reads from this)
    ├── AccountManager.cs                    GetAccountKey() — read or create-and-persist PEM
    ├── OrderInfo.cs                         per-order state (Order + Challenge + DomainName)
    └── ServiceLocator.cs                    static handoff for Program.cs / Kestrel wiring
```

## Notable patterns / gotchas

- **`AssemblyName = Pod.LetsEncryptMiddleware`** in csproj — the DLL name doesn't match the project name. References in other `csproj`s use the project name (`Pod.LetsEncrypt`), but the on-disk DLL is `Pod.LetsEncryptMiddleware.dll`. Watch for this when reading bin/.
- **`ServiceLocator` exists because `Program.cs` configures Kestrel BEFORE the DI container is built.** The `CertificateSelector` is registered as a singleton in `AddLetsEncrypt(...)`, and the singleton factory writes itself into `ServiceLocator.SetCertificateSelector(...)` as a side effect. `Program.cs` then reads it via `ServiceLocator.GetCertificateSelector()` inside the `httpsOptions.ServerCertificateSelector` callback. Ugly, intentional, replace at your peril.
- **The renewal loop suppresses all exceptions** and reports them via `ILetsEncryptHook` instead. A failed renewal does not crash the host — the existing cert keeps serving until expiry. Implement a hook if you want loud failures.
- **HTTP-01 challenge requires port 80 to be reachable.** When `IsEnabled = true`, `Program.cs` opens both 80 and 443. If a reverse proxy fronts the server, port 80 must be forwarded to this process or the challenge will fail.
- **Host setup of routing in `Program.cs` uses `app.MapWhen(path startswith /.well-known/acme-challenge ...)`** to send only ACME requests through `UseLetsEncrypt()`; everything else goes through `UseHttpsRedirection()`. Don't accidentally invert the predicate.
- **`UseStagingServer = true`** points at LE's staging endpoint (rate limits relaxed, certs untrusted). Always use this in non-production environments.
- **The cert cache folder** (default `certs`, relative to `ContentRootPath`) holds the PEM account key (filename `account`) and `<HostName>.pfx` files. Encrypted with `EncryptionPassword` — protect this folder.
- **`DaysBefore` defaults to 15** but `appsettings.json` overrides to 20.
- **No DNS-01 challenge support** — HTTP-01 only. Wildcard certs are not possible with this implementation.

## Consumers

- `Pod.Web.Center` — the only consumer. Registers via `AddLetsEncrypt()` in `Startup.ConfigureServices`, mounts via `UseLetsEncrypt()` in `Startup.Configure`, and configures Kestrel listeners in `Program.cs`.

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.Web.Center/`](../Pod.Web.Center/) — the host that wires this in
- [`docs/architecture/build-and-deploy.md`](../../architecture/build-and-deploy.md) — production hosting notes
