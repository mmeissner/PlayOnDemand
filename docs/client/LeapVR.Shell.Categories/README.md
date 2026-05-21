# LeapVR.Shell.Categories

> App-categorisation provider plus the bundled multilingual category-icon resource dictionary. Maps category identifiers (e.g. `IconCategoryShooter`, `IconCategoryRacing`) to display names and `ImageSource`s; merges custom per-station category icons from disk on top of defaults.

## Purpose

Every installable app declares one or more category identifiers (defined by Content Creator or by a Steam library scan). The kiosk needs to render those categories with a localised name and an icon — both of which can be customised per-station by dropping image files into `%APPDATA%\LeapPlay\Images\Categories\`.

This project owns:

1. The default icon dictionary (`Resources/Images/Categories/Categories.xaml`, merged into the app at startup via `App.xaml`).
2. The default category labels (`Resource.resx` + `Resource.en-US.resx` + `Resource.zh-CN.resx`, with the corresponding `MultilingualResources/*.xlf` translation units).
3. The runtime `CategoryProvider`, which:
   - Lazily creates `AppCategory` instances on first `GetOrCreateAppCategory(identifier)` call.
   - Tries to resolve the icon from the merged WPF resource dictionary first, then falls back to a default icon.
   - Watches for changes via `IUIMessageBroker` (e.g. when a custom icon is dropped in).

## Tech

- **Target framework:** .NET Framework 4.7.1, x64
- **Output:** `Library` (`LeapVR.Shell.Categories.dll`)
- **Key NuGet packages:**
  - `Caliburn.Micro` — `IEventAggregator` injection point (only the core types)
  - `WPFLocalizeExtension` 3.1.2 — runtime culture-aware string lookup
  - `XAMLMarkupExtensions` 1.3.0 — used by `WPFLocalizeExtension`
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib.Wpf`
  - `LeapVR.Shell.Domain.Models`

## Responsibility

**It IS responsible for:**
- Producing `IAppCategory` instances on demand.
- Merging on-disk custom icons (`%APPDATA%\LeapPlay\Images\Categories\`) into the WPF resource dictionary at runtime.
- Owning the default icon `ResourceDictionary` and the localised category-name resources.
- Publishing category-related events through `IUIMessageBroker`.

**It is NOT responsible for:**
- Persisting category info (categories are computed from app metadata).
- Localising anything beyond category labels (general UI localisation is in `LeapVR.Shell.Language`).

## Public API surface

| Type | Purpose |
|---|---|
| `ICategoryProvider` | Public contract. Methods: `GetAllCategories`, `GetOrCreateAppCategory(identifier)`. |
| `CategoryProvider` | Implementation. Holds `ConcurrentDictionary<string, AppCategory>`; loads default categories on construction; watches for events from `IUIMessageBroker`. |
| `Resource` (auto-generated `Resource.Designer.cs`) | Accessor for the `.resx`-backed category-label strings. |
| `Annotations` (`Properties/Annotations.cs`) | JetBrains-style nullability attributes — internal only. |
| `MsBuildAL1073WarningWorkaround.targets` | MSBuild glue to silence `AL1073` (multilingual app-localisation warning). |

Resource dictionaries (consumed via XAML `pack://application:,,,/LeapVR.Shell.Categories;component/...`):

- `Resources/Images/Categories/Categories.xaml` — merged into `App.xaml` at startup; provides every default category icon as an `ImageSource` keyed by identifier (e.g. `IconCategoryShooter`, `IconCategoryDefault`).

## Internal structure

```
LeapVR.Shell.Categories/
├── ICategoryProvider.cs                Public contract
├── CategoryProvider.cs                 Implementation
├── Resource.resx / Resource.Designer.cs        Default (invariant) category strings
├── Resource.en-US.resx                          English overrides
├── Resource.zh-CN.resx                          Simplified Chinese overrides
├── MultilingualResources/
│   ├── LeapVR.Shell.Categories.en-US.xlf       XLIFF source for translation tooling
│   └── LeapVR.Shell.Categories.zh-CN.xlf
├── Resources/
│   └── Images/
│       └── Categories/
│           ├── Categories.xaml                   Merged into App.xaml — default icons
│           └── *.png                             Per-category icon assets
├── MsBuildAL1073WarningWorkaround.targets        Silences AL1073
├── Properties/AssemblyInfo.cs
├── packages.config
└── LeapVR.Shell.Categories.csproj
```

## Notable patterns / gotchas

- **`Categories.xaml` index in `App.xaml`'s merged dictionaries is fixed.** `CategoryProvider.cs` defines `private const int CategoryResourcesImagesIndex = 4;` — that is the position of the categories dictionary inside `App.xaml`'s `MergedDictionaries`. If a developer reorders entries in `App.xaml`, this constant must change too. The `App.xaml` itself has a comment warning about this. (Source: `CategoryProvider.cs`, `App.xaml` of `LeapVR.Shell`.)
- **Custom-icon drop-in folder is `%APPDATA%\LeapPlay\Images\Categories\`** (`Path.Combine(GlobalConfig.GetGlobalConfiguration().PersistentDirectory, "Images", "Categories")`). PNG files dropped here override defaults at runtime — `RegisterResourceByKey` checks the disk first.
- **Default fallback key is `IconCategoryDefault`** when an identifier isn't found in either custom or default dictionaries.
- **`Multilingual` resources require a special MSBuild target.** `MsBuildAL1073WarningWorkaround.targets` is referenced from the `.csproj` to silence a known noise warning from the multilingual app toolkit.
- **No direct `LeapVR.Shell.Language` reference.** Localisation here is via `WPFLocalizeExtension` against the project's own `Resource.*.resx` — it does not delegate to the `LeapVR.Shell.Language` project.

## Consumers

- `LeapVR.Shell` — `Bootstrapper.RegisterViewModels` registers `ICategoryProvider` as singleton, and `App.xaml` merges `Categories.xaml`.
- `LeapVR.Shell.Controllers` — references this for categorisation during install (`AppInfoProcessor` uses categories).
- `LeapVR.Shell.Modules` — references this for app categorisation during catalogue building.
- `LeapVR.Shell.Setup` — uses a `CategoryProviderDummy` (in `Setup/Dummies.cs`) during uninstall to avoid loading the WPF resource dictionary unnecessarily.

## Related docs

- Sister projects: [`LeapVR.Shell.Language`](../LeapVR.Shell.Language/README.md) (general UI localisation), [`LeapVR.Shell.Domain.Models`](../LeapVR.Shell.Domain.Models/README.md) (`IAppCategory` lives there), [`LeapVR.Shell`](../LeapVR.Shell/README.md) (`App.xaml` merges this project's dictionary)
- Tier overview: [`docs/client/README.md`](../README.md)
