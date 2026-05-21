# Pod.DtoModels

> REST request DTOs (`Request*Dto`). One netstandard2.0 leaf assembly that defines every shape a controller deserialises from `[FromBody]`, with `System.ComponentModel.DataAnnotations` validation attributes baked in.

## Purpose

When a REST controller in `Pod.Web.Center/Areas/Api/v1/` accepts a JSON body, the bound type is a `Request*Dto` from this project. Keeping these in their own tiny assembly does three things:

1. **`Pod.Web.Client.Rest`** can reference them directly so the SDK and the server share the exact same shape — no parallel hand-maintained client types.
2. The validation attributes (`[Required]`, `[MinLength]`, `[MaxLength]`, `[EmailAddress]`, `[Range]`) live with the DTOs and are enforced server-side via `ModelState.IsValid` before any service-layer code runs.
3. The DTOs are the **input** half of the REST contract; `Pod.ViewModels` is the **output** half. The two are deliberately separate even when a request and a response would carry the same fields.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:**
  - `System.ComponentModel.Annotations` 4.5.0 — for `[Required]`, `[MinLength]`, `[EmailAddress]`, etc.
- **Project references (in this repo):**
  - `Pod.Enums` — for typed enum properties on DTOs (e.g. `StationControlMode Mode`, `SmtpAuthentication AuthMethod`, `EMailTemplateIdentifier Identifier`)

## Responsibility

**Is responsible for:**
- Declaring REST request body / query parameter shapes
- Field-level validation (length, range, required-ness, email format)
- Carrying enums into the request layer with their `Pod.Enums` types

**Is NOT responsible for:**
- Cross-field validation — that's a service-layer concern (`result.ArgNotEqual(...)` etc.)
- Response shapes — `Pod.ViewModels` owns those
- Authorisation logic — that's controller `[Authorize]` attributes + service checks

## Public API surface

DTOs are grouped one file per domain. Naming convention is invariant: every type ends in `Dto` and (in current code) every type starts with `Request`. There are no `Response*Dto` — the response side is `Pod.ViewModels`.

| File | Notable types |
|---|---|
| `AccountModels.cs` | `RequestRegisterUserDto`, `RequestLoginModelDto` (no — that's in AuthModels), `RequestForgotPasswordDto`, `RequestResendConfirmationEmailDto`, `RequestEmailConfirmationDto`, `RequestResetPasswordDto`, `RequestForgotPasswordInputModel` (Razor binding twin) |
| `AdminModels.cs` | `RequestAddRemoveUserToRoleDto`, `RequestSetSystemSettings` |
| `AuthModels.cs` | `RequestLoginModelDto`, `RequestTokenRefreshDto` |
| `EmailModels.cs` | `RequestCreateEmailAccountDto`, `RequestSetSmtpAuthSettingsDto`, `RequestSetSmtpServerDto`, `RequestSetEmailSenderDto`, `RequestSendEmailDto`, plus template-CRUD DTOs |
| `Server.cs` | `RequestNewServerDto`, `RequestServerDisplayNameUpdateDto`, `RequestServerTimeSettingsUpdateDto`, `RequestServerConnectionSettingsUpdateDto` (ShellServer admin) |
| `StationModels.cs` | `RequestCreateStationDto`, `RequestSetStationPasswordDto`, `RequestStationSettingsDto`, plus QR-code + control-mode DTOs |

## Internal structure

Flat — one file per controller / domain. No subfolders.

```
Pod.DtoModels/
├── AccountModels.cs
├── AdminModels.cs
├── AuthModels.cs
├── EmailModels.cs
├── Server.cs            (note: not "ServerModels.cs" — historical inconsistency)
└── StationModels.cs
```

## Notable patterns / gotchas

- **`Request*Dto` naming convention.** Every public type in this assembly is a request shape. There are no nested types, no helpers, no logic — just plain POCOs with attributes.
- **One historical odd file name:** `Server.cs` (not `ServerModels.cs`). Don't fix it without grepping for the file name elsewhere.
- **Validation attribute lengths are tighter than DB column limits.** This is intentional — the DTO is the front gate, the DB is the back stop. A `[MaxLength(30)]` on `Username` here pairs with a `varchar(50)` in the DB to give some room for migration.
- **Enums on DTOs** (e.g. `StationControlMode Mode`, `SmtpAuthentication AuthMethod`) flow through the Newtonsoft `StringEnumConverter` configured in `Pod.Web.Center/Startup.cs`. Wire format is the enum's **string name**, not its int value. So renaming an enum value breaks REST clients — the rename is the same wire-breaking change as on `Pod.ViewModels`.
- **`RequestForgotPasswordInputModel` exists alongside `RequestForgotPasswordDto`** because the Razor account portal binds the same data with different display attributes. They are intentionally separate types.
- **No DTO inherits from another.** Inheritance hierarchies confuse Swashbuckle's schema generation and produce odd `allOf` shapes — keep them flat.
- **`SchemaIdStrategy.RemoveModelSufixStrategy`** in `Pod.Web.Center/Swagger/SchemaIdStrategy.cs` strips the `Dto` suffix from Swagger model names, so the published API doc shows `RequestLoginModel`, not `RequestLoginModelDto`. The `Dto` suffix exists only inside the .NET codebase.

## Consumers

Direct project references:

- `Pod.Web.Center` — controllers `[FromBody]` bind to these
- `Pod.Web.Client.Rest` — REST SDK builds requests from these
- `Pod.Web.Client.Rest.Internal` — internal endpoint extensions

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.ViewModels/`](../Pod.ViewModels/) — the response-side counterpart
- [`docs/server/Pod.Web.Client.Rest/`](../Pod.Web.Client.Rest/) — the SDK that pairs request DTOs with HTTP calls
