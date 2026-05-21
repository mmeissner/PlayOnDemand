# 09 — Remote control & sessions

> How the operator app drives the kiosk over the network: minting
> sessions, controlling station mode, sending stop / extend / restart,
> and reading heartbeats.

This chapter is the operational counterpart to the kiosk-side flows in
[**Chapter 02**](02-station-modes.md) and the server-side wire-format docs
under [`../architecture/`](../architecture/). The mental model:

```
       Operator app                      Server                    Kiosk
       (Flutter / web)                   (gRPC + REST)             (WPF)
            │                                 │                      │
   POST /api/v1/stations/{id}/sessions ──────►│                      │
            │     {gameId, durationSec}       │                      │
            │◄────  200 OK + sessionId        │                      │
            │                                 │                      │
            │                                 │  gRPC heartbeat ────►│
            │                                 │◄─── ack + session ───│
            │                                 │                      │
            │                                 │       (kiosk transitions
            │                                 │        Requested→Delivered→Started)
            │                                 │                      │
   GET /api/v1/stations/{id}        ────────► │                      │
            │◄── 200: state=InGame, etc.      │                      │
```

The server is always the source of truth. The kiosk pulls intent on each
heartbeat (~1 Hz in production) and acts on it; the operator app pushes
intent into the server and reads state back. There is no direct
operator → kiosk channel.

## Minting a session

From the [**station detail / Overview tab**](08-operator-app.md#overview-tab),
hitting **Start session**:

1. Operator picks a game from a dropdown (the catalog is fetched from the
   server, which knows what's installed on each kiosk from heartbeat
   sync).
2. Operator sets a duration / cost / "free play" flag.
3. App fires `POST /api/v1/stations/{id}/sessions` with the JWT.
4. Server validates: operator is allowed, station is online, station
   isn't already in a session.
5. Server writes the session to Postgres in `Requested` state.
6. Kiosk's next heartbeat (sub-second) pulls the session, transitions
   `Requested → Delivered → Started`, activates the required VR module,
   launches the game.
7. Operator app's next poll shows the session card flip from "no
   session" to "In Game".

The FSM is the same one a QR-login or operator-driven login walks
through — see [`../architecture/session-lifecycle.md`](../architecture/session-lifecycle.md).
What's different is the trigger: a REST call from the operator instead of
a player at the kiosk.

## Stopping a session

**Stop session** in the same tab fires `DELETE` on the active session.
The kiosk picks this up on its next heartbeat and:

1. Sends the game's main executable a `WM_CLOSE` (clean exit).
2. After a configurable grace period (default 3s), force-kills the
   process tree using the watchdog rules from the package's
   `ProcessMonitorInstructionDto` list.
3. Tears down SteamVR if the station mode allows it.
4. Returns to the login screen and reports end-of-session to the server.

The "end reason" is recorded as `OperatorStopped` and is visible in the
sessions history view ([**Chapter 08**](08-operator-app.md#sessions-tab)).

## Extending a session

**Extend** is a `PATCH` to bump `endTimeUtc` on the active session. The
kiosk picks the new end time up on its next heartbeat and the on-screen
countdown jumps forward. Useful for "five more minutes" requests at a
staffed venue.

## Changing control mode remotely

The same control modes the kiosk admin panel exposes
([**Chapter 04**](04-admin-panel.md)) are settable from the
[**station detail / Settings tab**](08-operator-app.md#settings-tab):

| Server-side value | Kiosk behaviour |
|-------------------|----------------|
| `Local` | Operator-driven mode — kiosk shows Click-to-Start. |
| `Remote` | Driven entirely from the operator app. Kiosk shows the multimedia background and waits for a server-pushed session. |
| `RemoteWithQrCode` | QR-login mode — kiosk shows the QR for the player to self-checkout. |

Switching mode mid-shift takes effect on the next heartbeat; an in-flight
session is **not** interrupted. If the operator switches a Remote-mode
station to Local while a player is mid-game, the player finishes their
session and the *next* login flow uses the new mode.

## Heartbeats and station health

Every station opens a long-lived gRPC stream on boot (`SessionHostService`
in `Pod.Grpc.ShellHost`) and emits a heartbeat at ~1 Hz with:

- Current FSM state.
- CPU / RAM / disk usage snapshots (so the operator app can show "this
  station is at 95% disk").
- Hardware fingerprint diff (alerts on the server if the GPU changed).
- VR runtime state (SteamVR Ready / Not Ready / Recovering).
- Any error / warning the kiosk wants surfaced.

If the server stops hearing from a station for >15s it marks the station
**Offline**. The operator app surfaces this in the stations list with a
red dot; an alert can also be wired up via the server's notification
hooks (out of scope for v1.0.0, but the hook points are in place).

## REST API surface (quick reference)

| Verb | Path | What it does |
|------|------|--------------|
| `POST` | `/api/v1/auth/login` | Email+password → JWT. |
| `GET`  | `/api/v1/stations` | List stations + live state. |
| `GET`  | `/api/v1/stations/{id}` | Single-station detail. |
| `PATCH`| `/api/v1/stations/{id}` | Rename, change control mode. |
| `POST` | `/api/v1/stations/{id}/sessions` | Mint a session. |
| `GET`  | `/api/v1/stations/{id}/sessions` | Session history. |
| `PATCH`| `/api/v1/stations/{id}/sessions/{sid}` | Extend session. |
| `DELETE` | `/api/v1/stations/{id}/sessions/{sid}` | Stop session. |
| `GET`  | `/api/v1/stations/{id}/api-keys` | List API key public keys. |
| `POST` | `/api/v1/stations/{id}/api-keys` | Mint one (Secret returned once). |
| `DELETE` | `/api/v1/stations/{id}/api-keys/{kid}` | Revoke. |

The full route table is generated by the ASP.NET Core MVC layer and
documented in [`../architecture/grpc.md`](../architecture/grpc.md) for the
gRPC twin (station → server side) and is browsable via the Swagger UI at
`/swagger/` on a running server.

## What's not in v1.0.0 (planned)

- **Operator → kiosk file push**: minting a session and selecting a game
  works, but pushing a *new* `.vbox` to a station over the network is not
  wired up. USB-driven install ([**Chapter 05**](05-installing-games.md))
  is the canonical path for now.
- **Multi-operator audit log**: the server records who minted each
  session, but the operator app doesn't yet surface a "who did what"
  trail. The data is in Postgres; only the UI is missing.
- **Live websocket push**: the operator app polls. A real-time push
  channel would be nice but isn't critical at venue scale.

---

← Back to [**manual index**](README.md)<br>
→ [Top-level README](../../README.md)
