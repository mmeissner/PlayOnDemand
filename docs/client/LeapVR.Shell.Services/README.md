# LeapVR.Shell.Services

> The kiosk's gRPC client wrappers. Connects to `Pod.Web.Center` (Connect host + Shell host servers), attaches the station's `(identity, password)` credentials as plain gRPC metadata via a `CallCredentials.FromInterceptor`, and exposes typed service surfaces (`ConnectService`, `ShellService`, `ApplicationService`) consumed via the `IRemoteServiceSet` aggregate.

## Purpose

This is the only client-tier project that knows about gRPC. Everything above it (controllers, view models) sees a `IRemoteServiceSet` — a bundle of `ISessionServiceOutgoing`, `IPlatformServiceOutgoing`, `IStationServiceOutgoing`, plus their incoming counterparts — and never imports `Grpc.Core` or any `Pod.Grpc.*` namespace directly.

Two RPC servers live behind the wire (see server-tier docs):

- **Connect host** — `Pod.Grpc.ConnectHost.Server` — initial connect / "where do I send shell traffic" lookup.
- **Shell host** — `Pod.Grpc.ShellHost.Server` — the operational kiosk channel (sessions, app lifecycle, heartbeats).

The factory pattern: `RemoteServiceFactory.GetStationServices()` constructs a `RemoteServicesSet` that lazily builds `ConnectService`, `ShellService`, and `ApplicationService` clients, each wrapped over the appropriate generated gRPC client (`Pod.Grpc.ConnectHost.ConnectHostServiceGrpc.ConnectHostServiceGrpcClient`, etc.) with server-cert TLS credentials and per-call `(identity, password)` metadata supplied by `CallCredentials.FromInterceptor`.

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Services.dll`)
- **Key NuGet packages:** (gRPC pulled transitively via `Pod.Grpc.Base.Client`)
  - `Newtonsoft.Json` — DTO serialisation glue
  - `NLog` — logging
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`, `LeapVR.Shared.Lib.Win`
  - `LeapVR.Shell.Controllers` (consumes `RemoteServiceController` / `IRemoteServiceSet` interface)
  - `LeapVR.Shell.Domain.Models`, `LeapVR.Shell.Modules.Interfaces`
  - `Pod.Data.Infrastructure`, `Pod.Enums`
  - `Pod.Grpc.Base.Client` (the `GrpcClient<T>` + `RpcHostDetails` infrastructure)
  - `Pod.Grpc.Messages` (DTO ↔ Protobuf converters)
  - `Pod.Grpc.Utilities` (error → exception mapping)

## Responsibility

**It IS responsible for:**
- Building gRPC channels with server-cert TLS and the `(identity, password)` `CallCredentials.FromInterceptor` (`GrpcSslCredentials`). The kiosk's `StationId` and `Station.Password` are attached as plain `identity` / `password` metadata headers per call; the server PBKDF2-verifies the password against `Station.PasswordHash`.
- One-class-per-RPC-service wrapper that *speaks the kiosk's local interfaces* and translates them to/from generated Protobuf messages.
- Subscribed/long-poll session lifecycle (`SubscribedSession`, `RemoteSession`, billing rate types).
- Hosting `IServerConfig` constants and `RpcClientConfig` (timeouts) in their persisted form.

**It is NOT responsible for:**
- The gRPC protocol *definition* — `.proto` files live in `Pod.Grpc.Base`.
- Server-side message handling (`Pod.Grpc.ShellHost.Server`, `Pod.Grpc.ConnectHost.Server`).
- Session state-machine logic (`StationController` / `RemoteServiceController`).
- Persistence (LiteDB lives in `LeapVR.Shell.Repository`).

## Public API surface

### Factory + service-set

| Type | Purpose |
|---|---|
| `RemoteServiceFactory` | Builds `IRemoteServiceSet` instances. Owns `GrpcSslCredentials`. Methods: `GetStationServices()`, `GetConnectService(stationId, password)`, `GetStationService(...)`. The `(stationId, password)` pair is fed into `GrpcChannelCredentialsHandler.SetChannelCredentials` so every gRPC call carries it as `identity` / `password` metadata. |
| `RemoteServicesSet` (`RemoteServicesSet.cs`) | Implementation of `IRemoteServiceSet` (interface lives in `LeapVR.Shell.Controllers/RemoteService/Interfaces/`). Lazy properties for each typed service. |
| `RpcClientConfig` (`Factory/RpcClientConfig.cs`) | Persisted client config — gRPC call timeout, retry policy. |

### Service wrappers (`RpcServices/`)

| Class | Wraps | Implements |
|---|---|---|
| `BaseService` | Common error mapping, retry, logging | (abstract) |
| `ConnectService` | `ConnectHostServiceGrpc.ConnectHostServiceGrpcClient` | The "first call" service that hands the kiosk back its `ShellServer` host details. |
| `ShellService` | `ShellHostServiceGrpc.ShellHostServiceGrpcClient` | Sessions, login intentions, billing, heartbeat. |
| `ApplicationService` | `ShellApplicationServiceGrpc.ShellApplicationServiceGrpcClient` | App-installation reporting, app-execution reporting. |
| `RpcConnection` | (helper) | Implements `IRpcConnection` from `Controllers/RemoteService/Interfaces/`; tracks current connection state for the UI. |

### Session

| Class | Purpose |
|---|---|
| `RemoteSession` (`Session/RemoteSession.cs`) | Server-driven session bookkeeping; consumed by the login viewmodel. |
| `SubscribedSession` (`SubscribedSession.cs`) | Subscription wrapper that opens a long-call to `LongGetLogInIntentionAsync` and surfaces `ILoginIntention` pushes as events. |
| `BaseSessionRate`, `NoBillingSessionRate`, `PrepaidSessionRate` | Strategy types for billing-rate handling. |
| `ISessionService` (`Session/ISessionService.cs`) | Internal aggregator interface. |
| `UISessionData` | View-facing session DTO. |

### RPC client glue

| Class | Purpose |
|---|---|
| `GrpcSslCredentials` (`RpcClient/GrpcSslCredentials.cs`) | Builds `Grpc.Core.SslCredentials` from `IServerConfig` (root CA, server-cert TLS only) plus an optional client cert/key (treated by the application layer as a *license* certificate, not as session auth). The call-credentials handler injects the literal `StationId` (header `identity`) and `Station.Password` (header `password`) as gRPC metadata for every call. |

### Data

| Class | Purpose |
|---|---|
| `Data/Entities.cs` | DTO containers (e.g. `ShellServer`). |
| `Data/GeneralErrorNotification.cs` | Server-pushed error wrapping. |
| `Data/ShellClientInfo.cs` | Client identity bundle handed across services. |

### Static service hooks

| Type | Purpose |
|---|---|
| `IdentificationServiceClientOut` | Outgoing identification-service surface (used at connect time). |

## Internal structure

```
LeapVR.Shell.Services/
├── RemoteServicesSet.cs                IRemoteServiceSet implementation (lazy bag of services)
├── SubscribedSession.cs                Long-poll wrapper for login-intention subscription
├── IdentificationServiceClientOut.cs
├── Factory/
│   ├── RemoteServiceFactory.cs         Builds services with server-cert TLS + (identity, password) call-credentials
│   └── RpcClientConfig.cs              Persisted client config
├── RpcClient/
│   └── GrpcSslCredentials.cs           SSL + call-credentials assembly
├── RpcServices/
│   ├── BaseService.cs                  Shared error mapping
│   ├── ConnectService.cs               ConnectHost wrapper
│   ├── ShellService.cs                 ShellHost wrapper
│   ├── ApplicationService.cs           ShellApplication wrapper
│   └── RpcConnection.cs                Connection-state tracker
├── Session/
│   ├── ISessionService.cs              Internal aggregator
│   ├── RemoteSession.cs                Server-driven session bookkeeping
│   ├── BaseSessionRate.cs / NoBillingSessionRate.cs / PrepaidSessionRate.cs
│   └── UISessionData.cs                View-facing DTO
├── Data/
│   ├── Entities.cs                     ShellServer + connection DTOs
│   ├── GeneralErrorNotification.cs
│   └── ShellClientInfo.cs
├── Properties/AssemblyInfo.cs
├── packages.config / app.config
└── LeapVR.Shell.Services.csproj
```

## Notable patterns / gotchas

- **No `Grpc.Core` types leak above this project.** Service wrappers receive Protobuf messages, convert with `Pod.Grpc.Messages`, and return `LeapVR.Shell.Domain.Models` shapes (or `Pod.Data.Infrastructure.IResult<T>`-style returns). Maintain that boundary.
- **gRPC auth is `(identity, password)` plain metadata per call — no HMAC, no signing, no timestamps.** The call-credentials handler in `GrpcSslCredentials` adds the station id (header `identity`) and the literal station password (header `password`) on every outgoing call. Server-side verification is `Pod.Services.Extensions.VerifyCredentials` which loads the `Station` and PBKDF2-compares against `Station.PasswordHash`. Confidentiality is supplied by TLS (server cert), not by HMAC. ⚠️ This was previously documented as HMAC ApiKey/Secret — that scheme (`Pod.Web.Authentication.ApiKeySecret`, scheme name `"amx"`) is REST-only middleware and does not gate gRPC. Clock skew is irrelevant.
- **`ConnectService` is bootstrap-only.** The kiosk hits the connect host first to learn which `ShellServer` (host + port) to use, *then* uses `ShellService` / `ApplicationService` against that. The `RemoteServiceFactory` exposes both paths; the controller layer drives the order.
- **`SubscribedSession` keeps a long-poll open.** It calls `LongGetLogInIntentionAsync` repeatedly, sleeping briefly on `isTimeouted = true`. Disposing it cancels the loop.
- **`RpcClientConfig` has a `GrpcCallTimeout`.** Tweak via the persisted JSON, not via code constants.
- **`StaticServerConfig` is registered in `LeapVR.Shell/Bootstrapper.cs#RegisterConfigurations`** — it carries the embedded root CA / connect-host details. It's read here as an `IServerConfig` interface, but the implementation lives in the bootstrapper for now.

## Consumers

- `LeapVR.Shell` — `Bootstrapper.RegisterRPCServices` registers `RemoteServiceFactory` (singleton) and `IRemoteServiceSet` produced by it.
- `LeapVR.Shell.Controllers` — `RemoteServiceController` adapts `IRemoteServiceSet` into the local controller-friendly surface; `StationController` and `PlatformController` consume the controller wrapper rather than touching this project directly.

## Related docs

- Sister contract: [`LeapVR.Shell.Services.Interfaces`](../LeapVR.Shell.Services.Interfaces/README.md)
- Closely related (server side): `docs/server/grpc/Pod.Grpc.Base/`, `docs/server/grpc/Pod.Grpc.Base.Client/`, `docs/server/grpc/Pod.Grpc.ShellHost.Server/`, `docs/server/grpc/Pod.Grpc.ConnectHost.Server/`, `docs/server/Pod.Web.Authentication.ApiKeySecret/`
- Architecture: `docs/architecture/grpc.md` (planned), `docs/architecture/auth.md` (planned)
- Tier overview: [`docs/client/README.md`](../README.md)
