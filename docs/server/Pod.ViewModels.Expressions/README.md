# Pod.ViewModels.Expressions

> Static helper classes that hold `Expression<Func<TEntity, TViewModel>>` projections so EF can translate "give me this view model" into SQL `SELECT col1, col2, …` instead of materialising whole entities.

## Purpose

The `Pod.Services` layer queries `PodDbContext` directly (no repository). To stop those queries from accidentally pulling entire entity graphs into memory, the projection from entity to view model is expressed as an `Expression<Func<TEntity, TViewModel>>` and used inside `.Select(...)`. EF Core then translates it into a SQL `SELECT` listing only the columns the view model needs.

This project is the home of those expressions. Each entity-to-VM mapping is a static class named `To<VM>`, with a `From<Entity>()` method returning the expression and (sometimes) a `FuncFrom<Entity>` precompiled delegate for in-memory use.

The project exists separately from `Pod.ViewModels` because it depends on `Pod.Data.Models` (entity types). Keeping that dependency out of `Pod.ViewModels` lets `Pod.Web.Client.Rest` reference the view models without dragging entities into the SDK.

## Tech

- **Target framework:** `netstandard2.0`
- **Key NuGet packages:** none
- **Project references (in this repo):**
  - `Pod.Data.Models` — entity types (`ApplicationUser`, `Station`, `Session`, `ShellServer`, `EMailAccountData`, `SubscriptionOrder`, …)
  - `Pod.Enums` — for enums in projection bodies
  - `Pod.ViewModels` — the destination shapes

## Responsibility

**Is responsible for:** declaring entity → view-model projections in a form EF Core 2.1 can translate to SQL.

**Is NOT responsible for:**
- Calling the projections — that's `Pod.Services` (`.Select(ToStationCurrentStateVm.FromStation())`)
- Defining the entities or the view models
- Any business logic — projections are pure shape mappings

## Public API surface

One static class per entity → view model mapping. Convention: `To<VmName>` with a `From<EntityName>()` method.

| File | Static classes (selection) |
|---|---|
| `StationViewModels.cs` | `ToStationCurrentStateVm`, `ToStationSettingsVm`, `ToSessionLogVm`, `ToSessionViewVm`, `ToChangeRequestVm`, `ToStationConnectionLogVm` |
| `BillingViewModels.cs` | `ToSubscriptionPaymentVm`, `ToSubscriptionOrderVm`, `ToSubscriptionOrderBasicVm`, `ToStationSubscriptionVm` |
| `UserViewModels.cs` | `ToUserVm` |
| `EMailViewModels.cs` | `ToEMailAccountVm`, `ToEmailTemplateVm`, `ToEMailTemplateDetailsVm` |
| `ShellServerViewModels.cs` | `ToShellServerVm`, `ToShellServerDetailsVm`, `ToShellServerConnectedClientVm` |

Each class typically exposes:
- `public static Expression<Func<TEntity, TVm>> From<Entity>()` — the EF-translatable form used inside `.Select(...)` calls.
- `public static readonly Func<TEntity, TVm> FuncFrom<Entity>` — pre-compiled delegate for in-memory use (e.g. mapping a single fetched entity outside an `IQueryable`).

## Internal structure

Flat — one file per VM-domain.

```
Pod.ViewModels.Expressions/
├── StationViewModels.cs
├── BillingViewModels.cs
├── UserViewModels.cs
├── EMailViewModels.cs
└── ShellServerViewModels.cs
```

## Notable patterns / gotchas

- **The expression is what EF translates, not the compiled func.** Use `FromX()` inside `.Select(...)`. Use `FuncFromX` only on already-loaded entities (e.g. `FuncFromSession.Invoke(session)` in a singleton factory). Mixing the two is a frequent source of "Method not supported by EF" exceptions.
- **`x.Collection.Count()` not `x.Collection.Count`.** EF Core 2.1 cannot translate the `.Count` *property* on a navigation collection — only the `.Count()` *extension method*. The note `//Do not optimize Count as it will otherwise not work` appears on `ToUserVm` and `ToShellServerDetailsVm` for this exact reason. Both files include the resulting SQL as a `#region` comment for posterity.
- **Null-checks inside expressions are EF-translatable** when written as `x.Nav != null ? expr : null`. They become a SQL `CASE WHEN`. Don't use `?.` in expression bodies — the C# compiler turns it into a chained call EF cannot handle.
- **Sub-projections must use the **expression**, not the func.** `x.ChangeRequests.Select(ToChangeRequestVm.FuncFromChangeRequest)` works inside a top-level `Select` because EF inlines the expression on its way out — but watch the SQL: a sub-projection like this produces a sub-`SELECT` per parent row. Acceptable for small fan-outs, problematic for large ones.
- **`AsEnumerable()` at the end of a sub-projection** (e.g. `x.ChangeRequests.Select(...).OrderBy(...).AsEnumerable()`) is needed in some cases to satisfy the view model's `IEnumerable<...>` property type without EF complaining about the assignment.
- **No `Include(...)` should be needed when projecting** — EF figures out the joins from the projection. If you find yourself writing `.Include(...).Select(...)`, you're materialising more than you need.

## Consumers

Direct project references:

- `Pod.Services` — uses these inside LINQ-to-EF queries

## Related docs

- [`docs/server/README.md`](../README.md) — server-tier overview
- [`docs/server/Pod.ViewModels/`](../Pod.ViewModels/) — the view-model shapes produced
- [`docs/server/Pod.Services/`](../Pod.Services/) — the consumer pattern (`.Select(ToXVm.FromX())`)
- [`docs/server/data/Pod.Data.Models/`](../data/Pod.Data.Models/) — the entities being projected
