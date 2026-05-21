# Pod.Web.Client.Rest

> RestSharp-based public REST SDK for the `Pod.Web.Center` API. Wraps token management, refresh, and one extension method per endpoint group.

## Purpose

A C# SDK for any consumer that wants to call the public REST API — internal tooling, integration tests, the stress-test simulator, third-party integrators. Uses RestSharp 106 to handle the HTTP plumbing and ships an `IAuthenticator` implementation (`PodAuthenticator`) that:

1. Logs in with username/password on first use, captures the access + refresh tokens.
2. Schedules a `Timer` to fire 10 seconds before the access token expires.
3. On the next request after the timer fires, transparently calls `auth/refreshToken` to get a fresh access token without disturbing the caller.

Endpoints are added as **C# extension methods** on the `PodRestClient` class, grouped into "areas" via marker extensions (`client.Account()`, `client.Stations()`, `client.Auth()`). The marker methods just return `client` — they exist purely as a syntax aid so callers can write `client.Stations().Get()` for discoverability.

The companion project `Pod.Web.Client.Rest.Internal` adds extension methods for the `api/v1/internal/*` endpoints (admin / support / server). They use the same `PodRestClient`.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:**
  - `RestSharp` 106.6.9 — HTTP client + `IRestRequest` / `IRestResponse` + `IAuthenticator` extension point
- **Project references (in this repo):**
  - `Pod.DtoModels` — request body shapes
  - `Pod.ViewModels` — response deserialisation targets
  - `Pod.Enums` — `UserError` for error response parsing

## Responsibility

**Is responsible for:**
- Wrapping every public (non-`internal`) REST endpoint as a typed extension method
- JSON ↔ DTO serialisation via `JsonNetSerializer` (Newtonsoft.Json under the hood, configured with `StringEnumConverter` to match the server's wire format)
- Bearer token acquisition + automatic refresh
- Translating an unsuccessful response into a `Dictionary<UserError, List<string>>` (`ApiErrorResponseExtensions.GetErrors`)

**Is NOT responsible for:**
- Internal/admin endpoints — `Pod.Web.Client.Rest.Internal` extends this client with those
- The `amx` HMAC scheme — stations sign their own requests (no helper exists today; if needed they would build it on top of `RestRequest` directly)
- Retry / circuit-breaker policies — bring your own (e.g. Polly)
- Telemetry — RestSharp's interceptor surface is available but unused here

## Public API surface

### `PodRestClient` (extends `RestSharp.RestClient`)

```csharp
new PodRestClient(string baseUrl, string username, string password)
client.SetCredentials(string username, string password)   // resets and re-logs-in on next call
client.Logout()                                           // calls auth/logout, invalidates refresh token
```

### `PodAuthenticator` (`IAuthenticator`)

The actual login/refresh state machine. Created internally by `PodRestClient`; you generally don't touch it directly.

### Extension method groups (one per file in `Api/v1/`)

| Marker | File | Endpoints exposed |
|---|---|---|
| `client.Auth()` | `ApiAuthenticationExtensions.cs` | `Login`, `Logout`, `RefreshToken` (all `auth/*`) — these are also called internally by `PodAuthenticator` |
| `client.Account()` | `ApiAccountExtensions.cs` | `Register`, `ChangePassword`, `ForgotPassword`, `ConfirmEmail`, `ResetPassword`, `ResendConfirmationEmail` |
| `client.Stations()` | `ApiStationsExtensions.cs` | `Get` (all stations), `GetCurrentState(stationId)`, `GetSettings(stationId)`, `Create`, plus station-settings update calls |

### Error parsing

```csharp
Dictionary<UserError, List<string>> errors = response.GetErrors();
```

`ApiErrorResponseExtensions.GetErrors` deserialises the `IResult` body the server returns on failure.

### `JsonNetSerializer`

Implements `IRestSerializer` so RestSharp uses Newtonsoft.Json (with `StringEnumConverter` and `AllowNonPublicDefaultConstructor`) instead of its built-in serializer. Required for round-tripping the server's view models.

## Internal structure

```
Pod.Web.Client.Rest/
├── PodRestClient.cs         PodRestClient + JsonNetSerializer + PodAuthenticator
└── Api/v1/
    ├── ApiAuthenticationExtensions.cs   Login / Logout / RefreshToken
    ├── ApiAccountExtensions.cs          Register / Password / Email confirmation
    ├── ApiStationsExtensions.cs         Stations CRUD + state queries
    └── ApiErrorResponseExtensions.cs    response.GetErrors() decoder
```

## Notable patterns / gotchas

- **The "area marker" pattern** (`Account()`, `Stations()`, `Auth()`) is just an extension method that returns the client — it's not a wrapper or a sub-client. This is a deliberate cosmetic choice for IDE auto-complete: typing `client.S` shows `Stations()` first, after which `Get()` autocompletes only the station-area methods.
- **`PodAuthenticator` is not thread-safe** — multiple concurrent requests during a refresh window will each see `_tokenRefreshRequired = true` and each call `auth/refreshToken`. The server is idempotent here (the refresh token isn't single-use) but the client wastes calls. If you hit this, wrap calls in a per-client `SemaphoreSlim`.
- **The refresh `Timer` fires `_tokenRefreshRequired = true` 10 seconds before expiry.** On the next request after that, `Authenticate(...)` will issue the refresh call. So if no requests happen for hours after a token expires, the first one will trigger a refresh — but if the **refresh token itself** has expired, you'll get a failed refresh and then a re-login on the same call (the code falls through `if(!_hasToken)`).
- **`Authenticate` always overwrites the `Authorization` header**, even if a successful refresh has already added one. The final assignment is `request.AddHeader("Authorization", $"Bearer {_accessToken}");`. This means a prior `request.AddHeader` call would be appended (RestSharp dedupe semantics depend on `Request.Headers` collection mode).
- **`ApiAccountExtensions.cs` uses lowercase URLs (`/api/v1/account/...`)**, but `ApiStationsExtensions.cs` uses mixed case (`/api/v1/Stations`). The server forces `LowercaseUrls = true` so both work, but the inconsistency is a minor code smell.
- **`ApiErrorResponseExtensions.GetErrors`** assumes the server's error body deserialises as `Dictionary<UserError, List<string>>`. The server's `IResult` actually has more fields (e.g. error messages without codes) — those get dropped by this deserialisation. Sufficient for typical use, lossy in edge cases.
- **`ChangedPasswordUserViewModel`** is referenced in `ApiAccountExtensions` but lives in `Pod.ViewModels.User` (not in `Pod.ViewModels.Customer`).
- **No async surface.** RestSharp 106 has async methods but the SDK exposes only synchronous (`Execute(...)`) calls. Not a deliberate decision — historical.

## Consumers

Direct project references:

- `Pod.Web.Client.Rest.Internal` — extends with internal endpoints

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.Web.Client.Rest.Internal/`](../Pod.Web.Client.Rest.Internal/) — the internal-endpoints companion
- [`docs/server/Pod.DtoModels/`](../Pod.DtoModels/) — request shapes used here
- [`docs/server/Pod.ViewModels/`](../Pod.ViewModels/) — response shapes used here
- [`docs/architecture/auth.md`](../../architecture/auth.md) — JWT lifecycle (what this SDK manages)
