# Pod.Grpc.Base.Server

> Server-side AspNetCore plumbing for the kiosk-facing gRPC services: a custom `AuthenticationHandler` for the `(identity, password)` station credentials, plus the helper that surfaces the authenticated StationId to service implementations.

## Purpose

Two responsibilities, both server-only:

1. **Authentication** — `GrpcMetadataAuthenticationHandler` runs once per call as part of the standard ASP.NET Core authentication pipeline. It reads the `identity` + `password` HTTP/2 metadata headers, calls `IGrpcStationCredentialVerifier` (defaulting to PBKDF2 against `Station.PasswordHash` via the existing `Pod.Services.Extensions.VerifyCredentials` path), and on success builds a `ClaimsPrincipal` with the StationId. Service classes opt in via a single class-level attribute:

   ```csharp
   [Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]
   public class ShellHostServiceGrpc : ShellHost.ShellHostServiceGrpc.ShellHostServiceGrpcBase { ... }
   ```

2. **Call-context helper** — `CallContextUtil.ToClientCredentials(ServerCallContext)` returns the authenticated `ClientCredentials` (StationId from the principal, Password from the header) so existing service code keeps using the same one-liner it always did.

The pre-net10 abstract `GrpcServer<T>` that hand-rolled a `Grpc.Core.Server` is gone — Kestrel + `services.AddGrpc()` + `endpoints.MapGrpcService<T>()` in `Pod.Web.Center/Startup.cs` replace it. A vestigial `GrpcServer` stub class is kept in this assembly purely as a documentation anchor (its XML doc spells out the new wiring shape).

## Tech

- **Target framework:** `net10.0`
- **Configurations:** `Debug`, `Release` (no `Release_ShellClient` — server-only)
- **Key NuGet packages:**
  - `Grpc.AspNetCore` **2.66.0** — server-side ASP.NET Core gRPC integration; transitively brings `Grpc.AspNetCore.Server` + `Grpc.Core.Api` so `ServerCallContext`, `RpcException`, etc. resolve.
  - `Microsoft.AspNetCore.App` (FrameworkReference) — `AuthenticationHandler<T>`, `[Authorize]`, `IHeaderDictionary`, DI extensions.
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure` — `ClientCredentials`, `IResult`
  - `Pod.Data` — `PodDbContext` (consumed by `DefaultGrpcStationCredentialVerifier`)
  - `Pod.Enums` — `UserError`
  - `Pod.Grpc.Const` — `AuthConstants` (header names: `identity`, `password`)
  - `Pod.Services` — `Extensions.VerifyCredentials` (PBKDF2 path)

## Responsibility

**IS** responsible for:
- Authenticating gRPC calls against `Station` credentials (handler + verifier + extension).
- Surfacing the authenticated StationId to service code via `ClaimsPrincipal` + `CallContextUtil`.
- Documenting (via the `GrpcServer` stub's XML doc) where the legacy hosting code moved.

**IS NOT** responsible for:
- TLS termination / port binding — Kestrel does that, configured under `Kestrel:Endpoints` in `Pod.Web.Center/appsettings.json`.
- Service implementations — those live in `Pod.Grpc.ConnectHost.Server` and `Pod.Grpc.ShellHost.Server`.
- Authorisation policies beyond authentication — there's only one scheme today (`grpc-station`); RBAC layered on top would go in `Pod.Web.Center`.

## Public API surface

`namespace Pod.Grpc.Base.Server`:

- `sealed class GrpcMetadataAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>`
  - `const string SchemeName = "grpc-station"`
  - `const string ClaimType_GrpcStationId = "GrpcStationId"`
  - `const string ClaimType_ApiKeyStationId = "ApiKeyStationId"` (mirrored so consumers already keyed on `PodClaimsTypes.ApiKeyStationId` keep working)
  - Failure paths:
    - missing/whitespace `identity` AND `password` → `NoResult` (other schemes can attempt the call)
    - non-Guid or `Guid.Empty` `identity` → `Fail("Malformed station identity header.")`
    - verifier returns error/null → `Fail("Invalid station credentials.")`
- `interface IGrpcStationCredentialVerifier`
  - `Task<Result> VerifyAsync(ClientCredentials credentials)` — abstracted out so the handler can be unit-tested without EF Core.
- `sealed class DefaultGrpcStationCredentialVerifier : IGrpcStationCredentialVerifier`
  - Production binding; calls `credentials.VerifyCredentials(PodDbContext)`.
- `static class GrpcMetadataAuthenticationExtensions`
  - `AuthenticationBuilder.AddGrpcStationMetadata(Action<AuthenticationSchemeOptions>? = null)` — single-line registration that also `TryAddScoped`s the default verifier.
- `static class CallContextUtil`
  - `ClientCredentials ServerCallContext.ToClientCredentials()` — reads StationId from `HttpContext.User`, password from request headers; throws `RpcException(StatusCode.Internal, ...)` if no station claim is present (signals misconfiguration, not a bad client).
- `class GrpcServerConfig` + `class SslCredentials` — legacy DTO retained as a binding target so existing `appsettings.json` sections still bind during the cutover; XML doc on each property points at the equivalent Kestrel setting.
- `static class GrpcServer` — vestigial; the only thing on it is an `[Obsolete]` attribute and an XML doc explaining the migration to `Pod.Web.Center/Startup.cs`.

## Internal structure

```
Pod.Grpc.Base.Server/
├── Pod.Grpc.Base.Server.csproj
├── GrpcMetadataAuthenticationHandler.cs        ← AuthenticationHandler<T> implementation
├── GrpcMetadataAuthenticationExtensions.cs     ← AddGrpcStationMetadata() helper
├── IGrpcStationCredentialVerifier.cs           ← seam for testability
├── DefaultGrpcStationCredentialVerifier.cs     ← production binding
├── CallContextUtil.cs                           ← ServerCallContext → ClientCredentials
├── GrpcServerConfig.cs                          ← legacy DTO (kept for appsettings binding)
└── GrpcServer.cs                                ← vestigial migration anchor (no code)
```

## Notable patterns / gotchas

- **The auth handler reads `Context.Request.Headers` directly**, not the gRPC-side `Metadata`. Under Grpc.AspNetCore the kiosk's gRPC metadata IS the HTTP/2 headers, and ASP.NET's `IHeaderDictionary` lookup is case-insensitive — so the lowercase `AuthConstants.ShellClientIdentityKey` / `ShellClientPasswordKey` keys (mandated by gRPC's HTTP/2 normalisation) match cleanly either way.
- **`NoResult` vs `Fail` matters.** Missing headers → `NoResult` so other authentication schemes can attempt the call. Bad headers → `Fail` so the gRPC pipeline returns `Unauthenticated` immediately. The result is: an unauthenticated call to a `[Authorize]`-decorated service shows up as `Unauthenticated` on the wire either way (the authorisation step rejects `NoResult` for `[Authorize]`), but the distinction matters if more than one gRPC scheme is ever wired.
- **`CallContextUtil.ToClientCredentials` throws `Internal` (not `Unauthenticated`) on a missing claim.** That's because reaching the helper without a validated StationId means a service is missing `[Authorize(...)]` or the auth handler isn't wired in `Startup.cs` — it's a server bug, not a bad client. Surfacing it as `Internal` puts it where ops alerts will see it.
- **Two claim types on the principal.** The handler sets BOTH `GrpcStationId` AND `ApiKeyStationId`. The mirror onto `ApiKeyStationId` is so that any helper / controller keyed on `Pod.Services.Authentication.PodClaimsTypes.ApiKeyStationId` (which today refers only to the REST `amx` scheme) keeps working transparently when called via the gRPC channel.
- **`IGrpcStationCredentialVerifier` is the test seam.** Production gets `DefaultGrpcStationCredentialVerifier` which calls `Pod.Services.Extensions.VerifyCredentials` (the PBKDF2 path). Tests substitute their own to avoid spinning up EF Core.
- **`GrpcServerConfig` will eventually disappear.** It's only kept so an existing `appsettings.json:GrpcServerConfig` section still binds without throwing during the cutover. Once the integration step lands and the section is migrated to `Kestrel:Endpoints`, this class can be deleted.

## Consumers

- `Pod.Grpc.ConnectHost.Server.Services.ConnectHostServiceGrpc` — `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]` on the class; `context.ToClientCredentials()` inside the method body.
- `Pod.Grpc.ShellHost.Server.Services.ShellHostServiceGrpc` + `ShellApplicationServiceGrpc` — same pattern.
- `Pod.Web.Center/Startup.cs` (out of scope of this branch — wired by the orchestrator's integration step):
  ```csharp
  services.AddGrpc();
  services.AddAuthentication()
          .AddJwtBearer(...)
          .AddScheme<...>(ApiKeySecretHandler.AuthenticationScheme, _ => { })
          .AddGrpcStationMetadata();
  ...
  endpoints.MapGrpcService<ConnectHostServiceGrpc>();
  endpoints.MapGrpcService<ShellHostServiceGrpc>();
  endpoints.MapGrpcService<ShellApplicationServiceGrpc>();
  ```
- `Pod.Grpc.Base.Server.Test` — xUnit project, exercises `GrpcMetadataAuthenticationHandler` and `CallContextUtil` against a `DefaultHttpContext` + `Grpc.Core.Testing.TestServerCallContext` (no real listener, no real DB).

## Related docs

- [`docs/server/grpc/README.md`](../README.md) — gRPC tier overview
- [`docs/server/grpc/Pod.Grpc.Const/README.md`](../Pod.Grpc.Const/README.md) — `AuthConstants` (header names)
- [`docs/architecture/auth.md`](../../../architecture/auth.md) — full picture of the three coexisting auth schemes (this project implements scheme #3)
- [`docs/architecture/grpc.md`](../../../architecture/grpc.md) — protocol-level overview
