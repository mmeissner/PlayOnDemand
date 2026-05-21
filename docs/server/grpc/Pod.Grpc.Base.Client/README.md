# Pod.Grpc.Base.Client

> Generic gRPC client scaffolding: builds a TLS channel (server-cert TLS by default), attaches a `CallCredentials.FromInterceptor` that adds the station's `(identity, password)` plain metadata to every call, and hands back a strongly-typed `ClientBase<T>` you can call.

## Purpose

This project is the **client-side counterpart** to `Pod.Grpc.Base.Server`. It does not know about any specific service — instead it provides three reusable types:

1. `GrpcChannelCredentialsHandler` — wraps `SslCredentials` (server-cert TLS by default; the optional client cert/key is conventionally a *license* certificate, not a mutual-TLS auth credential) and layers a `CallCredentials.FromInterceptor` that pushes the station's `identity` (the literal `StationId`) and `password` (the literal `Station.Password`) as plain metadata on every outgoing call. The server PBKDF2-verifies the password against `Station.PasswordHash` — there is no HMAC.
2. `GrpcConnectionHandler` — owns the actual `Grpc.Core.Channel` and computes RPC deadlines.
3. `GrpcClient<T>` — generic envelope: given an `IRpcHostDetails`, a credentials handler, and a `T : ClientBase<T>` (a generated stub like `ShellHostServiceGrpcClient`), it builds the channel and instantiates the stub via reflection (`Activator.CreateInstance(typeof(T), channel)`).

Because it targets `netstandard2.0` it can be referenced both by the netcoreapp simulators and by the .NET Framework 4.7.1 kiosk's `LeapVR.Shell.Services` project.

## Tech

- **Target framework:** `netstandard2.0`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Key NuGet packages:**
  - `Grpc.Core` **2.46.6** — the managed gRPC core. Last release that supports netstandard2.0; bumped from 1.19.0. The wire format is unchanged, so the kiosk talks to the new `Grpc.AspNetCore`-hosted server transparently.
  - `NLog` **4.5.11** — logging (kiosk uses NLog throughout)
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure` — for `IResult` (returned by `Connect`)
  - `Pod.Grpc.Const` — `AuthConstants` for header keys, `PodChannelOptions` for channel tuning
  - `Pod.Grpc.Utilities` — `RpcException.ToResult()` for the `Connect` failure path

## Responsibility

**IS** responsible for:
- Channel lifecycle (`new Channel(host, port, credentials, options)` and `Dispose` → `ShutdownAsync`).
- Building TLS credentials. By default this is **server-cert TLS only**; if a client cert chain *and* private key are also supplied, the client will present them, but in the deployed kiosk that's a **license** identifier handled at the application layer — not a mutual-TLS session credential. The server defaults `ForceClientCertificate=false`, so the handshake never *requires* a client cert.
- Pushing per-call `(identity, password)` plain metadata via an `AsyncAuthInterceptor`.
- Computing per-call `DateTime` deadlines from a configured timeout (default 3s; `0` = effectively no timeout).
- Wrapping the channel-`ConnectAsync` call into an `IResult`-returning `Connect` so callers don't have to handle `RpcException` themselves.

**IS NOT** responsible for:
- Calling any specific RPC — that's the consuming project's job (e.g. `LeapVR.Shell.Services/RpcServices/`).
- Generating the gRPC client stubs (those come from `Pod.Grpc.Base/Export/`).
- Mapping business errors (`Pod.Grpc.Utilities`).
- Picking the host/port (callers provide an `IRpcHostDetails`; for kiosk the value is fetched via `ConnectHost.GetHost`).

## Public API surface

- `class GrpcChannelCredentialsHandler`
  - `static GrpcChannelCredentialsHandler Create(string serverRootCert, string clientCertChain = null, string clientPrivateKey = null)` — factory; returns `null` if root CA is missing. The optional `clientCertChain` / `clientPrivateKey` are conventionally **license certificate** material — they are NOT used as mutual-TLS session auth.
  - `void SetChannelCredentials(string identity, string password)` — sets the literal `(identity, password)` metadata pair attached to every call by the interceptor (`identity` = StationId, `password` = Station.Password — verified server-side via PBKDF2 against `Station.PasswordHash`).
  - `ChannelCredentials GetCredentials()` — composes `SslCredentials` + (optional) `CallCredentials.FromInterceptor`.
- `class GrpcConnectionHandler : IDisposable`
  - `class ConnectionSettings { IRpcHostDetails HostDetails, GrpcChannelCredentialsHandler ChannelCredentialsHandler, uint RpcTimeoutMs, ResetDefaultTimeoutValues() }`
  - `Channel GrpChannel { get }`
  - `DateTime GetDeadline([CallerMemberName])` — `UtcNow + RpcTimeoutMs`, or `+1 year` when timeout is 0.
- `class GrpcClient<T> where T : ClientBase<T>`
  - `GrpcClient(IRpcHostDetails, GrpcChannelCredentialsHandler, uint grpcCallTimeout)` — owns its own connection.
  - `GrpcClient(GrpcConnectionHandler)` — share a connection between two stubs.
  - `T RpcClient { get }` — the generated client stub.
  - `GrpcConnectionHandler Handler { get }`
  - `Task<IResult> Connect(uint timeoutMs = 10000, [CallerMemberName])` — opens the underlying channel, returns errors as `IResult` not exceptions.
- `interface IRpcHostDetails { string ServerHost; uint ServerPort; }`
- `interface IRpcShellHostDetails : IRpcHostDetails { Guid ConnectionId; }`
- `class RpcHostDetails : IRpcHostDetails`
  - `static RpcHostDetails Create(string serverHost, uint port)` — verifies port range.
- `class RpcShellHostDetails : RpcHostDetails, IRpcShellHostDetails`
  - `static RpcShellHostDetails Create(string serverHost, uint port, Guid connectionId)` — also verifies non-empty connection id.

## Internal structure

```
Pod.Grpc.Base.Client/
├── Pod.Grpc.Base.Client.csproj
├── GrpcChannelCredentialsHandler.cs   ← TLS + (identity, password) metadata interceptor
├── GrpcClient.cs                      ← generic envelope GrpcClient<T>
├── GrpcConnectionHandler.cs           ← Channel + ConnectionSettings + GetDeadline
└── RpcHostDetails.cs                  ← IRpcHostDetails + IRpcShellHostDetails + factories
```

## Notable patterns / gotchas

- **TLS modes are decided by what you pass to `Create`.** Pass only `serverRootCert` → server-cert TLS only. Pass cert chain + private key → the client *will present* a cert during the handshake, but on the deployed system that cert is a **license** identifier (e.g. `LeapVR-License.crt`) handled at the application layer, not session auth — the server's `ForceClientCertificate` defaults to `false`, so the handshake never *requires* one. (In the deployed kiosk, `StaticServerConfig.GetClientCertificate()` returns `null`, so no client cert is sent at all.)
- **The `(identity, password)` metadata pair is also called "channel credentials" in the code.** It's set per channel, attached per call. The handler tracks whether it was set so calls fall back to plain TLS when no station is logged in. ⚠️ This was previously documented as HMAC; it is **not** HMAC. The kiosk sends the literal `StationId` and `Station.Password` as plain `identity` / `password` metadata; the server PBKDF2-verifies the password against `Station.PasswordHash` (`Pod.Services.Extensions.VerifyCredentials`). The `amx` HMAC scheme is REST-only and unrelated.
- **`GrpcClient<T>` instantiates `T` via reflection** (`Activator.CreateInstance(typeof(T), channel)`). This relies on every generated client stub having a public `(Channel)` ctor — a contract upheld by `Grpc.Tools`. If that ever changes, this falls over silently because the catch-all logs and returns `null`.
- **Sharing a connection** between two stubs (`GrpcClient(GrpcConnectionHandler)` overload) is the supported way to talk to two services on the same port without opening two channels — the kiosk uses this for `ShellHostServiceGrpc` and `ShellApplicationServiceGrpc`.
- **`Connect` returns `IResult`, not throws.** RpcExceptions are converted via `.ToResult()` from `Pod.Grpc.Utilities`. Other exceptions become `UserError.InternalError`. Use this rather than calling `Channel.ConnectAsync` directly.
- **`GetDeadline`'s "no timeout" path is one year**, not `DateTime.MaxValue`. That's because gRPC will refuse `DateTime.MaxValue`.
- **Disposing the handler** calls `ShutdownAsync()` *without awaiting* — the caller has no way to wait for it. Be aware during process-shutdown sequences.

## Consumers

- `LeapVR.Shell.Services/Factory/RemoteServiceFactory.cs` — builds one `GrpcChannelCredentialsHandler` per station, calls `SetChannelCredentials(stationId, password)` on it, and creates per-service `GrpcClient<T>` envelopes. `BaseService` (in `LeapVR.Shell.Services/RpcServices/`) holds the shared `GrpcConnectionHandler`.

## Related docs

- [`docs/server/grpc/README.md`](../README.md) — gRPC tier overview
- [`docs/server/grpc/Pod.Grpc.Const/README.md`](../Pod.Grpc.Const/README.md) — header keys and `PodChannelOptions.DefaultClientOptions()`
- [`docs/architecture/auth.md`](../../../architecture/auth.md) — the three coexisting schemes; in particular the gRPC `(identity, password)` path used by the kiosk
- [`docs/client/LeapVR.Shell.Services/`](../../../client/LeapVR.Shell.Services/) — kiosk-side wrappers built on top of this
