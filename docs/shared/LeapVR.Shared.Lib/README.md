# LeapVR.Shared.Lib

> The repo's only `netstandard2.0` library — pure C# helpers, sanity-check
> framework, expression utilities, and an x509/RSA toolkit. Zero WinAPI, zero UI.

## Purpose

This is the lowest layer in the shared tier. Everything in here is supposed to
compile against `netstandard2.0` so it can be referenced by both the .NET
Framework 4.7.1 client (`LeapVR.Shell`, `LeapVR.Content.Creator`) and — in
principle — a future cross-platform consumer. In practice today it is consumed
only by client-side and content-creator projects; the ASP.NET Core 2.1 server
does not link it.

The library bundles three independent toolkits that just happened to land in the
same assembly: a small set of generic LINQ-style extensions and helpers
(`QuickLeap.*`), a declarative input-validation framework (`SanityCheck<T>` +
`SanityRule<T>`), and an x509 / RSA helper used for the gRPC certificate
identity scheme (`ClientIdentity`, `Crypto`). The original `_ReadMe.txt`
describes it as "Helper Classes & Utilities that are Platform independent".

## Tech

- **Target framework:** `netstandard2.0`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Platforms:** `AnyCPU`, `x64`
- **Key NuGet packages:**
  - `Microsoft.Extensions.Logging` 2.1.1 — abstraction so the lib doesn't bind
    directly to NLog
  - `NLog` 4.5.11 — used by `ClientIdentity` to log identification failures
- **Project references (in this repo):** none. This is a leaf project.

## Responsibility

It IS responsible for:

- Generic helpers that never need WinAPI or WPF (string, datetime, enumerable,
  task, type extensions).
- The `SanityCheck<T>` / `SanityRule<T>` validation DSL used for input checking
  in business code.
- Loading PEM/ASN.1-encoded RSA private keys (`Crypto.DecodeRsaPrivateKey`) and
  parsing the LeapVR-flavoured X.509 Common Name format
  (`{callerType:NN}:{guid:D}` → `ClientIdentity`).

It is NOT responsible for:

- Anything Windows-specific — that lives in `LeapVR.Shared.Lib.Win`.
- Any UI / WPF concerns — see `LeapVR.Shared.Lib.Wpf`.
- Process management, file system or registry — see `LeapVR.Utilities.Windows`.

## Public API surface

| Type | Where | Purpose |
|------|-------|---------|
| `QuickLeap` (static partial) | `Helper/QuickLeap_*.cs` | Big bag of helpers split into files: `_Constants`, `_DateTime`, `_Expressions` (with `GetFieldPropertyName`), `_InputQuality`, `_Logging` (`ConfigureLogging`, `Logger`, `LogLevel`), `_Numeric` (`Bound`), `_Reflection`, `_StringFormatting`, `_Threading`. |
| `SanityCheck<T>` + `ISanityCheck` | `Classes/`, `Interfaces/` | Bundle a value (as `Expression<Func<T>>` so the field name can be extracted) with a list of `SanityRule<T>` and call `Check(out string err)`. |
| `SanityRule<T>` (abstract) + 25+ concrete rules | `Classes/SanityRules/` | Declarative rules: `NotNullRule`, `EmailRule`, `RegexRule`, `MinLengthRule`, `AllowedCharactersRule`, `ChineseIdNumberRule`, `ChineseMobileNumberRule`, `ChineseBankAccountNumberRule`, `MinDateTimeRule`, `EnumerableRuleSet`, `CustomRule`, etc. |
| `OrderedHashSet<T>` | `Classes/` | Insertion-order-preserving hash set with `MoveFirst/Last/Before/After`. Implements `ICollection<T>`, `ISerializable`. |
| `ICalculator<T>` + `DecimalCalculator`, `TimeSpanCalculator` | `Classes/GenericCalculator/` | Generic arithmetic (the workaround C# needs because operators aren't on interfaces). |
| `SubStream` | `Objects/` | Stream that exposes a window over an underlying stream. Used when reading container archives. |
| `CollectionChange<T>` + `ICollectionChange` | `Objects/`, `Interfaces/` | Add/Remove change record (used by services that report list deltas). |
| `IValidatable` | `Interfaces/` | `bool Validate(out string err)`. |
| `Empty` | root | Singleton `Empty.Get` — used as a sentinel "no payload" object. |
| `Crypto`, `CryptoHelpers`, `RSAParameterTraits` | `x509/` | RSA private-key ASN.1 decoder. |
| `ClientIdentity`, `ClientRole` | `x509/` | Parses a certificate Subject string into `(LicenseId, ClientRole)` for gRPC TLS auth. `ClientRole` values are wire-stable: `Station = 11`, `InternalIpc = 20`. |

## Internal structure

```
LeapVR.Shared.Lib/
├── Classes/
│   ├── SanityCheck.cs
│   ├── SanityRules/                  ← 25+ SanityRule<T> implementations
│   ├── GenericCalculator/            ← ICalculator<T> + impls
│   ├── OrderedHashSet.cs / CollectionDebugView.cs / HashHelpers.cs
│   └── ParameterRebinder.cs          ← LINQ Expression rewriting
├── Extensions/                       ← String / DateTime / Enumerable / etc.
├── Helper/                           ← QuickLeap.* partial-class buckets
├── Interfaces/
├── Objects/                          ← SubStream, CollectionChange
├── x509/                             ← Crypto, ClientIdentity, ClientRole
├── Empty.cs
└── _ReadMe.txt
```

## Notable patterns / gotchas

- **`SanityCheck<T>` uses `Expression<Func<T>>` for the value** so it can
  reflect the property name into error messages
  (`SanityCheck of field 'X' with value 'Y' failed on rule 'Z'`). Pass
  `() => myObj.SomeField`, not the value directly.
- **`QuickLeap` is a single static class spread across many files** with
  `partial` declarations. Every `_*.cs` adds methods to the same namespace.
- **`ClientRole` numeric values are part of the wire protocol** —
  the values appear inside X.509 Common Name fields on station certificates.
  Editing them breaks every existing license certificate. The enum comment
  warns explicitly: `DO NOT CHANGE ENUM VALUES`.
- **`Crypto.DecodeRsaPrivateKey` only supports specific ASN.1 layouts**
  (0x8130 / 0x8230 length headers) — copy-pasted from the canonical Microsoft
  workaround for `RSACryptoServiceProvider` on .NET 4.0+. It silently
  `Debug.Assert(false)` and returns `null` on malformed input.
- **`OrderedHashSet<T>` is forked from .NET reference source** — note the
  `m_buckets` / `m_slots` fields, mirror of `HashSet<T>`'s internals.
- **License header** at the top of most files refers to "VSpace Tech Dev Ltd."
  — this is the original company name; ignore for licensing purposes (project
  has been re-branded; see the recent commit history).

## Consumers

- `LeapVR.Shared.Lib.Win`, `LeapVR.Shared.Lib.Wpf`
- `LeapVR.Utilities.Windows`, `LeapVR.Utilities.Steam`
- All `LeapVR.Shell.*` projects (kiosk + setup wizard)
- All `LeapVR.Content.*` projects (Content Creator)

Not consumed by any `Pod.*` server project.

## Related docs

- [shared tier overview](../README.md)
- [`docs/architecture/auth.md`](../../architecture/auth.md) — how
  `ClientIdentity` / `ClientRole` relate to the kiosk's gRPC `(identity, password)`
  plain-metadata scheme (server-cert TLS only; no HMAC, no mutual TLS).
