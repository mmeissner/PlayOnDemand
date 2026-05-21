#region Licence
/****************************************************************
 *  Filename: EditableAppInstallationContainerTests.cs
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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Modules.Container;
using Xunit;

namespace LeapVR.Shell.Modules.Container.Test
{
    /// <summary>
    /// Round-trip tests for EditableAppInstallationContainer. Each test builds
    /// a tiny container via the existing NewAppInstallationContainer write
    /// path, edits it, and verifies:
    ///   - the edits persisted on re-open;
    ///   - the byte-range BEFORE the edited package is identical to the
    ///     pre-edit state (proves we don't rewrite the 50 GB GameFiles
    ///     payload on a metadata-only edit);
    ///   - the container's ApplicationGuid / Version / package count are
    ///     preserved.
    /// </summary>
    public class EditableAppInstallationContainerTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly AppInstallationHeaderSerializer _serializer;
        private readonly ContainerModule _module;

        public EditableAppInstallationContainerTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),
                "PoDEditTest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _serializer = new AppInstallationHeaderSerializer();
            _module = new ContainerModule(_serializer);
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempDir, recursive: true); }
            catch { /* nothing */ }
        }

        // ====================================================================
        // open
        // ====================================================================

        [Fact]
        public void Open_HydratesHeaderAndPackages()
        {
            var vbox = BuildContainer("Smoke-Open");
            var editor = _module.OpenForEdit(vbox);

            Assert.NotEqual(Guid.Empty, editor.ApplicationGuid);
            Assert.Equal(2, editor.Version);
            Assert.Equal("Smoke-Open", editor.DisplayName);
            Assert.NotNull(editor.ThumbnailAsBytes);
            Assert.Equal(3, editor.Packages.Count); // GameFiles + MediaFiles + Metadata
            Assert.Contains(editor.Packages,
                p => p.Data.ContentType == ContentType.GameFiles);
            Assert.Contains(editor.Packages,
                p => p.Data.ContentType == ContentType.MediaFiles);
            Assert.NotNull(editor.Metadata);
        }

        [Fact]
        public void Metadata_IsTheLastPackageByOffset()
        {
            var vbox = BuildContainer("Smoke-Order");
            var editor = _module.OpenForEdit(vbox);

            var sortedByOffset = editor.Packages
                .OrderBy(p => p.FileOffset).ToList();
            Assert.Equal(ContentType.Metadata,
                sortedByOffset.Last().Data.ContentType);
        }

        // ====================================================================
        // SaveHeaderOnly: DisplayName + Thumbnail rewrites must NOT touch
        // any package payload bytes.
        // ====================================================================

        [Fact]
        public void SaveHeaderOnly_PersistsDisplayName_AndLeavesAllPackagesByteIdentical()
        {
            var vbox = BuildContainer("Before-Rename");
            long originalSize = new FileInfo(vbox).Length;

            // Snapshot the entire package-payload region (everything before
            // the header).
            long headerStartBefore = ReadHeaderStartOffset(vbox);
            string hashBefore = HashFileRange(vbox, 0, headerStartBefore);

            // Edit and save.
            var editor = _module.OpenForEdit(vbox);
            editor.DisplayName = "After-Rename";
            editor.SaveHeaderOnly();

            // The package-payload region must be byte-for-byte unchanged.
            long headerStartAfter = ReadHeaderStartOffset(vbox);
            Assert.Equal(headerStartBefore, headerStartAfter);
            string hashAfter = HashFileRange(vbox, 0, headerStartAfter);
            Assert.Equal(hashBefore, hashAfter);

            // Re-open and verify the edit persisted.
            var reopened = _module.OpenForEdit(vbox);
            Assert.Equal("After-Rename", reopened.DisplayName);
        }

        [Fact]
        public void SaveHeaderOnly_PersistsThumbnail()
        {
            var vbox = BuildContainer("Smoke-Thumb");
            var newThumb = Enumerable.Range(0, 128)
                .Select(i => (byte)(i & 0xFF)).ToArray();

            var editor = _module.OpenForEdit(vbox);
            editor.ThumbnailAsBytes = newThumb;
            editor.SaveHeaderOnly();

            var reopened = _module.OpenForEdit(vbox);
            Assert.Equal(newThumb, reopened.ThumbnailAsBytes);
        }

        // ====================================================================
        // ReadPackageZipBytes returns a byte-for-byte copy of the package's
        // zip payload. The bytes are a valid zip at the directory-listing
        // level - but the production-created metadata package is written
        // by Ionic.Zip with Zip64.Always, and System.IO.Compression.ZipArchive
        // does NOT support that flavour for per-entry content reads. So we
        // can enumerate entries with SIC's ZipArchive but NOT read entry
        // content. The LeapVrContainerEditor uses the production
        // ZipReadablePackage path (also Ionic-based) for content reads.
        // ====================================================================

        [Fact]
        public void ReadPackageZipBytes_EnumeratesEntriesViaStdZipArchive()
        {
            var vbox = BuildContainer("Smoke-Read");
            var editor = _module.OpenForEdit(vbox);
            var metaBytes = editor.ReadPackageZipBytes(
                editor.Metadata.Data.PackageGuid);

            using (var ms = new System.IO.MemoryStream(metaBytes, writable: false))
            using (var zip = new System.IO.Compression.ZipArchive(
                ms, System.IO.Compression.ZipArchiveMode.Read))
            {
                var entryNames = zip.Entries.Select(e => e.FullName).ToList();
                Assert.Contains(entryNames,
                    n => n.EndsWith("displayData.json",
                        StringComparison.OrdinalIgnoreCase));
                Assert.Contains(entryNames,
                    n => n.EndsWith("platformData.json",
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Documents the SIC-vs-Ionic-Zip64 incompatibility: SIC's
        /// ZipArchive.Entry.Open() rejects the per-entry local file header
        /// Ionic writes when Zip64Option.Always is set on a small entry.
        /// LeapVrContainerEditor sidesteps this by extracting via the
        /// production ZipReadablePackage path (Ionic-based reader).
        /// </summary>
        [Fact]
        public void ReadPackageZipBytes_KnownIncompatibility_StdZipArchiveCannotReadIonicZip64Entries()
        {
            var vbox = BuildContainer("Smoke-Incompat");
            var editor = _module.OpenForEdit(vbox);
            var metaBytes = editor.ReadPackageZipBytes(
                editor.Metadata.Data.PackageGuid);

            using (var ms = new System.IO.MemoryStream(metaBytes, writable: false))
            using (var zip = new System.IO.Compression.ZipArchive(
                ms, System.IO.Compression.ZipArchiveMode.Read))
            {
                var entry = zip.Entries.First();
                Assert.Throws<InvalidDataException>(() =>
                {
                    using (var _ = entry.Open()) { }
                });
            }
        }

        // ====================================================================
        // SaveMetadataAndHeader: replacing the metadata package must NOT
        // touch GameFiles / MediaFiles bytes.
        // ====================================================================

        [Fact]
        public void SaveMetadataAndHeader_LeavesGameFilesAndMediaFilesByteIdentical()
        {
            var vbox = BuildContainer("Smoke-Meta");

            var preEdit = _module.OpenForEdit(vbox);
            long metadataOffset = preEdit.Metadata.FileOffset;
            string hashBefore = HashFileRange(vbox, 0, metadataOffset);

            // Build a tiny new metadata zip (just one trivial file).
            byte[] newMetadataZip = BuildSyntheticMetadataZip(
                displayDataJson: "{\"name\":\"Edited\",\"description\":\"new desc\"}",
                platformDataJson: "{\"applicationGuid\":\""
                    + preEdit.ApplicationGuid + "\"}");

            preEdit.StageMetadataPackageZip(newMetadataZip);
            preEdit.SaveMetadataAndHeader();

            // The bytes from 0..metadataOffset MUST be identical.
            // (This is the 50-GB-safety guarantee.)
            string hashAfter = HashFileRange(vbox, 0, metadataOffset);
            Assert.Equal(hashBefore, hashAfter);

            // The metadata package's offset stays put.
            var reopened = _module.OpenForEdit(vbox);
            Assert.Equal(metadataOffset, reopened.Metadata.FileOffset);

            // The new payload is what we wrote.
            var roundTripped = reopened.ReadPackageZipBytes(
                reopened.Metadata.Data.PackageGuid);
            Assert.Equal(newMetadataZip, roundTripped);
        }

        [Fact]
        public void SaveMetadataAndHeader_AlsoFlushesDisplayNameAndThumb()
        {
            var vbox = BuildContainer("Smoke-Combined");
            var editor = _module.OpenForEdit(vbox);

            editor.DisplayName = "Combined-After";
            editor.ThumbnailAsBytes = new byte[] { 1, 2, 3, 4 };
            editor.StageMetadataPackageZip(
                BuildSyntheticMetadataZip("{}", "{}"));
            editor.SaveMetadataAndHeader();

            var reopened = _module.OpenForEdit(vbox);
            Assert.Equal("Combined-After", reopened.DisplayName);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, reopened.ThumbnailAsBytes);
        }

        [Fact]
        public void Save_NoChanges_DoesNotMutateFile()
        {
            var vbox = BuildContainer("Smoke-Noop");
            string fullHashBefore = HashFileRange(vbox, 0, new FileInfo(vbox).Length);

            var editor = _module.OpenForEdit(vbox);
            editor.Save(); // nothing staged or changed

            string fullHashAfter = HashFileRange(vbox, 0, new FileInfo(vbox).Length);
            Assert.Equal(fullHashBefore, fullHashAfter);
        }

        // ====================================================================
        // A stale .editing sibling from a previous crashed save (or held open
        // by Defender / Indexer) must NOT block a fresh save. The current
        // strategy is to use a unique random-suffix temp name per save, so
        // collisions are impossible and the stale file is left alone.
        // ====================================================================

        [Fact]
        public void SaveHeaderOnly_UnaffectedByStaleEditingSibling()
        {
            var vbox = BuildContainer("Smoke-Stale");
            var staleTmp = vbox + ".editing";
            File.WriteAllBytes(staleTmp, new byte[] { 0xFF, 0xFE, 0xFD });
            Assert.True(File.Exists(staleTmp));

            var editor = _module.OpenForEdit(vbox);
            editor.DisplayName = "After-Recovery";
            editor.SaveHeaderOnly();

            var reopened = _module.OpenForEdit(vbox);
            Assert.Equal("After-Recovery", reopened.DisplayName);
        }

        // ====================================================================
        // helpers
        // ====================================================================

        /// <summary>
        /// Builds a tiny .vbox container with 3 packages (GameFiles +
        /// MediaFiles + Metadata) in the canonical order. Tiny so tests run
        /// fast. Returns the .vbox path.
        /// </summary>
        private string BuildContainer(string displayName)
        {
            var appGuid = Guid.NewGuid();
            var workDir = Path.Combine(_tempDir, "build-" + appGuid.ToString("N"));
            Directory.CreateDirectory(workDir);

            // game files: one trivial file
            var gameDir = Path.Combine(workDir, "game");
            Directory.CreateDirectory(gameDir);
            File.WriteAllText(Path.Combine(gameDir, "main.exe"),
                "fake-game-exe " + appGuid);

            // media files: tiny "image"
            var mediaDir = Path.Combine(workDir, "media");
            Directory.CreateDirectory(mediaDir);
            File.WriteAllBytes(Path.Combine(mediaDir, "mainPicture.png"),
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4 });

            // metadata files: two json blobs
            var metaDir = Path.Combine(workDir, "metadata", "database");
            Directory.CreateDirectory(metaDir);
            File.WriteAllText(Path.Combine(metaDir, "displayData.json"),
                "{\"applicationGuid\":\"" + appGuid + "\",\"name\":\""
                + displayName + "\"}");
            File.WriteAllText(Path.Combine(metaDir, "platformData.json"),
                "{\"applicationGuid\":\"" + appGuid + "\"}");

            var newContainer = _module
                .CreateNewApplicationInstallationContainer(appGuid);
            newContainer.Initialize();
            newContainer.Version = 2;
            newContainer.DisplayName = displayName;
            newContainer.ThumbnailAsBytes = new byte[] { 0xAA, 0xBB, 0xCC };

            var gamePkg = newContainer.AddNewPackage(ContentType.GameFiles);
            gamePkg.PackageVersion = 1;
            gamePkg.AddDirectory(gameDir, "");

            var mediaPkg = newContainer.AddNewPackage(ContentType.MediaFiles);
            mediaPkg.PackageVersion = 1;
            mediaPkg.AddDirectory(mediaDir, "images");

            var metaPkg = newContainer.AddNewPackage(ContentType.Metadata);
            metaPkg.PackageVersion = 1;
            metaPkg.AddDirectory(Path.Combine(workDir, "metadata"), "");

            var vboxPath = Path.Combine(_tempDir,
                "smoke-" + appGuid.ToString("N") + ".vbox");
            newContainer.SaveToFiles(vboxPath);
            return vboxPath;
        }

        /// <summary>
        /// Builds an in-memory zip with two JSON files in database/, suitable
        /// for staging via EditableAppInstallationContainer.StageMetadataPackageZip.
        /// </summary>
        private static byte[] BuildSyntheticMetadataZip(string displayDataJson,
            string platformDataJson)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipFile())
                {
                    zip.AlternateEncoding = Encoding.UTF8;
                    zip.AlternateEncodingUsage = ZipOption.Always;
                    zip.AddEntry("database/displayData.json", displayDataJson);
                    zip.AddEntry("database/platformData.json", platformDataJson);
                    zip.Save(ms);
                }
                return ms.ToArray();
            }
        }

        private static long ReadHeaderStartOffset(string vboxPath)
        {
            using (var fs = File.Open(vboxPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(-sizeof(long), SeekOrigin.End);
                using (var br = new BinaryReader(fs))
                {
                    return br.ReadInt64();
                }
            }
        }

        private static string HashFileRange(string path, long from, long count)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var sha = SHA256.Create())
            {
                fs.Seek(from, SeekOrigin.Begin);
                var buffer = new byte[64 * 1024];
                long remaining = count;
                while (remaining > 0)
                {
                    int want = (int)Math.Min(remaining, buffer.Length);
                    int got = fs.Read(buffer, 0, want);
                    if (got == 0) break;
                    sha.TransformBlock(buffer, 0, got, null, 0);
                    remaining -= got;
                }
                sha.TransformFinalBlock(new byte[0], 0, 0);
                return BitConverter.ToString(sha.Hash).Replace("-", "");
            }
        }
    }
}
