# PlayOnDemand

> **Turn any room into a VR arcade.**

<p align="center">
  <img src="docs/assets/marketing/hero-girl-vive.jpg" alt="A young player on an HTC Vive in front of a PlayOnDemand kiosk cabinet" width="100%">
</p>

[![License](https://img.shields.io/badge/license-Apache_2.0-blue.svg)](LICENSE)
[![Server](https://img.shields.io/badge/server-.NET_10_LTS-512BD4)](docs/server/)
[![Kiosk](https://img.shields.io/badge/kiosk-.NET_Framework_4.7.1_WPF-512BD4)](docs/client/)
[![Operator UI](https://img.shields.io/badge/operator-Flutter-02569B)](flutter_operator_mobile/)
[![Deployment](https://img.shields.io/badge/deploy-Docker_Compose-2496ED)](docs/usage/server-deployment.md)
[![Status](https://img.shields.io/badge/status-v1.0_open--source_release-success)](docs/open-source-readiness.md)

PlayOnDemand is an open-source, self-hosted **VR-arcade management platform**.
It's the same software stack that shipped commercially as **LeapVR / LeapPlay**
in production VR-arcade venues from ~2018, now released under Apache 2.0 as a
reference implementation operators can build on, extend, and run themselves.

Drop a Windows PC behind a VR headset, pair it with a single Linux server,
and you have a session-billed, operator-managed, multi-station VR arcade —
catalog, login, launch, monitor, restart, all of it.

---

## What you get

- **A Windows kiosk shell** that replaces `explorer.exe`, presents a touch-
  and gamepad-friendly game catalog, launches into SteamVR / OpenVR, and
  watches the game process to know when the session ends.
- **A central server** (ASP.NET Core on .NET 10 LTS) that authenticates
  stations and operators, mints time-bound play sessions, monitors station
  heartbeats over gRPC/TLS, and persists everything in PostgreSQL.
- **An operator UI** (Flutter, web + mobile) for the back-office: see who's
  playing, top up balances, lock and unlock stations, mint API keys, drive
  remote sessions from a phone.
- **A content authoring tool** (`LeapPlay.Content.Creator`) that packages a
  folder of game files plus its launch metadata into a single `.vbox`
  container that any station can install over USB or LAN — including
  partial-edit support so you can fix a title or thumbnail on a 50 GB
  package without re-zipping the whole thing.
- **One-command deployment** via Docker Compose: postgres + server + nginx
  + operator UI in three containers, optional Let's Encrypt TLS.

<p align="center">
  <img src="docs/assets/marketing/hero-ecosystem.jpg" alt="The PlayOnDemand ecosystem: kiosk UI on TV, HTC Vive headset and controllers, Xbox gamepad, and the operator phone app" width="100%">
</p>

---

## Features at a glance

| | |
|---|---|
| <img src="docs/assets/manual/11-game-catalog-gamepad.jpg" alt="Game catalog with gamepad navigation" width="320"> | **Game catalog with category tabs.**<br>Touch-, gamepad-, and VR-pointer-friendly. Per-game artwork, Steam/Screen/VR badges, multi-launch options per app. |
| <img src="docs/assets/manual/02-qr-login.jpg" alt="QR-code login screen" width="320"> | **Multiple station login modes.**<br>Operator-driven (staffed), QR-code self-checkout (mobile-paid), or fully remote (operator drives from the back office). |
| <img src="docs/assets/manual/23-vbox-installer.jpg" alt="VBox installer screen showing games on disk" width="320"> | **Self-contained game packages.**<br>One `.vbox` file holds the game binaries, the icon, the launch instructions, and the watchdog rules. Install over USB or push from the server. |
| <img src="docs/assets/manual/22-app-advanced-edit.jpg" alt="Advanced execution-instruction editor with process monitoring options" width="320"> | **Real launch + process control.**<br>Per-app working dir, args, required VR module, multi-process watchdog with "is main", "kill on exit", "kill on hang" flags. |
| <img src="docs/assets/manual/30-hardware-info.jpg" alt="Hardware info screen with disk usage and station mode toggle" width="320"> | **Operator panel on every station.**<br>PIN-protected admin: disk usage, hardware specs, station mode toggle, multimedia background, app library management. |
| <img src="docs/assets/manual/33-skinning.jpg" alt="Skinned game catalog UI" width="320"> | **Skinnable and localized.**<br>Drop-in skin themes, English + 简体中文 out of the box, custom logo and background music per station. |

---

## Quick start

**Server** — one Linux VM, Docker Compose, ~30 seconds to a working stack:

```sh
git clone https://github.com/<your-org>/PlayOnDemand
cd PlayOnDemand
cp .env.example .env                # set JWT_SECRET, POSTGRES_PASSWORD, ADMIN_*
docker compose up -d                # postgres + server + operator UI
curl http://localhost/health        # -> "Healthy"
```

The operator UI is on `http://localhost:8080/` — log in with the admin
credentials from `.env`, then create a station and mint an API key for it.

**Kiosk** — Windows machine, .NET 10 SDK + .NET Framework 4.7.1 Dev Pack
+ VS 2022 Build Tools + Inno Setup:

```cmd
LeapVR.Shell.Build\Build_Free.bat   :: -> dist\LeapPlayInstaller.exe
LeapPlay.Shell.exe -debug           :: ALWAYS use -debug on dev boxes
```

The setup wizard registers the station against the server with the API key
from the operator UI.

Full deployment, hardening, and troubleshooting are in
[`docs/usage/server-deployment.md`](docs/usage/server-deployment.md) and
[`docs/usage/kiosk-known-issues.md`](docs/usage/kiosk-known-issues.md).

---

## Documentation

| For… | Start here |
|------|-----------|
| **Operators / venue staff** — running a venue, managing stations, taking sessions | [📖 Illustrated manual](docs/manual/) |
| **System integrators** — deploying the server, sizing hardware, configuring TLS | [`docs/usage/server-deployment.md`](docs/usage/server-deployment.md) |
| **Engineers** — extending or porting the code | [`docs/architecture/overview.md`](docs/architecture/overview.md) |
| **Anyone curious** — what this repo is, where it came from, what shipped | [`docs/about.md`](docs/about.md) |

The full doc index is at [`docs/README.md`](docs/README.md).

---

## Hardware

PlayOnDemand was originally shipped with custom VR-arcade cabinets, but it
runs on any Windows 10/11 PC with an SteamVR-compatible HMD (HTC Vive,
Valve Index, Oculus / Meta via SteamVR). The screenshots in this README and
the manual were captured on the original LeapVR cabinets — but every screen
is just a WPF window with no hardware dependency.

<p align="center">
  <img src="docs/assets/marketing/hero-hardware-lineup.jpg" alt="Three example VR-arcade cabinet form factors: free-roam, seated cockpit, and treadmill" width="100%">
</p>

---

## Contributing

Issues and PRs welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for the
workflow. Security disclosures go via [SECURITY.md](SECURITY.md), not the
public issue tracker.

## License

Apache 2.0 — see [LICENSE](LICENSE). Bundled third-party software is listed
in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
