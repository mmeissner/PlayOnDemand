# Pod.Enums

> Pure enum definitions shared across server, gRPC, REST, and (transitively) the WPF kiosk client. The bottom of the dependency graph — references nothing.

## Purpose

Many enums in this codebase appear in three places at once: as a column type in the database (`Pod.Data.Models`), as a field on a request DTO (`Pod.DtoModels`), and as a field on a response view model (`Pod.ViewModels`). Defining them once in a tiny leaf project means all three layers see the same int values and string names.

The project is `netstandard2.0` so it can be referenced from anywhere — including the .NET Framework 4.7.1 `LeapVR.Shell.*` client when needed (today only via the `LeapVR.Shell` graph through `Pod.Web.Client.Rest`).

It contains no logic, no extension methods (extensions live in the consumer that needs them, e.g. `Pod.Services/Extensions.cs`). Just `public enum` declarations with explicit numeric values for stability.

## Tech

- **Target framework:** `netstandard2.0`
- **Configurations:** `Debug;Release;Release_ShellClient` — the third configuration is needed because this project transitively flows into the WPF client which builds with that custom configuration. (See [`docs/architecture/build-and-deploy.md`](../../architecture/build-and-deploy.md).)
- **Key NuGet packages:** none
- **Project references (in this repo):** none

## Responsibility

**Is responsible for:** declaring enums whose values must round-trip stably between server (DB), gRPC, REST, and client.

**Is NOT responsible for:**
- Enum-to-enum mapping logic — `Pod.Services/Extensions.cs` does that for the gRPC ↔ DB direction
- Display strings, localisation — out of scope; UI layers handle that
- Flags / bitmask helpers — none present, none expected

## Public API surface

Each file is one or two related enums. Numeric ranges are deliberately spaced (e.g. `100`, `200`, `300`) so groups can grow without renumbering.

| File | Enums | Notes |
|---|---|---|
| `ConnectionClosedBy.cs` | `ConnectionClosedBy` | Who tore down a station↔server connection (server, client, timeout, …) |
| `CreatorType.cs` | `CreatorType` | Who created an entity row (system, user, support) — used for audit columns |
| `CurrencyIsoCode.cs` | `CurrencyIsoCode` | ISO 4217 currency codes used in `SubscriptionOrder.Currency` |
| `EMailTemplateIdentifier.cs` | `EMailTemplateIdentifier` | `RegisterAccount`, `ResendEMailVerification`, `ResetPassword` — the lookup key the engine uses to find a template |
| `EmailReceiverType.cs` | `EmailReceiverType` | `To`, `CarbonCopy`, `BlindCarbonCopy` |
| `NetworkState.cs` | `NetworkState` | `Disconnected = 0`, `Connecting = 10`, `Connected = 20` — the station's perceived link state |
| `PlatformType.cs` | `PlatformType` | Game platform: `Local`, `Steam`, `EpicGames`, `GoodOldGames`, `UbiSoft`, `EaOrigin` |
| `RequestSource.cs` | `RequestSource` | Who initiated a session request (operator UI, kiosk-local, API, …) |
| `SessionResponse.cs` | `SessionResponse` | gRPC-side reason codes for session call failures |
| `SessionState.cs` | `SessionState`, `ConnectionRequestResult` | The session state machine (`Requested → Delivered → Started → Ended/Canceled/…Timeout`) |
| `SmtpAuthentication.cs` | `SmtpAuthentication` | `Plain`, `OAuth2` — drives `EMailTemplateSenderFactory` routing |
| `StationControlMode.cs` | `StationControlMode` | `Local`, `Remote`, `RemoteWithQrCode` — operator-vs-onsite control of the station |
| `StopReason.cs` | `StopReason` | Why a session ended (`UserLogout`, `Inactivity`, `StationShutdown`, `LimitReached`, `Unknown`) |
| `SubscriptionChangeOperation.cs` | `SubscriptionChangeOperation` | Add-time / extend / cancel operations on a subscription |
| `TemplateVariableKey.cs` | `TemplateVariableKey`, `EmailVariableType` | The known variables the email engine can substitute (`Username`, `WebHostRoot`, `EMailVerificationTokenLink`, `PasswordResetToken`, …) and their scope (subject / text / html) |
| `UserErrors.cs` | `UserError` | The big one. Every business error code that can appear in an `IResult.Errors`. Grouped in 100-numbered ranges per domain. |

## Internal structure

Flat — one file per enum or tightly-coupled enum pair.

```
Pod.Enums/
├── ConnectionClosedBy.cs
├── CreatorType.cs
├── CurrencyIsoCode.cs
├── EMailTemplateIdentifier.cs
├── EmailReceiverType.cs
├── NetworkState.cs
├── PlatformType.cs
├── RequestSource.cs
├── SessionResponse.cs
├── SessionState.cs
├── SmtpAuthentication.cs
├── StationControlMode.cs
├── StopReason.cs
├── SubscriptionChangeOperation.cs
├── TemplateVariableKey.cs
└── UserErrors.cs            ← contains the UserError enum, ~200+ values
```

## Notable patterns / gotchas

- **Explicit numeric values everywhere.** Adding a value in the middle of an enum is forbidden — append at the end of its range, or start a new range (see `UserError.cs` 100-numbered groups). Wire formats (gRPC, JSON via `StringEnumConverter`, EF int columns) all assume value stability.
- **JSON emits enums as strings**, not ints — `StartupCS::ConfigureServices` adds `new StringEnumConverter()` to MVC's serializer. So renaming a value is also a breaking change for REST clients (the wire string changes). Treat enum names as part of the API contract.
- **EF stores enums as their underlying int.** Renames are safe at the DB level; reorderings are not.
- **gRPC defines its own enums in `.proto`.** Mapping is manual — see `Pod.Services/Extensions.cs` for the `ToGrpc*` methods. When you add a value here, also add the matching value in the `.proto` *and* extend the mapping switch (the switches throw `ArgumentOutOfRangeException` on unknown values, so a missing mapping is a runtime crash).
- **`UserError` ranges:**
  - `0` — `InternalError` (catch-all that should not leak details)
  - `100`s — Station entity validation
  - `200`s — Order entity validation
  - `300`s — Connection
  - `400`s — Session
  - `500`s — EMail
  - `1000`s — Server-side service errors (Admin, ShellServer, ShellClient, …)
  - Higher ranges live further down the file, organised by service.
- **`SessionState` numeric jumps (10, 20, 30, 100, 110, 120, 130)** are deliberate — gaps allow new intermediate states without renumbering the terminals.

## Consumers

A lot. Direct project references to `Pod.Enums`:

- `Pod.Services`
- `Pod.MailEngine`
- `Pod.DtoModels`
- `Pod.ViewModels`, `Pod.ViewModels.Expressions`
- `Pod.Data`, `Pod.Data.Models`, `Pod.Data.Infrastructure`
- `Pod.Grpc.Base.Server`, `Pod.Grpc.Utilities`
- `Pod.Web.Client.Rest`, `Pod.Web.Client.Rest.Internal`
- `LeapVR.Shell`, `LeapVR.Shell.Services`, `LeapVR.Shell.Controllers` (the WPF kiosk)

If you need to know who depends on this project, the answer is "almost everything".

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.DtoModels/`](../Pod.DtoModels/) — REST request DTOs that use these enums
- [`docs/server/Pod.ViewModels/`](../Pod.ViewModels/) — REST response view models that use these enums
- [`docs/server/Pod.Services/`](../Pod.Services/) — `Extensions.cs` for gRPC ↔ DB enum mapping
