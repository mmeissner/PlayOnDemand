# LeapVR.Utilities.VersionInfo

> Tiny build-time CLI: prints the `AssemblyVersion` of one or more managed
> assemblies to stdout. Used by `Build.bat` / `Build_Free.bat` to stamp the
> installer filename.

## Purpose

The kiosk and Content Creator pipelines need to embed the freshly-compiled
assembly version into the Inno Setup installer filename
(e.g. `LeapPlay.Shell.Setup.2019.6.123.exe`). The build is a `.bat` script
that has no easy way to read a managed assembly's version, so this 30-line
console app exists solely to do that:

```bat
"%BUILD_DIR%LeapVR.Utilities.VersionInfo.exe" "%SHELL_BIN%\LeapPlay.Shell.exe" > "%TEMP%\_pod_ver.txt"
```

The script then `for /f`-reads the file back into a `version` variable that is
passed to ISCC.

## Tech

- **Target framework:** `.NET Framework 4.7.1`
- **Output type:** `Exe`
- **Platforms:** `AnyCPU` (Debug), `x64` (Release / Release_ShellClient)
- **Configurations:** `Debug`, `Release`, `Release_ShellClient` (note:
  `Prefer32Bit=true` under `Release_ShellClient`)
- **Key NuGet packages:** none.
- **Project references (in this repo):** none.

## Responsibility

It IS responsible for:

- Loading each path passed on the command line via `Assembly.LoadFrom`
- Writing each assembly's `Name.Version.ToString()` to `stdout` on its own line
- Writing any `Exception.Message` to `stderr` (prefixed with the input path)
  so the build script can detect failure

It is NOT responsible for:

- Parsing or formatting the version (it just calls `.ToString()` — produces
  e.g. `2019.6.123.0`)
- Filtering by file type (it tries `LoadFrom` on whatever path you give it)
- Anything else. Total source: 30 lines in `Program.cs`.

## Public API surface

None — single `class Program { static void Main(string[] args) }`.

Behaviour:

```
> LeapVR.Utilities.VersionInfo.exe path\to\one.dll path\to\two.exe
1.2.3.4
2.0.0.0
```

On error:

```
> LeapVR.Utilities.VersionInfo.exe missing.dll
missing.dll: Could not load file or assembly...
```

The error goes to `stderr`; the `for /f` redirection in the build script reads
only `stdout`, so failures bubble up as a missing/empty version string.

## Internal structure

```
LeapVR.Utilities.VersionInfo/
├── Program.cs               ← the entire program
├── Properties/
│   └── AssemblyInfo.cs
└── App.config
```

## Notable patterns / gotchas

- **`Assembly.LoadFrom` locks the file** for the lifetime of the process. Since
  the process exits immediately, this is fine in practice — but don't try to
  reuse this code from a long-running host.
- **The version it prints is the `AssemblyVersion`**, *not* the
  `AssemblyFileVersion` or `AssemblyInformationalVersion`. The kiosk's
  `AssemblyInfo.cs` uses the wildcard `[assembly: AssemblyVersion("2019.6.*")]`
  so the build/revision parts are auto-incremented per-build by the C# compiler.
  Changing this format breaks the installer filename convention.
- **Stdout uses `Console.Out.WriteLine`** with the platform default encoding —
  works for ASCII version strings, but if you ever embed non-ASCII into the
  version it may garble.
- **Nothing pins this exe's location.** Both `Build.bat` (using `LeapVR.Utilities.VersionInfo.exe`
  on PATH/cwd) and `Build_Free.bat` (using `%BUILD_DIR%LeapVR.Utilities.VersionInfo.exe`
  with the build output folder) work because the build copies the exe alongside
  the kiosk binaries before invoking it.

## Consumers

- `LeapVR.Shell.Build/Build.bat` line 38 — extracts the version of
  `LeapPlay.Shell.exe` for the obfuscated build pipeline.
- `LeapVR.Shell.Build/Build_Free.bat` line 185 — same, for the
  un-obfuscated "Free" pipeline.

No other code in the repo references the assembly. It is a pure build helper.

## Related docs

- [shared tier overview](../README.md)
- [`docs/architecture/build-and-deploy.md`](../../architecture/build-and-deploy.md)
  for the full build / installer pipeline that calls this exe.
