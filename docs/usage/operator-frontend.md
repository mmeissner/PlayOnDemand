# Operator frontends (Flutter web)

There are two sibling Flutter web apps in the repo. They are complementary, not duplicates:

| | [`flutter_operator/`](../../flutter_operator/) | [`flutter_operator_mobile/`](../../flutter_operator_mobile/) |
|---|---|---|
| Audience | Admin desk | Floor staff (phone / tablet) |
| Primary action | Mint station API keys | **Start / stop / extend sessions** |
| Auth | Login each visit (in-memory token) | **Auto-login + refresh-token rotation** |
| Origin | Fresh minimal v1.0.0 reference impl | Modernised port of the legacy `leap_play_x_app` Dart-2 codebase, finished as originally envisioned |
| State management | none — stateless screens | `provider: ^6.1.2` |
| Build | `flutter build web` → nginx | same |

The rest of this page describes the admin web app (`flutter_operator/`). The mobile/daily-ops app has its own README at [`flutter_operator_mobile/README.md`](../../flutter_operator_mobile/README.md) with the same build path (`docker build -f flutter_operator_mobile/Dockerfile -t pod-operator-mobile .`).

The legacy `leap_play_x_app` Dart-2 codebase lives in a separate repo (not part of this one) and was used as a one-time porting reference for `flutter_operator_mobile/`. Screens it had but neither current app does (billing, multi-tenant admin) are listed in the "what's not yet shipped" section below.

## What v1.0.0 ships

Three screens:

| Screen | What it does |
|---|---|
| **Login** | POSTs `/api/v1/auth/login` with username + password. On 200, stores the access token in memory and pushes to the station list. On 400 surfaces the server's `userIdentityPasswordMismatch` (or any other validation error). |
| **Stations** | GETs `/api/v1/Stations` with the Bearer header. Lists each station's `displayName`, `controlMode`, `networkState`. Tap to drill in. Refresh button re-fetches. |
| **Station detail** | KV-style display of the station fields. **Mint API key** button (`PUT /api/v1/Stations/{id}/apikeys?keyName=...`) shows the freshly-issued Secret in an amber card — exactly once — because the server side elides Secret from the list response. List view (`GET /api/v1/Stations/{id}/apikeys`) shows name + public key + creation date. |

Auth state lives in memory only. Refresh the browser → re-login. (Refresh tokens, persistent storage, biometric unlock, etc. are out of scope for v1.0.0.)

## How it ships in docker-compose

The `operator` service in [`docker-compose.yml`](../../docker-compose.yml) is an nginx container that:

- Serves the static `flutter build web --release` output at `:8080/`.
- Proxies `/api/*`, `/swagger/*`, and `/health` to the `server` container.

Same origin → no CORS. The Flutter SPA hits relative URLs (`/api/v1/auth/login`) and nginx routes them.

## Building

### Option A — host-side build (recommended; offline-friendly)

```sh
# Inside the repo root.
cd flutter_operator
flutter pub get
flutter build web --release
cd ..
docker build -t pod-operator:latest -f flutter_operator/Dockerfile.prebuilt .
docker compose up -d
```

The host needs Flutter 3.24+ on `PATH`. The resulting image is a stock `nginx:1.27-alpine` with the Flutter web output baked in — ~33 MB.

### Option B — fully containerised build

```sh
docker compose build operator
docker compose up -d
```

`flutter_operator/Dockerfile` uses `ghcr.io/cirruslabs/flutter` as the build stage (pre-installed Flutter SDK), then COPYs the `build/web` output into nginx. The build takes a few minutes the first time (image pull + pub get + compile); subsequent builds are cached.

Use option A when ghcr.io is unreachable or when the build host already has Flutter and you don't want to pull the SDK into every CI job.

### Verifying

After `docker compose up -d` settles (~30 seconds), all four containers should report `healthy`:

```sh
docker compose ps
# postgres         Up (healthy)
# server           Up (healthy)
# operator         Up (healthy)     - http://localhost:8080
# operator-mobile  Up (healthy)     - http://localhost:8081
```

Log in to either with admin credentials from `.env` (`ADMIN_USERNAME` / `ADMIN_PASSWORD`).

## What `flutter_operator_mobile` ships

The full daily-ops web app — drawer-based navigation, station list with auto-polling, four-tab per-station detail covering every per-station endpoint, all-sessions admin view, account password change.

**Stations list** (`/`):
- Drawer: Stations, All sessions, Account, Support, Sign out (each entry with a dedicated PNG icon).
- Stats bar: total / online / in-session counts. Pulsing "Live · 5s" dot indicates active polling.
- Per-station card: state icon (idle / session / disconnected), control-mode icon (couch / remote / QR), session-state chip, quick-action buttons (start / extend / stop), Details button.
- Floating action button to **create a new station** (PUT `/api/v1/stations`).

**Station detail** — four tabs:

| Tab | Endpoints exercised |
|---|---|
| **Overview** | `GET /stations/{id}`, `PUT /sessions`, `POST /sessions/current/update`, `POST /sessions/current/stop` — with a live 1-second remaining-time countdown on the active session |
| **Sessions** | `GET /stations/{id}/sessions` — full history, expandable rows. Polls every 8 s. |
| **API keys** | `GET /apikeys`, `PUT /apikeys?keyName=`, `DELETE /apikeys/{publicKey}` — one-shot **secret-reveal card** with copy buttons, since the list endpoint elides `secret` after mint |
| **Settings** | `GET/POST /settings`, `POST /settings/mode`, `POST /settings/qrcode`, `POST /settings/password` — rename / control-mode picker / QR-code URL / rotate station password (gated behind a "danger" UI) |

**All sessions** (drawer entry): `GET /api/v1/stations/sessions` — admin overview with filter chips (All / Active / Ended). Polls every 8 s.

**Account** (drawer entry): `POST /api/v1/accounts/password/change`, plus an About card with the LEAP PLAY logo.

**Auto-refresh, not buttons**. No refresh buttons anywhere. Each screen registers a `Timer.periodic` (5 s for stations list, 3 s for station detail, 8 s for session lists) and pauses it when the tab is backgrounded. Pull-to-refresh is kept as a manual override. A thin `LinearProgressIndicator` at the top of the AppBar shows when a fetch is in flight.

## What `flutter_operator` (the minimal admin app) ships

Three screens:

| Screen | What it does |
|---|---|
| **Login** | `POST /api/v1/auth/login`; in-memory token. |
| **Stations** | `GET /api/v1/stations`. Tap to drill in. |
| **Station detail** | KV-style. **Mint API key** + revealed Secret card. |

This is the original v1.0.0 reference. Kept for documentation purposes / minimal example.

## What neither app ships yet

These need server-side surfaces that don't exist yet, or design decisions still open:

- **Realtime push** — server is poll-only. Both apps use REST polling at 3–8 s intervals; a real "live" view needs SSE or SignalR.
- **Billing view** — `SubscriptionState` + `SubscriptionOrder` entities exist server-side but the API isn't exposed yet for non-admin operators.
- **Admin settings** — `Pod.Services/Administrator/AdminService` exists; no controller wires it to REST yet.
- **Multi-language UI** — kiosk side has EN + ZH-CN; the operator UIs are English-only.

Each is a small, well-bounded follow-up. PRs welcome.

## What lives in `leap_play_x_app/` (sibling repo)

The original closed-source operator UI. Predates the .NET 10 migration. Uses Dart 2 pre-null-safety and `provider: ^3.0.0`. Has more screens than the v1.0.0 reference (above) but won't build against a modern Flutter SDK without:

1. Dart 3 / null-safety migration (`dart fix --apply` handles ~80% of it).
2. `provider` 3 → 6 migration (constructor signature changed).
3. Regenerated Dart API client from the current Swagger (the hardcoded `leap_api: path: C:\Repositories\pod\SwaggerCodegen\leap-play-clients\dart-client` no longer exists).

If you have the bandwidth, port the screens that aren't in `flutter_operator/` over. Otherwise, treat `leap_play_x_app/` as a reference for the kinds of flows a richer operator UI would have, and grow `flutter_operator/` from there.

## NOT to be confused with the kiosk

`flutter_operator/` is a **web app** loaded in an operator's browser. It does NOT take a `-debug` flag — that's the kiosk (`LeapPlay.Shell.exe`), which is a Windows WPF binary that replaces `explorer.exe` in production mode. See [kiosk-known-issues.md](kiosk-known-issues.md) for the kiosk's `-debug` rule and build status.

The kiosk is what runs on each VR PC. The Flutter operator UI is what arcade operators load on their phone or laptop to manage the kiosks remotely. They're separate artifacts with separate runtime constraints.
