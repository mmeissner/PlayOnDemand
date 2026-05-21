# Admin tasks (CLI / config)

> Common operator/admin tasks that live below the Flutter operator-frontend UI.

These are the recipes for rotating secrets, seeding users, regenerating certs, and otherwise touching server state without going through the operator UI. All commands assume the Docker Compose deployment (`docker compose ...` from the repo root). For a non-Docker deployment, run the underlying `dotnet`/`psql` commands directly.

## Rotate the JWT signing key

Invalidates every currently-issued operator access + refresh token. Operators have to log in again.

```sh
# 1. Generate a new 32-byte ASCII secret
openssl rand -base64 32 | tr -d '/+' | cut -c1-44

# 2. Update .env
sed -i 's/^JWT_SECRET=.*/JWT_SECRET=<paste-new-secret-here>/' .env

# 3. Restart the server
docker compose up -d --force-recreate server
```

`docker compose logs -f server` should show the server come back up cleanly.

## Rotate the admin password

The admin user is identified by `ADMIN_EMAIL` from `.env`. Three options:

**Option A — operator UI (preferred).** Log in as the admin → Account → Change Password. Uses the standard ASP.NET Identity flow.

**Option B — env-var reseed (admin locked out).** Stop the server, set a fresh `ADMIN_PASSWORD` in `.env`, delete the existing admin user from Postgres so `DbSetupUsers` reseeds on next boot:

```sh
docker compose exec postgres psql -U "${POSTGRES_USER:-pod_user}" -d "${POSTGRES_DB:-pod}" \
    -c "DELETE FROM \"AspNetUserRoles\" WHERE \"UserId\" IN (SELECT \"Id\" FROM \"AspNetUsers\" WHERE \"Email\" ILIKE '${ADMIN_EMAIL}');" \
    -c "DELETE FROM \"AspNetUsers\" WHERE \"Email\" ILIKE '${ADMIN_EMAIL}';"
docker compose up -d --force-recreate server
```

**Option C — Postgres direct.** If you have a long admin password and just want to rotate it from a script: not recommended, because you'd have to recompute the PBKDF2-V3 hash with the right parameters (`Pod.Data/PasswordHasher.cs`). Use Option A or B.

## Force a Let's Encrypt cert renewal

The renewal job runs `LetsEncryptOptions.DaysBefore` days before expiry (default 20). To force one earlier:

```sh
docker compose exec server rm -rf /app/certs
docker compose restart server
```

The `letsencrypt-data` volume is now empty; on next start the server re-registers an ACME account, completes the HTTP-01 challenge, and persists a fresh chain. Confirm the new cert:

```sh
echo | openssl s_client -connect <POD_HOSTNAME>:443 -servername <POD_HOSTNAME> 2>/dev/null \
    | openssl x509 -noout -dates -issuer
```

## Mint a station + API key for a kiosk (without the operator UI)

End-to-end provisioning via curl. Replace placeholders with real values.

```sh
# 1. Log in as the admin to get a JWT.
TOKEN=$(curl -fkS -X POST https://localhost/api/v1/auth/login \
    -H 'Content-Type: application/json' \
    -d '{"username":"superuser","password":"<admin-password>"}' \
    | jq -r '.accessToken.token')

# 2. Create a station.
STATION_ID=$(curl -fkS -X POST https://localhost/api/v1/Stations \
    -H "Authorization: Bearer $TOKEN" \
    -H 'Content-Type: application/json' \
    -d '{"displayName":"Booth-01","password":"<strong-station-password>"}' \
    | jq -r '.stationId')

# 3. Mint a (PublicKey, Secret) pair for the new station.
curl -fkS -X PUT "https://localhost/api/v1/Stations/$STATION_ID/apikeys?keyName=primary" \
    -H "Authorization: Bearer $TOKEN" \
    | jq
# Output includes both publicKey and secret. The secret is shown ONCE; save it.

# 4. Hand the kiosk both pairs:
#    - (StationId, Station.Password) for gRPC metadata.
#    - (PublicKey, Secret) for REST amx HMAC.
```

## Tail logs

```sh
docker compose logs -f server               # server only
docker compose logs -f postgres             # postgres only
docker compose logs -f                      # both, interleaved
```

NLog writes structured-ish lines to stdout; the container's `json-file` log driver captures them. If you ship logs off-host, configure the Docker daemon's `log-driver` (e.g. `syslog`, `journald`, `fluentd`).

## Postgres backup + restore

```sh
# Backup (writes a custom-format dump that's restorable across PG minor versions)
docker compose exec postgres pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -F c -f /tmp/pod.dump
docker compose cp postgres:/tmp/pod.dump ./backups/pod-$(date +%Y%m%d-%H%M).dump

# Restore (drops + recreates the schema)
docker compose cp ./backups/pod-20260516-1200.dump postgres:/tmp/pod.dump
docker compose exec postgres pg_restore --clean --if-exists --no-owner \
    -U "$POSTGRES_USER" -d "$POSTGRES_DB" /tmp/pod.dump
docker compose restart server
```

## Stop + remove everything (clean slate)

```sh
docker compose down -v          # drops the named volumes too — DESTROYS the DB and LE cert
rm .env                         # the secrets aren't in version control; you have the only copy
```

The `letsencrypt-data` volume is the only durable LE state; deleting it forces a fresh ACME registration on the next start (you keep the same 20-cert-per-week quota window with Let's Encrypt). The kiosk side keeps using whatever `(StationId, Password)` and API key it was provisioned with; those are server-side records, so wiping the DB invalidates them too — every kiosk needs to be re-provisioned through the Setup wizard.

## See also

- [server-deployment.md](server-deployment.md) — initial setup, `.env` reference, healthcheck semantics.
- [kiosk-known-issues.md](kiosk-known-issues.md) — kiosk build + the `-debug` safety rule.
- [operator-frontend.md](operator-frontend.md) — Flutter app state + planned integration.
- [`docs/architecture/auth.md`](../architecture/auth.md) — auth scheme details for the curl examples above.
