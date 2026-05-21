# 01 — Overview

> What a PlayOnDemand station is, what it talks to, and who uses it.

<p align="center">
  <img src="../assets/marketing/hero-hardware-lineup.jpg" alt="Three example PlayOnDemand cabinet form factors: free-roam, seated cockpit, and treadmill" width="100%">
</p>

## A "station" is one VR PC

The unit of deployment is a **station**: a Windows 10/11 PC with a SteamVR-
compatible HMD attached, running the kiosk binary `LeapPlay.Shell.exe` as a
replacement for `explorer.exe`. The station has a station ID + password
issued by the central server, and from boot to power-off it speaks to the
server over gRPC/TLS for session control and heartbeat reporting.

Stations are typically built into a custom cabinet (the image above shows
three of the original LeapVR form factors — free-roam tracking rig, seated
cockpit, treadmill), but any spare desktop next to a couch works.

## What the kiosk shows the player

The kiosk's main screen is one of three "login modes" — see
[**Chapter 02 — Station login modes**](02-station-modes.md). Once the player
is logged in, the station shows a category-tabbed game catalog with the
games that have been installed on it. The catalog UI looks the same whether
you're using a touchscreen, a gamepad, the VR motion controllers, or a
keyboard+mouse — every input device is wired to the same Caliburn.Micro
view-model. See [**Chapter 03 — Game catalog & launch**](03-game-catalog.md).

## The three roles

PlayOnDemand makes a clean separation between three groups of people:

| Role | What they see | Tools |
|------|--------------|-------|
| **Player** | The catalog, the launch picker, the game itself. | Touch / VR pointer / gamepad on the kiosk. |
| **Station admin** | Hardware info, install/uninstall, library management, multimedia, skins. | PIN-protected admin panel **on the kiosk** itself. See [**Chapter 04 — Admin panel**](04-admin-panel.md). |
| **Operator** | All stations across a venue, sessions in-flight, balances, billing, API keys. | Flutter web/mobile **operator app** — chapters [**08**](08-operator-app.md) and [**09**](09-remote-control.md). |

A "content owner" — somebody packaging games into `.vbox` containers for
distribution to stations — uses the standalone
[**Content Creator tool**](07-content-creator.md). That's a desktop app, not
a panel on the kiosk.

## How the pieces talk

```
                       ┌──────────────────────────┐
                       │   Server (Pod.Web.Center)│
                       │   .NET 10 + Postgres     │
                       │   REST  +  gRPC  +  TLS  │
                       └──┬────────┬───────┬──────┘
                          │        │       │
        gRPC (station)    │        │       │  REST + JWT (operator)
        ──────────────────┘        │       └──────────────────
                                   │
              ┌────────────────────┼─────────────────────┐
              │                    │                     │
        ┌─────▼──────┐       ┌─────▼──────┐       ┌──────▼──────┐
        │  Station 1 │       │  Station N │       │  Operator   │
        │  WPF kiosk │  ...  │  WPF kiosk │       │  Flutter UI │
        │  + OpenVR  │       │  + OpenVR  │       │  web + mob  │
        └────────────┘       └────────────┘       └─────────────┘
```

- **Station → Server**: gRPC over TLS, authenticated by
  `(StationId, Password)` in metadata. Heartbeats, session state, station
  catalogue sync.
- **Operator → Server**: REST + JWT (humans) or `amx` HMAC
  (`StationApiKey.PublicKey` + `Secret`) for machine-to-machine.
- **Server**: PostgreSQL persistence, EF Core migrations, optional Let's
  Encrypt on a single hostname.

The deep dive on protocols is in
[`../architecture/grpc.md`](../architecture/grpc.md) and
[`../architecture/auth.md`](../architecture/auth.md). For everything that
follows in this manual you only need to know: there is a server, and every
station phones home to it.

## What you'll see in the rest of the manual

Most chapters open with one full-bleed screenshot of the screen we're about
to walk through, then break it down with smaller crops or close-ups for
each interactive element. The screenshots are from real production builds —
real games, real station IDs, real catalog content. Treat them as worked
examples; the UI in your build will look identical, just with your own
games installed.

Onward:

- → [**02 — Station login modes**](02-station-modes.md)
- → [**03 — Game catalog & launch**](03-game-catalog.md)
- → [**04 — Admin panel**](04-admin-panel.md)
