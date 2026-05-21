# Authentication

> **Three** authentication mechanisms coexist. They live on **different channels** — don't conflate them.

---

## The schemes at a glance

| # | Scheme | Channel | Identifies | How it's verified | Status |
|---|--------|---------|------------|-------------------|--------|
| 1 | **JWT Bearer** | REST (HTTP) | Humans (operator, admin via web UI) | `Authorization: Bearer <jwt>`; symmetric-key signature; ASP.NET Identity | Live |
| 2 | **`amx` API key + HMAC** | REST (HTTP) | Stations (when hitting the station-facing REST surface) and any future machine clients | `Authorization: amx <key>:<sig>:<nonce>:<timestamp>`; HMAC-SHA256 over canonicalised request, keyed by `StationApiKey.SecretKey` | Live — used by every `StationController` endpoint |
| 3 | **Station identity + password as gRPC metadata** | gRPC | Stations (the kiosk's gRPC session traffic) | `identity` + `password` metadata headers per call; server's `GrpcMetadataAuthenticationHandler` (scheme `grpc-station`) loads the `Station` and calls `Station.VerifyPassword(...)` (PBKDF2) | Live (post-net10: now an `AuthenticationHandler` integrated with `[Authorize(AuthenticationSchemes="grpc-station")]`) |

> **Historical note (excised in v1.0.0):** A fourth scheme — **per-station x509 licensing** — was sketched but never finished. The kiosk-side plumbing (`LeapCertLicense`, `ClientIdentity`, `ClientRole`, the embedded production CA in `StaticServerConfig`, the dev cert files under `_Certificates/grpc client/`) was removed during the open-source release because no server-side issuance endpoint or validation gate ever existed and scheme #2 had already superseded it operationally. If you're spelunking through git history and see `ILeapCertLicense` / `StaticServerConfig`, that's what they were.

The kiosk uses **two** of the live schemes simultaneously:
- Scheme #3 for its **gRPC** session traffic (`identity`/`password` metadata).
- Scheme #2 for its **REST** calls to `/api/v1/Station/*` (`amx` HMAC, signed with the `StationApiKey.Secret` the station was provisioned with).

The handler-side classes share words like "ApiKey" and "Station" so they're easy to mix up; scheme #2's wire format (HMAC signature) is nothing like scheme #3's (plain headers). Each is detailed below.

---

## Scheme 1 — JWT Bearer (humans)

**For:** REST API + Razor pages backed by ASP.NET Core Identity.

**Defined in:**
- `Pod.Services/Authentication/AuthenticationService.cs` — issues + validates tokens.
- `Pod.Web.Center/Configuration/ConfigureJwtBearerOptions.cs` — wires bearer middleware.
- `Pod.Web.Center/Startup.cs` — `services.AddAuthentication(...).AddJwtBearer(...)`.

**Identity model:**
- `ApplicationUser : IdentityUser<Guid>` (in `Pod.Data.Models/Users/`)
- `ApplicationRole : IdentityRole<Guid>`
- All Identity tables live in PostgreSQL.

**Token kinds (all JWT but with different purposes/lifetimes):**

| Token | Lifetime | Issued when | Carried where |
|-------|----------|-------------|---------------|
| Access | 2 hours | login, refresh | `Authorization: Bearer <token>` header |
| Refresh | 20 years | login | client storage; exchanged for a new access token |
| EmailConfirmation | (one-shot, short) | registration / resend | URL query parameter sent in the email |
| PasswordReset | (one-shot, short) | "forgot password" | URL query parameter |

The token kind is encoded in a custom claim. `AuthenticationService` rejects tokens used for the wrong purpose.

**Custom token providers** in `Pod.Web.Center/TokenProvider/`:
- `RefreshAccessTokenProvider`
- `PasswordResetTokenProvider<TUser>` (+ matching `Options` class)
- `EmailConfirmationTokenProvider<TUser>` (+ matching `Options` class)

**Key configuration** (`appsettings.json` → `AuthConfig`):
- `SecretKey` — symmetric signing key. (⚠️ committed to repo — see `docs/open-source-readiness.md`.)
- `Issuer`, `Audience`
- Lifetime values per token kind

**Login flow:**
```
POST /api/v1/account/login { email, password }
  → AuthenticationService.LoginAsync()
  → SignInManager.PasswordSignInAsync()
  → on success: issue Access (2h) + Refresh (20y), return both
  → on lockout / not confirmed: return appropriate IResult error
```

**Refresh flow:**
```
POST /api/v1/account/refresh-token { refreshToken }
  → validate token (signature, kind, expiry, user state)
  → issue new Access token (refresh stays the same — single-use is not enforced)
```

---

## Scheme 2 — `amx` API key + HMAC (REST: stations + any future machine clients)

**For:** REST endpoints decorated with `[Authorize(AuthenticationSchemes = ApiKeySecretHandler.AuthenticationScheme)]` (the constant resolves to `"amx"`). Today this includes **every method on `StationController`** (`Pod.Web.Center/Areas/Api/v1/StationController.cs:23`).

⚠️ **The kiosk's gRPC traffic does NOT use this scheme — the kiosk's REST traffic does.** Scheme #2 gates the station's REST surface (which is the only way to call most operator-style endpoints under the station's own identity). The kiosk's gRPC session traffic is scheme #3. The naming overlap with `Pod.Web.Authentication.ApiKeySecret` and the `StationApiKey` entity covers both, which is confusing — but the wire formats and code paths are completely separate.

**Defined in:**
- `Pod.Web.Authentication.ApiKeySecret/ApiKeySecretHandler.cs` — `AuthenticationHandler<AuthenticationSchemeOptions>` subclass; `AuthenticationScheme = "amx"`.
- `Pod.Web.Authentication.ApiKeySecret/IApiKeySecretValidator.cs` — validation contract.
- The HMAC validator implementation lives in `Pod.Web.Center/Authentication/` (it needs `StationApiKeyService` + `IMemoryCache`, which is why it's not in the standalone scheme project).

**Wire format** — single HTTP header:
```
Authorization: amx <ApiKey>:<Signature>:<Nonce>:<Timestamp>
```

Parsed in `ApiKeySecretHandler.GetAuthorizationHeaderValues`: scheme name and 4-tuple separated by `:`. Anything not exactly four colon-separated fields is rejected as malformed.

**Credential model:** `StationApiKey` row (in `Pod.Data.Models/Shell/Station.cs`):
- PK: `PublicKey` (Guid) — the `ApiKey` field on the wire.
- `SecretKey` — 256-bit Base64 string generated via `RandomNumberGenerator`. Never sent over the wire; used only as the HMAC key.
- FK: `StationId` (cascade-delete with the parent Station).
- Created via `Station.CreateStationApiKey(displayName)` which calls the internal `StationApiKey.Generate(...)` factory.

**Validation flow** (`Pod.Web.Center/Authentication/ApiKeySecretValidator.cs:75-156`):
1. Handler splits the header into `(publicKey, signature, nonce, timestamp)`.
2. Validator calls `StationApiKeyService.GetStationApiKey(publicKey)` — DB lookup, eagerly includes `Station` (which carries the `ApplicationUserId` FK).
3. **Replay-attack check**: nonce checked against `IMemoryCache`; rejected if seen recently; rejected if `|now − timestamp| > 10 seconds` (`MaxClockSkewBetweenClientAndServerInMs`).
4. Recomputes HMAC-SHA256 over `{publicKey}{httpMethod}{requestUri}{timestamp}{nonce}{base64(md5(body))}` (so the body is bound into the signature; body stream is rewound after hashing so controllers can re-read it). Compares against the supplied signature.
5. On success, builds a `ClaimsPrincipal` carrying **two** claims:
   - `PodClaimsTypes.ApiKeyUserId` = `apiKey.Station.ApplicationUserId` — **the owning account**
   - `PodClaimsTypes.ApiKeyStationId` = `apiKey.StationId` — **the station**

   Controllers read both via the extension `User.GetStationApiKeyData(out var userId, out var stationId)`. This is **how the server knows which account a station belongs to** on every request — the validator does the `StationApiKey → Station → ApplicationUserId` traversal once at auth time and surfaces both IDs to the controller.

**Body-size limit on hashing**: 20 MB (`ApiKeySecretValidator.GetBytes` throws `NotSupportedException` past `1280 × 16KB` buffer rounds). Large request bodies will fail auth.

**Live consumers of the `amx` scheme today:**
- `StationController` — all methods (`/api/v1/Station/*`). Used by the kiosk for non-gRPC station-management operations (settings, mode, session update/stop, session history).

The scheme is also available for any future REST endpoint that wants it — just decorate with `[Authorize(AuthenticationSchemes = ApiKeySecretHandler.AuthenticationScheme)]`.

---

## Scheme 3 — Station identity + password (gRPC CallCredentials)

**For:** every gRPC call from `LeapPlay.Shell.exe` to `Pod.Web.Center` (`ServiceConnectHost`, `ServiceShellHost`, `ServiceShellApplications`).

This is the **actual** kiosk-to-server authentication. There is no HMAC, no signed payload, no client certificate involved. Just two metadata headers per request, sent over a TLS-encrypted channel.

### How the client attaches credentials

`Pod.Grpc.Base.Client/GrpcChannelCredentialsHandler.cs`:

```csharp
public void SetChannelCredentials(string identity, string password)
{
    _hasChannelCredentials = true;
    _identity = identity;     // StationId (Guid as string)
    _password = password;     // Station password (literal)
}

public ChannelCredentials GetCredentials()
{
    var sslCredentials = new SslCredentials(ServerRootCert /*, optional license cert keypair */);
    if (!_hasChannelCredentials) return sslCredentials;

    var callCredentials = CallCredentials.FromInterceptor(
        new AsyncAuthInterceptor((context, metadata) =>
        {
            metadata.Add(AuthConstants.ShellClientIdentityKey, _identity);   // header: "identity"
            metadata.Add(AuthConstants.ShellClientPasswordKey, _password);   // header: "password"
            return TaskUtils.CompletedTask;
        }));

    return ChannelCredentials.Create(sslCredentials, callCredentials);
}
```

`AuthConstants` (`Pod.Grpc.Const/AuthConstants.cs`) defines the two header keys — both lowercase because gRPC normalises header names to lowercase:

```csharp
public const string ShellClientIdentityKey = "identity";
public const string ShellClientPasswordKey = "password";
```

The credentials are wired in by `RemoteServiceFactory.GetHandler(stationId, password)` (`LeapVR.Shell.Services/Factory/RemoteServiceFactory.cs:92-100`) — every gRPC client built for a station carries this interceptor.

### How the server reads them

Under net10.0 / Grpc.AspNetCore, the headers reach Kestrel as ordinary HTTP/2 headers and are processed by an ASP.NET Core authentication handler before any service method runs:

`Pod.Grpc.Base.Server/GrpcMetadataAuthenticationHandler.cs` — `AuthenticationHandler<AuthenticationSchemeOptions>`, scheme name `"grpc-station"`:

```csharp
protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
{
    var identityHeader = ReadSingleHeader(AuthConstants.ShellClientIdentityKey);
    var passwordHeader = ReadSingleHeader(AuthConstants.ShellClientPasswordKey);

    if (string.IsNullOrWhiteSpace(identityHeader) ||
        string.IsNullOrWhiteSpace(passwordHeader))
        return AuthenticateResult.NoResult();                                  // → Authorize rejects as Unauthenticated

    if (!Guid.TryParse(identityHeader, out var stationId) || stationId == Guid.Empty)
        return AuthenticateResult.Fail("Malformed station identity header.");

    var verifyResult = await _verifier.VerifyAsync(new ClientCredentials {
        StationId = stationId, Password = passwordHeader,
    });
    if (verifyResult == null || verifyResult.HasError())
        return AuthenticateResult.Fail("Invalid station credentials.");

    var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, stationId.ToString()),
        new Claim(ClaimTypes.Name,           stationId.ToString()),
        new Claim(GrpcMetadataAuthenticationHandler.ClaimType_GrpcStationId,    stationId.ToString()),
        new Claim(GrpcMetadataAuthenticationHandler.ClaimType_ApiKeyStationId,  stationId.ToString()),
    };
    return AuthenticateResult.Success(
        new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName));
}
```

Each gRPC service class is decorated with `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]`, so the handler runs *before* any method body. `CallContextUtil.ToClientCredentials(context)` then returns the StationId from `HttpContext.User` (the authenticated principal) and the password from request headers — keeping the same `ClientCredentials` shape that legacy in-method re-checks (e.g. `ShellHostServiceGrpc.GetNotifications`) expect.

The `ApiKeyStationId` mirror claim is intentional: any controller / extension already keyed on `Pod.Services.Authentication.PodClaimsTypes.ApiKeyStationId` (today: the REST `amx` scheme) keeps working transparently when the same caller comes in via gRPC. The distinct `GrpcStationId` claim lets service code tell which channel the caller arrived on.

### How the server verifies them

The handler delegates to `IGrpcStationCredentialVerifier.VerifyAsync(...)`, defaulting to `DefaultGrpcStationCredentialVerifier`, which calls the unchanged `Pod.Services.Extensions.VerifyCredentials` path:

```csharp
public static async Task<Result> VerifyCredentials(this ClientCredentials credentials, PodDbContext context)
{
    var result = new Result();

    result.ArgNotEqual(credentials.StationId, nameof(credentials.StationId), Guid.Empty, UserError.ShellClientInvalidStationId);
    result.ArgNotNullOrWhitespace(credentials.Password, nameof(credentials.Password), UserError.ShellClientInvalidPassword);
    if (result.HasError()) return result;

    var stationDb = await context.Stations.FindAsync(credentials.StationId);
    result.RefNotNull(stationDb, nameof(stationDb), UserError.ShellClientInvalidStationId);
    if (result.HasError()) return result;

    result.ValueTrue(
        stationDb.VerifyPassword(credentials.Password, new PasswordHasher()),
        "IsPasswordValid",
        UserError.ShellClientInvalidPassword);

    return result;
}
```

So:
1. Validates the inputs are present + well-formed.
2. Loads the `Station` row by `StationId`.
3. Calls `Station.VerifyPassword(password, hasher)` — PBKDF2-V3 hash comparison against `Station.PasswordHash`.

Returns an `IResult` either way. Failure becomes the gRPC response payload (`Result<TResponse>` wraps it); the call does not throw.

### The `StationApiKey` is not used here

The `StationApiKey` entity (with its `PublicKey`/`SecretKey` HMAC pair) belongs to **Scheme #2**, not Scheme #3. The gRPC path uses the `Station.PasswordHash` only.

### The "ApiKeySecret" name in the project does not refer to gRPC

`Pod.Web.Authentication.ApiKeySecret` is the REST `amx` scheme. The fact that the project name and the gRPC station-auth concept both contain "Api" / "Secret" / "Station" is unfortunate naming — they're orthogonal.

---

## Transport security (TLS) on gRPC — what's actually enforced

Server-side (`Pod.Grpc.Base.Server/GrpcServer.cs:48-69`):

```csharp
var credentials = new SslServerCredentials(
    new[] { new KeyCertificatePair(serverCert, serverKey) },
    rootClientCertificateBytes,
    grpcServerConfig.ForceClientCertificate);   // <-- the third arg = forceClientAuth
```

`grpcServerConfig.ForceClientCertificate` defaults to **`false`** (`Pod.Web.Center/appsettings.json:19` and `GrpcServerConfig.cs:39`). That value is passed straight to gRPC's `SslServerCredentials` `forceClientAuth` parameter.

**Implication:** TLS handshake requires the server cert (validated by the client against `ServerRootCert`), but **does not require a client cert**. If the client sends one, gRPC will accept it; if not, the handshake still succeeds. **The server doesn't need a client cert to authenticate the request — auth is via the `(identity, password)` metadata in Scheme #3.**

Client-side (`Pod.Grpc.Base.Client/GrpcChannelCredentialsHandler.cs:48-61`): if `ClientCertChain` AND `ClientPrivateKey` are both supplied, the client builds a `KeyCertificatePair` and presents it. Otherwise it constructs `new SslCredentials(ServerRootCert)` only.

Under net10 the kiosk's `LeapVR.Shell/Bootstrapper.cs` registers `IServerConfig` as the file-backed `ConfigFileRepository<ServerConfig>().Get()` (a `ServerConfig.json` next to the kiosk binary). The interface no longer carries `GetClientCertificate()` / `GetClientPrivateKey()` — those were part of the orphaned cert-licensing path (see note below) and have been excised.

Server certificate verification happens against the system trust store (Windows on the kiosk side, the container's default trust roots on the server side). Production deployments use a publicly trusted Let's Encrypt-issued certificate; the optional `ServerConfig.ServiceRootCert` field lets a dev box override that with a self-signed CA PEM.

`Pod.Grpc.Base.Client/GrpcChannelCredentialsHandler.cs` keeps a backward-compatible mTLS path (`ClientCertChain` + `ClientPrivateKey` parameters default to `null`) but the kiosk no longer passes them — see [`RemoteServiceFactory.GetHandler`](../../LeapVR.Shell.Services/Factory/RemoteServiceFactory.cs).

---

## Cert-based licensing (historical / removed in v1.0.0)

A fourth scheme was sketched but never finished: per-station x509 certs minted by a central "Leap Auth Service" CA, with the kiosk's `LicenseId` encoded in the cert CN. The kiosk-side plumbing existed in `LeapCertLicense.TryReadLicense` (which loaded the cert + RSA key and parsed `ClientIdentity.GetClientIdentity(commonName, ...)`); the server side had no issuance endpoint and no auth gate that enforced it. The `(StationApiKey.PublicKey, Secret)` model (scheme #2 above) covered the same business intent — bind a station to an account, give it a provable credential — with less infrastructure (no CA to run, no cert rotation), so the cert-licensing path was deleted entirely during the open-source release.

What got removed:
- `LeapVR.Shell.Domain.Models/CertLicense/ILeapCertLicense.cs`, `LicenseRole` enum.
- `LeapVR.Shell.Modules/ShellConfigurator/LeapCertLicense.cs`.
- `LeapVR.Shared.Lib/x509/` (the whole folder: `ClientIdentity`, `ClientRole`, `Crypto`, `CryptoHelpers`, `RSAParameterTraits`).
- `LeapVR.Shell.Services.Interfaces/FileConfig/CredentialConfig.cs`.
- `IServerConfig.ClientCertificateChain` + `.PrivateKey` and `StaticServerConfig` (whose embedded production CA PEM was the only hardcoded production hostname in the codebase).
- All dev cert artefacts under `_Certificates/grpc client/`, `_Certificates/grpc client internal/`, and `_Certificates/ssl create/*.{crt,key,srl}`.

If you want to revive it: server side needs a full PKI flow (issuance, rotation, revocation lists, a controller that mints certs against a server-held CA private key); kiosk side needs to re-add a license-aware bootstrapper and a `<None Include="License/...">` step in the build. It's a project, not a refactor. Scheme #2 covers the realistic threat model for an arcade-management product.

---

## What's protected by what

| Endpoint family | Scheme | Notes |
|-----------------|--------|-------|
| `/api/v1/account/*` | JWT (some endpoints anonymous: login, register) | Identity-driven |
| `/api/v1/Admin/*`, other internal/admin REST surfaces | JWT + role check | Operator/admin-only |
| `/api/v1/Stations/*` (operator-facing) | JWT | Operator manages their stations (CRUD, settings, apikeys, sessions) |
| **`/api/v1/Station/*` (station-facing — singular!)** | **`amx` HMAC (Scheme #2)** | Each kiosk authenticates as itself with its `(PublicKey, Secret)`. Validator surfaces `(ApplicationUserId, StationId)` as claims for the controller. |
| Razor pages under `/Account` | Identity cookies | User-flow pages — register, confirm email, reset password |
| **gRPC `ServiceShellHost.*`** | **Scheme #3 (`(identity, password)` metadata)** | Plain headers over TLS, server does PBKDF2 verify against `Station.PasswordHash`. |
| **gRPC `ServiceConnectHost.*`** | **Scheme #3** | Same. |
| **gRPC `ServiceShellApplications.*`** | **Scheme #3** | Same. |
| `/swagger`, static files | Anonymous | Dev / discovery |

---

## Provisioning a new station

End-to-end flow (covered by `StationService`, `StationApiKeyService`, and the kiosk's Setup wizard):

1. **Operator creates the `Station`** via REST (`StationsController`, JWT-authenticated) and sets a password. The `Station.PasswordHash` is computed with PBKDF2-V3 (`Pod.Data/PasswordHasher.cs`).
2. **Operator creates a `StationApiKey`** for the Station: `PUT /api/v1/Stations/{stationId}/apikeys?keyName=...` (`StationsController.CreateApiKey` → `StationApiKeyService.CreateStationApiKey(userId, stationId, displayName)`). Response: `StationApiKeyViewModel { CreateOnUtc, Name, PublicKey, Secret }`. **`Secret` is the only chance to read it** — it's not retrievable later (the list endpoint exposes the public key but not the secret).
3. **Operator hands the kiosk both credentials** during the **Setup wizard** flow (`LeapPlay.Shell.exe -config`, served by `LeapVR.Shell.Setup`), via direct entry or a QR code rendered server-side:
   - `(StationId, password)` — used as gRPC `identity`/`password` metadata (scheme #3).
   - `(PublicKey, Secret)` — used to sign REST `amx` requests (scheme #2).
4. **Kiosk persists** the gRPC pair in `LoginConfig.json` and (presumably) the `amx` pair in a sibling config file (verify against the live Setup wizard implementation).
5. **First contact**: gRPC `Connect` carries the `identity`/`password` metadata; the kiosk's REST calls into `StationController` carry an `amx` signed header.
6. From then on every call uses the appropriate scheme for its channel.

If the password needs to rotate, the operator edits the `Station.PasswordHash` server-side. If the `amx` secret needs to rotate, the operator deletes the existing `StationApiKey` (`DELETE /api/v1/Stations/{stationId}/apikeys/{publicKey}`) and creates a new one. Either rotation forces the kiosk back to the Setup wizard on the next failed call.

---

## Common confusions / gotchas

- **`Pod.Web.Authentication.ApiKeySecret` does not gate gRPC.** It's REST-only middleware. Today it gates `StationController` (which is the station-facing REST surface — the kiosk DOES use it for its REST calls). It does NOT touch any gRPC dispatch.
- **`StationApiKey` (entity) is for `amx` REST auth, not for gRPC.** The kiosk's gRPC traffic uses `Station.PasswordHash` (scheme #3). Easy to confuse because the same kiosk uses both.
- **The kiosk authenticates *two* channels with *two* different secrets.** Scheme #3 (gRPC) needs `(StationId, Station.Password)`; scheme #2 (REST `amx`) needs `(StationApiKey.PublicKey, StationApiKey.Secret)`. The Setup wizard provisions both pairs.
- **The kiosk's `password` is the literal `Station.Password`** — sent verbatim in a gRPC header on every call. The `VerifyCredentials` server-side does a PBKDF2 hash compare. Confidentiality maintained by TLS, not by HMAC.
- **gRPC metadata header names are lowercase (`identity`, `password`).** gRPC normalises header names; `AuthConstants` are defined lowercase to make this explicit.
- **`ForceClientCertificate=false` on the server** — TLS handshake never *requires* a client cert. The optional mTLS path in `GrpcChannelCredentialsHandler` exists for backward compatibility (callers can opt in); the kiosk no longer does.
- **`ServerConfig.json` is now the canonical kiosk-side connect config.** Edit `ConnectServerHost`/`ConnectServerPort` next to the binary to point the kiosk at a different server.
- **The "20-year refresh token"** in Scheme #1 is intentional — operator-managed kiosks shouldn't be forced to log in their staff often. It's still revocable server-side via the user's security stamp.
- **`appsettings.json` ships with placeholder secrets** (`<set-via-env-or-user-secrets>`). Override via `dotnet user-secrets` for development or env vars (`DOTNET_<Section>__<Key>` / `<Section>__<Key>` form) for the Docker Compose deployment.

---

## Read next

- [grpc.md](grpc.md) — the wire protocol that Scheme #3 gates.
- [data-model.md](data-model.md) — `ApplicationUser`, `Station`, `StationApiKey` schema.
- `docs/usage/shell.md` § "ServerConfig.json" — how the cert paths in the kiosk's config map to the install (and why most of them are dead schema).
