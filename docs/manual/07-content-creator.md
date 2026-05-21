# 07 — Content Creator

> The standalone authoring tool that packages a folder of game files plus
> launch metadata into a single `.vbox` container — and edits or unpacks
> existing ones.

The Content Creator (`LeapPlay.Content.Creator.exe`) is a WPF wizard. It
ships in the same Build_Free.bat output as the kiosk and runs on any
Windows machine — you don't need a VR headset or a station to author
content.

## Three flows

The shell shows three buttons on launch:

| Button | What it does |
|--------|--------------|
| **Create** | Pack a folder of game files + cover art + launch info into a new `.vbox`. |
| **Edit** | Open an existing `.vbox` and change its display data (title, description, category, tags) **without re-packing the GameFiles**. Safe for multi-GB packages. |
| **Unpack** | Extract an existing `.vbox` back into a folder structure (`<basename>/GameFiles/`, `MediaFiles/`, `Metadata/`). For inspection or recovery. |

## Create

The Create wizard is three steps:

1. **App Executable Info** — point at the game's main exe, set the working
   directory and command-line args, declare any required VR module
   (OpenVR or none), and configure per-process watchdog flags
   ("is main executable", "kill on exit", "kill on hang"). This is the
   same data the [**advanced edit dialog**](04-admin-panel.md#advanced--process-monitoring)
   in the kiosk shows after install.
2. **App Detail Info** — display name, description, category, tags, cover
   art (a 460×215 PNG, used for the catalog tile). One or more launch
   flavours can be added at this step for multi-build games.
3. **Summary** — confirm and write. The wizard shows a live progress bar
   while the GameFiles zip is built — usually the slow phase for any
   real game. As of v1.0.0 this packs in parallel across CPU cores at
   compression Level 3, which is roughly **4-6× faster than the original
   Level-8 single-threaded path** at a ~10-15% size cost.

Output: one `.vbox` file at the path the wizard selected. Distribute it on
a USB stick (see [**Chapter 05**](05-installing-games.md)) or hand it off
to the operator app.

## Edit — the 50-GB-friendly path

A common operator scenario: the description is misspelled, or the cover
art has the wrong aspect ratio, or the launch parameters need a tweak —
on a package whose GameFiles zip is 50 GB. Re-creating from scratch means
re-zipping 50 GB just to fix three lines of JSON.

The Edit wizard does it in seconds:

1. Pick an existing `.vbox`.
2. The wizard hydrates `displayData.json` + `platformData.json` from the
   metadata package and binds them to the standard detail-info form.
3. The operator changes Title / Description / Category / etc.
4. On Save, the wizard rebuilds **only the metadata zip** (KB-scale),
   truncates the source `.vbox` at the metadata offset, appends the new
   metadata + a fresh Protobuf header + the 8-byte trailer offset, and
   atomically swaps the file via `File.Replace`.

The GameFiles and MediaFiles bytes are **never touched** — verified by
hashing bytes 0..`metadataOffset` before and after a save and confirming
they're byte-identical. The protocol design and the safety property are
covered in `EditableAppInstallationContainer.cs` and the test project
`LeapVR.Shell.Modules.Container.Test/`.

## Unpack

Pick an existing `.vbox`, pick an output directory, hit go. The wizard
extracts each internal package (`GameFiles`, `MediaFiles`, `Metadata`)
into its own subfolder under `<output-dir>/<basename>/`. Each phase is
parallelised across CPU cores; a progress bar reports
files-done / total-files and bytes-done / total-bytes while it runs.

Useful for:

- **Inspection** — what's actually inside a third-party `.vbox`?
- **Recovery** — the game files exist as a sibling but the catalog row
  has been deleted; unpack the `.vbox`, copy the GameFiles into the
  station's install dir manually, re-register the metadata.
- **Forking a package** — unpack, hand-edit, re-Create with the changes.

## When to use which

| Want to… | Use |
|----------|-----|
| Build a brand-new package from a folder of game files | **Create** |
| Fix a title, description, tag, or category on an existing package | **Edit** |
| Change the launch executable / VR module / process-monitor flags | **Edit** (the same wizard tabs) |
| Look at what's inside a `.vbox` you didn't author | **Unpack** |
| Replace artwork or game files | Unpack → re-Create (no in-place file mutation for GameFiles) |

## Verification harness

For automated test workflows there's a console verifier under
`LeapVR.Shell.Modules.Container.Test/EndToEndEditVerifier/` that exercises
the same code path the WPF wizard uses:

```cmd
EndToEndEditVerifier.exe C:\path\to\package.vbox
```

It opens the file, hashes bytes 0..metadataOffset, mutates the
Description via the real editor, saves, reopens, asserts the description
persisted **and** asserts the pre-metadata bytes are byte-identical.
Exits 0 on PASS / 1 on FAIL. Designed to be runnable against any `.vbox`
on disk including production-sized ones.

A matching `SampleVboxBuilder/` console exe builds a tiny `.vbox`
suitable for manual UI smoke-testing without needing a real game.

---

→ [**08 — Operator app**](08-operator-app.md)
