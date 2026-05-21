# Pod.Web.Authentication.ApiKeySecret

> The `amx` HMAC-SHA256 authentication scheme for ASP.NET Core. Defines the handler and validator interface; the actual signature-verification implementation lives in `Pod.Web.Center` because it needs DB access.

## Purpose

Machine REST clients don't authenticate against the REST API the same way humans do. A human gets a JWT bearer token via `auth/login`. A station gets a per-station `(PublicKey, Secret)` pair (issued via `PUT /api/v1/Stations/{stationId}/apikeys`, persisted as a `StationApiKey` row) and uses HMAC-SHA256 to sign each REST request — proving ownership of the secret without ever transmitting it. ⚠️ This scheme runs **only** on REST endpoints that explicitly carry `[Authorize(AuthenticationSchemes = "amx")]`. Today that's every method on `StationController` (`Pod.Web.Center/Areas/Api/v1/StationController.cs`) — the kiosk uses this scheme for all of its non-gRPC operations under its own identity. The kiosk's gRPC traffic does **not** go through this scheme — it uses `(identity, password)` plain gRPC metadata verified PBKDF2-style against `Station.PasswordHash` (see `docs/architecture/auth.md` § Scheme #3).

(Historical context: an earlier internal design used per-station x509 license certs for station identification. That scheme was superseded by `StationApiKey` before the server side was ever finished; the orphaned client-side plumbing has been excised for v1.0.0. See `docs/architecture/auth.md`.)

This project implements that scheme — called `amx` after the `Authorization: amx <publicKey>:<signature>:<nonce>:<timestamp>` header format. It sits as a tiny `net10.0` library so the scheme can be registered in `Pod.Web.Center/Startup.cs` alongside the standard JWT scheme:

```csharp
services.AddAuthentication(o => o.AddScheme(
    ApiKeySecretHandler.AuthenticationScheme, // "amx"
    a => a.HandlerType = typeof(ApiKeySecretHandler)));
services.AddScoped<IApiKeySecretValidator, ApiKeySecretValidator>(); // impl in Pod.Web.Center
```

A controller opts in to the scheme with `[Authorize(AuthenticationSchemes = ApiKeySecretHandler.AuthenticationScheme)]`. Today this attribute appears on `StationController` (every method); the registration also makes the scheme available for any future endpoint that wants HMAC-authenticated machine access.

The handler-vs-validator split lets this assembly stay tiny and dependency-free, while the validator (which has to query `StationApiKeyService` in `Pod.Services` to find the secret for a given public key, and uses `IMemoryCache` for replay protection) lives in the host project.

## Tech

- **Target framework:** `net10.0`
- **Key NuGet packages:**
  - ASP.NET Core 10 framework reference — for `AuthenticationHandler<TOptions>`, `AuthenticationSchemeOptions`, `HttpRequestHeaders`
- **Project references (in this repo):** none

## Responsibility

**Is responsible for:**
- The `amx` scheme name as a `const` (`ApiKeySecretHandler.AuthenticationScheme = "amx"`)
- Parsing `Authorization: amx <publicKey>:<signature>:<nonce>:<timestamp>` into a 4-element string array
- Calling the injected `IApiKeySecretValidator` and translating its `IApiKeySecretResponse` into an `AuthenticateResult`
- Defining the contracts (`IApiKeySecretValidator`, `IApiKeySecretRequest`, `IApiKeySecretResponse`) that the host implements

**Is NOT responsible for:**
- Looking up the secret for a public key — `Pod.Web.Center.Authentication.ApiKeySecretValidator` does that via `StationApiKeyService`
- Computing the HMAC — same place
- Replay protection / nonce tracking — same place (uses `IMemoryCache`)
- Issuing or rotating API keys — `Pod.Services.Station.StationApiKeyService` owns that

## Public API surface

| Type | Visibility | Purpose |
|---|---|---|
| `ApiKeySecretHandler` | public | The `AuthenticationHandler<AuthenticationSchemeOptions>` ASP.NET picks up via `AddScheme(...)`. Parses the header, calls the validator, returns `AuthenticateResult.{Success, Fail, NoResult}`. |
| `ApiKeySecretHandler.AuthenticationScheme` | public const | The string `"amx"`. Used in `[Authorize(AuthenticationSchemes = "amx")]`, in Swagger security definitions, and in the header value prefix. |
| `IApiKeySecretValidator` | public interface | One method: `Task<IApiKeySecretResponse> Validate(IApiKeySecretRequest request)`. The host implements this. |
| `IApiKeySecretRequest` | public interface | `{ string[] AuthorizationHeaderArray; HttpRequest HttpRequest; }` — the parsed input handed to the validator. |
| `IApiKeySecretResponse` | public interface | `{ bool IsSuccess; bool IsInvalidSignature; ClaimsPrincipal ClaimsPrincipal; }` — the output. `IsInvalidSignature` distinguishes "the signature was wrong" from "the request was malformed" so the handler can return a clearer error. |
| `ApiKeySecretRequest` | internal | The concrete `IApiKeySecretRequest` the handler builds from the header + `Context.Request`. |

## Internal structure

```
Pod.Web.Authentication.ApiKeySecret/
├── ApiKeySecretHandler.cs     AuthenticationHandler<AuthenticationSchemeOptions>
├── ApiKeySecretRequest.cs     internal data carrier
└── Interfaces.cs              IApiKeySecretValidator, IApiKeySecretRequest, IApiKeySecretResponse
```

## Notable patterns / gotchas

- **`HandleAuthenticateAsync` returns `AuthenticateResult.NoResult()` when no `amx ...` header is present** — *not* `Fail()`. This is critical: when a request comes in with a JWT (e.g. a human hitting an endpoint that allows both schemes), `NoResult` lets the JWT scheme run; `Fail` would short-circuit the whole pipeline.
- **The handler casts `Context.Request.Headers` to `Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestHeaders`** — that's an internal Kestrel type. This works because Kestrel is the only supported host. It would break under HTTP.sys or IIS in-process. Don't change the host.
- **Header format is `amx <publicKey>:<signature>:<nonce>:<timestamp>`.** Four colon-separated values, after the scheme name. `GetAuthorizationHeaderValues` splits on space then on colon and rejects anything that isn't exactly 4 non-whitespace pieces.
- **The handler does not compute or verify the HMAC itself.** It hands the raw 4-element array + the `HttpRequest` to the validator. The implementation of `IApiKeySecretValidator` (in `Pod.Web.Center.Authentication.ApiKeySecretValidator`) does the MD5 of the body, replicates the signature string `{publicKey}{httpMethod}{requestUri}{timestamp}{nonce}{contentBase64}`, and compares HMAC-SHA256s.
- **`IsInvalidSignature` is currently never set to `true` by the host's validator** — the validator returns `IsSuccess = false` for both "no such API key" and "signature mismatch", and the handler's special-case message ("Invalid amx signature") is never produced. Cosmetic deficiency, not a security issue.
- **Multiple `Authorization` headers are allowed.** `headers.HeaderAuthorization.FirstOrDefault(x => x.StartsWith(AuthenticationScheme))` picks the first `amx ...` value, ignoring any `Bearer ...` siblings. The JWT handler does the symmetric thing.
- **The Swagger security definition for this scheme** is set up in `Pod.Web.Center/Startup.cs` (`AddSecurityDefinition("amx", new ApiKeyScheme {...})`). The description tells operators to enter `amx <signature>` — the Swagger UI does not actually generate the HMAC, it just attaches whatever the user types. Stations use `Pod.Web.Client.Rest` for real signing.

## Consumers

Direct project references:

- `Pod.Web.Center` — registers the scheme + provides the validator implementation. Available for any REST controller that opts in via `[Authorize(AuthenticationSchemes = ApiKeySecretHandler.AuthenticationScheme)]`; no endpoint currently does.

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/architecture/auth.md`](../../architecture/auth.md) — the full JWT vs `amx` story
- [`docs/server/Pod.Web.Center/`](../Pod.Web.Center/) — host-side `ApiKeySecretValidator` implementation in `Authentication/`
- [`docs/server/Pod.Services/`](../Pod.Services/) — `StationApiKeyService` (DB lookup + `(ApiKey, Secret)` issuance)
