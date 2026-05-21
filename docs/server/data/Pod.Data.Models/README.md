# Pod.Data.Models

> The entity catalogue. Every persisted type in the server tier lives here, grouped by domain area into `Users/`, `Servers/`, `Shell/`, `Billing/`, `Mail/`, plus a small `Interfaces/` folder for cross-cutting contracts.

## Purpose

This project holds the **domain entity classes**: ASP.NET Identity user/role types, the station/session/connection aggregate, the subscription/order/payment chain, and the email-template/email-send pipeline. Each entity carries its own business-rule methods that return `IResult<T>` (from `Pod.Data.Infrastructure`); the EF mapping that turns these classes into Postgres tables lives one project up in `Pod.Data` (specifically the nested `ModelConfig.<Entity>Config` classes).

Entities are designed in a **rich-domain-model** style — most properties are `private set;` and mutation goes through methods (`Station.SetPassword(…)`, `SessionDetails.RequestSession(…)`, `SubscriptionState.CreateOrExtend(…)`, etc.) that validate via the `Result` builder before changing state. Collections are exposed as `IReadOnlyCollection<T>` backed by private `HashSet<T>` fields; EF reads/writes them via field access (configured in `ModelConfig`).

The project targets **`netstandard2.0`** so it can be referenced both by the .NET Core 2.1 server *and*, where needed, by client-side projects that want to share the data shape.

## Tech

- **Target framework:** `netstandard2.0`.
- **Configurations:** `Debug;Release;Release_ShellClient`.
- **Key NuGet packages:**
  - `Microsoft.Extensions.Identity.Stores` 2.1.6 — supplies `IdentityUser<TKey>` / `IdentityRole<TKey>` base types (used by `ApplicationUser` / `ApplicationRole`). Pulled via Identity to avoid a full ASP.NET Core dependency.
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure` — `IResult<T>` / `Result<T>` returned by entity factory methods.
  - `Pod.Enums` — every enum-typed property and every `UserError` code.

## Responsibility

**It IS responsible for:**
- Defining all entity classes the server persists.
- Holding the domain rules on those entities (validation + state transitions, not just data bags).
- Defining enums-of-records like `EmailVariableType`, `ConnectionRequestResponse` (a struct on `ConnectionState`), `LocalAppUpdate`.
- Exposing cross-cutting model interfaces (`IPasswordHasher`, `IUniqueAppFactory`, `IAppUpdate`, `IVariableParser`, `IContentTemplateVariable`, `IEMailAccountData`).

**It is NOT responsible for:**
- EF mapping / configuration (in `Pod.Data/Config/ModelConfig.cs`).
- Migrations (in `Pod.Data/Migrations/`).
- Persistence — entities never call `SaveChanges` themselves; that is the responsibility of the service / handler that owns the unit of work.
- DTO shapes for transport (those live in `Pod.DtoModels`, `Pod.ViewModels`, and `Pod.Grpc.Messages`).

## Public API surface

The "API" of this project is its entities. The complete catalogue is in **Internal structure** below — every class, grouped by domain area, with a one-line summary and key relationships.

Cross-cutting interfaces (in `Interfaces/`):

| Interface | Purpose |
|---|---|
| `IPasswordHasher` | Implemented by `Pod.Data.PasswordHasher`. Used by `Station.SetPassword` / `VerifyPassword`. |
| `IUniqueAppFactory` | Service-side factory: get-or-create a `UniqueApp` for a given `(CreatorType, creatorId, IAppUpdate)`. Implementation lives in `Pod.Services`. Used during station ↔ server app sync. |
| `IAppUpdate` | Read-only DTO of an app update reported by the station: `ApplicationId`, `InstanceVersion`, `DisplayName`, `IsEnabled`. Implemented by `Pod.Grpc.Messages` mappers. |
| `IVariableParser` | Parses an email-template body for `{Variable}`-style placeholders. Implementation lives in `Pod.MailEngine`. |
| `IContentTemplateVariable` | Result item of `IVariableParser.Parse()`: `VariableKey`, `StartChar`, `Length`. |
| `IEMailAccountData` | Read-only view of SMTP account data. Implemented by `EMailAccountData`; consumers can pass test doubles. |
| `PasswordVerificationResult` (enum) | `Failed` / `Success` / `SuccessRehashNeeded` — re-declared here so `IPasswordHasher` doesn't drag in `Microsoft.AspNetCore.Identity` for downstream consumers. |

## Internal structure

```
Pod.Data.Models/
├── Users/        — Identity user + role
├── Servers/      — Shell server registry
├── Shell/        — Stations, sessions, connections, apps (the big aggregate)
├── Billing/      — Subscription orders / payments / changes
├── Mail/         — Email accounts, templates, send orders
└── Interfaces/   — Cross-cutting contracts
```

---

## Entity catalogue

Below: every persisted class in the project, grouped by area. **Aggregate roots** are bolded. State-machine carriers (entities that hold an FSM) are flagged with **(FSM)**. Relationships are stated as cardinality + the navigation property name on each side.

### Identity / accounts (`Users/Users.cs`, namespace `Pod.Data.Models.Users`)

| Class | What it represents | Key relationships |
|---|---|---|
| **`ApplicationUser : IdentityUser<Guid>`** | A human account in the portal. Adds a `CustomerNumber` (auto-assigned via Postgres sequence `User_CustomerNumber_seq` starting at 100, used as the human-readable billing reference) and a private `_stations` set surfaced as `IReadOnlyCollection<Station> Stations`. | 1 → many `Station`. Identity-side: many `IdentityUserClaim<Guid>`, `IdentityUserLogin<Guid>`, `IdentityUserToken<Guid>`, `IdentityUserRole<Guid>`. |
| `ApplicationRole : IdentityRole<Guid>` | A role assignable to an account. Has parameterless and `(string roleName)` ctors; otherwise just the Identity base. The five seeded role names (`User`, `Accountant`, `ServerManager`, `CustomerSupport`, `Administrator`) come from `Pod.Data/Config/RolesConfig.cs`. | Standard Identity user-role join. |

The Identity primary key is `Guid` everywhere — this drives the `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` declaration on `PodDbContext`.

---

### Servers (`Servers/ShellServer.cs`, namespace `Pod.Data.Models.Servers`)

| Class | What it represents | Key relationships |
|---|---|---|
| **`ShellServer`** | A registered gRPC shell server endpoint that stations can be told to connect to. Carries `DisplayName`, `PublicHostAddress`, `PublicPort`, `PublicInterfaceVersion`, `IsActive`, and timing knobs `HeartbeatInterval` / `HeartbeatTimeout` / `ConnectTimeout`. Static defaults: `HeartbeatInterval=15min`, `HeartbeatTimeout=17min`, `ConnectTimeout=5s`. Static min/max bounds are validated by `SetHeartbeatValues` / `SetConnectTimeout`. | 1 → many `ConnectionState` (`ConnectedClients`); 1 → many `ClosedConnection` (`ConnectionHistory`). Unique index on `(PublicHostAddress, PublicPort)`. |

Created via `ShellServer.Create(displayName, hostAddress, port, publicInterfaceVersion)`. Mutated via `SetActive(bool)`, `SetDisplayName`, `SetHeartbeatValues`, `SetConnectTimeout`, `SetHostDetails`.

---

### Stations / sessions / connections (`Shell/`, namespace `Pod.Data.Models.Shell`)

This is the largest and most central area. All these classes live together because they form the **Station aggregate**.

#### Station aggregate root

| Class | What it represents | Key relationships |
|---|---|---|
| **`Station`** (in `Station.cs`) | A physical VR PC running the kiosk. Owns its password hash (private `PasswordHash`), is owned by an `ApplicationUser`, and aggregates everything else listed below. Static factory `Station.Create(userId, displayName, password, IPasswordHasher)` creates the station + its `StationSettings`, `SubscriptionState`, `ConnectionState`, and `SessionDetails` in one go. Domain methods: `CreateOrder(...)` → `IResult<SubscriptionOrder>`; `SetPassword`, `VerifyPassword`; `CreateStationApiKey(displayName)` / `RemoveStationApiKey(key)`. Constant `MaximumLengthPaymentReference = 128`. Helper `NameOfPasswordHash()` exposes the private property name to `ModelConfig`. | belongs-to → `ApplicationUser` (`ApplicationUserId`); 1↔1 → `StationSettings`, `SubscriptionState`, `ConnectionState`, `SessionDetails`; 1 → many `ApplicationRoot`, `Session`, `ClosedConnection`, `StationApiKey`. |

#### Station configuration & identity

| Class | What it represents | Key relationships |
|---|---|---|
| `StationSettings` (in `Station.cs`) | Per-station operator settings: `DisplayName`, `QRCode`, `ControlMode` (`StationControlMode` enum: `Local` / `RemoteWithQrCode` / `Undefined`). Methods `SetDisplayName`, `SetControlMode`, `SetQrCode`, `SetQrCodeAndControlMode` enforce the rule "QRCode must be set if ControlMode == RemoteWithQrCode". | 1↔1 → `Station`. |
| `StationApiKey` (in `Station.cs`) | An HMAC `(PublicKey, SecretKey)` pair used by the **`amx` REST scheme** (`Pod.Web.Authentication.ApiKeySecret`). PK is `PublicKey` (Guid). `SecretKey` is a 256-bit Base64 string generated via `RandomNumberGenerator`. Created by `Station.CreateStationApiKey(displayName)` (called via `PUT /api/v1/Stations/{id}/apikeys`); the `Secret` is shown to the operator once and not retrievable afterwards. Used by every `StationController` endpoint — the kiosk holds one of these for its REST traffic. ⚠️ **NOT used by the kiosk's gRPC auth** — that path uses `Station.PasswordHash` via scheme #3. The same kiosk authenticates two channels with two different secrets. | many → 1 `Station` (cascade delete). |
| `DeviceIdentity` (in `ConnectionState.cs`) | A unique-per-machine identifier the kiosk reports on connect (so the server can tell different physical boxes apart even if they share station credentials). PK is `string Id` (the identity itself). Created via `DeviceIdentity.Create(uniqueIdentity)`. | 1↔1 → `ConnectionState`; 1 → many `ApplicationRoot`, `ClosedConnection`. |

#### Connection lifecycle (FSM)

| Class | What it represents | Key relationships |
|---|---|---|
| **`ConnectionState` (FSM)** (in `ConnectionState.cs`) | Aggregate root for a station's *current* gRPC connection. Carries `NetworkState` (`Disconnected → Connecting → Connected`), `ConnectionId`, `ServerRequestOnUtc`, `ConnectedOnUtc`, `LastHeartbeatOnUtc`, `DeviceIdentityId`, `ShellServerId`. Public methods `RequestConnecting`, `RequestConnected`, `RequestDisconnected`, `RequestHeartbeat`, `RequestTimeout` are the FSM transitions. Each returns `IResult<ConnectionRequestResponse>` where `ConnectionRequestResponse` is a nested struct carrying a `ConnectionRequestResult`, an optional `ClosedConnection` snapshot, and a `CloseLastSessionIfExist` flag. The `[NotMapped]` `LoadedFromDatabaseUtc` lets the FSM make timeout decisions against load-time without trusting `DateTime.UtcNow`. | 1↔1 → `Station`; many → 1 `ShellServer`, `DeviceIdentity`. |
| `ClosedConnection` (in `ConnectionState.cs`) | An audit row for every connection that has ended. Carries `ConnectionId`, `RequestedServerOnUtc`, `ConnectedToServerOnUtc?`, `DisconnectedOnUtc`, `ClosedBy` (`ConnectionClosedBy` enum: `Client` / `Reconnect` / `Timeout` / `UngracefulDisconnect` / `Undefined`), `StationId`, `ServerId`, `DeviceIdentityId`. Created via internal `ClosedConnection.Create(connectionState, reason)` invoked by `ConnectionState` transitions. | many → 1 `Station`, `ShellServer`, `DeviceIdentity`. |
| `ConnectionState.ConnectionRequestResponse` (struct, nested) | Return-value carrier for FSM transitions: `Result`, `CloseLastSessionIfExist`, `HasClosedConnection`, `ClosedConnection`. Not persisted. | — |

#### Session lifecycle (FSM)

| Class | What it represents | Key relationships |
|---|---|---|
| **`SessionDetails` (FSM controller)** (in `SessionDetails.cs`) | The aggregation root for the *current* session on a station. Holds the per-session timeouts (`TimeoutLoginRequestDelivery`, `TimeoutLoginRequestResponse`, `UserTimeForLoginRequestResponse`) and either the live `Session` reference or `null`. Drives the FSM externally: `RequestSession(source, ip, ref)` → creates a new `Session` in `Requested`; `RequestDelivery(connectionId)` → moves to `Delivered`; `SetResponse(connectionId, accepted)` → `Started` or `Canceled`; `RequestSessionChange(...)` → adds a `ChangeRequest` while `Started`; `EndSession(...)` (two overloads) → `Ended`. Also `HandleConnectResponse(...)` reacts to `ConnectionState` transitions (e.g. closing a session if a fresh connect signals an ungraceful prior disconnect). `SetIntentionTimeouts(...)` lets operators tune the per-station defaults within the static min/max. | 1↔1 → `Station`; 0..1↔1 → `Session` (nullable `SessionId`). |
| **`Session` (FSM)** (in `Session.cs`) | A single play session. State enum `SessionState`: `Requested → Delivered → Started → Ended` (happy path), with `Canceled`, `DeliveryTimeout`, `ResponseTimeout` failure terminals. Carries who requested it (`RequestedBy`, `RequestFromIpAddress`, `RequestReference`), pickup info (`SendOnUtc`, `SendToConnectionId`), start/stop info (`StartedUtc`, `Duration?`, `StoppedUtc`, `StopReason`), an optional `SessionRule`, and the change-request history. `IsClosed` is set true by terminal transitions. Internal transition methods (`RequestDelivery`, `SetConfirmation`, `EndSession`, `AddChangeRequest`, `IsPreRunTimeOut`) are called only via `SessionDetails`. The constant `LoadedFromDatabaseUtc` (`[NotMapped]`) is the basis for timeout calculations, mirroring `ConnectionState`'s pattern. | many → 1 `Station`; 1↔0..1 ↔ `SessionDetails`; 0..1 → `SessionRule`; 1 → many `ChangeRequest`. |
| `SessionRule` (in `Session.cs`) | Optional constraint set on a session: `StartDuration?`, `StartApplication?`, plus a `SessionRuleLocalApp` collection limiting which `LocalApp`s the kiosk may launch during the session. Created lazily inside `Session.AddOrGetRule()`. | 1↔1 → `Session`; many ↔ many → `LocalApp` via `SessionRuleLocalApp`. |
| `SessionRuleLocalApp` (in `Session.cs`) | Pure M:N join row: `(SessionRuleId, LocalAppId)`. | join. |
| `ChangeRequest` (in `Session.cs`) | An in-session time adjustment (e.g. operator extending playtime). Records `RequestFrom` (`RequestSource` enum), `SourceIpAddress`, `Reference`, `TimeChange` (TimeSpan, can be negative), `CreatedOnUtc`. Always created via `Session.AddChangeRequest(...)` which extends `Session.Duration` and may auto-`EndSession(StopReason.LimitReached)` if the new total puts the session past the limit. | many → 1 `Session`. |

#### Subscription / station billing state

| Class | What it represents | Key relationships |
|---|---|---|
| **`SubscriptionState`** (in `SubscriptionState.cs`) | Aggregate root for "is this station paid up?" Holds `StartOnUtc?` / `ExpiresOnUtc?` and the history of `SubscriptionChange`s and `SubscriptionOrder`s. Internal `CreateOrExtend(SubscriptionPayment)` decides whether the next payment is an `InitialCreated`, `Renewed` (gap), or `Extend` (overlap) operation and returns a `SubscriptionChange`. Created automatically by `Station.Create(...)`. | 1↔1 → `Station`; 1 → many `SubscriptionChange`, `SubscriptionOrder`. |

#### Application catalogue (per-station)

| Class | What it represents | Key relationships |
|---|---|---|
| `ApplicationRoot` (in `Station.cs`) | Per-(`Station`, `DeviceIdentity`) collection of installed apps. Carries `LastSyncTimestampUtc` for the latest full station-sync cycle. Static factory `ApplicationRoot.CreateApplicationRoot(station, deviceIdentity)`. Has a private dictionary cache for `_localApps` keyed by `UniqueAppId`. Unique index on `(StationId, DeviceIdentityId)`. | many → 1 `Station`; many → 1 `DeviceIdentity`; 1 → many `LocalApp`. |
| `LocalApp` (in `LocalApp.cs`) | A specific app installation on a specific `ApplicationRoot`. Carries `InstanceVersion` (monotonic counter the client increments on each update), `LocalDisplayName`, `IsEnabled`, `IsInstalled`. Lifecycle methods: `CreateLocalApp` (factory), `Update(LocalAppUpdate)` (only if newer version), `Reinstalled(LocalAppUpdate)`, `SetUninstalled()`. | many → 1 `ApplicationRoot`; many → 1 `UniqueApp`; many ↔ many → `SessionRule` via `SessionRuleLocalApp`. |
| `LocalAppUpdate` (in `Station.cs`) | Plain DTO (NOT persisted) used to ship update info into `LocalApp.Update(...)`. Carries `ApplicationId`, `InstanceVersion`, `DisplayName`, `IsEnabled`. Has `IsValid()` and `IsNewer(currentInstanceVersion)`. Built from an `IAppUpdate` via `FromIAppUpdateInfo`. | — |
| **`UniqueApp`** (in `UniqueApp.cs`) | The global, cross-station identity of an app (analogous to a SteamAppId). Carries the global `Id`, an international `DisplayName`, the `Platform` (`PlatformType` enum: `VBox`, `Steam`, …), and provenance `(Origin: CreatorType, CreatorId)`. Static factory `UniqueApp.Create(origin, creatorId, platformType, applicationId, displayName)`. Get-or-create-by-update flow lives behind `IUniqueAppFactory` (implementation in `Pod.Services`). | 1 → many `LocalApp`. |

---

### Billing (`Billing/`, namespace `Pod.Data.Models.Billing`)

The billing chain is `Station.CreateOrder(...)` → `SubscriptionOrder.PayOrder(...)` → internally creates `SubscriptionPayment` → which calls `SubscriptionState.CreateOrExtend(...)` → which creates `SubscriptionChange`. All four are linked.

| Class | What it represents | Key relationships |
|---|---|---|
| `SubscriptionOrder` (in `SubscriptionOrder.cs`) | An order created (but not yet paid) for `n` time on a station. Carries `ApplicationUserId`, an auto-assigned human-readable `OrderNumber` (Postgres sequence `SubscriptionOrder_OrderNumber_seq` starting at 1000), `CreatedOnUtc`, `ExpiresOnUtc` (when the order can no longer be paid), `CreatedFromIpAddress`, `TimeOrdered`, `PaymentAmount`, `Currency` (`CurrencyIsoCode` enum), optional `CustomerOrderReference` (≤ 128 chars), FK to `SubscriptionState`, optional FK to its `SubscriptionPayment`. Method `PayOrder(paymentReceivedUtc, paymentGatewayReference)` builds the `SubscriptionPayment`. | many → 1 `SubscriptionState`; 0..1 → `SubscriptionPayment`. |
| `SubscriptionPayment` (in `SubscriptionPayment.cs`) | Payment record for an order. Carries `CreatedOnUtc`, `PaymentReference` (mirrors the order's customer ref), `PaymentGatewayReference`, `PaymentAmount`, `Currency`, `PaymentReceivedDate`. Internal `Create(...)` factory simultaneously calls `SubscriptionState.CreateOrExtend(this)`, captures the resulting `SubscriptionChange`, and returns the payment. | 1↔1 → `SubscriptionOrder`; 1↔1 → `SubscriptionChange`. |
| `SubscriptionChange` (in `SubscriptionChange.cs`) | The audit row for one operation that affected the `SubscriptionState`. Carries `ExtendsFromUtc`, `ExtendsToUtc`, `Type` (`SubscriptionChangeOperation` enum: `InitialCreated` / `Renewed` / `Extend` / `Undefined`), back-references to both `SubscriptionState` and `SubscriptionPayment`. | many → 1 `SubscriptionState`; 1↔1 → `SubscriptionPayment`. |

---

### Mail (`Mail/`, namespace `Pod.Data.Models.Mail`)

Three core entities (`EMailAccountData`, `EmailContentTemplate`, `EmailSendOrder`) plus join + value-object types.

| Class | What it represents | Key relationships |
|---|---|---|
| **`EMailAccountData : IEMailAccountData`** (in `EMailAccountData.cs`) | An SMTP sender account. Carries `DisplayName`, `SenderName`, `EmailAddress`, `SmtpServer`, `SmtpPort`, `UseSsl`, `IsEnabled`, `AuthMethod` (`SmtpAuthentication` enum), `Username`, `Password`. Created with `Create(displayName)` (deferred config) or `CreateSmtp(...)` (full config). Fluent setters: `SetSender`, `SetSmtpServer`, `SetSmtpAuth`, `SetEnabled` (which validates everything is set before flipping `IsEnabled`). Manages the M:N link to templates via `AddEMailContentTemplate(template)` / `RemoveEMailContentTemplateBy(templateId)`. | many ↔ many → `EmailContentTemplate` via `EMailAccountDataEMailContentTemplate`. |
| **`EmailContentTemplate`** (in `EmailContentTemplate.cs`) | A template for an outgoing email. Carries `DisplayName`, `Identifier` (`EMailTemplateIdentifier` enum: registration, password-reset, etc.), `SubjectText`, `ContentText`, `ContentHtml`, `VariableControlChar` (the escape character for placeholder vars; e.g. `{`). On `Create(...)`, runs the body text through `IVariableParser` and stores discovered placeholders as `EmailVariable` rows. | many ↔ many → `EMailAccountData` via `EMailAccountDataEMailContentTemplate`; 1 → many `EmailVariable`. |
| `EmailVariable : TemplateVariable` (in `EmailContentTemplate.cs`) | A placeholder occurrence in a template body. Adds `Type` (`EmailVariableType` enum: `Subject` / `Content` / `ContentHtml`) on top of `TemplateVariable`. `Create(...)` validates that the literal `variableKey` parses into the strongly-typed `TemplateVariableKey` enum. | many → 1 `EmailContentTemplate`. |
| `TemplateVariable` (abstract, in `EmailContentTemplate.cs`) | Base record of a placeholder: `VariableKey` (enum form), `VariableKeyString` (snapshot of the literal so renaming the enum doesn't break old templates), `StartChar`, `Length`. | base for `EmailVariable`. |
| `EMailAccountDataEMailContentTemplate` (in `EMailAccountData.cs`) | Pure M:N join row: `(EMailAccountDataId, EMailContentTemplateId)` plus both navigations. | join. |
| **`EmailSendOrder`** (in `EmailSendOrder.cs`) | A queued outbound email. Carries `CreatedOnUtc`, `LastSendAttemptUtc`, `TemplateIdentifier` (`EMailTemplateIdentifier`), `SendState` (`EmailSendState` enum: `Unsend` / `Send` / `Error`), `SendAttempts`, `ErrorMsg`. Plus a hashset of `EMailReceiver`s (must include at least one of `EmailReceiverType.To`) and a hashset of `EMailVariableValue`s. Factory `CreateOrder(templateIdentifier, receivers, variableValues?)`. Mutator `SetSendAttemptResult(wasSuccess, maxTotalAttempts, errorMessage)` advances state. Indexed on `SendState` so the mail-engine background worker can scan for `Unsend` quickly. | 1 → many `EMailReceiver`, `EMailVariableValue`. |
| `EMailReceiver` (in `EmailSendOrder.cs`) | Recipient of an `EmailSendOrder`. Carries `Type` (`EmailReceiverType` enum: `To` / `Cc` / `Bcc`), `EmailAddress`, `Name`. Cascade-deletes with the order. | many → 1 `EmailSendOrder`. |
| `EMailVariableValue` (in `EmailSendOrder.cs`) | A single `(TemplateVariableKey → string)` substitution for an order. Cascade-deletes with the order. | many → 1 `EmailSendOrder`. |

---

## Notable patterns / gotchas

- **Rich domain model.** Almost every entity has a private `set;` on its properties and a `private` parameterless ctor (so EF can hydrate without exposing the API). State changes go through methods that build a `Result` and validate before mutating. Don't make properties public-set when adding new ones.

- **`internal` constructors mark "only the parent aggregate creates me".** E.g. `SessionDetails(Station)`, `SubscriptionState(Station)`, `ConnectionState(Station)`, `Session(Guid stationId, ...)`. `Station.Create(...)` is the only place these are wired together.

- **`Station.NameOfPasswordHash()` exists for `ModelConfig`.** EF needs to map the private `PasswordHash` property without reflecting through the private modifier; the helper exposes the property name as `nameof(PasswordHash)`. If you rename the property, the helper ensures the mapping breaks at compile time.

- **State-machine entities (`Session`, `ConnectionState`) take their reference time from `LoadedFromDatabaseUtc`** (set in the parameterless constructor used by EF). Timeout decisions never read `DateTime.UtcNow` directly during a transition, so the same in-memory instance produces deterministic behaviour. If you re-fetch the entity, you get a fresh "now".

- **Collections are `IReadOnlyCollection<T>` over `HashSet<T>`.** The corresponding `ModelConfig.<Entity>Config.Configure` calls `.Metadata.FindNavigation(name).SetPropertyAccessMode(PropertyAccessMode.Field)` so EF reads/writes the backing field. If you add a new collection navigation, you MUST add the `SetPropertyAccessMode(Field)` line in the corresponding config or EF will silently never populate it.

- **`HashSet` equality across new entities.** Several `Add*` methods use `_collection.Any(x => x.Id == newItem.Id)` to deduplicate against the persisted set. New entities (with `Id == Guid.Empty`) take a separate code path that compares by `Equals` — see `EMailAccountData.AddEMailContentTemplate`.

- **`DeviceIdentity` PK is a `string`, everything else is `Guid`.** It's the raw machine identifier the kiosk reports.

- **`UniqueApp.Id` is supplied by the caller**, not auto-generated. It carries platform encoding (the docstring mentions "follows special encoding to ensure uniqueness for applications from different platforms"). Two `UniqueApp`s with the same `Id` must be the same logical title.

- **`SubscriptionState.CreateOrExtend` chooses the change type from time gap.** If `StartOnUtc == null` → `InitialCreated`. Else if `DateTime.UtcNow > ExpiresOnUtc` (gap exists) → `Renewed`, starting from now. Else (still active) → `Extend`, starting from the current `ExpiresOnUtc`.

- **`Session.AddChangeRequest` may auto-end the session.** If applying `TimeChange` puts the elapsed time past the new total, it calls `EndSession(StopReason.LimitReached)`. Callers must inspect both the `SessionResponse` *and* the resulting `Session.State`.

- **`EmailContentTemplate.Create(...)` requires `subject` plus at least one of `(content, contentHtml)`.** It calls `IVariableParser.Parse` for each non-empty body and stores the discovered placeholders as `EmailVariable` rows up front; runtime substitution then walks those rows rather than re-parsing. Adding a new template variable means adding a new value to the `TemplateVariableKey` enum (in `Pod.Enums`).

- **`EMailAccountData.SetEnabled(true)` rejects partially-configured accounts.** It re-runs all field validations (email, server, port, username, password) before flipping the bit, so an account with missing SMTP details cannot accidentally start sending.

- **`ApplicationUser.CustomerNumber` is database-assigned.** The fluent config uses `HasDefaultValueSql("nextval('\"User_CustomerNumber_seq\"')")`. The C# property has `private set;` and no factory writes to it.

- **No `*.cs` file maps 1:1 to a class.** `Station.cs` declares `Station`, `StationSettings`, `ApplicationRoot`, `LocalAppUpdate`, `StationApiKey`. `ConnectionState.cs` declares `ConnectionState`, `DeviceIdentity`, `ClosedConnection`, plus the nested `ConnectionRequestResponse` struct. `Session.cs` declares `Session`, `SessionRule`, `SessionRuleLocalApp`, `ChangeRequest`. `EMailAccountData.cs` declares `EMailAccountData` + the M:N link entity. `EmailContentTemplate.cs` declares `EmailContentTemplate`, `EmailVariable`, `TemplateVariable`. `EmailSendOrder.cs` declares `EmailSendOrder`, `EMailReceiver`, `EMailVariableValue`. Search by class name, not by file name.

- **Naming inconsistency: `EMail*` vs `Email*`.** Both prefixes exist (`EMailAccountData` vs `EmailContentTemplate`, `EMailReceiver` vs `EmailSendOrder`). It's not a rename in flight, just historical inconsistency. Match the existing form when extending.

## Consumers

- `Pod.Data` — `OnModelCreating` configures every entity here.
- `Pod.Services` — business services consume entities and call their domain methods.
- `Pod.Grpc.Messages` — translates between protobuf DTOs and entity shapes.
- `Pod.Grpc.ConnectHost.Server`, `Pod.Grpc.ShellHost.Server` — handlers operate on entities returned by services.
- `Pod.Web.Center` — controllers / Razor pages render entity-derived view models.
- `Pod.MailEngine` — drains `EmailSendOrder`s and updates their `SendState`.
- `Pod.Web.Authentication.ApiKeySecret` — looks up `StationApiKey` rows.
- `Pod.DtoModels`, `Pod.ViewModels` — define transport / UI shapes that mirror these entities.
- Tests in `Pod.Data.Test`, `Pod.Services.Test`, `Pod.Test.Sandbox`.

## Related docs

- `docs/architecture/data-model.md` — relationship overview and the 2019-migration-freeze caveat (changes to entities here are NOT covered by a migration).
- `docs/architecture/session-lifecycle.md` — the FSM diagram the `Session` / `SessionDetails` entities implement.
- `docs/architecture/auth.md` — the three coexisting auth schemes; covers how `ApplicationUser` participates in JWT, how `StationApiKey` participates in the `amx` HMAC REST scheme (NOT in gRPC), and how `Station.PasswordHash` is what gates the kiosk's gRPC `(identity, password)` traffic.
- `docs/server/data/Pod.Data/README.md` — where the EF configuration for these entities lives (`ModelConfig.<Entity>Config`).
- `docs/server/data/Pod.Data.Infrastructure/README.md` — the `IResult<T>` pattern that every entity factory/mutator returns.
- `docs/server/Pod.Enums/README.md` — every enum referenced above (`SessionState`, `NetworkState`, `ConnectionClosedBy`, `RequestSource`, `StopReason`, `StationControlMode`, `PlatformType`, `CreatorType`, `CurrencyIsoCode`, `SubscriptionChangeOperation`, `SmtpAuthentication`, `EMailTemplateIdentifier`, `EmailVariableType`, `EmailReceiverType`, `EmailSendState`, `TemplateVariableKey`, `UserError`, `ConnectionRequestResult`).
- `docs/server/Pod.MailEngine/README.md` — for how `EmailSendOrder` is processed.
