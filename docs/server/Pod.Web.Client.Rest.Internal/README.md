# Pod.Web.Client.Rest.Internal

> Tiny extension-method-only project that adds bindings for the `api/v1/internal/*` REST endpoints (admin, support, server-management) onto the existing `PodRestClient`.

## Purpose

The public REST API (`api/v1/...`) is served as the `v1` Swagger document and is what the SDK in `Pod.Web.Client.Rest` covers. There is also an **internal** API surface (`api/v1/internal/...`) used by the operator portal and support staff — role assignment, ShellServer registration, looking up other users' data, etc. — served as a separate `v1_internal` Swagger document.

Rather than fork `PodRestClient`, this project just adds extension methods on the same client class. A consumer adds a single project reference and gets `client.Admin().RolesGetAll()` alongside `client.Stations().Get()` from the public SDK. No separate authenticator, no separate base URL — same JWT, same `PodRestClient` instance.

The split exists so external integrators (who reference only `Pod.Web.Client.Rest`) cannot accidentally call internal endpoints, and so the published OpenAPI surface stays clean. If you can see this assembly in your `using`s, you're inside the operator/support world.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:** none (RestSharp comes transitively via `Pod.Web.Client.Rest`)
- **Project references (in this repo):**
  - `Pod.DtoModels` — request shapes for internal endpoints
  - `Pod.ViewModels` — response shapes
  - `Pod.Enums` — for typed enums in payloads
  - `Pod.Web.Client.Rest` — the `PodRestClient` being extended

## Responsibility

**Is responsible for:** wrapping every `api/v1/internal/*` REST endpoint as an extension method on `PodRestClient`.

**Is NOT responsible for:**
- Any auth concern — reuses `PodAuthenticator` from the public SDK
- Public endpoints — those live in `Pod.Web.Client.Rest`
- Any logic beyond constructing `IRestRequest` objects

## Public API surface

Same area-marker pattern as the public SDK. Three groups:

| Marker | File | Endpoints exposed |
|---|---|---|
| `client.Admin()` | `AdminEx.cs` | `RolesGetAll`, `RolesGetUser(username)`, `RolesAddToUser`, `RolesRemoveFromUser`, plus system-settings + email-template management |
| `client.Servers()` | `ServerEx.cs` | `Get` (all ShellServers), `Get(serverId)`, `GetConnectedStations(serverId)`, `Create(newServerDto)`, `SetDisplayName`, time-settings + connection-settings updates |
| `client.Support()` | `SupportEx.cs` | `UsersGetAll(skip, take)`, `UsersGetByEmail(email)`, `UsersGetStations(userId)`, `StationsGetConnections(stationId)` |

All routes start with `api/v1/internal/` — that's the marker that opts into the `v1_internal` Swagger document on the server side.

## Internal structure

Flat — three files, one per area.

```
Pod.Web.Client.Rest.Internal/
├── AdminEx.cs       client.Admin().*
├── ServerEx.cs      client.Servers().*
└── SupportEx.cs     client.Support().*
```

## Notable patterns / gotchas

- **Consumers add `using Pod.Web.Client.Rest.Internal;`** to a file to make the area markers visible. Without the `using`, the methods don't show up in IntelliSense — this is the *de facto* opt-in.
- **Naming convention `*Ex` suffix on file names** is shorthand for "extensions" and matches no other folder in the repo (the public SDK uses `Api*Extensions`). Historical.
- **`client.Admin()` returns `client`** — pure marker, no state, no wrapper. Same trick as the public SDK.
- **All routes start `api/v1/internal/`**, but URL casing varies — `api/v1/internal/Admin/roles` (capital A), `api/v1/internal/support/users` (lowercase). The server forces lowercase routing so it works either way; the inconsistency is a code smell.
- **No validation that the caller has internal permissions client-side.** The server enforces it via `[Authorize(Roles = "Admin")]` (or similar) on the controllers in `Pod.Web.Center/Areas/Api/v1/`. A non-admin JWT will get a 403 — `response.IsSuccessful` will be `false`, `response.GetErrors()` will return the error dictionary.

## Consumers

No tracked consumers in-tree. The legacy stress-test driver that previously referenced this project was removed; the SDK is still kept available for downstream administrative tooling. The operator portal in `Pod.Web.Center/Pages/` does not consume this — it talks to its services directly via DI rather than through HTTP.

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.Web.Client.Rest/`](../Pod.Web.Client.Rest/) — the public SDK this extends
- [`docs/server/Pod.Web.Center/`](../Pod.Web.Center/) — the matching `*Controller` files in `Areas/Api/v1/` (search for `[Route("api/v1/internal/...")]`)
