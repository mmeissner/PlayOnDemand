# LeapVR.Shared.Lib.Wpf

> WPF-only helpers: `IValueConverter` set, visual-tree helpers,
> `EnumBindingSourceExtension`, and a localised `DescriptionAttribute`.

## Purpose

A thin support library shared between the kiosk WPF app (`LeapVR.Shell` and the
`LeapVR.Shell.Setup` wizard) and the Content Creator WPF app. It does nothing
that's not directly tied to WPF data binding, the visual tree, or markup
extensions — anything more general lives in `LeapVR.Shared.Lib`, anything
WinAPI-shaped lives in `LeapVR.Shared.Lib.Win`.

Think "the converters and helpers your XAML needs" — and not much more.

## Tech

- **Target framework:** `.NET Framework 4.7.1` with WPF (`ProjectTypeGuids`
  includes the WPF GUID `{60dc8134-eba5-43b8-bcc9-bb4bc16c2548}`).
- **Platforms:** `AnyCPU`, `x64`
- **Configurations:** `Debug`, `Release`, `Release_ShellClient`
- **Key NuGet packages:** none. Pure framework references
  (`PresentationCore`, `PresentationFramework`, `WindowsBase`, `System.Xaml`,
  `System.Drawing`).
- **Project references (in this repo):**
  - `LeapVR.Shared.Lib`

## Responsibility

It IS responsible for:

- WPF `IValueConverter`s used by binding XAML.
- A merged dictionary (`Converters/Converters.xaml`) that exposes all converters
  as static resources, so consumers `MergedDictionaries`-import it and use
  `{StaticResource BoolToVisibility}` rather than instantiating per-window.
- `EnumBindingSourceExtension` markup extension — feeding `Enum.GetValues(...)`
  to a XAML binding without code-behind.
- `LocalizedDescriptionAttribute : DescriptionAttribute` — reads a localised
  description from a `ResourceManager`. Used together with
  `EnumDescriptionTypeConverter` so `[LocalizedDescription("Enum_Foo", typeof(MyResx))]`
  on enum members gives data-bound combo boxes a translated label.
- Visual-tree traversal helpers under `UIHelpers/`.

It is NOT responsible for:

- App-specific styles or themes — those live with each WPF app
  (`LeapVR.Shell`, `LeapVR.Content.Creator`).
- Interactivity behaviours (`System.Windows.Interactivity`) — vendored
  separately under `LeapVR.Shell.3rdParty/bin/`.

## Public API surface

### Converters (`LeapVR.Shared.Lib.Wpf.Converters`)

| Converter | Notes |
|-----------|-------|
| `BoolToVisibilityConverter` | `parameter=true` inverts the mapping. Falls back to `Visible` when input isn't parseable. |
| `InvertBoolConverter` | `!bool` for bindings. |
| `VisibilityToBoolConverter` | reverse of the first. |
| `NullToVisibilityConverter` | null → `Collapsed`, otherwise `Visible` (param inverts). |
| `BytesToProperSpaceUnitConverter` | bytes → "1.4 GB" style strings. |
| `CultureMatchToBoolConverter` | bind a culture name; true if matches `CurrentUICulture`. |
| `ImageConverter` | URI / byte[] / Bitmap → `BitmapImage`. |
| `KeyToCorrespondingIconConverter` | maps a key to its UI icon resource. |
| `PercentageMultiConverter` | `IMultiValueConverter` for "x of y" displays. |
| `PlatformAppLoadingErrorToVisibilityConverter` | App-specific: shows error UI when a platform module reports a load failure. |
| `ProportionValueConverter` | scales a numeric input by a constant. |
| `UriToTrackStringConverter` | filename-from-URI for now-playing strings. |

A `Converters/Converters.xaml` `ResourceDictionary` exposes all of them as
static resources for `MergedDictionaries` consumption.

### Markup extensions / attributes

| Type | Purpose |
|------|---------|
| `EnumBindingSourceExtension : MarkupExtension` | `<ComboBox ItemsSource="{wpf:EnumBindingSource {x:Type local:MyEnum}}"/>`. Supports nullable enums by prepending a null slot. |
| `LocalizedDescriptionAttribute : DescriptionAttribute` | `[LocalizedDescription("Key", typeof(Resx))]` on enum members. Returns `[[Key]]` if missing — easy to spot in the UI. |
| `EnumDescriptionTypeConverter : EnumConverter` | the converter that pairs with `LocalizedDescriptionAttribute`. |

### UI helpers (`UIHelpers/`)

`UIHelper` is a static partial class spread across:

| File | Purpose |
|------|---------|
| `UIHelper_ElementTree.cs` | `GetChildrenElements(UIElement)` — recursive `VisualTreeHelper` walk. |
| `UIHelper_VisualState.cs` | `VisualStateManager` helpers. |
| `UIHelper_ResourceConverter.cs` | resolve a string key into a `FrameworkElement` resource. |

## Internal structure

```
LeapVR.Shared.Lib.Wpf/
├── Converters/
│   ├── Converters.xaml           ← merged-dictionary of all converters
│   └── *Converter.cs             ← see table above
├── UIHelpers/
│   ├── UIHelper_ElementTree.cs
│   ├── UIHelper_ResourceConverter.cs
│   └── UIHelper_VisualState.cs
├── EnumBindingSourceExtension.cs
├── EnumDescriptionTypeConverter.cs
├── LocalizedDescriptionAttribute.cs
└── Properties/                   ← Resources.resx, Settings.settings
```

Note: an empty `ValidationRules\` folder is declared in the csproj but contains
no files in this repo snapshot.

## Notable patterns / gotchas

- **`EnumBindingSourceExtension` adds a null slot for nullable enums** by
  growing the array by one and shifting — easy to miss when consumers index
  into the result.
- **`LocalizedDescriptionAttribute` returns `[[Key]]` on missing resource keys**
  — intentional: surfaces missing translations directly in the UI rather than
  silently showing the enum name.
- **`Converters.xaml` is the single import point.** Apps add it to
  `App.xaml`'s merged dictionaries; per-page reuse just references the keys.
- **`PlatformAppLoadingErrorToVisibilityConverter`** leaks a domain concept
  ("Platform" = a game-source plug-in). It's here for convenience but
  conceptually belongs to the kiosk; the Setup wizard never uses it.
- **Bool parsing is string-based**: each bool converter uses
  `bool.TryParse(value?.ToString(), ...)` rather than casting. This means
  any non-string, non-bool input degenerates to "default visibility" (=`Visible`)
  — surprising but rarely matters in practice.

## Consumers

- `LeapVR.Shell` (kiosk WPF app)
- `LeapVR.Shell.Setup` (first-run wizard)
- `LeapVR.Shell.Categories` (uses `LocalizedDescriptionAttribute`)
- `LeapVR.Content.Creator`, `LeapVR.Content.Creator.Logic`

Not consumed by any `Pod.*` server project (server is ASP.NET Core, no WPF).

## Related docs

- [shared tier overview](../README.md)
- [`docs/client/README.md`](../../client/README.md) for how the converters are
  wired into the kiosk's `App.xaml`.
