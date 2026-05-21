# Pod.Grpc.ShellHost.Server

> Server-side implementation of `ShellHostServiceGrpc` (10 RPCs incl. server-stream notifications) and `ShellApplicationServiceGrpc` (5 RPCs for app sync). This is where all per-station session and application traffic lands.

## Purpose

Once a station has called `ConnectHost.GetHost` and learned which Shell server to use, *every* further call goes here. This project owns the two service classes that implement:

- **`ShellHostServiceGrpc`** — connection lifecycle (`Connect`/`Disconnect`), settings (`GetServerSettings`/`GetClientSettings`), keep-alive (`SendHeartbeat`), the long-lived push stream (`GetNotifications`), and the full session login state machine (`SendLoginIntention` → `GetLoginIntention` → `SendLoginResponse` → `GetSessionState` → `SendLogoutRequest`).
- **`ShellApplicationServiceGrpc`** — bulk and single-event sync of the station's installed application catalog with the server.

As with `Pod.Grpc.ConnectHost.Server`, almost no business logic lives here. Each method is a thin shell that:
1. Pulls headers via `context.ToClientCredentials()`.
2. Opens a per-call DI scope.
3. Delegates to a `Pod.Services` service (`ShellService` or `ShellApplicationService`).
4. Returns the wire DTO on success or `throw result.ToException()` on error.
5. (Some methods) publishes a `ClientCommandType` event onto `PublisherHub<ClientCommandType>` so other connected stations / the notification stream learn about the change.

## Tech

- **Target framework:** `net10.0`
- **Configurations:** `Debug`, `Release`
- **Key NuGet packages:**
  - `Microsoft.AspNetCore.App` (FrameworkReference) — provides `[Authorize]`, `ILogger<T>`, DI, and (transitively) the `Grpc.AspNetCore.Server` runtime needed to bind these services.
- **Project references (in this repo):**
  - `Pod.Data.Models` — `ShellServer`, `Station`, etc. injected into the service ctors
  - `Pod.Data` — `PodDbContext`
  - `Pod.Grpc.Base.Server` — `CallContextUtil`, `GrpcMetadataAuthenticationHandler`
  - `Pod.Grpc.Messages` — wire DTOs + Guid/Time converters
  - `Pod.Services` — `ShellService`, `ShellApplicationService`, `PublisherHub<T>`, `ClientCredentials`, `ClientCommandType`
- **Linked source files** (from `Pod.Grpc.Base/Export/`):
  - `ServiceShellApplications.cs`, `ServiceShellApplicationsGrpc.cs`
  - `ServiceShellHost.cs`, `ServiceShellHostGrpc.cs`

## Responsibility

**IS** responsible for:
- Implementing both `ShellHostServiceGrpcBase` and `ShellApplicationServiceGrpcBase`.
- Declaring `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]` on each service class.
- Per-call DI scoping.
- The `ClientCommandType` ↔ `ClientNotificationEvent` mapping (in `Extensions.cs`) used by the streaming method.
- Wiring `PublisherHub<ClientCommandType>` to the `responseStream` in `GetNotifications`, then bouncing certain RPCs (`SendLoginIntention`, `Disconnect`) into that hub so connected listeners get a push.

**IS NOT** responsible for:
- The session state machine itself (`SessionState_LoginRequested → AwaitingConfirmation → Running` etc.) — that's `Pod.Services.ShellHost.ShellService`.
- The application catalog sync algorithm — that's `Pod.Services.Applications.ShellApplicationService`.
- Verifying the station password — that happens once, up-front, in `GrpcMetadataAuthenticationHandler`.
- Hosting the gRPC endpoint or binding the services (that's `Pod.Web.Center/Startup.cs` via `services.AddGrpc()` + `endpoints.MapGrpcService<T>()`).

## Public API surface

`namespace Pod.Grpc.ShellHost.Server.Services`:

- `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]`
- `class ShellHostServiceGrpc : ShellHost.ShellHostServiceGrpc.ShellHostServiceGrpcBase`
  - `ctor(ILogger<ShellHostServiceGrpc>, IServiceProvider, ShellServer serverInfo)` — under Grpc.AspNetCore the service itself is scoped (auto-registered by `MapGrpcService<T>()`); `ShellServer` stays singleton and identifies *this* server instance.
- `[Authorize(AuthenticationSchemes = GrpcMetadataAuthenticationHandler.SchemeName)]`
- `class ShellApplicationServiceGrpc : ShellApplications.ShellApplicationServiceGrpc.ShellApplicationServiceGrpcBase`
  - `ctor(ILogger<ShellHostServiceGrpc>, IServiceProvider)` — note the logger generic re-uses the other class deliberately.
- `static class Extensions`
  - `ClientNotificationEvent ToNotificationMessage(this ClientCommandType clientCommandType)` — maps service-level events (`UpdateServerSettings`, `UpdateClientSettings`, `GetLoginRequest`, `UpdateSession`, `Unset`, `SendHeartbeat`) to their wire enum equivalents. Throws `ArgumentOutOfRangeException` for unknown values.

## RPC catalog — `ShellHostServiceGrpc`

Every method follows the **standard pattern** unless noted:

```csharp
using (var scope = _serviceProvider.CreateScope())
{
    var credentials = context.ToClientCredentials();
    var shellService = scope.ServiceProvider.GetRequiredService<ShellService>();
    var result = await shellService.SOMETHING(credentials, request, ...);
    if (result.IsSuccess()) return result.ReturnValue;
    throw result.ToException();
}
```

| RPC | Delegates to | Notes |
|-----|--------------|-------|
| `Connect(ConnectRequest)` | `ShellService.ConnectToServer(credentials, request, _serverInfo.Id)` | Binds the connection to *this* server. |
| `GetServerSettings(ServerSettingsRequest)` | *no service call* — built inline from `_serverInfo` | Returns `HeartbeatInterval`, `HeartbeatTimeout`, `ServerTimeUtcNow`. |
| `GetClientSettings(ClientSettingsRequest)` | `ShellService.GetClientSettings(credentials)` | DisplayName, QrCode, ControlMode. |
| `SendHeartbeat(HeartbeatRequest)` | `ShellService.SetHeartbeat(credentials, request, _serverInfo.Id)` | |
| `GetNotifications(NotificationRequest, IServerStreamWriter<ClientNotification>)` | **server-stream**, see below | Long-lived push channel. |
| `SendLoginIntention(LoginRequest)` | `ShellService.RequestLogin(credentials, request, context.Peer)` | **Publishes** `ClientCommandType.GetLoginRequest` to the hub on success. |
| `GetLoginIntention(RequestedLoginRequest)` | `ShellService.GetLoginIntention(credentials, request)` | |
| `SendLoginResponse(LoginIntentionReplyRequest)` | `ShellService.SetLoginResponse(credentials, request)` | Carries `IsLoginAccepted`. |
| `GetSessionState(SessionStateRequest)` | `ShellService.GetSessionState(credentials, request)` | |
| `SendLogoutRequest(LogoutRequest)` | `ShellService.LogoutSession(credentials, request)` | Carries `LogoutReason`. |
| `Disconnect(DisconnectRequest)` | `ShellService.Disconnect(credentials, request, _serverInfo.Id)` | **Publishes** `ClientCommandType.Disconnect` to the hub on success — that's the in-band signal that ends `GetNotifications`. |

### `GetNotifications` in detail

This is the only server-stream RPC and the only method that does not follow the standard pattern. It exists to push events from the server to a connected station in real time (login requests, settings changes, heartbeat reminders).

```csharp
public override async Task GetNotifications(
        NotificationRequest request,
        IServerStreamWriter<ClientNotification> responseStream,
        ServerCallContext context)
```

Sequence:

1. Extract credentials (StationId from the authenticated principal; password from headers), open DI scope with the request `PodDbContext`.
2. **Verify**:
   - `await credentials.VerifyCredentials(dbContext)` — re-runs the PBKDF2 check the auth handler already did. Defence-in-depth; redundant under `[Authorize(AuthenticationSchemes = "grpc-station")]` but kept because the call is bundled with the connection-id checks below and removing only the password half would be confusing.
   - `request.ConnectionId.HasConnectionId(out var connectionId)` (request is well-formed).
   - The connection id matches what `Stations.ConnectionState.ConnectionId` says for this station.
   - Any failure ⇒ `throw verificationResult.ToException();`
3. Acquire the singleton `PublisherHub<ClientCommandType>` and call `GetHandler(stationId, responseStream, …)`. Parameters:
   - `stationId` — the dictionary key for this connection.
   - `responseStream` — where messages are written.
   - `message => new ClientNotification { Event = message.ToNotificationMessage() }` — converter from internal enum to wire DTO.
   - `message => message == ClientCommandType.Disconnect` — the **end-of-stream sentinel**. Receiving this stops the loop.
   - `new[] { ClientCommandType.UpdateServerSettings, ClientCommandType.UpdateClientSettings }` — initial messages queued so the kiosk pulls fresh settings on (re)connect.
4. `await handler.ReceiveMessages(context.CancellationToken)` — blocks until cancellation or the disconnect sentinel.
5. Catches and logs any exception as `LogCritical` then rethrows.

The handler returned by `PublisherHub<T>.GetHandler` can be `null` if the hub is shutting down — checked before the await.

### Publisher-hub side-effects

Two RPCs intentionally publish events back into the hub *after* the underlying service call succeeds:

- `SendLoginIntention` → `Publish(stationId, ClientCommandType.GetLoginRequest)` — tells the open notification stream to send `ClientNotificationEvent.CheckLoginRequest` to the kiosk so it knows to call `GetLoginIntention`.
- `Disconnect` → `Publish(stationId, ClientCommandType.Disconnect)` — the sentinel that closes `GetNotifications` cleanly.

Other notifications (settings updates, session-state changes) are published from elsewhere in `Pod.Services` / web controllers, not from this assembly.

## RPC catalog — `ShellApplicationServiceGrpc`

All five RPCs follow the **standard pattern** with `ShellApplicationService`:

| RPC | Delegates to | Purpose |
|-----|--------------|---------|
| `GetSyncAppStates(SyncRequest)` | `ShellApplicationService.GetSyncState(credentials, request)` | Server's view of installed apps (or just last-sync timestamp depending on `SendOnlyLastSyncTimestamp`). |
| `SendSyncAppStates(SyncAppsRequest)` | `ShellApplicationService.SyncStates(credentials, request)` | Bulk delta upload (Installations + Updates + Uninstalls). |
| `SendAppInstalled(AppInstalledRequest)` | `ShellApplicationService.AppInstalled(credentials, request)` | Single install event. |
| `SendAppUpdate(AppUpdateRequest)` | `ShellApplicationService.AppUpdated(credentials, request)` | Single update event (name/enabled state). |
| `SendAppUninstalled(AppUninstalledRequest)` | `ShellApplicationService.AppUninstalled(credentials, request)` | Single uninstall event. |

None of them publish to `PublisherHub<T>`.

## Internal structure

```
Pod.Grpc.ShellHost.Server/
├── Pod.Grpc.ShellHost.Server.csproj
├── Extensions.cs                          ← ClientCommandType → ClientNotificationEvent map
└── Services/
    ├── ShellHostServiceGrpc.cs            ← 10 RPCs above (incl. server-stream)
    └── ShellApplicationServiceGrpc.cs     ← 5 RPCs above
```

The four linked generated files (`Service*.cs`, `Service*Grpc.cs`) appear in the IDE under `Services/` thanks to the csproj `Link=` attribute but are not on disk in this folder.

## Notable patterns / gotchas

- **`_serverInfo` is a singleton `ShellServer` entity.** It identifies *this* hosted server instance. Several RPCs pass `_serverInfo.Id` so the service knows which `ShellServer` row owns the connection — important when multiple servers are clustered.
- **`GetServerSettings` does not open a DI scope.** It's a pure projection of the singleton `_serverInfo` plus `DateTime.UtcNow`. Cheap.
- **Heartbeat is *server-side timed***: settings expose `HeartbeatInterval` (when the kiosk should send) and `HeartbeatTimeout` (after which the server considers the connection dead). The kiosk computes its own send rhythm from those values.
- **The `Disconnect` notification is in-band.** Publishing `ClientCommandType.Disconnect` is what causes the still-running `GetNotifications` task on the *same* station to break out of `ReceiveMessages` cleanly. If the kiosk just kills the channel without calling `Disconnect`, the stream is closed by gRPC instead — both are handled.
- **Result-chain validators in `GetNotifications`** use `verificationResult.ArgTrue(condition, name, UserError.X)` and `verificationResult.ValueTrue(condition, name, UserError.X)` from `Pod.Data.Infrastructure` — the standard fluent error-accumulation pattern. The same `verificationResult` is mutated in two places before being thrown.
- **Note the logger generic re-use** in `ShellApplicationServiceGrpc`: `ILogger<ShellHostServiceGrpc>`, not `<ShellApplicationServiceGrpc>`. Probably an oversight, but it means application-service logs land in the shell-host service category.
- **Login flow has two halves**: the kiosk calls `SendLoginIntention` (creating the request); the operator UI/external trigger drives the server to publish `CheckLoginRequest`; the kiosk then calls `GetLoginIntention` followed by `SendLoginResponse(IsLoginAccepted=…)`. Both halves reach the same `SessionDetails` row.
- **`HasConnectionId(out var)`** is an extension on `GuidAsBytes` defined in `Pod.Services` (not `Pod.Grpc.Messages`) that distinguishes wire-empty from `Guid.Empty`. Use it instead of `ToGuidNullable()` when you actually need to know whether the field was sent.

## Consumers

- `Pod.Web.Center/Startup.cs` — `services.AddGrpc()` + `endpoints.MapGrpcService<ShellHostServiceGrpc>()` + `endpoints.MapGrpcService<ShellApplicationServiceGrpc>()`. (Wired in by the orchestrator's integration step; not part of this branch.)
- (Indirect, client-side) `LeapVR.Shell.Services/RpcServices/ShellService.cs` and `ApplicationService.cs`. The kiosk path stays on `Grpc.Core` 2.46.6 (netstandard2.0); wire format unchanged.
- The two simulators for load and integration tests.

## Related docs

- [`docs/server/grpc/README.md`](../README.md) — gRPC tier overview & call flow
- [`docs/server/grpc/Pod.Grpc.Base/README.md`](../Pod.Grpc.Base/README.md) — full message catalog (`SessionDetails`, `ClientNotificationEvent`, etc.)
- [`docs/server/grpc/Pod.Grpc.ConnectHost.Server/README.md`](../Pod.Grpc.ConnectHost.Server/README.md) — what runs *before* this service is reachable
- [`docs/server/Pod.Services/`](../../Pod.Services/) — `ShellService`, `ShellApplicationService`, `PublisherHub<T>` business logic
- [`docs/architecture/session-lifecycle.md`](../../../architecture/session-lifecycle.md) — the state machine these RPCs drive
- [`docs/architecture/grpc.md`](../../../architecture/grpc.md) — protocol-level overview
