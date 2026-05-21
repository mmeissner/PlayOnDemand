#region Licence
/****************************************************************
 *  Filename: LeapVrContainerEditorTests.cs
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
using System.Security.Cryptography;
using System.Text;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Shared;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Modules.Container;
using Newtonsoft.Json;
using Xunit;

namespace LeapVR.Shell.Modules.Container.Test
{
    /// <summary>
    /// End-to-end tests for the LeapVrContainerEditor surface that the
    /// Content Creator's edit wizard uses: open .vbox -> mutate DTOs ->
    /// DoWork() -> re-open -> verify edits persisted AND that no game
    /// payload bytes were rewritten.
    /// </summary>
    public class LeapVrContainerEditorTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly AppInstallationHeaderSerializer _serializer;
        private readonly ContainerModule _module;

        public LeapVrContainerEditorTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(),
                "PoDEditorTest-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _serializer = new AppInstallationHeaderSerializer();
            _module = new ContainerModule(_serializer);
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempDir, recursive: true); }
            catch { /* nothing */ }
        }

        [Fact]
        public void Open_HydratesDtosFromMetadataJson()
        {
            var vbox = BuildContainer("Open-Test", originalDescription: "Original desc");
            var editor = new LeapVrContainerEditor(vbox);

            Assert.True(editor.IsValid,
                "ctor failed: " + editor.OccuredException);
            Assert.NotEqual(Guid.Empty, editor.ApplicationGuid);
            Assert.Equal("Open-Test", editor.DisplayName);
            Assert.NotNull(editor.DisplayData);
            Assert.Equal("Original desc", editor.DisplayData.Description);
            Assert.NotNull(editor.PlatformData);
            Assert.Single(editor.PlatformData.ExecutionLogicInstructions);
        }

        [Fact]
        public async System.Threading.Tasks.Task DoWork_PersistsDescriptionEdit_AndLeavesGameFilesByteIdentical()
        {
            var vbox = BuildContainer("Edit-Desc", originalDescription: "before");
            var probe = _module.OpenForEdit(vbox);
            long metadataOffsetBefore = probe.Metadata.FileOffset;
            string hashBefore = HashFileRange(vbox, 0, metadataOffsetBefore);

            var editor = new LeapVrContainerEditor(vbox);
            Assert.True(editor.IsValid);
            editor.DisplayData.Description = "after-edit";
            editor.DisplayData.Category = "ActionVR";
            await editor.DoWork();

            // Everything up to (and not including) the metadata package
            // must be byte-identical. This is the 50-GB-safety property.
            var verify = _module.OpenForEdit(vbox);
            Assert.Equal(metadataOffsetBefore, verify.Metadata.FileOffset);
            string hashAfter = HashFileRange(vbox, 0, metadataOffsetBefore);
            Assert.Equal(hashBefore, hashAfter);

            // Re-open and confirm the DTO edit persisted.
            var reopened = new LeapVrContainerEditor(vbox);
            Assert.Equal("after-edit", reopened.DisplayData.Description);
            Assert.Equal("ActionVR", reopened.DisplayData.Category);
        }

        [Fact]
        public async System.Threading.Tasks.Task DoWork_PersistsExecutionParametersEdit()
        {
            var vbox = BuildContainer("Edit-Args");
            var editor = new LeapVrContainerEditor(vbox);

            var firstExec = (ProcessExecutionLogicDto)editor.PlatformData
                .ExecutionLogicInstructions.ToList()[0];
            firstExec.ExecutionParameters = "--fullscreen --novr";

            await editor.DoWork();

            var reopened = new LeapVrContainerEditor(vbox);
            Assert.Equal("--fullscreen --novr",
                reopened.PlatformData.ExecutionLogicInstructions.ToList()[0]
                    .ExecutionParameters);
        }

        [Fact]
        public async System.Threading.Tasks.Task DoWork_PersistsDisplayNameViaHeader()
        {
            var vbox = BuildContainer("Header-Name");
            var editor = new LeapVrContainerEditor(vbox);
            editor.DisplayName = "Renamed-Final";
            await editor.DoWork();

            var reopened = new LeapVrContainerEditor(vbox);
            Assert.Equal("Renamed-Final", reopened.DisplayName);
        }

        [Fact]
        public void Open_NonExistentFile_ReportsInvalid()
        {
            var editor = new LeapVrContainerEditor(
                Path.Combine(_tempDir, "does-not-exist.vbox"));
            Assert.False(editor.IsValid);
        }

        [Fact]
        public void Open_MalformedFile_ReportsInvalidWithException()
        {
            var bogusPath = Path.Combine(_tempDir, "bogus.vbox");
            File.WriteAllBytes(bogusPath, new byte[] { 1, 2, 3 });
            var editor = new LeapVrContainerEditor(bogusPath);
            Assert.False(editor.IsValid);
            Assert.NotNull(editor.OccuredException);
        }

        // ====================================================================
        // helpers
        // ====================================================================

        /// <summary>
        /// Builds a tiny .vbox via the existing create path with full DTO
        /// metadata (displayData.json + platformData.json) so the editor has
        /// realistic content to load.
        /// </summary>
        private string BuildContainer(string displayName,
            string originalDescription = "test description")
        {
            var appGuid = Guid.NewGuid();
            var workDir = Path.Combine(_tempDir, "build-" + appGuid.ToString("N"));
            Directory.CreateDirectory(workDir);

            var gameDir = Path.Combine(workDir, "game");
            Directory.CreateDirectory(gameDir);
            File.WriteAllText(Path.Combine(gameDir, "launch.exe"),
                "fake-game " + appGuid);

            var mediaDir = Path.Combine(workDir, "media");
            Directory.CreateDirectory(mediaDir);
            File.WriteAllBytes(Path.Combine(mediaDir, "mainPicture.png"),
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4 });

            // Build the metadata DTOs the same way LeapVrContainerCreation
            // does so the editor finds the expected JSON shape.
            var mediaPkgGuid = Guid.NewGuid();
            var gamePkgGuid = Guid.NewGuid();

            var displayData = new AppDisplayDataDto
            {
                ApplicationGuid = appGuid,
                Name = displayName,
                Description = originalDescription,
                Category = "casual",
                Tags = new[] { "smoke", "vbox-edit" },
                MainPicture = new DiskEntityDto
                {
                    ApplicationGuid = appGuid,
                    PackageGuid = mediaPkgGuid,
                    RelativePath = "images/mainPicture.png",
                },
            };

            var platformData = new AppPlatformDataDto
            {
                ApplicationGuid = appGuid,
                PlatformPluginId = Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3"),
                ExecutionLogicInstructions = new System.Collections.Generic.List<ProcessExecutionLogicDto>
                {
                    new ProcessExecutionLogicDto
                    {
                        ApplicationGuid = appGuid,
                        PlatformPluginId = Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3"),
                        DisplayName = "default",
                        ExecutionFile = new DiskEntityDto
                        {
                            ApplicationGuid = appGuid,
                            PackageGuid = gamePkgGuid,
                            RelativePath = "launch.exe",
                        },
                        ExecutionParameters = "--default",
                        MonitorInstructions = new IProcessMonitorInstructionDto[]
                        {
                            new ProcessMonitorInstructionDto
                            {
                                ExecutableRelativePathFileName = "launch.exe",
                                Instruction = ProcessMonitorOption.IsMainExecutable
                                    | ProcessMonitorOption.KillOnExit,
                            },
                        },
                        OptionalModuleGuids = new string[0],
                        RequiredModuleGuids = new string[0],
                    },
                },
            };

            // Drop the JSON files into a per-build metadata staging dir.
            var metaStaging = Path.Combine(workDir, "metadata", "database");
            Directory.CreateDirectory(metaStaging);
            File.WriteAllText(Path.Combine(metaStaging, "displayData.json"),
                ContainerJsonSerializer.SerializeObject(displayData,
                    Formatting.Indented));
            File.WriteAllText(Path.Combine(metaStaging, "platformData.json"),
                ContainerJsonSerializer.SerializeObject(platformData,
                    Formatting.Indented));

            // Build the container in the canonical order.
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
