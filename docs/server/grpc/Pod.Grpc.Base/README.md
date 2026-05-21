# Pod.Grpc.Base

> The single source of truth for the station ↔ server wire protocol: holds every `.proto` file and the generated C# stubs that all other `Pod.Grpc.*` projects consume.

## Purpose

This is the **contract project**. It owns three `service` definitions and four message files. The MSBuild step compiles every `*.proto` in the project root via `Grpc.Tools` and emits the generated `.cs` into `Export/`. Those generated files are not compiled here (`CompileOutputs="false"`) — they are *linked* (`<Compile Include="..\Pod.Grpc.Base\Export\..." Link="..."/>`) into `Pod.Grpc.Messages` (DTOs only) and into `Pod.Grpc.ConnectHost.Server` / `Pod.Grpc.ShellHost.Server` (service base classes only). That split is what lets the netstandard message library be referenced by the .NET Framework 4.7.1 kiosk while the netcoreapp2.1 service implementations stay server-side.

If you change a method signature, message field, or enum here, you change every consumer in the system. There is no other place to edit the protocol.

## Tech

- **Target frameworks:** `netstandard2.0;net10.0` (multi-target — see "Multi-targeting" below)
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Key NuGet packages:**
  - `Google.Protobuf` **3.27.0** — runtime for generated message classes (both targets)
  - `Grpc.Tools` **2.66.0** — MSBuild integration that runs `protoc` + `grpc_csharp_plugin` (PrivateAssets=all; both targets)
  - `Grpc.Core` **2.46.6** — managed gRPC core (netstandard2.0 target only; kiosk path)
  - `Grpc.AspNetCore` **2.66.0** — server-side AspNetCore gRPC integration (net10.0 target only; brings `Grpc.AspNetCore.Server` + `Grpc.Core.Api` transitively)
- **Project references (in this repo):** *none* — leaf project by design

### Multi-targeting

`Pod.Grpc.Base` is multi-targeted because the kiosk and server now run on different .NET flavours but must share the same generated stubs:

- The **netstandard2.0** target is for the kiosk path. `Pod.Grpc.Messages` and `Pod.Grpc.Base.Client` link the generated `.cs` files and are consumed (transitively) by the .NET Framework 4.7.1 WPF kiosk via `Grpc.Core` 2.46.6.
- The **net10.0** target is for the server path. `Pod.Grpc.{Base,ConnectHost,ShellHost}.Server` link the same generated files and run inside `Pod.Web.Center` on `Grpc.AspNetCore`.

The generated code is target-agnostic: `Grpc.Tools` emits `using grpc = global::Grpc.Core;` and references the `Grpc.Core.Marshaller<>`, `Grpc.Core.Method<>`, `Grpc.Core.ServerCallContext`, etc. types — all of which live in `Grpc.Core.Api` and are provided by both `Grpc.Core` and `Grpc.AspNetCore`.

Two `<Protobuf>` includes split by file pattern so `Grpc.Tools` 2.66 doesn't warn about missing service stubs for the message-only protos:

```xml
<Protobuf Include="Service*.proto"   GrpcServices="Both" OutputDir="%(RelativeDir)Export" CompileOutputs="false" />
<Protobuf Include="Messages*.proto"  GrpcServices="None" OutputDir="%(RelativeDir)Export" CompileOutputs="false" />
```

## Responsibility

**IS** responsible for:
- Holding the `.proto` files (the canonical schema).
- Generating the corresponding C# message classes and gRPC service base classes into `Export/`.

**IS NOT** responsible for:
- Implementing any service method (that's `Pod.Grpc.ConnectHost.Server` / `Pod.Grpc.ShellHost.Server`).
- Adding behavior to message types — partial-class extensions live in `Pod.Grpc.Messages` (e.g. `GuidAsBytes.ToGuid()`).
- Anything to do with channels, credentials, or hosting (`Pod.Grpc.Base.Client` / `Pod.Grpc.Base.Server`).

## Public API surface

The protocol is defined across **3 service files** (each maps 1:1 to a `service` block) and **4 message files**:

### Services

#### `ConnectHostServiceGrpc` — `ServiceConnectHost.proto`

| RPC | Request | Response | Streaming | Notes |
|-----|---------|----------|-----------|-------|
| `GetHost` | `ShellServerRequest` | `ShellServerResponse` | unary | Resolves which Shell server a station should talk to. |

#### `ShellHostServiceGrpc` — `ServiceShellHost.proto`

| RPC | Request | Response | Streaming | Notes |
|-----|---------|----------|-----------|-------|
| `Connect` | `ConnectRequest` | `ConnectResponse` | unary | Bind a station's `ConnectionId` to this server. |
| `GetNotifications` | `NotificationRequest` | `ClientNotification` | **server-stream** | Long-lived push channel. |
| `GetServerSettings` | `ServerSettingsRequest` | `ServerSettingsResponse` | unary | Heartbeat interval/timeout, server clock. |
| `GetClientSettings` | `ClientSettingsRequest` | `ClientSettingsResponse` | unary | DisplayName, QrCode, ControlMode. |
| `SendHeartbeat` | `HeartbeatRequest` | `HeartbeatResponse` | unary | Keep-alive. |
| `SendLoginIntention` | `LoginRequest` | `LoginRequestResponse` | unary | Station asks "may I log in?". |
| `GetLoginIntention` | `RequestedLoginRequest` | `RequestedLoginResponse` | unary | Pulls a pending login request from the server. |
| `SendLoginResponse` | `LoginIntentionReplyRequest` | `LoginIntentionReplyResponse` | unary | Accept/reject. Field `IsLoginAccepted`. |
| `GetSessionState` | `SessionStateRequest` | `SessionStateResponse` | unary | Current `SessionDetails`. |
| `SendLogoutRequest` | `LogoutRequest` | `LogoutResponse` | unary | Carries `LogoutReason`. |
| `Disconnect` | `DisconnectRequest` | `DisconnectResponse` | unary | Graceful tear-down. |

#### `ShellApplicationServiceGrpc` — `ServiceShellApplications.proto`

| RPC | Request | Response | Streaming | Notes |
|-----|---------|----------|-----------|-------|
| `GetSyncAppStates` | `SyncRequest` | `SyncResponse` | unary | Server's view of installed apps (or just the last-sync timestamp). |
| `SendSyncAppStates` | `SyncAppsRequest` | `SyncAppsResponse` | unary | Bulk delta upload from station. |
| `SendAppInstalled` | `AppInstalledRequest` | `UpdateResponse` | unary | Single install notification. |
| `SendAppUpdate` | `AppUpdateRequest` | `UpdateResponse` | unary | Single update notification. |
| `SendAppUninstalled` | `AppUninstalledRequest` | `UpdateResponse` | unary | Single uninstall notification. |

### Key message types

#### `MessagesShared.proto` (package `Pod.Grpc.Messages.Shared`)

DTO primitives + enums shared by all other proto files.

- `GuidAsBytes { bytes Value }` — Guid wire format (16-byte payload).
- `TimeSpanAsLong { int64 Value; bool HasValue }` — nullable timespan in ticks.
- `DateTimeUtcAsLong { int64 Value; bool HasValue }` — nullable UTC datetime in ticks.
- `enum ControlMode { Unset, Local, Remote, RemoteWithQrCode }`
- `enum ClientNotificationEvent { Unset, CheckLoginRequest, UpdateClientSettings, UpdateSessionState, UpdateServerSettings, SendHeartbeat, Untranslated=1000 }`
- `enum SessionState { Unset, NoSession, LoginRequested, AwaitingConfirmation, Running }`
- `enum LogoutReason { Unset, UserLogout, Inactivity, Shutdown, LimitReached }`

#### `MessagesConnectHost.proto` (package `Pod.Grpc.Messages.ConnectHost`)

- `ShellServerRequest { string IdentityId; uint32 MaxInterfaceVersion; GuidAsBytes ReconnectConnectionId }`
- `ShellServerResponse { GuidAsBytes ConnectionId; string HostAddress; uint32 Port; uint32 RequiredInterfaceVersion }`

#### `MessagesShellHost.proto` (package `Pod.Grpc.Messages.ShellHost`)

The big one — 16 messages including:
- `ConnectRequest/Response`
- `NotificationRequest`, `ClientNotification { ClientNotificationEvent Event }`
- `ServerSettingsResponse { DateTimeUtcAsLong ServerTimeUtcNow; TimeSpanAsLong HeartbeatInterval; TimeSpanAsLong HeartbeatTimeout }`
- `ClientSettingsResponse { string DisplayName; string QrCode; ControlMode Mode }`
- `HeartbeatRequest/Response`
- `LoginRequest/LoginRequestResponse`, `RequestedLoginRequest/Response`
- `LoginIntentionReplyRequest { ConnectionId, SessionId, bool IsLoginAccepted } / Response`
- `SessionStateRequest/Response`, `LogoutRequest/Response`, `DisconnectRequest/Response`
- **`SessionDetails`** — the central session DTO: `SessionId, RequestedOnUtc, SessionState, DeadlineUtcForPickUp, DeadlineUtcForConfirmation, MaxTimeForConfirmationDecision, StartTimeUtc, EffectiveDuration, SessionConditions Conditions`.
- **`SessionConditions`** — `InitialDurationOnSessionStart, AutostartAppIdOnSessionStart, AutoLogoutOnAppExit, repeated AllowedApps`.

Almost every request carries `GuidAsBytes ConnectionId` as field 1 — the per-connection token issued by `ConnectHost.GetHost`.

#### `MessagesShellApplications.proto` (package `Pod.Grpc.Messages.ShellApplications`)

- `SyncRequest { ConnectionId, bool SendOnlyLastSyncTimestamp }`, `SyncResponse { LastSyncTimestamp, repeated AppDataState AppStates }`
- `SyncAppsRequest { ConnectionId, repeated AppInstallInfo Installations, repeated AppUpdateInfo Updates, repeated AppUninstallInfo Uninstalls }`, `SyncAppsResponse`
- `AppInstalledRequest { ConnectionId, AppInstallInfo Installed }`
- `AppUpdateRequest { ConnectionId, AppUpdateInfo Updated }`
- `AppUninstalledRequest { ConnectionId, AppUninstallInfo Uninstalled }`
- `UpdateResponse {}` — empty ack
- `AppDataState { ApplicationId, InstanceVersion }`
- `AppInstallInfo { ApplicationId, PlatformId, InstanceVersion, InstalledOnUtc, DisplayName, IsEnabled }`
- `AppUpdateInfo { ApplicationId, InstanceVersion, DisplayName, IsEnabled }`
- `AppUninstallInfo { ApplicationId, UninstalledOnUtc }`

## Internal structure

```
Pod.Grpc.Base/
├── Pod.Grpc.Base.csproj
├── ServiceConnectHost.proto             ← service: ConnectHostServiceGrpc
├── ServiceShellHost.proto               ← service: ShellHostServiceGrpc
├── ServiceShellApplications.proto       ← service: ShellApplicationServiceGrpc
├── MessagesShared.proto                 ← shared DTO primitives + enums
├── MessagesConnectHost.proto            ← Connect host messages
├── MessagesShellHost.proto              ← Shell host messages (sessions, login, heartbeat, …)
├── MessagesShellApplications.proto      ← Application sync messages
└── Export/                              ← generated, checked in (see "Notable" below)
    ├── ServiceConnectHost.cs
    ├── ServiceConnectHostGrpc.cs        ← service base classes (Pod.Grpc.ConnectHost.Server links these)
    ├── ServiceShellHost.cs
    ├── ServiceShellHostGrpc.cs
    ├── ServiceShellApplications.cs
    ├── ServiceShellApplicationsGrpc.cs
    ├── MessagesShared.cs                ← message DTOs (Pod.Grpc.Messages links these)
    ├── MessagesConnectHost.cs
    ├── MessagesShellHost.cs
    └── MessagesShellApplications.cs
```

*(The previously orphaned `ServiceCenterHost*` / `MessagesCenterHost.cs` files were removed as part of the net10.0 migration — they referenced `Grpc.Core.Channel`, which `Grpc.Core.Api` no longer carries, and had no `.proto` source to regenerate from.)*

The csproj item group that wires this up:

```xml
<ItemGroup>
  <Protobuf Include="**/*.proto" OutputDir="%(RelativeDir)Export" CompileOutputs="false" />
</ItemGroup>
```

`OutputDir="%(RelativeDir)Export"` writes alongside each `.proto`, and `CompileOutputs="false"` prevents this project from compiling the generated files into its own assembly — it is a pure code-gen project.

## Notable patterns / gotchas

- **`Export/` files are committed.** Despite being generated, they're checked into git. Re-build to refresh, or hand-merge if `Grpc.Tools` produces noise. Don't edit them by hand.
- **`GuidAsBytes` not `string`.** All Guids cross the wire as 16 raw bytes. Convert via `ConverterExtensions.ToGuidAsBytes()` / `GuidAsBytes.ToGuid()` from `Pod.Grpc.Messages`.
- **Enum naming** uses the `EnumName_Value` convention because proto3 forbids two enum members in the same namespace from sharing a bare name. Do *not* drop the prefix.
- **Wire compatibility**: changing field numbers, removing fields, or renaming services is a breaking change for every deployed kiosk. Use `reserved` and additive evolution. The interface version is gated by `MaxInterfaceVersion` in `ShellServerRequest` and `RequiredInterfaceVersion` in `ShellServerResponse` — bump both when introducing breaking changes.
- **`Release_ShellClient` configuration** exists alongside `Debug`/`Release` so that the kiosk's build can pick up the same generated outputs without dragging in server-only configuration tweaks.

## Consumers

| Project | What it links | Why |
|---------|---------------|-----|
| `Pod.Grpc.Messages` | all 4 `Messages*.cs` | Adds partial-class extensions (Guid/Time converters). |
| `Pod.Grpc.ConnectHost.Server` | `ServiceConnectHost.cs` + `ServiceConnectHostGrpc.cs` | Server base class to inherit. |
| `Pod.Grpc.ShellHost.Server` | `ServiceShellHost*.cs` + `ServiceShellApplications*.cs` | Server base classes to inherit. |
| `LeapVR.Shell.Services` (kiosk) | the `*ClientGrpc` types from the same generated files | Calls the services as a client. |

## Related docs

- [`docs/server/grpc/README.md`](../README.md) — gRPC tier overview & call flow
- [`docs/architecture/grpc.md`](../../../architecture/grpc.md) — protocol-level architecture
- [`docs/architecture/session-lifecycle.md`](../../../architecture/session-lifecycle.md) — what the `SessionState` enum means in practice
