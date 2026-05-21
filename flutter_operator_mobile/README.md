# flutter_operator_mobile — PoD Operator Mobile

Operator daily-ops app. Modern Dart-3 / Flutter-3 port of the legacy `leap_play_x_app` (Flutter 1.7 / Dart 2 pre-null-safety, lives in a separate sibling repo — not part of this one), finished as the app was originally envisioned.

Sibling to `flutter_operator/`:

| | `flutter_operator/` | **`flutter_operator_mobile/`** |
|---|---|---|
| Audience | Admin desk | Floor staff (phone / tablet) |
| Primary action | Mint station API keys | **Start / stop / extend sessions** |
| Auth | JWT, login each visit | JWT with **refresh-token rotation + auto-login** |
| State management | none (stateless screens) | `provider` 6.x (`IdentityProvider`, `StationsProvider`, per-station `Station`) |
| Build | `flutter build web` via cirruslabs/flutter | same |
| Serve | nginx, `/api/*` → server | same |

## What's in it

- **Login** with refresh-token rotation. The legacy app's strongest piece — preserved and modernised. Tokens persist via `shared_preferences`; a `Timer` refreshes shortly before the access-token expiry.
- **Stations list** with pull-to-refresh, status icons (idle / session-running / disconnected, plus Local / Remote / RemoteWithQrCode mode icons), per-station action buttons.
- **Start session**: picks a duration (15min / 30min / 1h / 2h) and `PUT`s `/api/v1/stations/{id}/sessions`. Matches the Phase-6 round-trip behaviour.
- **Extend session**: `POST /api/v1/stations/{id}/sessions/current/update` — the legacy app's "update_session_icon" button finally does what its name says.
- **Stop session** with confirm dialog: `POST /api/v1/stations/{id}/sessions/current/stop`.
- **Station detail screen** — the legacy app's stub, now finished. Shows full state, current session details with remaining-time countdown, and a scrollable history of recent sessions.
- **Sign out** via the legacy `logout_icon.png` asset; calls server-side `/api/v1/auth/logout` and clears local prefs.

All 19 image assets from the legacy app are carried over under `assets/images/`.

## Local build

Same containerised path as `flutter_operator/`:

```sh
docker build -f flutter_operator_mobile/Dockerfile -t pod-operator-mobile .
docker run --rm -p 8081:80 \
  --network pod_default \
  pod-operator-mobile
```

When wired into the project's `docker-compose.yml`, set `POD_API_BASE` empty (default) so the SPA hits the same origin and nginx proxies `/api/*` to the `server` container.

## Source layout

```
lib/
├── main.dart                     ← MultiProvider root
├── models/
│   ├── auth_data.dart            ← persisted JWT bundle
│   └── result.dart               ← Result<T> wrapper
├── services/
│   └── api_client.dart           ← single http source-of-truth
├── providers/
│   ├── identity.dart             ← auth + refresh-token Timer
│   ├── stations.dart             ← list collection
│   └── station.dart              ← per-station node, exposes start/stop/extend
├── screens/
│   ├── splash_screen.dart        ← auto-login resolver
│   ├── login_screen.dart         ← form + error surface
│   ├── stations_screen.dart      ← pull-to-refresh list + sign-out
│   └── station_detail_screen.dart← state + current session + recent-history
└── widgets/
    └── station_card.dart         ← per-station tile + action buttons

web/                              ← Flutter web shell (index.html, manifest.json)
assets/images/                    ← 19 PNG/JPG carried from leap_play_x_app
Dockerfile                        ← multi-stage build → nginx
nginx.conf                        ← SPA + /api/* proxy to server:80
```

## Differences from the legacy reference

- **Null-safe Dart 3.** Every variable, every async return type, every collection. No `dynamic`-passthrough state.
- **`provider: ^6.1.2`** (was 3.x). `ChangeNotifierProxyProvider`'s API changed twice between then; the new code uses the current `create:` lambda form.
- **Path-dep `leap_api` (Swagger-generated client) replaced** by `ApiClient` in `lib/services/`. The legacy code path-deped a generated package outside the repo (`C:\\Repositories\\pod\\SwaggerCodegen\\…\\dart-client`); that directory is gone and the swagger has moved on anyway. The new code talks REST directly with `package:http` against the v1.0.0 server contract verified live in Phase 6.
- **Station detail screen actually exists.** The legacy app had a 36-line stub; this version shows full state + session details + history.
- **Extend session button is wired.** Legacy bound `update_session_icon` to `tryStartSession`; this version calls `POST /sessions/current/update` as intended.
- **Web-first.** The legacy app targeted Android; this build is web for parity with `flutter_operator/`. The same Dart source also runs on Android/iOS if `flutter create --platforms=android,ios .` is run in this directory and the platform shells are regenerated — not done here because it adds 200 MB of native scaffolding that isn't part of the docker-compose deployment.
