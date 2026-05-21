# Pod.MailEngine

> Templated transactional email engine. Builds `MimeMessage`s from DB-stored `EmailContentTemplate`s with variable substitution, then sends via SMTP (MailKit) or Gmail OAuth2.

## Purpose

The server-tier piece responsible for turning an `EmailContentTemplate` row + a dictionary of variable values into an actual sent email. Used for system flows: account registration confirmation, email-verification resend, forgot-password reset.

The engine is deliberately split from `Pod.Services.Email`. `Pod.Services.Email.EMailService` deals with **what** to send and **when** (queueing `EmailOrder` rows, scheduling, retry semantics). `Pod.MailEngine` deals with **how** to render and transmit a single message: variable substitution, choosing SMTP-vs-OAuth2, MailKit's `SmtpClient`. The two-layer split lets the higher layer stay synchronous-DB-centric while the engine can do the slow network I/O.

Two account types are supported. `SmtpAccount` (default) handles regular SMTP with `MailKit` username/password auth. `GMailAccount` handles Gmail-flavoured OAuth2 via `Google.Apis.Auth` — the factory routes any `@gmail.com` / `@googlemail.com` address to the latter.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:**
  - `MailKit` 2.1.5.1 — SMTP client (the actual wire-level send)
  - `Google.Apis.Auth` 1.40.0 + `Google.Apis.Gmail.v1` 1.40.0.1572 — Gmail OAuth2 flow
  - `Microsoft.Extensions.Hosting.Abstractions` 2.1.1 — for `ILogger<T>` only (the engine doesn't host anything itself)
- **Project references (in this repo):**
  - `Pod.Data.Infrastructure` — `IResult<T>`, `Result`, validator extensions
  - `Pod.Data.Models` — `EmailContentTemplate`, `EmailVariable`, `IEMailAccountData`, `IContentTemplateVariable`
  - `Pod.Enums` — `UserError`, `TemplateVariableKey`, `EmailReceiverType`, `EmailVariableType`, `SmtpAuthentication`

Note: targets `netstandard2.0`, not `netcoreapp2.1` — the engine is theoretically reusable from .NET Framework. There is no current consumer doing that, but keep the constraint in mind when adding dependencies.

## Responsibility

**Is responsible for:**
- Rendering an `EmailContentTemplate` (subject + text body + HTML body) into a `MimeMessage` with all `%VariableKey%`-style placeholders replaced
- Connecting and authenticating to the SMTP server
- Sending one or many messages over a single connection (`SendMail` opens the connection once, sends all, disconnects)
- Detecting unset / missing variables and reporting them as `Result` errors instead of leaking placeholder text into outgoing mail
- Parsing arbitrary text for variable placeholders (`VariableParser` — used by the operator UI to validate/preview templates)

**Is NOT responsible for:**
- Storing email orders or templates — `Pod.Data.Models.Mail.*` does that, persisted by `Pod.Data`
- Scheduling sends — `SendEmailServiceHosted` (in `Pod.Web.Center/ServicesHosted/`) ticks every 90 s and calls `EMailService.SendEmailOrders`
- Resolving system variables (e.g. `WebHostRoot`, `EMailVerificationTokenLink`) — `Pod.Services.Email.EmailVariableHelper` does that and feeds the dictionary down
- Attachments — there is currently no attachment support in the engine

## Public API surface

| Type | Visibility | Purpose |
|---|---|---|
| `EMailTemplate` | public | Wraps an `EmailContentTemplate`. Add receivers via `AddReceiver(...)`, set variable values via `SetOrReplaceVariable(...)`, then hand to a sender. |
| `EMailTemplateSenderFactory` | public | Registered as singleton in DI. `Create(IEMailAccountData)` inspects the account, returns a `SmtpAccount`-or-`GMailAccount`-backed `IEmailTemplateSender`. |
| `IEmailTemplateSender` | public (in `.Interfaces`) | Two `SendEmailAsync` overloads (single template / collection); both return `IResult`. |
| `VariableParser` (`IVariableParser` in `Pod.Data.Models.Interfaces`) | public | Scans a text for variable placeholders given a control char. Used by the operator UI to highlight/validate template content. |
| `IEmailAccount` | **internal** | Implementation detail — `SmtpAccount` and `GMailAccount` implement this. Don't add a third option without also extending `EMailTemplateSenderFactory.Create`. |
| `EMailTemplateSender` | **internal** | The thing returned by the factory. Has `_logger` + `_emailAccount` only. |

## Internal structure

```
Pod.MailEngine/
├── EMailTemplate.cs                 The template wrapper. Owns receivers + variable
│                                    dictionary. Internal BuildMail() returns IResult<MimeMessage>.
├── EMailTemplateSender.cs           internal IEmailTemplateSender implementation.
│                                    SendMail(...) loops messages over one connection.
├── EMailTemplateSenderFactory.cs    public factory. Routes to GMailAccount/SmtpAccount
│                                    based on email domain + auth method. IsAccountDataComplete()
│                                    front-loads validation as IResult errors.
├── EmailAccount.cs                  internal SmtpAccount + GMailAccount, both IEmailAccount.
│                                    SmtpAccount: MailKit AuthenticateAsync(user, pw).
│                                    GMailAccount: GoogleWebAuthorizationBroker + SaslMechanismOAuth2.
├── VariableParser.cs                public scanner: walks a string looking for
│                                    %VariableKey% spans (control char configurable).
├── Interfaces/
│   ├── IEmailTemplateSender.cs      the public sender contract
│   └── IEmailAccount.cs             internal account contract
└── README_SSL.txt                   note about Linux SSL_CERT_DIR/SSL_CERT_FILE for cert chains
```

## Notable patterns / gotchas

- **`IResult<T>` propagates from `EMailTemplate.BuildMail` → `EMailTemplateSender.SendMail` → `IEmailTemplateSender.SendEmailAsync`.** Build failures (missing variable, no receiver) and send failures (SMTP exception) all surface as `IResult` with a `UserError.EmailSendFailure` / `UserError.TemplateInvalidVariable` code — never thrown.
- **One connection per `SendEmailAsync(IEnumerable<EMailTemplate>, ...)` call.** Messages are batched into a `HashSet<MimeMessage>`, the SMTP client connects once, sends all, then disconnects. Use the collection overload when sending many to the same SMTP server.
- **`client.ServerCertificateValidationCallback = (_, _, _, _) => true`** in both `SmtpAccount` and `GMailAccount` — the engine accepts **any** server certificate. This is intentional but worth knowing if you're hardening.
- **Variable substitution is `StringBuilder.Replace`, not regex.** The placeholder format is `<controlChar><VariableKeyString><controlChar>`. The control char is per-template (`emailContentTemplate.VariableControlChar`), typically `%`. A missing variable is logged as `result.Add($"...{variable.VariableKey} is not set...")` and aborts the send.
- **`VariableParser.Parse` lets the operator UI detect what variables a draft template references** *before* saving — so the UI can show "you used `%Bogus%` but no such variable exists". It returns positions + lengths, not just names, so the UI can highlight in-place.
- **`GMailAccount.GetGoogleCredentialsAsync` calls `GoogleWebAuthorizationBroker.AuthorizeAsync`** which can open a browser window for consent. This is a real concern if the engine ever runs headless — currently it only runs after operator-initiated configuration. There is no token cache outside what `GoogleWebAuthorizationBroker` provides by default (`%AppData%\Google.Apis.Auth\...`).
- **Engine logs via `ILogger<IEmailTemplateSender>`.** Not via the closed concrete type. If you change that, downstream NLog filters in `Pod.Web.Center/nlog.config` will silently lose entries.
- **`netstandard2.0`** keeps the engine theoretically multi-runtime; do not drag in a `Microsoft.AspNetCore.*` reference.

## Consumers

Direct project references:

- `Pod.Services` — `Pod.Services.Email` injects `EMailTemplateSenderFactory`, `IVariableParser`; `Pod.Web.Center` registers them in DI

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.Services/`](../Pod.Services/) — `EMailService` (queueing) + `EmailVariableHelper` (system variable resolution)
- [`docs/server/data/Pod.Data.Models/`](../data/Pod.Data.Models/) — `EmailContentTemplate`, `EmailVariable`, `IEMailAccountData`
