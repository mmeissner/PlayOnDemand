# gRPC Contract

> The wire protocol that ties the kiosk and the server together. This doc summarises the contract; per-method depth lives in `docs/server/grpc/Pod.Grpc.Base/README.md`.

---

## Project layering

```
Pod.Grpc.Base                 .proto + generated stubs (THE contract)
                              ─ no project deps; leaf node
                                    │
        ┌───────────────────────────┼───────────────────────────────┐
        ▼                           ▼                               ▼
Pod.Grpc.Messages          Pod.Grpc.Base.Server / Client    Pod.Grpc.Const
DTO partial-class          hosting helpers                  shared constants
extensions                                                  (header names, etc.)
                                    │
                                    ▼
                  Pod.Grpc.Utilities (errors → gRPC status)
                                    │
            ┌───────────────────────┴────────────────────────┐
            ▼                                                ▼
Pod.Grpc.ConnectHost.Server               Pod.Grpc.ShellHost.Server
implements ServiceConnectHost             implements ServiceShellHost
                                                       + ServiceShellApplications
```

Generated stubs in `Pod.Grpc.Base/Export/` are **linked** (not compiled) into the consuming projects. `Pod.Grpc.Base` itself is multi-targeted (`netstandard2.0;net10.0`) so the same generated files satisfy:
- The kiosk path: `Pod.Grpc.Messages` + `Pod.Grpc.Base.Client` (netstandard2.0, backed by `Grpc.Core` 2.46.6) consumed by the .NET Framework 4.7.1 kiosk.
- The server path: `Pod.Grpc.{Base,ConnectHost,ShellHost}.Server` (net10.0, backed by `Grpc.AspNetCore` 2.66) hosted in `Pod.Web.Center`.

The generated code is target-agnostic (it references `Grpc.Core.Api` types provided by both runtimes). If you change `.proto` files, you change every consumer.

---

## The three services (per the actual proto)

### `ConnectHostServiceGrpc` — initial bootstrap

| RPC | Purpose |
|-----|---------|
| `GetHost(ShellServerRequest) → ShellServerResponse` | Station asks "where do I send my Shell traffic?" Server hands back a `ConnectionId` + host/port for the actual ShellHost service. Used at startup and on reconnect. |

### `ShellHostServiceGrpc` — session lifecycle

| RPC | Direction | Purpose |
|-----|-----------|---------|
| `Connect` | unary | Bind the station to this server using the `ConnectionId` from `GetHost`. |
| `GetNotifications` | **server-stream** | Long-lived push channel for `ClientNotification` events (server tells station: "check login request", "session state changed", "settings updated", etc.). |
| `GetServerSettings` | unary | Server clock + heartbeat interval/timeout. |
| `GetClientSettings` | unary | Per-station: DisplayName, QrCode, ControlMode (Local / Remote / RemoteWithQrCode). |
| `SendHeartbeat` | unary | Keep-alive. Drives `ConnectionHealthService` on the server. |
| `SendLoginIntention` | unary | Station: "user wants to log in". Server creates a `SessionDetails` in `LoginRequested` state. |
| `GetLoginIntention` | unary | Operator polls (or station polls — flow varies); returns a pending login. |
| `SendLoginResponse` | unary | Final accept/reject of a pending login (`IsLoginAccepted`). |
| `GetSessionState` | unary | Returns the full `SessionDetails`. Polled by the station. |
| `SendLogoutRequest` | unary | Carries `LogoutReason` (UserLogout / Inactivity / Shutdown / LimitReached). |
| `Disconnect` | unary | Graceful tear-down. |

### `ShellApplicationServiceGrpc` — app catalog sync

| RPC | Direction | Purpose |
|-----|-----------|---------|
| `GetSyncAppStates` | unary | Server's view of installed apps (or just the last-sync timestamp). |
| `SendSyncAppStates` | unary | Bulk delta upload from station. |
| `SendAppInstalled` | unary | Single install notification. |
| `SendAppUpdate` | unary | Single update notification. |
| `SendAppUninstalled` | unary | Single uninstall notification. |

For per-RPC payload field detail: `docs/server/grpc/Pod.Grpc.Base/README.md`.

---

## Wire-format conventions

- **Guids** are sent as `GuidAsBytes { bytes Value }` — 16-byte payload. Helper `.ToGuid()` lives in `Pod.Grpc.Messages` partial classes.
- **Nullable timespans / datetimes** use `TimeSpanAsLong { int64 Value; bool HasValue }` and `DateTimeUtcAsLong` — protobuf's lack of native nullables forced this.
- **`ConnectionId` is field 1** on almost every request. Issued by `ConnectHostServiceGrpc.GetHost` at session start, carried in every subsequent ShellHost call.
- **Enum values:** every enum reserves `0 = Unset` so that "default protobuf value" is never a meaningful state.

---

## The IResult<T> pattern (server-side)

Every gRPC service class is decorated with `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]`, which means `GrpcMetadataAuthenticationHandler` (the AspNetCore `AuthenticationHandler<T>` for the `"grpc-station"` scheme) has already validated the kiosk's `(identity, password)` credentials and built a `ClaimsPrincipal` carrying the StationId by the time any method body runs. Methods can then use the simpler shape:

```csharp
[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]
public class ShellHostServiceGrpc : ShellHost.ShellHostServiceGrpc.ShellHostServiceGrpcBase
{
    public override async Task<TResponse> SomeMethod(TRequest request, ServerCallContext context)
    {
        var clientCredentials = context.ToClientCredentials();        // StationId from authenticated principal

        var result = new Result<TResponse>();
        result.ArgNotNull(request.Field1, nameof(request.Field1))
              .ArgTrue(request.Field2 > 0, "Field2 must be positive");

        if (result.HasError())
            return result;  // implicit conversion / Result<T> wraps the error response

        // ... do work, touching PodDbContext / services ...

        return result.Add(new TResponse { ... });
    }
}
```

The legacy `clientCredentials.VerifyCredentials<TResponse>(_podContext)` call is no longer required at the start of every method — the auth handler runs that PBKDF2 check once, before the method dispatches. Existing methods that still call it (e.g. `ShellHostServiceGrpc.GetNotifications`, which bundles credential re-verification with a connection-id binding check) continue to work; the inline verification is now redundant defence-in-depth, not a gating step.

- **`Result<T>`** lives in `Pod.Data.Infrastructure`.
- Validation chain via `ArgNotNull` / `ArgTrue` / `ArgFalse` / `ArgEqual` / etc. — fluent.
- `result.HasError()` short-circuits — business validation failures are **not** exceptions, they become a structured response that the kiosk can interpret.
- `Pod.Grpc.Utilities` maps non-`IResult` exceptions to gRPC `Status` codes (e.g. `RpcException` with `StatusCode.Internal`).

---

## Auth on the wire

Every gRPC request is authenticated by **two** mechanisms working together:

1. **TLS (server-cert)** — channel encryption + server identity. Client validates the server's cert against `ServerRootCert`. Client-side certs are *not* required by the server (`ForceClientCertificate: false`); when present, they're a license credential, not auth.
2. **`(identity, password)` as gRPC CallCredentials** — every call attaches two metadata headers:
   - `identity` = `Station.Id` (Guid, sent as string)
   - `password` = `Station.Password` (literal, sent verbatim — TLS keeps it confidential)

   Wired client-side by `GrpcChannelCredentialsHandler.SetChannelCredentials(...)` (`Pod.Grpc.Base.Client/`). On the server they reach Kestrel as HTTP/2 headers; the AspNetCore authentication pipeline runs `GrpcMetadataAuthenticationHandler` (scheme name `"grpc-station"`, in `Pod.Grpc.Base.Server/`), which reads them, calls `IGrpcStationCredentialVerifier.VerifyAsync(...)` (PBKDF2 against `Station.PasswordHash` via the existing `Pod.Services.Extensions.VerifyCredentials` path), and on success builds a `ClaimsPrincipal` carrying the StationId. Each gRPC service class is decorated with `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]` so the handler runs before any method body. Inside the method, `CallContextUtil.ToClientCredentials(context)` returns the StationId from the authenticated principal (and the password from headers, for legacy in-method re-checks).

There is **no HMAC** on the gRPC path. The HMAC `amx` scheme in `Pod.Web.Authentication.ApiKeySecret` is a REST middleware authentication handler — it does not run for gRPC calls.

Auth failures: missing/empty headers → the handler returns `NoResult` and the `[Authorize]` attribute then rejects the call as `Unauthenticated`. Malformed StationId → `Fail("Malformed station identity header.")`. Bad password → `Fail("Invalid station credentials.")`. The gRPC pipeline turns either failure into `Status(Unauthenticated, ...)` on the wire.

Full detail of all three auth schemes (REST JWT, REST `amx` HMAC, gRPC station credentials): [auth.md](auth.md).

---

## Real-time push: `GetNotifications` server-stream

This is the only streaming RPC. The kiosk opens it once and holds it for the connection lifetime. The server pushes `ClientNotification { ClientNotificationEvent Event }` whenever:

- The operator approves a pending login → `CheckLoginRequest`
- `ClientSettings` changed → `UpdateClientSettings`
- Session state changed (e.g. operator force-logged a station) → `UpdateSessionState`
- `ServerSettings` changed (heartbeat config) → `UpdateServerSettings`
- Heartbeat reminder → `SendHeartbeat`

The **content** of each event is deliberately just an enum tag — the kiosk is expected to follow up with a unary RPC (`GetSessionState`, `GetClientSettings`, etc.) to fetch fresh state. This keeps the stream tiny and the source-of-truth unambiguous.

Server-side, `PublisherHub<T>` / `StationResponseHub` (in `Pod.Services`) is the in-process pub/sub. REST controllers and gRPC services both publish here; the streaming method subscribes per-station. **Single-instance only** — scaling out will need a real broker.

---

## Known limitations

- **API versioning is implicit** — `ShellServerRequest` carries a `MaxInterfaceVersion` and the server replies with `RequiredInterfaceVersion`, but there's no versioned proto package or backwards-compat strategy. Breaking changes to the proto require coordinated client + server deployment.
- **No deadlines on most calls** — clients should set their own `CallOptions.Deadline` per-call for resilience. Today only some do.
- **Streaming is one-way (server → client)** — bidi streaming is not used. If you need to react to client events without polling, you currently fake it with the server-push notification + unary follow-up pattern.

---

## Where to make changes

| Change | Where |
|--------|-------|
| Add a new RPC | Edit the relevant `Service*.proto` in `Pod.Grpc.Base/`; rebuild → stubs regenerate; implement in `*Server` project; add wrapper in `LeapVR.Shell.Services`. |
| Add a new message field | Same — but think about backward compat. Old fields keep their numbers; new fields always go on the end. |
| Add a new server-side validation | Inside the method, chain another `result.Arg*(...)` call before the `HasError()` check. |
| Add a new notification kind | Add to `ClientNotificationEvent` enum (preserve numeric values), publish via `PublisherHub<T>`, and update kiosk's notification handler in `LeapVR.Shell.Services`. |

---

## Read next

- `docs/server/grpc/Pod.Grpc.Base/README.md` — every message field and method signature.
- `docs/server/grpc/Pod.Grpc.ShellHost.Server/README.md` and `docs/server/grpc/Pod.Grpc.ConnectHost.Server/README.md` — the implementations.
- [session-lifecycle.md](session-lifecycle.md) — how the RPCs above sequence into a session.
- [auth.md](auth.md) — the three auth layers protecting these RPCs.
