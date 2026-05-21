# Pod.ViewModels

> REST response types (`*ViewModel`). The output half of the REST contract — what controllers and the operator portal hand back to clients.

## Purpose

Every JSON object the API returns is one of these. Keeping response shapes in their own assembly mirrors the request side (`Pod.DtoModels`) and lets `Pod.Web.Client.Rest` parse responses into the exact same types the server emits.

The split between `Pod.ViewModels` and `Pod.ViewModels.Expressions` is deliberate: this assembly carries the **shapes** (POCOs), the sibling project carries the **projections** (`Expression<Func<TEntity, TViewModel>>` lambdas) used by `Pod.Services` to project EF queries straight into view models without materialising entities. Splitting the two means `Pod.Web.Client.Rest` can reference just the shapes (no `Pod.Data.Models` pollution downstream into the SDK).

The `Pod.ViewModels.Customer.SubscriptionState` enum is the only enum living here rather than in `Pod.Enums` — it is purely a "shape of the response" thing and never appears in a DB column or gRPC contract.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:** none (other than implicit netstandard)
- **Project references (in this repo):**
  - `Pod.Enums` — for typed enum properties (`SessionState`, `NetworkState`, `StationControlMode`, `RequestSource`, `StopReason`, `EMailTemplateIdentifier`, `SmtpAuthentication`, `CurrencyIsoCode`)
  - `Pod.Data.Models` — needed by a small number of view models that surface a Data.Models enum directly (e.g. `EmailVariableType` originally lived in `Pod.Data.Models` before being moved)
  - `Pod.ViewModels.Expressions` references this project, not the other way around

## Responsibility

**Is responsible for:** declaring response shapes (`*ViewModel`) returned by REST controllers and surfaced to the operator portal.

**Is NOT responsible for:**
- Mapping entities → view models — `Pod.ViewModels.Expressions` does that, with EF-translatable lambdas
- Validation of any sort
- Holding logic — these are dumb POCOs

## Public API surface

Grouped by audience folder:

### `Admin/`
- `UserRoleViewModel` — `{ Name }`. Roles assigned to a user.
- `SystemSettingsViewModel` — `{ UserRegistrationEnabled, MaxStationsPerUser }`.

### `Auth/`
- `AccessTokenViewModel` — `{ Token, ExpiresIn }`. The JWT bearer token + lifetime.
- `LoginResponseViewModel` — `{ AccessToken, RefreshToken }`. Returned by `auth/login`. The refresh token is long-lived and is invalidated by `auth/logout` (only — the access token stays valid until expiry).

### `Customer/`
- `UserViewModel` — `{ Id, Username, CustomerNumber, EmailAddress, EmailConfirmed, IsLockedOut, StationCount }`. The end-customer view of an `ApplicationUser`.
- `SessionViewModel` — `{ SessionId, Reference, State, StartedOnUtc, StartDuration, MaxDurationLimit }`. A single live session.
- `SessionLogViewModel` — historical session record with `RequestedBy`, `LatestState`, `StartedUtc`, `EndedUtc`, `StoppedBy`, `MaxDurationLimit`, `ChangeRequests`.
- `StationCurrentStateViewModel` — `{ StationId, DisplayName, ControlMode, NetworkState, Session }`. The "what is this station doing right now" shape.
- `StationSettingsViewModel` — `{ StationId, DisplayName, QrCode, ControlMode }`.
- `StationSubscriptionViewModel`, `UserRequestSubscriptionOrderViewModel`, `SubscriptionPaymentViewModel`, `SubscriptionOrderViewModel`, `SubscriptionOrderBasicViewModel` — billing-side surfaces from `BillingViewModels.cs`.
- `StationConnectionLogViewModel`, `ChangeRequestViewModel`, `SessionRuleViewModel` — auxiliary log/detail shapes.
- `SubscriptionState` enum — `Inactive | Active | Expired`. **The only enum local to this project**, all others come from `Pod.Enums`.

### `Mail/`
- `EMailAccountViewModel` — full SMTP/Gmail account view including `AssignedTemplates`.
- `EMailTemplateViewModel` — `{ Id, DisplayName, Identifier }`.
- `EMailTemplateDetailsViewModel` — extended template view with subject/body/variables.
- `EmailTemplateInfo` — `{ Identifiers, VariableKeys }`. The "what's available" lookup endpoint.

### `ShellServer/`
- `ShellServerViewModel` — operational ShellServer summary.
- `ShellServerDetailsViewModel` — operational + creation date + connected-client count.
- `ShellServerConnectedClientViewModel` — `{ StationId, State, ServerRequestOn, ConnectedOnUtc, LastHeartBeatOnUtc, DeviceIdentity, ConnectionId }`.

### `User/`
- `RegisterUserViewModel` — `{ UserId, Username, EMail, EMailVerificationToken }`. Returned by registration; carries the token the user receives via email.
- `UserForgotPasswordViewModel` — equivalent for the password-reset flow.

## Internal structure

```
Pod.ViewModels/
├── Admin/        AdminViewModels.cs
├── Auth/         AuthViewModels.cs
├── Customer/     BillingViewModels.cs, StationViewModels.cs, UserViewModel.cs, ViewModelEnums.cs
├── Mail/         EMailViewModels.cs
├── ShellServer/  ShellServerViewModels.cs
└── User/         UserViewModels.cs
```

## Notable patterns / gotchas

- **`*ViewModel` suffix on every type** — Swagger drops the suffix via `SchemaIdStrategy.RemoveModelSufixStrategy` (in `Pod.Web.Center/Swagger/`), so the published OpenAPI shows `User` not `UserViewModel`.
- **Wire format for enums is the string name**, via `StringEnumConverter` configured in `Pod.Web.Center/Startup.cs`. Renaming a value on `Pod.Enums.*` is a breaking REST change.
- **`SubscriptionState` lives here, not in `Pod.Enums`.** Reason: it is a synthesised "view of state" derived from billing entities — it never gets stored, never crosses gRPC, and never appears in `Pod.DtoModels`. Putting it in `Pod.Enums` would suggest those things.
- **`UserViewModel.StationCount`** is computed in the projection (`ToUserVm.FromApplicationUser()` in `Pod.ViewModels.Expressions`) via `x.Stations != null ? x.Stations.Count() : 0`. The note `//Do not optimize Count as it will otherwise not work` is real — `x.Stations.Count` (property) breaks EF translation; `x.Stations.Count()` (method) generates the right SQL.
- **`StationCurrentStateViewModel.Session`** is `null` when the station has no active session; clients must null-check.
- **No inheritance.** Same reasoning as DTOs — Swashbuckle gets confused.
- **Don't add `[JsonIgnore]` or `[JsonProperty]` here.** The project intentionally has no Newtonsoft.Json reference; serialisation policy is centralised in `Pod.Web.Center/Startup.cs` (`CamelCasePropertyNamesContractResolver`, `StringEnumConverter`).
- **DateTime is always UTC.** Property names end in `Utc` (`StartedUtc`, `ConnectedOnUtc`) — the contract is "all timestamps in this assembly are UTC". Consumers should treat naked `DateTime` properties without `Utc` suffix as a code smell to file an issue against.

## Consumers

Direct project references:

- `Pod.Web.Center` — controller return types
- `Pod.Web.Client.Rest`, `Pod.Web.Client.Rest.Internal` — SDK response types
- `Pod.Services` — return types of every `*Service` method
- `Pod.ViewModels.Expressions` — projects entities into these shapes

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.DtoModels/`](../Pod.DtoModels/) — request-side counterpart
- [`docs/server/Pod.ViewModels.Expressions/`](../Pod.ViewModels.Expressions/) — the EF-translatable projections that produce these
