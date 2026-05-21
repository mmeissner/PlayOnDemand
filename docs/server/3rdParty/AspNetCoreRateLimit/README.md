# AspNetCoreRateLimit (vendored)

> Source-vendored copy of [`stefanprodan/AspNetCoreRateLimit`](https://github.com/stefanprodan/AspNetCoreRateLimit) v3.0.5. ASP.NET Core middleware for IP- and client-ID-based request rate limiting. Used as the very first middleware in the `Pod.Web.Center` pipeline.

## Purpose

The public REST API in `Pod.Web.Center` is exposed on the open internet. Without rate limiting, registration endpoints, password-reset endpoints, and the login endpoint are trivially abusable (account enumeration, password brute force, mailbomb via reset emails). This middleware solves all four cases with one library:

- Per-endpoint configurable limits (e.g. `1 register / 2s`, `5 password resets / 12h`, `5000 calls / 12h global`)
- Per-IP isolation
- Per-client-ID isolation (different limits for known clients via the `X-ClientId` header)
- Returns `429 Too Many Requests` with a `Retry-After` header when exceeded

The library is **vendored** (i.e. checked into the repo as source under `Pod.Web.Center.3rdParty/AspNetCoreRateLimit/`) rather than consumed via NuGet. See "Why vendored" below.

## What it provides

| Public class | Role |
|---|---|
| `IpRateLimitMiddleware` | The middleware applied via `app.UseIpRateLimiting()` in `Startup.Configure`. Inspects the request, looks up the matching rule, increments a counter, blocks if exceeded. |
| `ClientRateLimitMiddleware` | Equivalent for client-ID-based limiting. **Not currently used by `Pod.Web.Center`** — only IP-based is wired up. |
| `IpRateLimitOptions` | Bound to the `IpRateLimiting` config section. Holds `GeneralRules`, `EndpointWhitelist`, `ClientWhitelist`, `RealIpHeader`, `HttpStatusCode`, etc. |
| `IpRateLimitPolicies` | Bound to `IpRateLimitPolicies` — per-IP rule overrides. |
| `IIpPolicyStore` (`MemoryCacheIpPolicyStore`, `DistributedCacheIpPolicyStore`) | Where IP policy data lives. The host registers the in-memory variant. |
| `IRateLimitCounterStore` (`MemoryCacheRateLimitCounterStore`, `DistributedCacheRateLimitCounterStore`) | Where the increment counters live. The host registers the in-memory variant. |
| `IRateLimitConfiguration` (`RateLimitConfiguration`) | Resolves client IP / client-ID from a request. Pluggable via `IIpResolveContributor` / `IClientResolveContributor`. |
| `IpRateLimitProcessor`, `ClientRateLimitProcessor` | The actual rule-matching + counter-increment logic. |
| `Extensions` (in `Core/Extensions.cs`) | Helpers like wildcard matching for endpoint glob patterns. |

## How `Pod.Web.Center` uses it

DI registration in `Startup.ConfigureServices`:

```csharp
services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));
services.Configure<IpRateLimitPolicies>(_configuration.GetSection("IpRateLimitPolicies"));
services.AddMemoryCache();
services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

Pipeline insertion in `Startup.Configure` — must be **first**, before authentication:

```csharp
app.UseIpRateLimiting();   // BEFORE everything else
// ... developer exception page / exception handler ...
// ... LetsEncrypt branch ...
// ... Authentication / MVC / static files ...
```

Policy seeding at startup in `Program.cs`:

```csharp
var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();
await ipPolicyStore.SeedAsync();   // pulls IpRateLimitPolicies.IpRules into the store
```

Default rules in `Pod.Web.Center/appsettings.json` (`IpRateLimiting.GeneralRules`):

| Endpoint | Period | Limit |
|---|---|---|
| `post:/api/v1/accounts/register` | 2 s | 1 |
| `post:/api/v1/accounts/password/forgot` | 15 m | 5 |
| `post:/api/v1/accounts/password/reset` | 12 h | 5 |
| `post:/api/v1/accounts/email/confirmation` | 1 d | 5 |
| `post:/api/v1/accounts/email/confirmation/send` | 1 m | 3 |
| `*` (catch-all) | 12 h | 5000 |

## Why vendored (not NuGet)

The library is checked into the repo as a `csproj` reference rather than consumed via NuGet. There is no commit message explaining why, but the most plausible reasons given the codebase context:

1. **No upstream patches.** The csproj sets `<PackageId>AspNetCoreRateLimit</PackageId>` and `<Version>3.0.5</Version>`, identical to upstream — this strongly suggests no local modifications were made. The vendoring is for **build determinism**, not patching.
2. **Targets `netstandard2.0`** with an ASP.NET Core 2.1 host. The 3.x line of the upstream package supports this combination, but newer upstream versions (4.x+) require ASP.NET Core 3.x — which the project cannot use because of the SDK pin to 2.1.818. Vendoring locks the version where it works.
3. **The `Pod.Web.Center.3rdParty/` folder** sits outside the `_Tools/` and `packages/` conventions. The repo uses this folder pattern for a reason that's not documented elsewhere — likely a one-off carve-out so 3rd-party source can be edited if a critical fix becomes necessary, without round-tripping through a fork on GitHub.

If the system ever moves to .NET 6+, the vendored copy can be replaced with a NuGet reference to `AspNetCoreRateLimit` 4.x or higher (breaking changes apply — see the upstream 3.0 → 4.0 migration notes).

## Internal structure

Mirror of upstream v3.0.5:

```
AspNetCoreRateLimit/
├── README.md, LICENSE.md
├── AspNetCoreRateLimit.csproj   netstandard2.0; PackageId=AspNetCoreRateLimit; Version=3.0.5
├── AsyncKeyLock/                async-friendly per-key lock used by counter increments
├── Core/                        ClientRateLimitProcessor, IpRateLimitProcessor, RateLimitProcessor,
│                                 WildcardMatcher, IRateLimitProcessor, Extensions
├── CounterKeyBuilders/
├── Middleware/                  IpRateLimitMiddleware, ClientRateLimitMiddleware, RateLimitMiddleware,
│                                 IRateLimitConfiguration, RateLimitConfiguration, MiddlewareExtensions
├── Models/                      IpRateLimitOptions, ClientRateLimitOptions, RateLimitRule,
│                                 RateLimitPolicy, IpRateLimitPolicies, ClientRateLimitPolicies,
│                                 ClientRequestIdentity, QuotaExceededResponse, RateLimitCounter,
│                                 RateLimitHeaders, RateLimitOptions
├── Net/                         IP parsing helpers
├── Resolvers/                   IClientResolveContributor + ClientHeaderResolveContributor;
│                                 IIpResolveContributor + IpConnectionResolveContributor / IpHeaderResolveContributor
├── Store/                       IIpPolicyStore, IClientPolicyStore, IRateLimitCounterStore (+ MemoryCache + DistributedCache impls)
└── Properties/
```

## Notable patterns / gotchas

- **The middleware MUST be first.** `app.UseIpRateLimiting()` is the very first call in `Startup.Configure`. If you put it after authentication or static files, those middlewares will run for blocked requests too.
- **In-memory stores only.** The host wires `MemoryCache*` variants. This means **rate-limit counters do not survive a process restart**, and **multiple `Pod.Web.Center` instances behind a load balancer would each have independent counters** — i.e. a customer could trivially bypass limits by spreading requests. If horizontal scaling is ever introduced, swap to `DistributedCache*` + Redis.
- **`ClientRateLimitMiddleware` is included but not registered.** Only IP-based limiting is active.
- **`RealIpHeader = "X-Real-IP"`** — the host expects the immediate client IP in `X-Real-IP`. If a CDN or reverse proxy rewrites this differently (e.g. `X-Forwarded-For`), update both this config and verify the proxy actually overwrites (not appends to) the header.
- **`HttpStatusCode = 429`** is the default and is set explicitly in config. Don't change without checking client-side handling in `Pod.Web.Client.Rest` (which currently doesn't special-case 429).
- **Memory cache thread-safety caveat.** `Startup.ConfigureServices` has comment links to [aspnet/Caching#359](https://github.com/aspnet/Caching/issues/359) — `IMemoryCache.GetOrAdd` is not strictly thread-safe in 2.1 and the same instance is shared with `ApiKeySecretValidator`. Don't pile more cache consumers on without testing under load.

## Consumers

- `Pod.Web.Center` — the only consumer. Project reference path: `..\Pod.Web.Center.3rdParty\AspNetCoreRateLimit\AspNetCoreRateLimit.csproj`.

## Related docs

- [`docs/server/README.md`](../../README.md) — server-tier overview (the rate limiter sits in front of the entire pipeline)
- [`docs/server/Pod.Web.Center/`](../../Pod.Web.Center/) — `Startup.cs` shows the registration + pipeline insertion
- Upstream wiki: https://github.com/stefanprodan/AspNetCoreRateLimit/wiki
