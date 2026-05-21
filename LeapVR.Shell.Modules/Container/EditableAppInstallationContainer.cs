#region Licence
/****************************************************************
 *  Filename: EditableAppInstallationContainer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2026-05-18
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using IAppInstallationHeaderSerializer = LeapVR.Shell.Domain.Models.Container.IAppInstallationHeaderSerializer;

namespace LeapVR.Shell.Modules.Container
{
    /// <summary>
    /// Edits an existing .vbox container in place. Designed for the operator's
    /// "rename / change description / re-target executable" use cases where
    /// the GameFiles package can be 50 GB+ and full extract-repack would be
    /// catastrophically wasteful.
    ///
    /// On-disk layout (cf. NewAppInstallationContainer.SaveToFiles):
    ///
    ///   [ pkg0_len(8) ][ pkg0 zip ][ pkg1_len(8) ][ pkg1 zip ] ...
    ///   [ header (protobuf) ][ header_start_offset(8) ]
    ///
    /// The header carries Dictionary&lt;IPackageData, long&gt; PackageDataFileOffsets,
    /// so any package's payload occupies a known contiguous byte range. The
    /// last 8 bytes of the file point to where the header starts.
    ///
    /// This class exposes three flavours of mutation, each with a known cost:
    ///   - DisplayName + ThumbnailAsBytes: header-only rewrite. Truncate at
    ///     headerStartOffset, append new header + new trailer. The package
    ///     payloads are not read or written.
    ///   - Replace the Metadata package's zip blob (typical case: edit
    ///     displayData.json / platformData.json inside it). Truncate at
    ///     metadataOffset, append [length-prefix][new metadata zip][header]
    ///     [trailer]. The GameFiles + MediaFiles payloads at offsets &lt;
    ///     metadataOffset are not touched. Requires the Metadata package to
    ///     be the last package by file-offset (this is the order
    ///     LeapVrContainerCreation emits them in).
    ///   - SaveHeaderOnly() is preferred when the caller only changed
    ///     DisplayName / Thumbnail; it is unconditional. Save() picks the
    ///     right strategy based on what was staged.
    ///
    /// All writes go to a sibling ".vbox.editing" temp file and are atomic-
    /// replaced over the original on success. On any exception the temp file
    /// is deleted and the original is untouched.
    /// </summary>
    public sealed class EditableAppInstallationContainer
    {
        private const int LongSize = sizeof(long);

        private readonly string _path;
        private readonly IAppInstallationHeaderSerializer _serializer;

        private NewAppInstallationHeader _header; // mutable in-memory copy
        private List<PackageEntry> _packages;     // sorted by file offset
        private byte[] _stagedMetadataZipBytes;   // null if metadata not changed
        private bool _displayNameChanged;
        private bool _thumbnailChanged;
        private bool _opened;

        public EditableAppInstallationContainer(string path,
            IAppInstallationHeaderSerializer serializer)
        {
            QuickLeap.AssertNotNull(path, serializer);
            _path = path;
            _serializer = serializer;
        }

        public Guid ApplicationGuid =>
            _opened ? _header.ApplicationGuid : Guid.Empty;
        public int Version => _opened ? _header.Version : 0;

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                EnsureOpened();
                if (_displayName == value) return;
                _displayName = value;
                _displayNameChanged = true;
            }
        }

        private byte[] _thumbnailAsBytes;
        public byte[] ThumbnailAsBytes
        {
            get => _thumbnailAsBytes;
            set
            {
                EnsureOpened();
                _thumbnailAsBytes = value;
                _thumbnailChanged = true;
            }
        }

        /// <summary>
        /// Read-only view of the container's packages, sorted by file offset.
        /// </summary>
        public IReadOnlyList<PackageEntry> Packages
        {
            get
            {
                EnsureOpened();
                return _packages;
            }
        }

        /// <summary>
        /// The trailing Metadata package entry, or null if there isn't one.
        /// </summary>
        public PackageEntry Metadata
        {
            get
            {
                EnsureOpened();
                for (int i = _packages.Count - 1; i >= 0; i--)
                {
                    if (_packages[i].Data.ContentType == ContentType.Metadata)
                        return _packages[i];
                }
                return null;
            }
        }

        // ---------------------------------------------------------------
        // open
        // ---------------------------------------------------------------

        public void Open()
        {
            using (var fs = File.Open(_path, FileMode.Open, FileAccess.Read))
            {
                long headerStartOffset = ReadHeaderStartOffset(fs);
                long headerLength = fs.Length - headerStartOffset - LongSize;

                fs.Seek(headerStartOffset, SeekOrigin.Begin);
                var headerBytes = new byte[headerLength];
                ReadExactly(fs, headerBytes, 0, headerBytes.Length);

                using (var ms = new MemoryStream(headerBytes, writable: false))
                {
                    var loaded = _serializer.LoadFromStream(ms);
                    var loadedHeader = loaded as IAppInstallationHeader
                        ?? throw new InvalidDataException(
                            "Header could not be cast to IAppInstallationHeader.");

                    _header = new NewAppInstallationHeader
                    {
                        ApplicationGuid = loadedHeader.ApplicationGuid,
                        Version = loadedHeader.Version,
                        DisplayName = loadedHeader.DisplayName,
                        ThumbnailAsBytes = loadedHeader.ThumbnailAsBytes,
                        TotalFilesCount = loadedHeader.TotalFilesCount,
                        TotalFilesSize = loadedHeader.TotalFilesSize,
                        PackageDataFileOffsets = loadedHeader.PackageDataFileOffsets
                            ?? new Dictionary<IPackageData, long>(),
                    };
                }

                _packages = _header.PackageDataFileOffsets
                    .Select(kv => new PackageEntry(kv.Key, kv.Value))
                    .OrderBy(p => p.FileOffset)
                    .ToList();
            }

            _displayName = _header.DisplayName;
            _thumbnailAsBytes = _header.ThumbnailAsBytes;
            _displayNameChanged = false;
            _thumbnailChanged = false;
            _stagedMetadataZipBytes = null;
            _opened = true;
        }

        // ---------------------------------------------------------------
        // package payload read access
        // ---------------------------------------------------------------

        /// <summary>
        /// Returns the raw zip-archive bytes for the named package (without
        /// the 8-byte length prefix). For the metadata package this is
        /// typically KB to MB; for game-content packages it can be many GB,
        /// so prefer ExtractPackageZipToFile for those.
        /// </summary>
        public byte[] ReadPackageZipBytes(Guid packageGuid)
        {
            EnsureOpened();
            var entry = _packages.FirstOrDefault(p =>
                p.Data.PackageGuid == packageGuid)
                ?? throw new ArgumentException(
                    $"Unknown package guid {packageGuid}",
                    nameof(packageGuid));

            using (var fs = File.Open(_path, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(entry.FileOffset, SeekOrigin.Begin);
                long len;
                using (var br = new BinaryReader(fs, Encoding.Default, true))
                {
                    len = br.ReadInt64();
                }
                var bytes = new byte[len];
                ReadExactly(fs, bytes, 0, (int)len);
                return bytes;
            }
        }

        // ---------------------------------------------------------------
        // staging
        // ---------------------------------------------------------------

        /// <summary>
        /// Stage a replacement zip-archive payload for the Metadata package.
        /// The replacement takes effect on Save(). The Metadata package must
        /// be the last package by file-offset (this is the LeapVrContainer-
        /// Creation order); otherwise Save() throws.
        /// </summary>
        public void StageMetadataPackageZip(byte[] newZipBytes)
        {
            EnsureOpened();
            QuickLeap.AssertNotNull(newZipBytes);
            _stagedMetadataZipBytes = newZipBytes;
        }

        // ---------------------------------------------------------------
        // save
        // ---------------------------------------------------------------

        /// <summary>
        /// Atomic save. Picks the cheapest strategy: header-only rewrite if
        /// nothing else was staged, metadata-tail rewrite if a new metadata
        /// zip was staged.
        /// </summary>
        public void Save()
        {
            EnsureOpened();
            if (_stagedMetadataZipBytes != null)
                SaveMetadataAndHeader();
            else if (_displayNameChanged || _thumbnailChanged)
                SaveHeaderOnly();
            // else: nothing changed; no-op.
        }

        /// <summary>
        /// Truncates the existing header + trailer and writes a new header
        /// (carrying current DisplayName / ThumbnailAsBytes) + new trailer.
        /// Touches zero of the package payload bytes.
        /// </summary>
        public void SaveHeaderOnly()
        {
            EnsureOpened();
            _header.DisplayName = _displayName;
            _header.ThumbnailAsBytes = _thumbnailAsBytes;

            var tmpPath = NewTempEditingPath();

            try
            {
                CopyTruncatedAndAppendHeader(tmpPath, truncateAt: GetHeaderStartOffset());
                // File.Replace is atomic on NTFS (single transactional rename).
                // The third argument requests no backup file.
                File.Replace(tmpPath, _path, destinationBackupFileName: null);
                _displayNameChanged = false;
                _thumbnailChanged = false;
            }
            catch
            {
                TryDeleteWithRetry(tmpPath);
                throw;
            }
        }

        /// <summary>
        /// Truncates at the metadata-package's file offset and writes:
        ///   [length-prefix][staged metadata zip][header][trailer].
        /// </summary>
        public void SaveMetadataAndHeader()
        {
            EnsureOpened();
            if (_stagedMetadataZipBytes == null)
                throw new InvalidOperationException(
                    "No metadata package staged; call StageMetadataPackageZip first.");

            var metadata = Metadata
                ?? throw new InvalidOperationException(
                    "Container has no Metadata package to replace.");
            var maxOffset = _packages.Max(p => p.FileOffset);
            if (metadata.FileOffset != maxOffset)
                throw new InvalidOperationException(
                    "Metadata package is not the last package by file offset; "
                    + "in-place metadata replacement is only safe when Metadata "
                    + "is the trailing package. Build the container with the "
                    + "canonical GameFiles -> MediaFiles -> Metadata order.");

            // Update the in-memory header to reflect the new metadata package
            // sizes. The package's file offset is unchanged.
            //
            // TotalFilesCount and TotalFilesSize are the SUM over all
            // packages' TotalFilesCount / TotalFilesSize members. The
            // metadata package's contribution may change if files were added
            // or removed inside the new zip; ask the caller to update
            // metadata.Data.TotalFilesCount / TotalFilesSize via the same
            // PackageDataDto before staging.

            _header.DisplayName = _displayName;
            _header.ThumbnailAsBytes = _thumbnailAsBytes;
            _header.TotalFilesCount = _packages.Sum(p => p.Data.TotalFilesCount);
            _header.TotalFilesSize = _packages.Sum(p => p.Data.TotalFilesSize);

            var tmpPath = NewTempEditingPath();

            try
            {
                CopyTruncatedAndAppendHeader(
                    tmpPath,
                    truncateAt: metadata.FileOffset,
                    appendPackagePayload: true);
                File.Replace(tmpPath, _path, destinationBackupFileName: null);

                _stagedMetadataZipBytes = null;
                _displayNameChanged = false;
                _thumbnailChanged = false;
            }
            catch
            {
                TryDeleteWithRetry(tmpPath);
                throw;
            }
        }

        // ---------------------------------------------------------------
        // helpers
        // ---------------------------------------------------------------

        private void EnsureOpened()
        {
            if (!_opened)
                throw new InvalidOperationException(
                    "Container is not opened; call Open() first.");
        }

        /// <summary>
        /// Build a fresh temp-file path in the same directory as the .vbox.
        /// Using a randomized suffix avoids two failure modes:
        ///   - a previous crashed save leaving a stale .editing file that
        ///     Defender / Search Indexer is still scanning (the open handle
        ///     makes File.Delete throw "used by another process"); and
        ///   - two concurrent edit sessions colliding on the same fixed
        ///     temp filename.
        /// File.Replace requires the temp file to live on the same volume
        /// as the destination, so we deliberately stay in the .vbox's own
        /// directory rather than using %TEMP%.
        /// </summary>
        private string NewTempEditingPath()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            return _path + ".editing-" + suffix;
        }

        /// <summary>
        /// Best-effort cleanup of a temp file with short backoff. Defender's
        /// post-create scan typically releases the handle within a second;
        /// a few quick retries cover the common case without making save
        /// failures slow. Never throws — used only on the error path where
        /// the caller is already propagating a higher-priority exception.
        /// </summary>
        private static void TryDeleteWithRetry(string path)
        {
            if (!File.Exists(path)) return;
            int[] delaysMs = { 0, 50, 200, 500, 1000 };
            foreach (var delay in delaysMs)
            {
                if (delay > 0) System.Threading.Thread.Sleep(delay);
                try { File.Delete(path); return; }
                catch (IOException) { /* retry */ }
                catch (UnauthorizedAccessException) { /* retry */ }
            }
        }

        private long GetHeaderStartOffset()
        {
            using (var fs = File.Open(_path, FileMode.Open, FileAccess.Read))
            {
                return ReadHeaderStartOffset(fs);
            }
        }

        private static long ReadHeaderStartOffset(FileStream fs)
        {
            fs.Seek(-LongSize, SeekOrigin.End);
            using (var br = new BinaryReader(fs, Encoding.Default, true))
            {
                return br.ReadInt64();
            }
        }

        /// <summary>
        /// Copies bytes 0..truncateAt from the source to tmpPath, then
        /// optionally appends [length-prefix][staged metadata zip], then
        /// appends [protobuf header][8-byte trailer-offset].
        /// </summary>
        private void CopyTruncatedAndAppendHeader(
            string tmpPath,
            long truncateAt,
            bool appendPackagePayload = false)
        {
            using (var src = File.Open(_path, FileMode.Open, FileAccess.Read))
            using (var dst = File.Open(tmpPath, FileMode.CreateNew, FileAccess.Write))
            {
                CopyExactly(src, dst, truncateAt);

                if (appendPackagePayload)
                {
                    using (var bw = new BinaryWriter(dst, Encoding.Default, true))
                    {
                        bw.Write((long)_stagedMetadataZipBytes.Length);
                    }
                    dst.Write(_stagedMetadataZipBytes, 0,
                        _stagedMetadataZipBytes.Length);
                }

                long newHeaderStart = dst.Position;
                _serializer.SaveToStream(dst, _header);

                using (var bw = new BinaryWriter(dst, Encoding.Default, true))
                {
                    bw.Write(newHeaderStart);
                }
            }
        }

        private static void CopyExactly(Stream src, Stream dst, long count)
        {
            const int bufSize = 1024 * 1024;
            var buffer = new byte[bufSize];
            long remaining = count;
            while (remaining > 0)
            {
                int want = (int)Math.Min(remaining, bufSize);
                int got = src.Read(buffer, 0, want);
                if (got == 0)
                    throw new EndOfStreamException(
                        $"Unexpected EOF while copying {count} bytes; "
                        + $"{remaining} bytes still wanted.");
                dst.Write(buffer, 0, got);
                remaining -= got;
            }
        }

        private static void ReadExactly(Stream src, byte[] buffer,
            int offset, int count)
        {
            int read = 0;
            while (read < count)
            {
                int got = src.Read(buffer, offset + read, count - read);
                if (got == 0)
                    throw new EndOfStreamException(
                        $"Expected {count} bytes, got {read} before EOF.");
                read += got;
            }
        }

        /// <summary>
        /// Lightweight tuple of a package's metadata + its data-file offset.
        /// Sorted-by-offset list lets callers reason about which package is
        /// the trailing one without having to re-walk the dict.
        /// Class (not struct) so Metadata/lookup-misses can return null.
        /// </summary>
        public sealed class PackageEntry
        {
            public PackageEntry(IPackageData data, long fileOffset)
            {
                Data = data;
                FileOffset = fileOffset;
            }
            public IPackageData Data { get; }
            public long FileOffset { get; }
        }
    }
}
