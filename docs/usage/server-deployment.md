# Server deployment

This page covers the canonical Docker Compose flow for self-hosting `Pod.Web.Center` on a single Linux VM. The same compose file works on Docker Desktop for Windows/macOS for local development.

## Prerequisites

- Docker Engine 24+ and Docker Compose v2 (i.e. the `docker compose` plugin, not the older `docker-compose` binary).
- A DNS name pointing at the host's public IP if you want Let's Encrypt to issue a real certificate. Without a public DNS name the server falls back to Kestrel's dev cert and the kiosk will refuse to connect.
- Ports `80` and `443` reachable from the public internet for the ACME challenge.

## Deploy in 10 minutes

```sh
git clone https://github.com/<your-org>/PlayOnDemand
cd PlayOnDemand
cp .env.example .env
$EDITOR .env                                 # fill in every variable marked `# required`
docker compose up -d --build                 # first run takes ~3 minutes for restore + publish
docker compose logs -f server                # watch for "Now listening on: https://[::]:443"
curl -fkS https://localhost/health           # expect HTTP 200 with body "Healthy"
```

`docker compose down` stops everything. `docker compose down -v` also drops the Postgres + Let's Encrypt volumes — destructive, only for clean-slate reinstalls.

## .env reference

`.env.example` is exhaustive; the short version:

| Variable | What it does |
|---|---|
| `POSTGRES_PASSWORD` | Postgres role password. Pick a strong random string. |
| `POSTGRES_USER`, `POSTGRES_DB` | Override only if you must collide with an existing role/DB name. |
| `ADMIN_EMAIL`, `ADMIN_PASSWORD` | First-run admin seed. The admin can log in to the operator UI at `https://<host>/`. Identity policy: digit + lower + upper + non-alphanumeric, length ≥ 10. |
| `STATION_PASSWORD` | Default station-side password baked into newly-created stations until the operator rotates it. |
| `JWT_SECRET` | At least 32 ASCII bytes of high-entropy data. Rotating it invalidates every currently-issued operator token (clients re-authenticate). |
| `POD_HOSTNAME` | The DNS name the server is reachable at. Also used as the Let's Encrypt SAN and the JWT audience. |
| `LETSENCRYPT_ENABLED` | `true` requests a real cert on first run. `false` falls back to the Kestrel dev cert (kiosk will reject it). |
| `LETSENCRYPT_ACCEPT_TOS` | Must be `true` for `LETSENCRYPT_ENABLED=true`. Indicates you have read the LE Subscriber Agreement. |
| `LETSENCRYPT_EMAIL` | LE uses this for expiry-warning emails. Use an inbox you actually read. |
| `LETSENCRYPT_ENCRYPTION_PASSWORD` | Encrypts the persisted ACME account key on disk under `/app/certs`. Treat as a secret. |

## What the first run does

1. `postgres` container comes up. The healthcheck (`pg_isready`) waits until accepting connections.
2. `server` container starts, blocked on the postgres healthcheck.
3. `Pod.Web.Center` boots:
   - Reads config from env-vars (the `__` separator becomes `:` — `AuthConfig__SecretKey` → `AuthConfig:SecretKey`).
   - `ContextInitializer.Initialize()` runs EF migrations against the empty Postgres DB.
   - `DbSetupUsers`, `DbSetupShellServer`, `DbSetupEmail` execute in priority order:
     - admin user created from `ConfigSuperuser__{Username,Email,Password}`.
     - `ShellServer` row inserted from `ConfigShellServer__*`.
     - default email templates inserted.
4. Kestrel binds `:80` and `:443`. If LE is enabled, the ACME challenge endpoint is mounted at `/.well-known/acme-challenge/*` on `:80` and a self-signed transitional cert is served on `:443` until the first issuance completes.
5. The docker healthcheck (`curl -fkS https://localhost/health`) hits `/health` and reports back to Docker's restart policy.

## Operator-frontend integration

The Flutter operator UI is wired in as the `operator` service of [`docker-compose.yml`](../../docker-compose.yml). nginx serves the static `flutter build web --release` output at `:8080/` and proxies `/api/*`, `/swagger/*`, `/health` to the `server` container. Same-origin → no CORS to configure.

Visit `http://localhost:8080/` after `docker compose up -d`. See [`operator-frontend.md`](operator-frontend.md) for the build options (host-side `flutter build web` + nginx, or fully-containerised via `ghcr.io/cirruslabs/flutter`).

## Hardening checklist for production

- Tighten Postgres: replace the default `pod_user` role with a dedicated role that only has `CREATE`/`SELECT`/`INSERT`/`UPDATE`/`DELETE` on the `pod` database, no superuser, no `CREATE EXTENSION`.
- Use `dotnet user-secrets` or a real secret store (HashiCorp Vault, AWS Secrets Manager, Azure Key Vault) instead of `.env` on disk. Mount the secrets as env vars at container start.
- Move `IpRateLimiting` rules from defaults to whatever your traffic shape demands. The defaults are sane for a single-site arcade; multi-site needs more aggressive `Endpoint=*` caps.
- Keep `LETSENCRYPT_ENABLED=true` in any internet-facing deployment. The kiosk's TLS validation rejects untrusted CAs.
- Configure off-host log shipping (the server writes via NLog to `stdout`; let Docker's `json-file` or `journald` driver capture it).
- Schedule Postgres backups against the named volume `postgres-data`.

## Common failures

| Symptom | Cause |
|---|---|
| `POSTGRES_PASSWORD is required in .env` at `docker compose up` | Empty value for a `:?` required env var. Fill in `.env`. |
| Server keeps restarting, logs show `Connection refused` to Postgres | `postgres` container hasn't passed healthcheck yet. Wait 30 s; if it persists, `docker compose logs postgres` to diagnose the DB. |
| `/health` returns 503 with body `"Unhealthy"` and `description="postgres"` | Server can talk to itself but not to Postgres. Inspect the `ConnectionStrings__PodApiContext` env var inside the container with `docker compose exec server env | grep ConnectionStrings`. |
| Let's Encrypt challenge fails | DNS doesn't resolve `POD_HOSTNAME` to this host yet, or port 80 is blocked. Curl `http://<POD_HOSTNAME>/.well-known/acme-challenge/test` from another host to confirm reachability. |
| Operator login returns 401 with no body | JWT_SECRET changed since the operator last logged in. Re-login. |
