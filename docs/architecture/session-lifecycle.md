# Session Lifecycle

> The core domain concept: a "session" is a paid/timed VR play instance on a station. Everything else in the system orbits this finite-state machine.

> **Two FSMs share the name "session state".** The **domain FSM** (on the `Session` entity) and the **wire FSM** (on the gRPC `SessionState` enum) are not the same. The wire enum is a coarser projection. This doc treats both.

---

## Domain FSM (`Session.SessionState`)

The authoritative FSM lives on `Session` (entity in `Pod.Data.Models/Shell/Session.cs`). The state enum (`Pod.Enums/SessionState`):

```
                   Operator approves
   ┌──── Requested ──────────────────────▶ Delivered
   │         │                                │
   │         │ Pickup deadline missed         │ Station accepts
   │         ▼                                │ within UserTimeForLoginRequestResponse
   │   DeliveryTimeout (terminal)             ▼
   │         │                            Started ◀── ChangeRequest extends time
   │ Operator/timeout cancels                  │       (or auto-EndSession if it
   │         ▼                                 │        would push past limit)
   │   Canceled (terminal)                     │
   │                                           │ End triggers (any of):
   │   Station replied "no"                    │  - User logout
   │         ▼                                 │  - Inactivity / heartbeat timeout
   │   Canceled (terminal)                     │  - LimitReached (duration cap)
   │                                           │  - Station shutdown
   │   Pickup happened, response too slow      │
   │         ▼                                 ▼
   │   ResponseTimeout (terminal)         Ended (terminal, IsClosed=true)
   │
   └────────── all of the above set IsClosed = true ──────────────┘
```

States enumerated on `SessionState`:
- `Requested` — initial. Operator has not yet been asked or has not yet acted.
- `Delivered` — operator/portal saw it; station has been notified.
- `Started` — confirmation accepted. The play session is live.
- `Ended` — terminated normally (user logout, limit reached, etc.).
- `Canceled` — operator or system decided not to start this session.
- `DeliveryTimeout` — pickup never happened in time.
- `ResponseTimeout` — pickup happened but accept/reject didn't.

Terminal states are `Ended`, `Canceled`, `DeliveryTimeout`, `ResponseTimeout`. All set `Session.IsClosed = true`. Once a `Session` reaches a terminal state, `SessionDetails.SessionId` is cleared and a fresh `Session` can be requested.

**Source of truth:**
- `Pod.Data.Models/Shell/Session.cs` — the FSM-bearing entity.
- `Pod.Data.Models/Shell/SessionDetails.cs` — the controller; the only thing that calls `Session`'s `internal` transition methods.
- `Pod.Enums/SessionState` — the enum.

---

## Wire FSM (`Pod.Grpc.Messages.Shared.SessionState`)

The proto `SessionState` enum (in `MessagesShared.proto`) is **coarser** than the domain FSM:

| Wire value | Maps roughly to domain |
|------------|------------------------|
| `Unset = 0` | (default protobuf) |
| `NoSession = 1` | `SessionDetails.SessionId == null` (no live session) |
| `LoginRequested = 2` | `Session.State == Requested` or `Delivered` (operator-side approval pending) |
| `AwaitingConfirmation = 3` | `Session.State == Delivered` and station hasn't responded yet |
| `Running = 4` | `Session.State == Started` |

When the kiosk calls `ShellHostServiceGrpc.GetSessionState`, the server collapses the rich domain state into one of the 5 wire values. Failure-terminal domain states (`Canceled`, `DeliveryTimeout`, `ResponseTimeout`) all map to `NoSession` from the wire perspective — the kiosk only needs to know "you can request a new login now" or "you are currently running".

**Don't add a new wire enum value without coordinating both ends** — the kiosk and server proto packages must stay in lockstep.

---

## End-to-end flow (happy path)

```
1. Station boots                       LeapPlay.Shell starts
                                       → ShellHostServiceGrpc.Connect
                                       → server marks ConnectionState = Connected
                                       → opens GetNotifications stream
                                         (long-lived push channel)

2. End-user walks up                   UI shows Login (PIN or QR code)
                                       → user enters PIN
                                       → ShellHostServiceGrpc.SendLoginIntention
                                       → server: SessionDetails.RequestSession(...)
                                         creates Session in state = Requested
                                       → publishes ClientCommandType to
                                         StationResponseHub → operator UI sees
                                         the request realtime

3. Operator approves                   Web UI calls REST endpoint
                                       → SessionService walks SessionDetails
                                         + Session through internal transitions
                                       → state Requested → Delivered
                                       → server pushes ClientNotification
                                         { Event = CheckLoginRequest } on the
                                         GetNotifications stream

4. Station picks up                    GetNotifications fires
                                       → ShellHostServiceGrpc.GetLoginIntention
                                       → returns RequestedLoginResponse
                                       → station UI prompts user to confirm

5. Station responds                    ShellHostServiceGrpc.SendLoginResponse
                                       { IsLoginAccepted = true }
                                       → state Delivered → Started
                                       → SessionDetails wires up the Session;
                                         start timer based on
                                         SessionRule.StartDuration (if any)
                                       → if SessionRule.StartApplication is set,
                                         platform module launches that app

6. Active session                      Station sends SendHeartbeat every
                                       ServerSettings.HeartbeatInterval
                                       → server updates LastHeartbeatOnUtc
                                       → ConnectionHealthService background
                                         loop watches for HeartbeatTimeout
                                         and force-ends sessions on timeout

7. End                                 Triggered by any of:
                                       - User logout    (UserLogout)
                                       - App auto-exit  (AutoLogout)
                                       - Limit reached  (LimitReached)
                                       - Inactivity     (Inactivity)
                                       - Station shutdown (Shutdown)
                                       - Force-close from server
                                       → SendLogoutRequest with LogoutReason
                                       → state Started → Ended
                                       → SessionDetails.SessionId cleared
                                       → station returns to Login screen
                                       → AppStatistics rolled up locally;
                                         server-side aggregates updated via
                                         next sync cycle
```

---

## Session conditions / rule

Set as a `SessionRule` on the `Session` entity (created lazily via `Session.AddOrGetRule()`):

| Field | Type | Meaning |
|-------|------|---------|
| `StartDuration` | `TimeSpan?` | Hard time cap. Server force-ends when reached (`StopReason.LimitReached`). |
| `StartApplication` | `UniqueApp` ref (FK) | If set, kiosk auto-launches this app on session start. |
| `SessionRuleLocalApp` collection | M:N to `LocalApp` | App whitelist for this session. UI hides apps not in the collection. Empty = all allowed. |

These conditions are **not server-enforced for app launches**. The kiosk's `LeapVR.Shell.Modules.Platform.*` modules apply them locally. The server learns of violations (e.g. unauthorised launch attempts) for audit, not for blocking.

`Session.AddChangeRequest(...)` extends `Duration` mid-session (operator giving a player extra time). If the new total puts the elapsed past the cap, it auto-calls `EndSession(StopReason.LimitReached)` — callers must inspect both the response **and** the resulting `Session.State`.

---

## Heartbeat protocol

| Setting | Default | Source |
|---------|---------|--------|
| `HeartbeatInterval` | 15 minutes | `ShellServer.HeartbeatInterval` (per-server, configurable; default in `ShellServer.cs`) |
| `HeartbeatTimeout` | 17 minutes | `ShellServer.HeartbeatTimeout` |
| `ConnectTimeout` | 5 seconds | `ShellServer.ConnectTimeout` |

The kiosk pulls these on startup via `ShellHostServiceGrpc.GetServerSettings`. Then it calls `ShellHostServiceGrpc.SendHeartbeat` on the interval. Server-side `ConnectionHealthService` (background `IHostedService` in `Pod.Web.Center`) sweeps periodically for stale heartbeats and force-closes connections / sessions via `ConnectionState.RequestTimeout`.

Note: the **heartbeat is on the connection**, not the session. Connection-level timeout takes down the session as a side effect (`HandleConnectResponse` on `SessionDetails` reacts to `ConnectionState` transitions).

---

## Real-time notifications back to operator UI

Operator-facing UI needs realtime feedback when station-driven state changes happen. The pattern:

```
Station ──gRPC──▶ ShellHostServiceGrpc.<method>
                  │
                  ├─ touches PodDbContext (entity transitions)
                  │
                  └─ publishes to PublisherHub<T> / StationResponseHub
                                            │
                                            │ (in-process pub/sub)
                                            ▼
REST controller ◀─ subscribes ─◀ same-process subscriber
       │
       ▼
Web client (SignalR / SSE / poll — varies per controller)
```

The hubs live in `Pod.Services` (under the `Notifications/` area). They're an in-memory pub/sub bridge — no external broker. **Single-instance only.** Scale-out needs Redis Pub/Sub or similar.

The `ClientNotification` events on the kiosk-facing `GetNotifications` stream go in the opposite direction (server → station) but use the same hub plumbing.

`SendLoginIntention` and `Disconnect` notably publish to `PublisherHub<ClientCommandType>`. The `Disconnect` enum value also doubles as the stream sentinel that closes `GetNotifications`.

---

## Billing hooks

`SubscriptionState` (one per station) determines whether a session can start at all. `SessionService` checks it during the transition into `Started`:

- **Pre-paid model** — `SubscriptionState.ExpiresOnUtc` must be in the future. If not, the session can be `Requested` but won't transition to `Started`.
- Each `SubscriptionOrder.PayOrder()` produces a `SubscriptionPayment` which calls `SubscriptionState.CreateOrExtend(payment)` to extend coverage. Type of change (`InitialCreated` / `Renewed` / `Extend`) is auto-determined from the time gap.

See [data-model.md](data-model.md) for the full billing chain.

---

## Where to make changes

| Change | Where |
|--------|-------|
| Add a new domain state | Add to `Pod.Enums/SessionState`; add transition method on `Session` (internal); update `SessionDetails` to call it; update wire-state mapping on the gRPC service. |
| Add a new `SessionRule` field | Add property to `SessionRule` entity (DB migration!); update `Pod.Grpc.Messages` mapping; update kiosk `SessionController` to apply it. |
| Change heartbeat interval | Update `ShellServer` defaults (or operator-configurable per server) — kiosk reads via `GetServerSettings`. |
| Change timeout sweep policy | `ConnectionHealthService` in `Pod.Web.Center/HostedServices/`. |
| Add a new `LogoutReason` | Add to enum; ensure both kiosk (which sets it) and server (which records it via `Session.EndSession`) handle the new value. |
| Add a new `StopReason` | Same — kiosk-side `Session.EndSession(stopReason)` carries it into the audit. |

---

## Read next

- [grpc.md](grpc.md) — the methods listed above (`Connect`, `SendLoginIntention`, `GetLoginIntention`, `SendLoginResponse`, `SendHeartbeat`, `SendLogoutRequest`, `Disconnect`) live on `ShellHostServiceGrpc`.
- [data-model.md](data-model.md) — entity catalogue including `SessionDetails`, `Session`, `SessionRule`, `SubscriptionState`.
- `docs/server/data/Pod.Data.Models/README.md` — the entity's full method list and the factory chain.
- [auth.md](auth.md) — how stations authenticate the gRPC calls listed above.
