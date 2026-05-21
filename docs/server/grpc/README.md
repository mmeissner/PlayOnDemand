# gRPC Tier — `Pod.Grpc.*`

> The wire-protocol layer between the **WPF kiosk (`LeapPlay.Shell.exe`)** and the **central server (`Pod.Web.Center`)**. Eight C# projects, three `service` definitions, server-cert TLS + per-call `(identity, password)` metadata that the server PBKDF2-verifies against `Station.PasswordHash`.

---

## Why two transports?

PoD has two completely independent client surfaces:

| Audience | Transport | Auth |
|----------|-----------|------|
| Operators / browsers / 3rd-party tools | REST + JWT | `Pod.Web.Center` controllers |
| **Stations** (kiosks) | **gRPC + server-cert TLS + `(identity, password)` metadata** | `Pod.Grpc.*` |

This README covers the second one. For the REST side see `docs/server/Pod.Web.Center/`.

---

## The eight projects at a glance

```
                          ┌──────────────────────────────────┐
                          │  Pod.Grpc.Base                   │
                          │  • all .proto files              │
                          │  • generated stubs (Export/)     │
                          │  • Google.Protobuf + Grpc.Tools  │
                          │  • multi-target: netstandard2.0  │
                          │    (Grpc.Core 2.46.6, kiosk)     │
                          │    + net10.0 (Grpc.AspNetCore)   │
                          └─────────────┬────────────────────┘
                                        │ exports .cs
              ┌─────────────────────────┼──────────────────────────────┐
              │                         │                              │
              ▼                         ▼                              ▼
   ┌──────────────────┐     ┌────────────────────┐         ┌────────────────────┐
   │ Pod.Grpc.Const   │     │ Pod.Grpc.Messages  │         │ Pod.Grpc.Utilities │
   │ header keys,     │     │ partial classes &  │         │ IResult ⇄ RpcException
   │ channel options  │     │ converters for     │         │ mapping by UserError
   │ (netstandard2.0) │     │ Guid/Time DTOs     │         │ enum → StatusCode    │
   └────────┬─────────┘     └─────────┬──────────┘         └─────────┬──────────┘
            │                         │                              │
            ├─────────────┬───────────┴──────────────┬───────────────┤
            ▼             ▼                          ▼               ▼
   ┌──────────────────┐ ┌──────────────────────────────────────────────────┐
   │ Pod.Grpc.Base.   │ │ Pod.Grpc.Base.Server  (net10.0, Grpc.AspNetCore) │
   │ Client           │ │ • GrpcMetadataAuthenticationHandler              │
   │ (netstandard2.0) │ │   (AspNetCore AuthenticationHandler<T> for the   │
   │ • GrpcClient<T>  │ │    "grpc-station" scheme)                        │
   │ • Channel/SSL    │ │ • IGrpcStationCredentialVerifier (test seam)     │
   │ • (identity,pwd) │ │ • CallContextUtil → ClientCredentials from       │
   │   interceptor    │ │   ClaimsPrincipal                                │
   └──────────────────┘ └────────────────────┬──────────────────────────────┘
                                             │ used by
                          ┌──────────────────┴──────────────────┐
                          ▼                                     ▼
              ┌────────────────────────┐         ┌──────────────────────────┐
              │ Pod.Grpc.ConnectHost.  │         │ Pod.Grpc.ShellHost.      │
              │ Server                 │         │ Server                   │
              │ [Authorize(scheme=     │         │ [Authorize(scheme=       │
              │  "grpc-station")]      │         │  "grpc-station")]        │
              │ • ConnectHostServiceGrpc        │ • ShellHostServiceGrpc       │
              │   (1 RPC: GetHost)              │   (10 RPCs, incl. server-stream)
              │                                 │ • ShellApplicationServiceGrpc │
              │                                 │   (5 RPCs for app sync)       │
              └────────────────────────┘         └──────────────────────────┘
                          │                                     │
                          └──────────┬──────────────────────────┘
                                     ▼
                  ┌──────────────────────────────────────┐
                  │ Pod.Web.Center  (Grpc.AspNetCore)    │
                  │ • Startup.cs:                         │
                  │     services.AddGrpc()                │
                  │     .AddGrpcStationMetadata()         │
                  │     endpoints.MapGrpcService<T>() ×3  │
                  │ • Kestrel handles HTTP/2 + TLS        │
                  └──────────────────────────────────────┘
```

### Layering rules

1. **`Pod.Grpc.Base`** — the only project that compiles `.proto` files. Generated `.cs` lands in `Pod.Grpc.Base/Export/` and is **linked** (not copied) by every consumer. This is the canonical contract.
2. **`Pod.Grpc.Const`** + **`Pod.Grpc.Messages`** + **`Pod.Grpc.Utilities`** — leaf utility libraries (netstandard2.0) shared by client and server. No business deps.
3. **`Pod.Grpc.Base.Server`** + **`Pod.Grpc.Base.Client`** — hosting/connecting infrastructure.
   - Server side is `net10.0`, built on `Grpc.AspNetCore` 2.66. It owns the `GrpcMetadataAuthenticationHandler` (the AspNetCore `AuthenticationHandler<T>` that validates the kiosk's `(identity, password)` headers) and the `CallContextUtil` helper that surfaces the authenticated `StationId` to service implementations.
   - Client side is `netstandard2.0` so it can also be referenced by the .NET Framework 4.7.1 WPF kiosk; backed by `Grpc.Core` 2.46.6 (last release that supports netstandard2.0).
4. **`Pod.Grpc.ConnectHost.Server`** + **`Pod.Grpc.ShellHost.Server`** — the actual service implementations, one per `service` block in the `.proto` files. Each service class is decorated with `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]`. Hosting + binding is done in `Pod.Web.Center/Startup.cs` via `services.AddGrpc()` + `endpoints.MapGrpcService<T>()`.

### Asymmetric folder name

The folder is `Pod.Grpc.Const/` but the csproj inside it is `Pod.Grpc.Base.Const.csproj`. Every reference uses the csproj name. Don't rename either without updating the other.

---

## The three services

| Service | Defined in `.proto` | Implemented in | Purpose |
|---------|--------------------|-----------------|---------|
| `ConnectHostServiceGrpc` | `ServiceConnectHost.proto` | `Pod.Grpc.ConnectHost.Server.Services.ConnectHostServiceGrpc` | Station discovery / "give me a server I can connect to". 1 RPC. |
| `ShellHostServiceGrpc` | `ServiceShellHost.proto` | `Pod.Grpc.ShellHost.Server.Services.ShellHostServiceGrpc` | Connect, heartbeat, login, session state, server-stream notifications, disconnect. 10 RPCs. |
| `ShellApplicationServiceGrpc` | `ServiceShellApplications.proto` | `Pod.Grpc.ShellHost.Server.Services.ShellApplicationServiceGrpc` | Application install/update/uninstall sync between station and server. 5 RPCs. |

The two server-side projects exist as separate assemblies but the **same hosted server process** binds all three services to one TCP port (defaults to `50061`).

---

## Call flow — station → server (typical session login)

```
LeapPlay.Shell.exe
   │
   │ 1. Resolve which Shell server to talk to
   ▼
ConnectHostServiceGrpc.GetHost(ShellServerRequest{IdentityId, MaxInterfaceVersion, ReconnectConnectionId})
   │ ← ShellServerResponse{ConnectionId, HostAddress, Port, RequiredInterfaceVersion}
   │
   │ 2. Open the actual ShellHost channel
   ▼
ShellHostServiceGrpc.Connect(ConnectRequest{ConnectionId})
ShellHostServiceGrpc.GetServerSettings()    → heartbeat interval/timeout, server clock
ShellHostServiceGrpc.GetClientSettings()    → display name, QR code, control mode
ShellHostServiceGrpc.GetNotifications(...)  ← *server-stream*  ClientNotification events
   │
   │ 3. Periodic keep-alive
   ▼
ShellHostServiceGrpc.SendHeartbeat(HeartbeatRequest{ConnectionId})
   │
   │ 4. Operator/QR triggers a login on the server
   ▼
ShellHostServiceGrpc.SendLoginIntention(LoginRequest)
   │ ← LoginRequestResponse{SessionDetails (state=LoginRequested)}
   │
   │ Server publishes ClientNotificationEvent.CheckLoginRequest on the notification stream
   ▼
ShellHostServiceGrpc.GetLoginIntention(...) → SessionDetails (state=AwaitingConfirmation)
ShellHostServiceGrpc.SendLoginResponse(LoginIntentionReplyRequest{IsLoginAccepted=true})
   │ ← Session details, state Running
   │
   │ 5. Application activity
   ▼
ShellApplicationServiceGrpc.GetSyncAppStates(...) / SendSyncAppStates(...)
ShellApplicationServiceGrpc.SendAppInstalled / SendAppUpdate / SendAppUninstalled
   │
   │ 6. Logout / shutdown
   ▼
ShellHostServiceGrpc.SendLogoutRequest(LogoutRequest{Reason})
ShellHostServiceGrpc.Disconnect(DisconnectRequest)
```

Every server method follows the same shape (see `ShellHostServiceGrpc.cs`):

```csharp
[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]   // class-level
public class ShellHostServiceGrpc : ShellHost.ShellHostServiceGrpc.ShellHostServiceGrpcBase
{
    ...
    public override async Task<TResponse> SomeMethod(TRequest request, ServerCallContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var credentials = context.ToClientCredentials();                       // StationId from authenticated principal
            var svc         = scope.ServiceProvider.GetRequiredService<ShellService>();
            var result      = await svc.SomeOperation(credentials, request, ...);  // returns IResult<T>
            if (result.IsSuccess()) return result.ReturnValue;
            throw result.ToException();                                            // → RpcException with metadata
        }
    }
}
```

`GrpcMetadataAuthenticationHandler` runs *before* the method body — by the time `context.ToClientCredentials()` runs, the StationId on the returned `ClientCredentials` is the one the auth handler validated; the password is still pulled from headers so legacy `verifyCredentials` re-checks (e.g. inside `GetNotifications`) keep working.

The client wrapper in `LeapVR.Shell.Services/RpcServices/` catches `RpcException`, calls `.ToResult()` (defined in `Pod.Grpc.Utilities`), and surfaces the same `IResult` shape upstream — so business errors round-trip without leaking gRPC types into the kiosk's domain code.

---

## Auth in one paragraph

`Pod.Grpc.Base.Client.GrpcChannelCredentialsHandler` builds an `SslCredentials` from the **server root CA** (server-cert TLS only by default — `ForceClientCertificate=false`), with an optional client key/cert that the application layer treats as a **license certificate**, not as session auth. It then layers a `CallCredentials.FromInterceptor` that puts plain `identity` (the `StationId`) and `password` (the literal `Station.Password`) headers on every call. On the server, those headers reach the AspNetCore pipeline as HTTP/2 headers; `Pod.Grpc.Base.Server.GrpcMetadataAuthenticationHandler` (the `AuthenticationHandler<T>` for the `"grpc-station"` scheme) reads them, calls `IGrpcStationCredentialVerifier.VerifyAsync(...)` (which by default delegates to `Pod.Services.Extensions.VerifyCredentials`, the same PBKDF2 path as before), and on success builds a `ClaimsPrincipal` carrying the StationId. Each service class is decorated with `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]` so the handler runs before any method body. Inside the method, `context.ToClientCredentials()` returns the authenticated StationId (read from the principal) and the password (still in the header, for backwards-compat with code that re-verifies inline). **No HMAC, no signing, no mutual TLS.** The `amx` HMAC scheme (`Pod.Web.Authentication.ApiKeySecret`) and the `StationApiKey` entity are unrelated — they belong to the REST path. See `docs/architecture/auth.md` for the full story.

---

## Real-time push: `PublisherHub<T>`

The only server-stream RPC is `ShellHostServiceGrpc.GetNotifications`. It uses `PublisherHub<ClientCommandType>` (in `Pod.Services`) to receive enum events from anywhere in the server, translate them through `Extensions.ToNotificationMessage`, and write `ClientNotification` messages onto the open `IServerStreamWriter`. Other RPCs **publish** by calling `_serviceProvider.GetService<PublisherHub<ClientCommandType>>().Publish(stationId, ClientCommandType.GetLoginRequest)` (see `SendLoginIntention` and `Disconnect`).

---

## Versions

- **Server side (net10.0):** `Grpc.AspNetCore` **2.66.0** (transitively pulls `Grpc.AspNetCore.Server` and `Grpc.Core.Api`).
- **Kiosk side (netstandard2.0, consumed from net471):** `Grpc.Core` **2.46.6** — the last release that supports netstandard2.0.
- **Code generation (multi-target):** `Grpc.Tools` **2.66.0** + `Google.Protobuf` **3.27.0**.
- `proto3` syntax everywhere.
- Wire format unchanged from the 1.19.x days; the kiosk does not need to change to talk to the upgraded server.

---

## Per-project READMEs

- [Pod.Grpc.Base](./Pod.Grpc.Base/README.md) — `.proto` contracts + generated stubs (**read first**)
- [Pod.Grpc.Const](./Pod.Grpc.Const/README.md) — header keys, channel options
- [Pod.Grpc.Messages](./Pod.Grpc.Messages/README.md) — partial classes + Guid/Date converters
- [Pod.Grpc.Utilities](./Pod.Grpc.Utilities/README.md) — `IResult` ⇄ `RpcException` mapping
- [Pod.Grpc.Base.Client](./Pod.Grpc.Base.Client/README.md) — `GrpcClient<T>`, channel handler
- [Pod.Grpc.Base.Server](./Pod.Grpc.Base.Server/README.md) — `GrpcServer<T>`, cert loading
- [Pod.Grpc.ConnectHost.Server](./Pod.Grpc.ConnectHost.Server/README.md) — `ConnectHostServiceGrpc`
- [Pod.Grpc.ShellHost.Server](./Pod.Grpc.ShellHost.Server/README.md) — `ShellHostServiceGrpc` + `ShellApplicationServiceGrpc`

## Related docs

- [`docs/architecture/grpc.md`](../../architecture/grpc.md) — protocol-level overview
- [`docs/architecture/auth.md`](../../architecture/auth.md) — the three coexisting schemes (JWT REST, `amx` HMAC REST, gRPC `(identity, password)`)
- [`docs/architecture/session-lifecycle.md`](../../architecture/session-lifecycle.md) — the state machine these RPCs drive
