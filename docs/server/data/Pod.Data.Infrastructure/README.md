# Pod.Data.Infrastructure

> Cross-cutting infrastructure shared by every server-side project that handles data: the `Result<T>` validator/error-aggregator pattern, plus a couple of small JSON/clone helpers and a credentials struct.

## Purpose

This library is the lowest-level building block of the server tier. It contains **no entities, no DbContext, no business logic** — only the primitives that every other layer (entities, services, gRPC handlers, controllers) reuses.

Its centerpiece is `Result<T>` / `IResult<T>` — a fluent validation + error-aggregation type used **instead of throwing exceptions** for business-rule failures. Every domain method on every entity in `Pod.Data.Models`, every service in `Pod.Services`, and most gRPC handlers return `IResult` or `IResult<T>`. This is the single most-touched type in the codebase, so changing it has wide blast radius.

The project also exposes `Extensions.CloneJson<T>()` / `ToJson()` / `LogJson()` and the `ClientCredentials` struct used by station authentication.

## Tech

- **Target framework:** `netstandard2.0` (so it can be referenced by both the .NET Core 2.1 server and .NET Framework 4.7.1 client where ever needed).
- **Configurations:** `Debug;Release;Release_ShellClient` — the `Release_ShellClient` config exists to allow this assembly to be picked up by the WPF client build pipeline.
- **Key NuGet packages:**
  - `Newtonsoft.Json` 12.0.2 — used by `Extensions.CloneJson` and `ToJson` / `LogJson`.
- **Project references (in this repo):**
  - `Pod.Enums` — needed because every error attaches a `UserError` enum code.

## Responsibility

**It IS responsible for:**
- Defining the `Result` / `Result<T>` / `IResult` / `IResult<T>` validation types and their validator method surface (the `ArgXxx`, `RefXxx`, `ValueXxx`, `StringXxx` family).
- Aggregating validation errors keyed by `UserError` so callers can inspect `result.HasError()`, iterate codes, or render `result.ToErrorString()`.
- Generic JSON helpers (`CloneJson`, `ToJson`, `LogJson`) used for deep copies and structured logging.
- The `ClientCredentials` value type carrying `(StationId, Password)` for station logins.

**It is NOT responsible for:**
- Persistence, mapping, or the DbContext (see `Pod.Data`).
- Any entity definitions (see `Pod.Data.Models`).
- HTTP / gRPC error translation — that belongs to `Pod.Grpc.Utilities` (which knows how to map `UserError` codes onto gRPC `Status` values).
- Logging plumbing — `LogJson` only formats; consumers wire NLog or `ILogger`.

## Public API surface

### `IResult` / `Result`

```csharp
public interface IResult : IReadOnlyDictionary<UserError, IReadOnlyList<string>>
{
    bool IsSuccess();
    bool HasError();
    string ToErrorString();
    IEnumerable<string> GetValuesFlattened();
}

public interface IResult<out T> : IResult
{
    T ReturnValue { get; }
}
```

`Result` implements `IResult` and exposes a large fluent validator surface. `Result<T>` adds `Add(T)` to attach the success value and `ReturnValue` to read it back.

### Validator naming convention

Every validator follows one of three prefixes that describe **who** the value comes from. This convention is observed everywhere; respect it when adding new ones.

| Prefix | Meaning | Examples |
|---|---|---|
| `Arg…` | The value was passed in as a method argument. Failure means caller-supplied data is wrong. | `ArgNotNull`, `ArgNotNullOrWhitespace`, `ArgNotEmpty` (Guid), `ArgNotEqual`, `ArgEqual`, `ArgTrue`, `ArgFalse`, `ArgNotEnum`, `ArgIsEnum`, `ArgNotLowerThen`, `ArgNotLowerOrEqualThen`, `ArgNotHigherThen`, `ArgNotBefore`, `ArgNotBeforeOrEqualThen`, `ArgNotAfterOrEqualThen`, `ArgOutOfRange` |
| `Value…` | The value belongs to *this* entity (e.g. its own `Id` field). Failure means the entity itself is in an invalid state. | `ValueNotEqual`, `ValueEqual`, `ValueTrue`, `ValueFalse`, `ValueNotNull`, `ValueIdValid` |
| `Ref…` | A navigation property / FK pair on an EF entity. Distinguishes "must be loaded" from "must remain null". | `RefNotNull(value, name)`, `RefNotNull(value, valueId, name)` (must NOT exist), `RefNotNullIfExist(value, valueId, name)` (must be loaded if FK is set) |
| `String…` | String content rules. | `StringNotShorterThen`, `StringNotLongerThen`, `StringMustContainUpperCase`, `StringMustContainLowerCase`, `StringMustContainSpecialChars`, `StringMustContainNumbers`, `StringMustUniqueChars` |
| `InvalidOperation` | Free-form error setter for cases that don't fit a predicate. | `InvalidOperation(value, name, message, error)` |

Every validator returns `bool` (success) and adds an entry to the inner `Dictionary<UserError, List<string>>` on failure. Most validators take an optional `UserError error = UserError.InternalError` last parameter so that a meaningful enum code is attached.

### Adding & merging

- `Result.Add(string message, UserError error)` — record a free-form error.
- `Result.Add(IResult other)` — merge errors from another result. This is the **standard pattern** for composing validation pipelines: a parent method accumulates errors from sub-calls.
- `Result<T>.Add(T value)` — attach the success value (must come last).
- `Result<T>.Add(IResult)` — same merge, but typed. Returns `Result<T>` so it remains chainable.

### Error introspection

`Result` implements `IReadOnlyDictionary<UserError, IReadOnlyList<string>>` so callers can iterate `foreach (var kv in result) …`, look up `result[UserError.OrderInvalidExpireDate]`, or call `result.ContainsKey(...)`. `ToErrorString()` joins all messages with `Environment.NewLine`; `ToErrorString(separator)` lets you pick. `GetValuesFlattened()` returns a flat `IEnumerable<string>` for log lines.

### `ClientCredentials`

```csharp
public struct ClientCredentials { public Guid StationId; public string Password; }
```

A bare value struct used to pass station password credentials around (specifically when a station authenticates by password rather than the HMAC ApiKey/Secret scheme).

### `Extensions`

- `CloneJson<T>(this T source)` — deep clone via Newtonsoft serialize/deserialize round-trip. Note: only public members survive; private state is lost.
- `ToJson(this object obj)` — `Formatting.Indented` JSON.
- `LogJson(this object obj)` — same as `ToJson` but prepends a newline and the type name and uses `StringEnumConverter` so enums log as names. Wraps in try/catch and returns the exception message on failure (so logging never throws).

## Internal structure

```
Pod.Data.Infrastructure/
├── Result.cs              — IResult, IResult<T>, Result, Result<T> (~1300 lines)
├── Extensions.cs          — CloneJson / ToJson / LogJson
├── ClientCredentials.cs   — { Guid StationId; string Password; } struct
└── Pod.Data.Infrastructure.csproj
```

One file per concern. Result.cs is intentionally one big file because the validator surface is the unit; splitting by validator family was rejected.

## Notable patterns / gotchas

- **Composition is by `Add(IResult)`**, not exceptions. A typical entity method looks like:

  ```csharp
  public IResult<Foo> DoThing(string name, int count) {
      var result = new Result<Foo>();
      result.ArgNotNullOrWhitespace(name, nameof(name), UserError.SomethingInvalid);
      result.ArgNotLowerThen(count, nameof(count), 1, "min");
      if (result.HasError()) return result;          // bail early on bad input
      var sub = SomeOtherEntity.Build(name);
      if (result.Add(sub).HasError()) return result; // merge & bail
      return result.Add(new Foo(name, count, sub.ReturnValue));
  }
  ```

  Read the existing entity files (e.g. `Station.cs`, `SessionDetails.cs`) for canonical examples.

- **Always pass a meaningful `UserError`**. The default `UserError.InternalError` exists as a safety net but lets the caller see no actionable info. Most public APIs choose a specific code so the gRPC layer (`Pod.Grpc.Utilities`) can translate it cleanly to a status.

- **`RefNotNull` has two overloads with opposite intent.** `RefNotNull(object value, string name)` checks "is it loaded?". `RefNotNull(object value, Guid? valueId, string name)` checks "must NOT already exist" (used in entity constructors that should not double-link). The companion `RefNotNullIfExist(value, valueId, name)` is the one usually wanted on the consumer side: "if the FK is set, the navigation must also be loaded."

- **`ArgNotLowerOrEqualThen(int)` returns `void`, but its `long` / `double` siblings return `bool`.** Inconsistency to be aware of when chaining; if you need an early-exit pattern use one of the bool-returning overloads or check `result.HasError()`.

- **`Result<T>.Add(T)` sets `ReturnValue` only — it does not clear errors.** A result can simultaneously have errors *and* a return value, although that's rarely intentional. Always check `HasError()` before relying on `ReturnValue`.

- **No async story.** Validators are synchronous in-memory predicates. Async validation (e.g. uniqueness checks against the DB) is done in services that build a `Result` from query outcomes manually.

- **Equality semantics in `ArgEqual` / `ArgNotEqual` / `ValueEqual`** treat both sides being `null` as equal. Be careful when comparing nullable types.

## Consumers

Effectively every server project depends on this one (transitively via `Pod.Data.Models` or directly):

- `Pod.Data.Models` — every entity factory/method returns `IResult<T>`.
- `Pod.Data` — DbContext layer; passes results upward.
- `Pod.Services` — business services compose Result chains.
- `Pod.Grpc.Utilities` — maps `UserError` codes onto gRPC status responses.
- `Pod.Grpc.ConnectHost.Server`, `Pod.Grpc.ShellHost.Server` — service handlers consume `IResult` from services.
- `Pod.Web.Center` — controllers and Razor handlers consume results when calling services.
- `Pod.MailEngine` — same pattern for delivery outcomes.

`ClientCredentials` and `Extensions.CloneJson` have a smaller footprint (mostly station-auth services and a few diagnostics call sites).

## Related docs

- `docs/architecture/grpc.md` — explains how `IResult<T>` + `UserError` flows across the gRPC boundary.
- `docs/server/data/Pod.Data.Models/README.md` — the entity catalog whose factory methods all return `IResult<T>`.
- `docs/server/data/Pod.Data/README.md` — DbContext layer.
- `docs/architecture/data-model.md` — entity relationships overview.
