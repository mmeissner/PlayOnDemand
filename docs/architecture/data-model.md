# Data Model

> Server-side entities, relationships, and the "rich domain model" conventions that govern them. Authoritative entity catalogue lives in `docs/server/data/Pod.Data.Models/README.md`; this doc is the topological view.

---

## Where it all lives

| Layer | Project | What's there |
|-------|---------|--------------|
| Entity classes | `Pod.Data.Models` | All persisted types, with their domain-rule methods. Targets `netstandard2.0`. |
| EF mapping (Fluent API) | `Pod.Data/Config/ModelConfig.cs` | Nested `ModelConfig.<Entity>Config` classes. Manual registration in `OnModelCreating`. |
| Migrations | `Pod.Data/Migrations/` | **Frozen since July 2019.** Entity files have changed since (last touched 2023) without matching migrations. |
| `Result<T>` returned by entity methods | `Pod.Data.Infrastructure` | Validation chain (`Arg…`/`Value…`/`Ref…`/`String…`) + composition. |
| Enums used as entity properties | `Pod.Enums` | `SessionState`, `NetworkState`, `StationControlMode`, `PlatformType`, `EMailTemplateIdentifier`, `UserError`, etc. |

The full per-entity catalogue is in `docs/server/data/Pod.Data.Models/README.md` — every class with a one-liner, key relationships, and which factory creates it. This file is the relational map and the "how things hang together" doc.

---

## Top-level domain areas

```
                          ApplicationUser ─┐ (Identity-side: roles, claims,
                                           │  logins, tokens — IdentityDbContext)
                                           │
                                           │  1 → many
                                           ▼
                    ┌──────────────  Station  ──────────────┐
                    │           (aggregate root)            │
                    │                                        │
       1↔1  ┌───────┴───────┬─────────┬─────────┬─────────┐ │  1 → many
            │               │         │         │         │ │
   StationSettings  SubscriptionState ConnectionState  SessionDetails  StationApiKey
            │               │         │         │         │
            │       1→many  │         │ 1↔0..1  │         │
            │               ▼         │         ▼         │
            │       SubscriptionOrder │      Session ◀────┤ (FSM)
            │       SubscriptionChange│         │
            │       SubscriptionPayment        │
            │                       (FSM)      │ 0..1 → many
            │                                  ▼
            │                            SessionRule
            │                            ChangeRequest
            │
            │
            │  1 → many
            ▼
   ApplicationRoot ── 1 → many ── LocalApp ── many → 1 ── UniqueApp
   (per device)                     │                      (cross-station identity)
                                    ▼
                              SessionRuleLocalApp (M:N join)


           ShellServer ── 1 → many ── ConnectionState
                       └─ 1 → many ── ClosedConnection (audit)


             EMailAccountData ◀── M:N ──▶ EmailContentTemplate
                                                    │
                                                    │  1 → many
                                                    ▼
                                              EmailVariable

              EmailSendOrder ── 1 → many ── EMailReceiver
                              └─ 1 → many ── EMailVariableValue
```

(Read the per-project doc for the precise field names; this is the shape.)

---

## The Station aggregate

`Station` is the central aggregate root. `Station.Create(userId, displayName, password, IPasswordHasher)` is the only place a station + its hangers-on come into existence:

- `StationSettings` — operator-controlled per-station settings (display name, QR code, control mode).
- `SubscriptionState` — billing state for the station (start / expiry / change history).
- `ConnectionState` — the **current** gRPC connection FSM.
- `SessionDetails` — the **current** session FSM controller.
- (provisioned alongside) `StationApiKey`s — HMAC `(PublicKey, SecretKey)` pairs for the **REST `amx` scheme**. The kiosk uses these to authenticate calls to `StationController` (its non-gRPC REST surface). Its gRPC calls use `Station.PasswordHash` directly via scheme #3 — see [auth.md](auth.md). The same kiosk holds *both* credentials.
- (later) `ApplicationRoot`s — installed-app catalogue, one per device identity.

Lifecycle of secondary entities:
- `SubscriptionOrder`, `SubscriptionPayment`, `SubscriptionChange` are created via the chain `Station.CreateOrder(...)` → `SubscriptionOrder.PayOrder(...)` → internally `SubscriptionPayment` is built → which calls `SubscriptionState.CreateOrExtend(payment)` → which produces a `SubscriptionChange`.
- `Session` is created via `SessionDetails.RequestSession(...)`. `SessionDetails` controls the FSM externally; `Session.*` transition methods are `internal` and only invoked by `SessionDetails`. See [session-lifecycle.md](session-lifecycle.md).
- `ClosedConnection` rows are created internally by `ConnectionState` whenever a connection ends (graceful or otherwise).

---

## Two FSMs you'll touch most

### `ConnectionState`

States: `Disconnected → Connecting → Connected`. Transition methods on the entity:

| Method | Trigger | Returns |
|--------|---------|---------|
| `RequestConnecting` | Station opened gRPC connection, server picking a host | `IResult<ConnectionRequestResponse>` |
| `RequestConnected` | Station bound to the server | same |
| `RequestDisconnected` | Graceful close | same |
| `RequestHeartbeat` | Station ping arrived | same |
| `RequestTimeout` | Heartbeat sweep determined silence | same |

`ConnectionRequestResponse` (nested struct) carries: `Result` (an enum of outcomes), `CloseLastSessionIfExist` (boolean — telling the caller to also close the linked `Session`), and a snapshot `ClosedConnection` for the audit row when applicable.

### `Session`

States: `Requested → Delivered → Started → Ended` (happy path). Failure terminals: `Canceled`, `DeliveryTimeout`, `ResponseTimeout`. The transition methods (`RequestDelivery`, `SetConfirmation`, `EndSession`, `AddChangeRequest`) are `internal` and called only by `SessionDetails`.

Both FSMs use `[NotMapped] LoadedFromDatabaseUtc` (set in the parameterless EF ctor) as their reference time for timeout decisions, instead of `DateTime.UtcNow`. Re-fetching the entity gets you a fresh "now"; in-memory transitions stay deterministic.

Full diagram + RPC sequence: [session-lifecycle.md](session-lifecycle.md).

---

## Identity / Auth

- `ApplicationUser : IdentityUser<Guid>`. Adds a Postgres-sequence `CustomerNumber` (sequence `User_CustomerNumber_seq` starting at 100 — the human-readable billing reference) and `IReadOnlyCollection<Station> Stations`.
- `ApplicationRole : IdentityRole<Guid>`. Five roles seeded in `Pod.Data/Config/RolesConfig.cs`: `User`, `Accountant`, `ServerManager`, `CustomerSupport`, `Administrator`.
- All Identity tables are managed via `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` on `PodDbContext`.

Stations don't have user accounts. They authenticate **gRPC** calls with `(Station.Id, Station.Password)` as plain metadata headers — server PBKDF2-verifies `Password` against `Station.PasswordHash`. They authenticate **REST** calls (`StationController`) with `(StationApiKey.PublicKey, StationApiKey.Secret)` via the `amx` HMAC scheme — the server's `amx` validator looks up `StationApiKey → Station → ApplicationUserId` and surfaces both IDs as claims, which is how the server knows which account owns the calling station on every request. See [auth.md](auth.md).

---

## Billing chain

```
Station.CreateOrder(...)
        │
        ▼
SubscriptionOrder (created, OrderNumber from sequence "SubscriptionOrder_OrderNumber_seq" starting at 1000)
        │
        │  .PayOrder(receivedUtc, gatewayRef)
        ▼
SubscriptionPayment (factory internally calls...)
        │
        ▼
SubscriptionState.CreateOrExtend(payment)
        │
        │  decides type from time gap:
        │   - StartOnUtc==null         → InitialCreated
        │   - now > ExpiresOnUtc (gap) → Renewed (starts now)
        │   - else (still active)      → Extend  (starts at ExpiresOnUtc)
        ▼
SubscriptionChange (audit row)
```

Currency is `CurrencyIsoCode` enum (in `Pod.Enums`). `PaymentReference` ≤ 128 chars (matches `Station.MaximumLengthPaymentReference`).

---

## Application catalogue

The kiosk's installed apps are tracked per-(`Station`, `DeviceIdentity`):

- `ApplicationRoot { StationId, DeviceIdentityId, LastSyncTimestampUtc, _localApps[] }` — unique on `(StationId, DeviceIdentityId)`.
- `LocalApp` — per-device installation: `InstanceVersion` (monotonic counter, station-incremented), `LocalDisplayName`, `IsEnabled`, `IsInstalled`. `Update(LocalAppUpdate)` only applies if newer version. `SetUninstalled()` flags but doesn't delete.
- `UniqueApp` — global cross-station identity. Carries `Origin: (CreatorType, CreatorId)`, `Platform: PlatformType` (VBox / Steam / …), `DisplayName` (international strings). `Id` is **caller-supplied** and follows special encoding to ensure uniqueness across platforms.

Lookup pattern: `IUniqueAppFactory.GetOrCreate(...)` (interface in `Pod.Data.Models`, impl in `Pod.Services`) — used during station ↔ server app sync via `ServiceShellApplications.SendSyncAppStates` (see [grpc.md](grpc.md)).

`DeviceIdentity` has a **string PK** (the raw machine identifier the kiosk reports) — every other entity uses `Guid`.

---

## Email subsystem

Three core entities:

- `EMailAccountData` — SMTP sender. `SetEnabled(true)` re-validates everything before flipping.
- `EmailContentTemplate` — template text + parsed `EmailVariable` rows (parsed once at create time via `IVariableParser` from `Pod.MailEngine`).
- `EmailSendOrder` — queued outbound email. Indexed on `SendState` so the background worker (`SendEmailServiceHosted` in `Pod.Services`) can scan for `Unsend` quickly.

M:N: `EMailAccountData ⟷ EmailContentTemplate` (which sender(s) can send which template).

Naming oddity: prefixes `EMail*` and `Email*` both exist — historical, not in-flight rename. Match the local form when extending.

---

## Conventions that matter when changing entities

1. **Rich domain model**. Properties default to `private set;`. Mutation goes through methods that return `IResult<T>`. Don't add `public set;` properties.
2. **Internal constructors mark "only the parent aggregate creates me"**. `Station.Create(...)` is the wiring point for `StationSettings`, `SubscriptionState`, `ConnectionState`, `SessionDetails`.
3. **Collections are `IReadOnlyCollection<T>` over private `HashSet<T>`**. EF reads/writes via field access — the matching `ModelConfig.<Entity>Config.Configure` MUST call `.Metadata.FindNavigation(name).SetPropertyAccessMode(PropertyAccessMode.Field)` or EF silently never populates the collection.
4. **`*.cs` files do not map 1:1 to classes**. `Station.cs` declares 5 classes; `ConnectionState.cs` declares 4; `Session.cs` declares 4; `EmailContentTemplate.cs` declares 3; `EmailSendOrder.cs` declares 3. Search by class name, not file name.
5. **`[NotMapped] LoadedFromDatabaseUtc`** is the FSM time-base on `Session` and `ConnectionState`. Don't replace timeout checks with `DateTime.UtcNow`.
6. **Helpers like `Station.NameOfPasswordHash()`** exist so `ModelConfig` can map private members without using string literals. If you rename the private property and forget the helper, the build breaks — that's the point.
7. **`UniqueApp.Id` is caller-supplied** with platform-encoded uniqueness. Don't generate it on the entity side.
8. **`ApplicationUser.CustomerNumber` is database-assigned** via Postgres sequence. The C# property has `private set;` and no factory writes to it.

---

## ⚠️ Migration freeze

Migrations under `Pod.Data/Migrations/` last touched **2019-07-06** ("Added_SendEmailOrders"). Entity files have evolved since then (last edits 2023). **Schemas in production may not match the entity classes.**

If you change an entity, you must:
1. Regenerate a migration: `dotnet ef migrations add YourChangeName -p Pod.Data -s Pod.Web.Center`
2. Verify the diff covers what you actually changed (the gap between code-now and DB-as-of-last-migration may surface unrelated changes — be careful).
3. Coordinate the deploy: production DBs may need bridge migrations.

The four existing migrations:

| Date | Name | What it did |
|------|------|-------------|
| 2019-06-09 | `CreateInitial` | All initial tables: Identity, Stations, Sessions, Subscriptions, Mail. |
| 2019-06-29 | `StationApiKeySecret` | Added `StationApiKey` table for the REST `amx` HMAC scheme (which gates `StationController`). Distinct from the gRPC auth — that uses `Station.PasswordHash`. |
| 2019-07-01 | `EmailTemplatesHtmlBody` | Added HTML body column to `EmailContentTemplate`. |
| 2019-07-06 | `Added_SendEmailOrders` | Added `EmailSendOrder` queue. |

---

## Read next

- `docs/server/data/Pod.Data.Models/README.md` — every class in detail, with field lists and the factory chains.
- `docs/server/data/Pod.Data/README.md` — `PodDbContext`, `ModelConfig` patterns, migration tooling, `PasswordHasher` (PBKDF2-V3) details.
- `docs/server/data/Pod.Data.Infrastructure/README.md` — the `IResult<T>` / `Result<T>` validation pattern entities return.
- [session-lifecycle.md](session-lifecycle.md) — the `Session` / `SessionDetails` FSM in motion.
- [auth.md](auth.md) — how `ApplicationUser`, `Station`, `StationApiKey` participate in authentication.
- `docs/server/Pod.Enums/README.md` — every enum referenced above (when written).
