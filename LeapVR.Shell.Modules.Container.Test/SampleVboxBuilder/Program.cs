#region Licence
/****************************************************************
 *  Filename: Program.cs
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
using LeapVR.Content.Shared;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Modules.Container;
using Newtonsoft.Json;

namespace SampleVboxBuilder
{
    /// <summary>
    /// Builds a tiny but realistic .vbox container so the Content Creator's
    /// edit wizard can be exercised end-to-end. The container has the
    /// canonical GameFiles -&gt; MediaFiles -&gt; Metadata package order so
    /// EditableAppInstallationContainer's metadata-only save path works.
    /// </summary>
    internal static class Program
    {
        private static readonly Guid VrLeapPlatformGuid =
            Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3");

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine(
                    "usage: SampleVboxBuilder <output_path.vbox> [displayName]");
                return 1;
            }
            var outputPath = args[0];
            var displayName = args.Length > 1 ? args[1] : "Sample VBox";

            try
            {
                Build(outputPath, displayName);
                Console.WriteLine("OK: " + outputPath);
                Console.WriteLine("size: " + new FileInfo(outputPath).Length + " bytes");
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("FAIL: " + e);
                return 1;
            }
        }

        private static void Build(string outputPath, string displayName)
        {
            var appGuid = Guid.NewGuid();
            var staging = Path.Combine(Path.GetTempPath(),
                "PoDSampleVbox-" + appGuid.ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                // GameFiles: one trivial exe-like blob.
                var gameDir = Path.Combine(staging, "game");
                Directory.CreateDirectory(gameDir);
                File.WriteAllText(Path.Combine(gameDir, "launch.exe"),
                    "fake game executable " + appGuid);

                // MediaFiles: a tiny PNG-headered blob as the main picture.
                var mediaDir = Path.Combine(staging, "media");
                Directory.CreateDirectory(mediaDir);
                File.WriteAllBytes(Path.Combine(mediaDir, "mainPicture.png"),
                    new byte[]
                    {
                        // PNG signature + a tiny 1x1-pixel chunk would be
                        // overkill - the wizard just renders this via the
                        // header's denormalized ThumbnailAsBytes anyway. A
                        // few header bytes are enough for IsValid.
                        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                    });

                var mediaPkgGuid = Guid.NewGuid();
                var gamePkgGuid = Guid.NewGuid();

                var displayData = new AppDisplayDataDto
                {
                    ApplicationGuid = appGuid,
                    Name = displayName,
                    Description =
                        "Original description (edit me in Content Creator)",
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
                    PlatformPluginId = VrLeapPlatformGuid,
                    ExecutionLogicInstructions = new List<ProcessExecutionLogicDto>
                    {
                        new ProcessExecutionLogicDto
                        {
                            ApplicationGuid = appGuid,
                            PlatformPluginId = VrLeapPlatformGuid,
                            DisplayName = "default",
                            ExecutionFile = new DiskEntityDto
                            {
                                ApplicationGuid = appGuid,
                                PackageGuid = gamePkgGuid,
                                RelativePath = "launch.exe",
                            },
                            ExecutionParameters = "--demo",
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

                var metaStaging = Path.Combine(staging, "metadata", "database");
                Directory.CreateDirectory(metaStaging);
                File.WriteAllText(Path.Combine(metaStaging, "displayData.json"),
                    ContainerJsonSerializer.SerializeObject(displayData,
                        Formatting.Indented));
                File.WriteAllText(Path.Combine(metaStaging, "platformData.json"),
                    ContainerJsonSerializer.SerializeObject(platformData,
                        Formatting.Indented));

                var serializer = new AppInstallationHeaderSerializer();
                var module = new ContainerModule(serializer);
                var container = module.CreateNewApplicationInstallationContainer(appGuid);
                container.Initialize();
                container.Version = 2;
                container.DisplayName = displayName;
                // 460x215-ish placeholder thumbnail bytes (raw - the WPF
                // imager will fail to decode them gracefully, but the header
                // round-trip still works).
                container.ThumbnailAsBytes = new byte[]
                {
                    0xAA, 0xBB, 0xCC, 0xDD,
                };

                var gamePkg = container.AddNewPackage(ContentType.GameFiles);
                gamePkg.PackageVersion = 1;
                gamePkg.AddDirectory(gameDir, "");

                var mediaPkg = container.AddNewPackage(ContentType.MediaFiles);
                mediaPkg.PackageVersion = 1;
                mediaPkg.AddDirectory(mediaDir, "images");

                var metaPkg = container.AddNewPackage(ContentType.Metadata);
                metaPkg.PackageVersion = 1;
                metaPkg.AddDirectory(Path.Combine(staging, "metadata"), "");

                if (File.Exists(outputPath)) File.Delete(outputPath);
                container.SaveToFiles(outputPath);
            }
            finally
            {
                try { Directory.Delete(staging, recursive: true); }
                catch { /* nothing */ }
            }
        }
    }
}
