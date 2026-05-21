# 08 — Operator app (Flutter)

> The back-office UI for the platform. Two flavours ship in v1.0.0: a
> minimal web SPA bundled in the Docker Compose stack, and a richer
> mobile-friendly Flutter app for daily ops on a phone or tablet.

## Two operator apps

| App | Where it lives | What it's for |
|-----|----------------|---------------|
| `flutter_operator/` | Web SPA, served by the nginx sidecar in docker-compose at `http://localhost:8080/`. | The v1.0.0 critical path: login, station list, station detail, API-key mint + list. One `main.dart` file, deliberately small. |
| `flutter_operator_mobile/` | Standalone Flutter app, builds for Android / iOS / Windows / web. | Day-to-day venue ops: dashboard, all stations, per-station detail with tabs (Overview / Sessions / API Keys / Settings), all sessions view, account screen. |

Both apps talk to the same REST endpoints on `Pod.Web.Center` —
authentication is JWT for human operators, with the option of `amx`-HMAC
for headless integrations. See
[`../architecture/auth.md`](../architecture/auth.md) for the wire
protocol.

The rest of this chapter walks the mobile app (the daily-ops one) since
it covers the largest surface area. The web SPA is a strict subset of the
same.

## Login

A standard email + password flow, hits `POST /api/v1/auth/login`, gets
back a JWT, stashes it in secure storage on mobile / `localStorage` on
web. Subsequent requests carry it in the `Authorization` header. Server
ships a seeded admin from `ADMIN_EMAIL` + `ADMIN_PASSWORD` in `.env` on
first run.

## Splash → station list

Right after login the app lands on the **Stations** screen. It polls
`GET /api/v1/stations` every 5 seconds while visible to keep the live
state fresh:

- **Online / offline** indicator per station (driven by the heartbeat
  recency on the server side).
- **Current control mode** — Local / Remote / RemoteWithQrCode — matching
  the [**login modes from Chapter 02**](02-station-modes.md).
- **Current session** if any, with elapsed time.
- A row tap opens the **station detail** view below.

The polling stops when the screen is hidden (Flutter lifecycle observer)
so the app is quiet when backgrounded.

## Station detail — four tabs

The per-station view is the main daily-ops surface. Four tabs, each
covering one slice of what the REST API exposes for a single station:

### Overview tab

- Live state (the same online/offline + control-mode the list shows).
- Current-session card if a session is in flight: which player, what
  game, time elapsed, time remaining, balance.
- **Start session** button — fires `POST /api/v1/stations/{id}/sessions`
  to mint a new session. Useful for staffed venues to grant time without
  the player needing to scan a QR.
- **Stop session** button — `DELETE` on the active session. The kiosk
  picks this up on its next heartbeat (sub-second) and returns to the
  idle login screen.
- **Extend session** button — `PATCH` to bump the time remaining.

The detail screen polls `GET /api/v1/stations/{id}` every 3 seconds while
visible — fast enough that a kiosk-side accept of a pending login
transitions through `Requested → Delivered → Started` within one tick.

### Sessions tab

Recent session history for this station, scrollable. Each row shows the
player, the game, start time, end time, end reason (game exit, operator
stop, watchdog kill, network drop), and elapsed time. Polls every 8s
while visible.

### API Keys tab

The machine-to-machine credential store for this station:

- **List** — public keys (no secrets) of all currently-valid API keys for
  this station.
- **Mint** — `POST /api/v1/stations/{id}/api-keys`. The server generates a
  fresh `(PublicKey, Secret)` pair and returns the Secret **once**. The
  app reveals it once with a one-shot copy-to-clipboard action; after
  that it's gone and the operator has to mint a new pair.
- **Revoke** — `DELETE` a public key. Used when rotating credentials or
  decommissioning a station.

The kiosk's Setup wizard takes the `(PublicKey, Secret)` pair and stores
it locally — the kiosk then uses it for the `amx` HMAC scheme on REST
calls (typically only used by management endpoints; the live gRPC traffic
uses station password auth from metadata).

### Settings tab

Per-station configuration:

- **Rename** — change the display label (`PATCH /api/v1/stations/{id}`,
  `name` field). Propagates to the catalog UI on next heartbeat.
- **Change control mode** — the same Local / Remote / RemoteWithQrCode
  toggle as the [admin panel](04-admin-panel.md) on the station, but
  driven from the back office. Picks up at the next heartbeat without
  losing the active session.
- **Edit QR code** — set the per-station QR-code payload (the URL or
  payment-flow identifier the QR encodes). Updates the kiosk's QR-login
  screen ([**Chapter 02**](02-station-modes.md)) the next time it
  renders.
- **Rotate password** — generate a new gRPC-metadata password for the
  station. Old password is invalidated immediately; the kiosk picks up
  the new one via its next setup-mode poll.

## All sessions view

A cross-station, time-ordered feed of every session in the venue. Useful
for "what's happening right now?" and for after-the-fact triage.

## Account screen

The operator's own account: change password, log out, see the JWT's
remaining lifetime. Multi-operator support is on the server but the v1.0.0
mobile app is single-account.

## Implementation notes

- State management: `provider` package, one `ChangeNotifier` per concept
  (Stations, Station, Sessions, Identity).
- API client: thin `http` wrapper at `lib/services/api_client.dart`. All
  requests carry the JWT; on 401 the app clears stored credentials and
  bounces to the login screen.
- Polling: per-screen, lifecycle-aware. Stops when the screen is invisible
  to keep mobile data usage and battery sane.
- No websockets — REST polling is good enough at venue scale (10s of
  stations) and avoids server-side long-poll machinery.

The shape of the REST API is documented in
[`../usage/operator-frontend.md`](../usage/operator-frontend.md). For
deeper protocol detail see
[`../architecture/auth.md`](../architecture/auth.md) and
[`../architecture/grpc.md`](../architecture/grpc.md).

---

→ [**09 — Remote control & sessions**](09-remote-control.md)
