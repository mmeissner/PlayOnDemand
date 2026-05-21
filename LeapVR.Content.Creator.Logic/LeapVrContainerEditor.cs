#region Licence
/****************************************************************
 *  Filename: LeapVrContainerEditor.cs
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Content.Shared;
using LeapVR.Content.Shared.Container;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Modules.Container;
using Newtonsoft.Json;

namespace LeapVR.Content.Creator.Logic
{
    /// <summary>
    /// Opens an existing .vbox container for partial-edit. Designed for the
    /// "operator wants to fix the title / description / executable path of a
    /// previously-packaged game" use cases where the GameFiles package can be
    /// 50 GB+ and full extract-repack would be catastrophic.
    ///
    /// Edits are scoped to:
    ///   - DisplayName + ThumbnailAsBytes (header-only rewrite, touches no
    ///     package payload bytes);
    ///   - displayData.json + platformData.json inside the Metadata package
    ///     (rewrites only the trailing metadata zip + header + trailer).
    ///
    /// The on-disk save is delegated to EditableAppInstallationContainer; this
    /// class adds the JSON-DTO marshalling and the WizardModule plumbing.
    /// </summary>
    public class LeapVrContainerEditor : IWizardModule
    {
        // The displayData.json and platformData.json files live in this
        // sub-directory inside the Metadata package's zip. Must match the
        // path used by LeapVrContainerCreation.CreateContainerAsyncLogic.
        private const string MetadataDbDir = "database";
        private const string DisplayDataJsonName = "displayData.json";
        private const string PlatformDataJsonName = "platformData.json";

        private readonly bool _isValid;
        private readonly EditableAppInstallationContainer _editor;
        private readonly ContainerModule _containerModule;
        private readonly Exception _occuredException;

        // ---------------------------------------------------------------
        // public surface
        // ---------------------------------------------------------------

        public bool IsValid => _isValid;
        public Exception OccuredException => _occuredException;
        public string VboxFilePath { get; }

        public Guid ApplicationGuid => _editor?.ApplicationGuid ?? Guid.Empty;
        public int Version => _editor?.Version ?? 0;

        /// <summary>
        /// Snapshot of the package data the editor knows about (sorted by
        /// file offset). Surface for callers that need to roll up sizes or
        /// inspect content types - the WPF wizard's ContainerInfo view-model
        /// uses this.
        /// </summary>
        public System.Collections.Generic.IEnumerable<IPackageData> Packages
            => _editor?.Packages.Select(p => p.Data)
               ?? System.Linq.Enumerable.Empty<IPackageData>();

        /// <summary>
        /// View-model bag the WPF edit wizard binds against. Lazily built
        /// on first access and cached; recreated on Reload().
        /// </summary>
        public ContainerInfo ContainerInfo { get; private set; }

        /// <summary>Mutable. Persists on Save / DoWork.</summary>
        public string DisplayName
        {
            get => _editor?.DisplayName;
            set
            {
                if (_editor == null) return;
                _editor.DisplayName = value;
            }
        }

        /// <summary>Mutable. Persists on Save / DoWork.</summary>
        public byte[] ThumbnailAsBytes
        {
            get => _editor?.ThumbnailAsBytes;
            set
            {
                if (_editor == null) return;
                _editor.ThumbnailAsBytes = value;
            }
        }

        /// <summary>
        /// Mutable display-metadata DTO loaded from displayData.json inside
        /// the Metadata package. Re-serialised on DoWork.
        /// </summary>
        public AppDisplayDataDto DisplayData { get; private set; }

        /// <summary>
        /// Mutable platform-metadata DTO loaded from platformData.json. Holds
        /// execution-logic-instructions list and platform plugin id.
        /// Re-serialised on DoWork.
        /// </summary>
        public AppPlatformDataDto PlatformData { get; private set; }

        // ---------------------------------------------------------------
        // open
        // ---------------------------------------------------------------

        public LeapVrContainerEditor(string vBoxAppFilePath)
        {
            VboxFilePath = vBoxAppFilePath;
            try
            {
                var headerFileInfo = new FileInfo(vBoxAppFilePath);
                if (!headerFileInfo.Exists)
                {
                    _isValid = false;
                    return;
                }
                var headerSerializer = new AppInstallationHeaderSerializer();
                _containerModule = new ContainerModule(headerSerializer);
                _editor = _containerModule.OpenForEdit(vBoxAppFilePath);

                LoadMetadataDtos();
                ContainerInfo = new ContainerInfo(this);

                _isValid = true;
            }
            catch (Exception e)
            {
                _occuredException = e;
                _isValid = false;
            }
        }

        // ---------------------------------------------------------------
        // save
        // ---------------------------------------------------------------

        /// <summary>
        /// Persists pending edits. Picks the cheapest write path:
        ///   - if only DisplayName / ThumbnailAsBytes changed, header-only
        ///     rewrite (touches no package payload bytes);
        ///   - if any DTO field changed, rebuild the Metadata package's zip
        ///     in memory, then truncate-from-metadata + append.
        /// </summary>
        public Task DoWork()
        {
            QuickLeap.AssertNotNull(_editor);
            return Task.Run(() => Save());
        }

        // 0 = idle, 1 = in-progress. Belt-and-braces guard so a double-click in
        // the wizard (or any other caller) can never race two Save() invocations
        // against the same .vbox file. The WPF button is already gated by
        // WizardViewModel.CanCreate, but UI events can be dispatched in
        // duplicate before the property-change fires.
        private int _saveInProgress;

        public void Save()
        {
            QuickLeap.AssertNotNull(_editor);

            if (System.Threading.Interlocked.CompareExchange(
                    ref _saveInProgress, 1, 0) != 0)
            {
                throw new InvalidOperationException(
                    "A Save() is already in progress for this editor.");
            }
            try
            {
                // Always re-serialise the JSON DTOs into a new metadata zip and
                // stage it. The header-only fast path is taken by the editor if
                // nothing on the metadata side was staged - we want to be sure
                // any DTO mutations land, so we stage unconditionally here.
                //
                // (A future optimisation: track per-field-changed flags on the
                // DTOs and skip metadata staging when no DTO property was
                // touched. For now the metadata zip is KB-MB anyway so the
                // overhead is negligible.)
                var newMetadataZipBytes = BuildMetadataZip(DisplayData, PlatformData);
                _editor.StageMetadataPackageZip(newMetadataZipBytes);
                _editor.Save();
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _saveInProgress, 0);
            }
        }

        // ---------------------------------------------------------------
        // helpers
        // ---------------------------------------------------------------

        private void LoadMetadataDtos()
        {
            // The metadata package was written by Ionic.Zip with Zip64
            // extensions forced on, which System.IO.Compression.ZipArchive
            // can't always parse cleanly on small entries. Use the existing
            // production read pipeline (ZipReadablePackage.ExtractToDirectory,
            // which uses Ionic.Zip on the read side too) to extract to a
            // temp dir; then load the JSON files via straight File.ReadAllText.
            var readable = _containerModule.GetAppInstallationContainer(VboxFilePath);
            var metadataPkg = readable.GetPackages()
                .FirstOrDefault(p => p.ContentType == ContentType.Metadata)
                ?? throw new InvalidDataException(
                    "Container has no Metadata package; this .vbox was "
                    + "written by an older schema and cannot be edited "
                    + "in place. Reconstruct it via the Create wizard.");

            var tempDir = Path.Combine(Path.GetTempPath(),
                "PoDEdit-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                metadataPkg.ExtractToDirectory(tempDir);
                var displayDataPath = Path.Combine(tempDir,
                    MetadataDbDir, DisplayDataJsonName);
                var platformDataPath = Path.Combine(tempDir,
                    MetadataDbDir, PlatformDataJsonName);
                if (!File.Exists(displayDataPath))
                    throw new InvalidDataException(
                        $"Metadata package is missing '{DisplayDataJsonName}'.");
                if (!File.Exists(platformDataPath))
                    throw new InvalidDataException(
                        $"Metadata package is missing '{PlatformDataJsonName}'.");
                DisplayData = ContainerJsonSerializer
                    .DeserializeObject<AppDisplayDataDto>(
                        File.ReadAllText(displayDataPath, Encoding.UTF8));
                PlatformData = ContainerJsonSerializer
                    .DeserializeObject<AppPlatformDataDto>(
                        File.ReadAllText(platformDataPath, Encoding.UTF8));
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); }
                catch { /* nothing */ }
            }
        }

        private static byte[] BuildMetadataZip(
            AppDisplayDataDto displayData, AppPlatformDataDto platformData)
        {
            // System.IO.Compression.ZipArchive is fine here - the metadata
            // package is KB-scale, never close to the 4 GB / Zip64 boundary
            // the GameFiles + MediaFiles paths need. UTF-8 entry names are
            // the default on .NET Framework 4.7.1.
            var displayJson = ContainerJsonSerializer.SerializeObject(
                displayData, Formatting.Indented);
            var platformJson = ContainerJsonSerializer.SerializeObject(
                platformData, Formatting.Indented);

            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                    AddTextEntry(zip,
                        MetadataDbDir + "/" + DisplayDataJsonName, displayJson);
                    AddTextEntry(zip,
                        MetadataDbDir + "/" + PlatformDataJsonName, platformJson);
                }
                return ms.ToArray();
            }
        }

        private static void AddTextEntry(ZipArchive zip, string entryName, string text)
        {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(text);
            }
        }
    }
}
