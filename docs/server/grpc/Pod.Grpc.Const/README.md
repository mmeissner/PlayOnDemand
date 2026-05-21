# Pod.Grpc.Const

> Tiny shared-constants library: header keys for the gRPC station-auth metadata (`identity` / `password`) and default gRPC channel options used by both client and server.

## Purpose

A leaf, dependency-free (apart from `Grpc.Core`) library that holds values both ends of the wire must agree on:

1. The **header keys** used to carry the station's `identity` (StationId) and `password` (literal Station password) through gRPC `Metadata` — verified server-side via PBKDF2 against `Station.PasswordHash`. Not HMAC. The `amx` HMAC scheme is REST-only and lives in `Pod.Web.Authentication.ApiKeySecret`.
2. The **string keys** for gRPC channel options (`grpc.keepalive_time_ms` etc.) so callers don't repeat raw strings.
3. The **default `ChannelOption` lists** for client and server channels — keep-alive timing, BDP probing, max message size — which are tuned identically on both sides.

It is `netstandard2.0` so the .NET Framework 4.7.1 kiosk references it directly. The net10.0 server consumes it transitively through `Pod.Grpc.Base.Server` (the `Grpc.AspNetCore` runtime carries `ChannelOption` via `Grpc.Core.Api`, so the same `ChannelOption` references resolve there too).

## Tech

- **Target framework:** `netstandard2.0`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Key NuGet packages:**
  - `Grpc.Core` **2.46.6** — needed for `ChannelOption` and `ChannelOptions.MaxReceiveMessageLength`. Last release that supports netstandard2.0; bumped from 1.19.0 alongside the server-side migration to Grpc.AspNetCore.
- **Project references (in this repo):** *none*

## Responsibility

**IS** responsible for:
- Constant strings used by both client (`Pod.Grpc.Base.Client`) and server (`Pod.Grpc.Base.Server`).
- Returning `List<ChannelOption>` factories so client and server use identical tuning.

**IS NOT** responsible for:
- Anything stateful — there are no instances or DI registrations.
- Cert paths or HMAC secret storage (those live in app config, not constants).

## Public API surface

- `static class AuthConstants`
  - `const string ShellClientIdentityKey = "identity"`
  - `const string ShellClientPasswordKey = "password"`
- `static class ChannelOptionsEx` — string keys for `Grpc.Core.ChannelOption`:
  - `KeepAliveTimeMs = "grpc.keepalive_time_ms"`
  - `KeepAliveTimeoutMs = "grpc.keepalive_timeout_ms"`
  - `KeepAlivePermitWithoutCalls = "grpc.keepalive_permit_without_calls"`
  - `Http2BdpProbe = "grpc.http2.bdp_probe"`
  - `Http2MinRecvPingIntervalWithoutDataMs = "grpc.http2.min_ping_interval_without_data_ms"`
  - `Http2MinSentPingIntervalWithoutDataMs = "grpc.http2.min_time_between_pings_ms"`
  - `Http2MaxPingsWithoutData = "grpc.http2.max_pings_without_data"`
- `static class PodChannelOptions`
  - `List<ChannelOption> DefaultServerOptions()` — `MaxReceiveMessageLength = int.MaxValue`, keep-alive 10s/10s, BDP probe on, min recv-ping 5s, min send-ping 10s. **No** `Http2MaxPingsWithoutData` (that's a client-only option).
  - `List<ChannelOption> DefaultClientOptions()` — same as server **plus** `Http2MaxPingsWithoutData = 0` (unlimited).

## Internal structure

```
Pod.Grpc.Const/
├── Pod.Grpc.Base.Const.csproj   ← note: csproj name differs from folder name
├── AuthConstants.cs
├── ChannelOptionsEx.cs
└── PodChannelOptions.cs
```

## Notable patterns / gotchas

- **Folder vs csproj name asymmetry.** The folder is `Pod.Grpc.Const/` but the csproj is `Pod.Grpc.Base.Const.csproj`. Every `<ProjectReference>` in this repo uses the csproj name (`..\Pod.Grpc.Const\Pod.Grpc.Base.Const.csproj`). Do not rename one without renaming the other.
- **Header keys must be lower-case.** gRPC's `Metadata` lower-cases all keys; using `"Identity"` will silently fail to match. The `// The Values must be lower case!` comment in `AuthConstants.cs` is load-bearing.
- **Comment block in `ChannelOptionsEx.cs`** lists the older `GRPC_ARG_…` env-var-style names. Those are commented out — the lower-cased dotted form is what `Grpc.Core` 1.19 expects.
- **Server vs client option set is intentionally different.** `Http2MaxPingsWithoutData` only appears on the client list because the server should not send unsolicited pings.
- `MaxReceiveMessageLength = int.MaxValue` matters for the `SyncResponse` payload, which can carry the full app catalog.

## Consumers

- `Pod.Grpc.Base.Client` — uses `AuthConstants` keys from the `AsyncAuthInterceptor` (the `(identity, password)` plain-metadata interceptor), and `PodChannelOptions.DefaultClientOptions()` when building the `Channel`.
- `Pod.Grpc.Base.Server` — `CallContextUtil` and `GrpcMetadataAuthenticationHandler` read headers via the same `AuthConstants` keys.
- `Pod.Web.Center` — registers `services.AddGrpc()` with the channel options surfaced here; the legacy standalone `Grpc.Core.Server` host (`GrpcServicesServer`) is gone and Kestrel + `endpoints.MapGrpcService<T>()` replace it.
- `LeapVR.Shell.Services` (kiosk) — same client-side usage as above.

## Related docs

- [`docs/server/grpc/README.md`](../README.md) — gRPC tier overview
- [`docs/architecture/auth.md`](../../../architecture/auth.md) — full auth story; the `(identity, password)` gRPC scheme uses these header keys
