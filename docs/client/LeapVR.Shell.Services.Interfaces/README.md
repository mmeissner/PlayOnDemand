# LeapVR.Shell.Services.Interfaces

> Pure contracts for the kiosk's gRPC client wrappers, split into per-host outgoing/incoming pairs (Station, Session, Platform) plus the wire-error exception types and the persisted `CredentialConfig` shape.

## Purpose

Where `LeapVR.Shell.Services` *implements* gRPC clients, this project declares only their visible surface. Controllers (and the local `RemoteServiceController` adapter) consume these interfaces; the implementation project provides the concrete types.

The split is deliberately by *host* (Station vs Session vs Platform) and by *direction* (Incoming vs Outgoing):

- **Outgoing** = the kiosk calls the server (most traffic).
- **Incoming** = the server pushes events to the kiosk (long-poll/streaming responses, subscribed channels).

A station's logical service surface is therefore six interfaces — one per host × direction — bundled into the `IRemoteServiceSet` aggregate (which lives in `LeapVR.Shell.Controllers/RemoteService/Interfaces/`, not here, because it's where controllers consume it).

## Tech

- **Target framework:** .NET Framework 4.6.2 (note: lower than the rest of the client tier — this project predates the .NET 4.7.1 bump and works fine at 4.6.2)
- **Output:** `Library` (`LeapVR.Shell.Services.Interfaces.dll`)
- **Key NuGet packages:** none — pure contracts.
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`
  - `LeapVR.Shell.Domain.Models`
  - `LeapVR.Shell.Modules.Interfaces`
  - `LeapVR.Shell.Managers`

## Responsibility

**It IS responsible for:**
- Declaring per-host service contracts (`IStationServiceOutgoing`, `IStationServiceIncoming`, `ISessionServiceOutgoing`, `ISessionServiceIncoming`, `IPlatformServiceOutgoing`, `IPlatformServiceIncoming`).
- Declaring the wire-error exception types (`GrpcConnectionException`, `GrpcUnexpectedCodeException`).
- The persisted `CredentialConfig` shape (the JSON that holds station id + secret on disk).

**It is NOT responsible for:**
- Implementations (in `LeapVR.Shell.Services`).
- The `IRemoteServiceSet` aggregate (in `LeapVR.Shell.Controllers/RemoteService/Interfaces/`, intentionally — it's where controllers consume it).
- DTO shapes flowing across the wire (those live in `LeapVR.Shell.Domain.Models` or `Pod.Grpc.Messages`).

## Public API surface

### Station

| Interface | File | Purpose |
|---|---|---|
| `IStationServiceOutgoing` | `Station/IStationServiceOutgoing.cs` | Calls station-host RPCs: register, heartbeat, fetch station-config. |
| `IStationServiceIncoming` | `Station/IStationServiceIncoming.cs` | Receives server-initiated station messages (mode change, force-shutdown). |

### Session

| Interface | File | Purpose |
|---|---|---|
| `ISessionServiceOutgoing` | `Session/ISessionServiceOutgoing.cs` | Calls session-host RPCs: `IntendAnonymousSession`, `MakeLoginDecision`, `DiscardLoginIntention`, `LongGetLogInIntentionAsync`, `AckLoginIntention`, billing operations. |
| `ISessionServiceIncoming` | `Session/ISessionServiceIncoming.cs` | Receives session-driven pushes (login intentions, session terminations). |
| `SessionServiceOutgoing.cs` | `Session/SessionServiceOutgoing.cs` | A small abstract base or DTO file shared between outgoing implementations. |

### Platform

| Interface | File | Purpose |
|---|---|---|
| `IPlatformServiceOutgoing` | `Platform/IPlatformServiceOutgoing.cs` | Reports app installation/execution lifecycle to the server. |
| `IPlatformServiceIncoming` | `Platform/IPlatformServiceIncoming.cs` | Receives platform-level pushes (force install/uninstall, library refresh). |

### Exceptions

| Type | File | Notes |
|---|---|---|
| `GrpcConnectionException` | `Exceptions/GrpcConnectionException.cs` | Thrown when the underlying channel fails (network, TLS, dead host). |
| `GrpcUnexpectedCodeException` | `Exceptions/GrpcUnexpectedCodeException.cs` | Thrown when the server returns a status code the wrapper doesn't have a typed mapping for. |

### Config

| Type | File | Notes |
|---|---|---|
| `CredentialConfig` | `FileConfig/CredentialConfig.cs` | Persisted JSON shape for station credentials (api key + secret). Loaded via `IConfigFileRepository<CredentialConfig>`. |

## Internal structure

```
LeapVR.Shell.Services.Interfaces/
├── Exceptions/
│   ├── GrpcConnectionException.cs
│   └── GrpcUnexpectedCodeException.cs
├── FileConfig/
│   └── CredentialConfig.cs
├── Platform/
│   ├── IPlatformServiceIncoming.cs
│   └── IPlatformServiceOutgoing.cs
├── Session/
│   ├── ISessionServiceIncoming.cs
│   ├── ISessionServiceOutgoing.cs
│   └── SessionServiceOutgoing.cs
├── Station/
│   ├── IStationServiceIncoming.cs
│   └── IStationServiceOutgoing.cs
├── Properties/AssemblyInfo.cs
└── LeapVR.Shell.Services.Interfaces.csproj
```

## Notable patterns / gotchas

- **`IRemoteServiceSet` is not here.** That aggregate lives in `LeapVR.Shell.Controllers/RemoteService/Interfaces/` so controllers can consume it without referencing services. The contract here is the per-host slice.
- **Outgoing/Incoming pairs match the gRPC topology.** Outgoing maps to client-streaming or unary calls the kiosk initiates; incoming maps to server-streaming or long-poll responses the kiosk receives. If you add a new RPC, put it on the right side of the split.
- **`GrpcConnectionException` vs `GrpcUnexpectedCodeException`.** The first means "I couldn't talk to the server"; the second means "I talked to the server and it said something I don't know how to interpret". Caller code typically retries the first (with backoff) and treats the second as a bug.
- **`.NET 4.6.2` target is intentional but odd.** The rest of the client tier targets 4.7.1; this project compiles at 4.6.2 because it has no dependencies that need newer surface. Don't bump it without a reason — moving it can ripple unexpectedly.

## Consumers

- `LeapVR.Shell.Services` — implements every contract here.
- `LeapVR.Shell.Controllers` — `RemoteServiceController` and `IRemoteServiceSet` (which lives in controllers) wrap these interfaces.
- `LeapVR.Shell` — registers nothing here directly (it registers the implementation project's `RemoteServiceFactory`).

## Related docs

- Sister implementation: [`LeapVR.Shell.Services`](../LeapVR.Shell.Services/README.md)
- Closely related: [`LeapVR.Shell.Controllers`](../LeapVR.Shell.Controllers/README.md) (consumers + `IRemoteServiceSet` aggregate); server-side: `docs/server/grpc/`
- Tier overview: [`docs/client/README.md`](../README.md)
- Architecture: `docs/architecture/grpc.md` (planned), `docs/architecture/auth.md` (planned), `docs/architecture/session-lifecycle.md` (planned)
