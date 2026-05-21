# Security Policy

## Reporting a vulnerability

PlayOnDemand is a small project and currently has no dedicated security team. If you discover a security issue:

1. **Do not** open a public GitHub issue. Vulnerability reports are processed privately to allow a fix before public disclosure.
2. Email the maintainers at **security@example.com** (replace with your fork's contact). Include:
   - A description of the issue and the affected component (kiosk, server, content-creator).
   - Reproduction steps or a proof-of-concept.
   - Your assessment of impact (confidentiality / integrity / availability) and exploitability.
3. We aim to acknowledge reports within 7 days and to publish a fix or workaround within 30 days for high-severity issues.

If you maintain a fork or downstream deployment, please rewrite the contact address in this file before publishing.

## Supported versions

This project is being prepared as a one-shot open-source release; no version tag has been cut yet. The first tag (planned: `v1.0.0`) will be created as the final action on the repo at the maintainer's call. Once tagging starts, security fixes will land on the main branch and the next minor tag — older tags will not be patched in place. Pin to a tag, not to a branch, in production.

## Hardening notes for operators

- **Secrets at rest**: `Pod.Web.Center/appsettings.json` ships with placeholder values for every secret. Override via `dotnet user-secrets` (development) or environment variables (`DOTNET_<Section>__<Key>` form) in production. The Docker Compose deployment template (`.env.example`) lists the env vars you need to set.
- **JWT signing key**: `AuthConfig.SecretKey` must be at least 32 ASCII bytes of high-entropy data. If you rotate the key, all currently-issued operator tokens are invalidated; clients re-authenticate.
- **Postgres credentials**: prefer a role with only the privileges PoD needs (own its schema; no superuser). The first-run bootstrap requires CREATE TABLE; you can revoke that after migrations apply.
- **Let's Encrypt account key**: `LetsEncryptOptions.EncryptionPassword` encrypts the persisted ACME account key inside `<cache-folder>/account.json`. Rotating this password requires re-registering with Let's Encrypt (handled automatically on next run after deletion of the cached key).
- **Station credentials**: stations authenticate to the gRPC server with `(StationId, Station.Password)` metadata pairs over TLS, and to the REST surface with `(StationApiKey.PublicKey, StationApiKey.Secret)` via the `amx` HMAC scheme. Treat both pairs as long-lived secrets — rotate via the operator UI if you suspect compromise.
- **Operator session lifetimes**: `RefreshAccessTokenProviderOptions.TokenLifespan` defaults to 20 years (`7300:00:00:00`) for kiosk-like UX. Shorten it if your threat model needs more aggressive revocation.

## Out of scope

- Vulnerabilities in third-party dependencies that are tracked upstream (NuGet, FFmpeg, Unity Lounge runtime, OpenVR). Please report those to the upstream project; we'll bump the dependency once they ship a fix.
- Issues that require an attacker to already have shell access on the kiosk or server machine.
- Denial-of-service via raw request volume. Rate limiting is provided by `AspNetCoreRateLimit`; tune `appsettings.json` if defaults are insufficient.
