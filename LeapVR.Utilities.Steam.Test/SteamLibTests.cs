#region Licence
/****************************************************************
 *  Filename: SteamLibTests.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using LeapVR.Utilities.Steam.Steam;
using LeapVR.Utilities.Steam.Steam.VDF;
using Xunit;

namespace LeapVR.Utilities.Steam.Test
{
    /// <summary>
    /// Characterization tests for the VDF-parsing path in SteamLib that
    /// crashed the kiosk on first launch ("Value cannot be null. Parameter
    /// name: path1" -> Path.Combine inside GetSteamLibraryFolders).
    /// </summary>
    public class SteamLibTests
    {
        // ResolveLibraryPath is the inner helper that pulls the library root
        // out of one numbered entry. Both VDF schemas need to work; the modern
        // schema is what crashed the kiosk.

        [Fact]
        public void ResolveLibraryPath_returns_null_for_null_entry()
        {
            Assert.Null(SteamLib.ResolveLibraryPath(null));
        }

        [Fact]
        public void ResolveLibraryPath_handles_legacy_direct_value_schema()
        {
            // Legacy libraryfolders.vdf format:
            //   "1"  "/tmp/SteamLibrary"
            // (Forward slashes are valid path separators on both Windows and POSIX;
            // the VDF parser stores values verbatim, no escape-handling.)
            var entry = ParseSingleEntry(
                "\"libraryfolders\"\n" +
                "{\n" +
                "  \"1\"  \"/tmp/SteamLibrary\"\n" +
                "}\n");
            Assert.Equal("/tmp/SteamLibrary", SteamLib.ResolveLibraryPath(entry));
        }

        [Fact]
        public void ResolveLibraryPath_handles_modern_nested_path_schema()
        {
            // Modern (post-2021) libraryfolders.vdf format:
            //   "0"
            //   {
            //     "path"  "/var/steam/library"
            //     "label" ""
            //   }
            var entry = ParseSingleEntry(
                "\"libraryfolders\"\n" +
                "{\n" +
                "  \"0\"\n" +
                "  {\n" +
                "    \"path\"  \"/var/steam/library\"\n" +
                "    \"label\" \"\"\n" +
                "  }\n" +
                "}\n");
            Assert.Equal("/var/steam/library", SteamLib.ResolveLibraryPath(entry));
        }

        [Fact]
        public void ResolveLibraryPath_returns_null_when_modern_entry_has_no_path_child()
        {
            // Future schema variant we don't recognise - return null instead of
            // crashing the kiosk on dispatcher-unhandled NullReferenceException.
            var entry = ParseSingleEntry(
                "\"libraryfolders\"\n" +
                "{\n" +
                "  \"0\"\n" +
                "  {\n" +
                "    \"label\" \"\"\n" +
                "  }\n" +
                "}\n");
            Assert.Null(SteamLib.ResolveLibraryPath(entry));
        }

        // GetSteamLibraryFolders is the entry point Initialize() calls. We test
        // it against on-disk fixtures because VDFFile reads a file path.

        [Fact]
        public void GetSteamLibraryFolders_with_null_steam_path_returns_empty()
        {
            // Null steamPath was the original crash:
            // System.ArgumentNullException: Value cannot be null. (path1)
            //   at System.IO.Path.Combine(...)
            //   at SteamLib.GetSteamLibraryFolders(string steamPath)
            var result = SteamLib.GetSteamLibraryFolders(null);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetSteamLibraryFolders_returns_only_steam_install_when_vdf_missing()
        {
            // If libraryfolders.vdf doesn't exist (fresh Steam install, no
            // additional libraries), the result still contains the default
            // "<steam>/steamapps".
            using (var tmp = new TempDir())
            {
                Directory.CreateDirectory(Path.Combine(tmp.Path, "steamapps"));
                var folders = SteamLib.GetSteamLibraryFolders(tmp.Path);
                Assert.Single(folders);
                Assert.Equal(Path.Combine(tmp.Path, "steamapps"), folders[0]);
            }
        }

        [Fact]
        public void GetSteamLibraryFolders_parses_modern_schema_libraryfolders_vdf()
        {
            // Reproduces the modern Steam install layout. Before the fix, this
            // threw ArgumentNullException at Path.Combine on the modern entry.
            using (var tmp = new TempDir())
            {
                var steamapps = Path.Combine(tmp.Path, "steamapps");
                Directory.CreateDirectory(steamapps);

                var extraLibrary = Path.Combine(tmp.Path, "Library_D");
                Directory.CreateDirectory(Path.Combine(extraLibrary, "steamapps"));

                // VDF values are stored verbatim by the parser (no escape handling),
                // so write the paths with forward slashes for round-tripping in the
                // test fixture. Path.Combine accepts mixed separators on Windows.
                var tmpForward = tmp.Path.Replace("\\", "/");
                var extraForward = extraLibrary.Replace("\\", "/");

                File.WriteAllText(Path.Combine(steamapps, "libraryfolders.vdf"),
                    "\"libraryfolders\"\n" +
                    "{\n" +
                    "  \"0\"\n" +
                    "  {\n" +
                    "    \"path\"  \"" + tmpForward + "\"\n" +
                    "    \"label\" \"\"\n" +
                    "  }\n" +
                    "  \"1\"\n" +
                    "  {\n" +
                    "    \"path\"  \"" + extraForward + "\"\n" +
                    "    \"label\" \"\"\n" +
                    "  }\n" +
                    "}\n");

                var folders = SteamLib.GetSteamLibraryFolders(tmp.Path);

                Assert.Contains(Path.Combine(tmp.Path, "steamapps"), folders);
                Assert.Contains(Path.Combine(extraForward, "steamapps"), folders);
            }
        }

        // ScanAppManifestsInLibrary - regression for the "Dictionary.Add duplicate
        // key" crash that disabled the Steam plugin when the same appId appeared
        // in two library folders.

        [Fact]
        public void ScanAppManifestsInLibrary_skips_silently_when_directory_missing()
        {
            var dict = new Dictionary<uint, string>();
            SteamLib.ScanAppManifestsInLibrary(
                Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N")),
                dict);
            Assert.Empty(dict);
        }

        [Fact]
        public void ScanAppManifestsInLibrary_records_one_appid_from_acf()
        {
            using (var tmp = new TempDir())
            {
                File.WriteAllText(Path.Combine(tmp.Path, "appmanifest_250820.acf"),
                    "\"AppState\"\n" +
                    "{\n" +
                    "  \"appid\"      \"250820\"\n" +
                    "  \"installdir\" \"SteamVR\"\n" +
                    "}\n");

                var dict = new Dictionary<uint, string>();
                SteamLib.ScanAppManifestsInLibrary(tmp.Path, dict);

                Assert.True(dict.ContainsKey(250820u));
                Assert.Equal(Path.Combine(tmp.Path, "common", "SteamVR"), dict[250820u]);
            }
        }

        [Fact]
        public void ScanAppManifestsInLibrary_handles_duplicate_appid_across_libraries()
        {
            // Pre-fix this scenario crashed with
            //   System.ArgumentException: An item with the same key has already been added.
            // - which disabled the Steam plugin globally in the kiosk session.
            using (var lib1 = new TempDir())
            using (var lib2 = new TempDir())
            {
                var acf =
                    "\"AppState\"\n" +
                    "{\n" +
                    "  \"appid\"      \"250820\"\n" +
                    "  \"installdir\" \"SteamVR\"\n" +
                    "}\n";
                File.WriteAllText(Path.Combine(lib1.Path, "appmanifest_250820.acf"), acf);
                File.WriteAllText(Path.Combine(lib2.Path, "appmanifest_250820.acf"), acf);

                var dict = new Dictionary<uint, string>();
                SteamLib.ScanAppManifestsInLibrary(lib1.Path, dict);
                // Second scan with the same appId should NOT throw.
                SteamLib.ScanAppManifestsInLibrary(lib2.Path, dict);

                // Last-wins semantics: the second library's path is what's recorded.
                Assert.Single(dict);
                Assert.Equal(Path.Combine(lib2.Path, "common", "SteamVR"), dict[250820u]);
            }
        }

        // Helpers --------------------------------------------------------------

        private static NestedElement ParseSingleEntry(string vdfContents)
        {
            // Write to a temp file because VDFFile only has a file-path ctor.
            using (var tmp = new TempFile(vdfContents))
            {
                var file = new VDFFile(tmp.Path);
                foreach (var pair in file.Elements)
                {
                    foreach (var child in pair.Value.Children)
                    {
                        return child.Value;
                    }
                }
            }
            return null;
        }

        private sealed class TempDir : IDisposable
        {
            public string Path { get; }
            public TempDir()
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    "leapplay-steamlib-test-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Path);
            }
            public void Dispose()
            {
                try { Directory.Delete(Path, recursive: true); }
                catch { /* best-effort */ }
            }
        }

        private sealed class TempFile : IDisposable
        {
            public string Path { get; }
            public TempFile(string contents)
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                    "leapplay-vdf-" + Guid.NewGuid().ToString("N") + ".vdf");
                File.WriteAllText(Path, contents);
            }
            public void Dispose()
            {
                try { File.Delete(Path); }
                catch { /* best-effort */ }
            }
        }
    }
}
