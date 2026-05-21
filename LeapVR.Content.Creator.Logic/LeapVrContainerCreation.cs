#region Licence
/****************************************************************
 *  Filename: LeapVrContainerCreation.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using LeapVR.Content.Shared.Container;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using Newtonsoft.Json;
using DiskEntityDto = LeapVR.Content.Shared.DiskEntityDto;

namespace LeapVR.Content.Creator.Logic
{
    public class LeapVrContainerCreation : ContainerCreation, IDisposable
    {
        #region Properties & Fields

        private static readonly string AppGlobalTempDirectory = Path.Combine(Path.GetTempPath(), "VrLeapContentCreator");
        private static readonly Guid VrLeapPlatformGuid = Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3");

        private const string AppFilesPackageRelativeDirectory = "";
        private const string MainPictureFilePackageRelativeDirectory = "images";
        private const string MetadataDatabaseFilesPackageRelativeDirectory = "database";
        private const string MainPictureFileName = "mainPicture.png"; // TODO [RM]: force such picture name (?)

        public string AppBaseDirectory { get; set; }

        public override int TotalFilesCount => _container?.TotalFilesCount ?? 0;
        public override int DoneFilesCount => _container?.DoneFilesCount ?? 0;
        public override long TotalFilesSize => _container?.TotalFilesSize ?? 0;
        public override long DoneFilesSize => _container?.DoneFilesSize ?? 0;

        private readonly ReplaySubject<Empty> _whenContainerCreationStartedSubject;
        public override IObservable<Empty> WhenContainerCreationStarted { get; }

        private readonly Subject<Empty> _whenProgressChangedSubject;
        public override IObservable<Empty> WhenProgressChanged { get; }

        private readonly ReplaySubject<Empty> _whenContainerCreationEndedSubject;
        public override IObservable<Empty> WhenContainerCreationEnded { get; }

        public override bool WasContainerCreationStarted => _container?.WasContainerCreationStarted ?? false;
        public override bool IsContainerCreationEnded => _container?.IsContainerCreationEnded ?? false;

        private Exception _containerCreationException;
        public override Exception OccuredException => _containerCreationException;

        private INewApplicationInstallationContainer _container;

        #endregion Properties & Fields

        #region Constructors

        public LeapVrContainerCreation()
            : base(PlatformType.LeapVr)

        {
            _whenContainerCreationStartedSubject = new ReplaySubject<Empty>();
            WhenContainerCreationStarted = _whenContainerCreationStartedSubject.AsObservable();

            _whenProgressChangedSubject = new Subject<Empty>();
            WhenProgressChanged = _whenProgressChangedSubject.AsObservable();

            _whenContainerCreationEndedSubject = new ReplaySubject<Empty>();
            WhenContainerCreationEnded = _whenContainerCreationEndedSubject.AsObservable();

        }

        #endregion Constructors

        #region Methods

        public override async Task DoWork()
        {
            try
            {
                await CreateContainerAsyncLogic();
                NotifyContainerCreationEnded(null);
            }
            catch (Exception e)
            {
                NotifyContainerCreationEnded(e);
            }
        }

        private async Task CreateContainerAsyncLogic()
        {
            var appGuid = Guid.NewGuid();
            var newContainer = ContainerModule.CreateNewApplicationInstallationContainer(appGuid);
            newContainer.Initialize();
            newContainer.Version = 2; // V2
            _container = newContainer;

            newContainer.DisplayName = DisplayName;

            var tempDir = Path.Combine(AppGlobalTempDirectory, appGuid.ToString());
            Directory.CreateDirectory(tempDir);

            var copiedMainPictureFilePath = Path.Combine(tempDir, MainPictureFileName);
            File.Copy(MainPictureFilePath, copiedMainPictureFilePath);

            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(copiedMainPictureFilePath, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(ms);
                    newContainer.ThumbnailAsBytes = ms.ToArray();
                }
            }

            var appPackage = newContainer.AddNewPackage(ContentType.GameFiles);
            appPackage.PackageVersion = 1;
            appPackage.AddDirectory(AppBaseDirectory, AppFilesPackageRelativeDirectory);

            var mediaPackage = newContainer.AddNewPackage(ContentType.MediaFiles);
            mediaPackage.PackageVersion = 1;
            mediaPackage.AddFile(copiedMainPictureFilePath, MainPictureFilePackageRelativeDirectory);

            var mainPictureRelativePackagePath = Path.Combine(MainPictureFilePackageRelativeDirectory, MainPictureFileName);
            var displayData = new AppDisplayDataDto
            {
                ApplicationGuid = appGuid,
                Name = DisplayName,
                Description = Description,
                Category = Category,
                Tags = TagStringToArray(Tags),
                MainPicture = new DiskEntityDto { ApplicationGuid = appGuid, PackageGuid = mediaPackage.PackageGuid, RelativePath = mainPictureRelativePackagePath },
            };

            var platformData = new AppPlatformDataDto
            {
                ApplicationGuid = appGuid,
                PlatformPluginId = VrLeapPlatformGuid,
                ExecutionLogicInstructions = Convert2ProcessExecutionLogic(appGuid,VrLeapPlatformGuid,appPackage,Executables)
            };

            var tempMetadataDir = Path.Combine(tempDir, "metadata");
            Directory.CreateDirectory(Path.Combine(tempMetadataDir, MetadataDatabaseFilesPackageRelativeDirectory));

            var metadataPackage = newContainer.AddNewPackage(ContentType.Metadata);
            metadataPackage.PackageVersion = 1;

            // Display data:
            var displayDataJsonFileName = "displayData.json";
            var displayDataJsonRelativePackagePath = Path.Combine(MetadataDatabaseFilesPackageRelativeDirectory, displayDataJsonFileName);
            var tempDisplayDataJsonPath = Path.Combine(tempMetadataDir, displayDataJsonRelativePackagePath);
            var displayDataJson = ContainerJsonSerializer.SerializeObject(displayData, Formatting.Indented);
            File.WriteAllText(tempDisplayDataJsonPath, displayDataJson);

            // Platform data:
            var platformDataJsonFileName = "platformData.json";
            var platformDataJsonRelativePackagePath = Path.Combine(MetadataDatabaseFilesPackageRelativeDirectory, platformDataJsonFileName);
            var tempPlatformDataJsonPath = Path.Combine(tempMetadataDir, platformDataJsonRelativePackagePath);
            var platformDataJson = ContainerJsonSerializer.SerializeObject(platformData, Formatting.Indented);
            File.WriteAllText(tempPlatformDataJsonPath, platformDataJson);

            metadataPackage.AddDirectory(tempMetadataDir, "");

            newContainer.WhenContainerCreationStarted.Subscribe(_whenContainerCreationStartedSubject);
            newContainer.WhenProgressChanged.Subscribe(_whenProgressChangedSubject);

            var headerFilePath = ContainerOutputFilePath;
            await Task.Run(() => newContainer.SaveToFiles(headerFilePath));
        }

        private void NotifyContainerCreationEnded(Exception e)
        {
            if (e != null)
            {
                _containerCreationException = e;
            }

            _whenProgressChangedSubject.OnNext(Empty.Get);
            _whenProgressChangedSubject.OnCompleted();
            _whenContainerCreationEndedSubject.OnNext(Empty.Get);
            _whenContainerCreationEndedSubject.OnCompleted();
        }

        private string[] TagStringToArray(string tagString)
        {
            if (String.IsNullOrEmpty(tagString)) return null;
            return tagString.Split(new []{',',';',' '},StringSplitOptions.RemoveEmptyEntries);
        }

        private List<ProcessExecutionLogicDto> Convert2ProcessExecutionLogic(Guid appGuid, Guid platformGuid,INewPackage package,IEnumerable<IAppExecuteInstruction> executables)
        {
            var retval = new List<ProcessExecutionLogicDto>();
            foreach (IAppExecuteInstruction executable in executables)
            {
                var executionLogic = new ProcessExecutionLogicDto();
                executionLogic.ApplicationGuid = appGuid;
                executionLogic.PlatformPluginId = platformGuid;
                executionLogic.DisplayName = executable.InstructionName;
                executionLogic.ExecutionFile = new DiskEntityDto
                {
                    ApplicationGuid = appGuid,
                    PackageGuid = package.PackageGuid,
                    RelativePath = QuickLeap.GetRelativePath(executable.ApplicationMainExecutablePath, AppBaseDirectory)
                };
                executionLogic.ExecutionParameters = executable.ApplicationMainExecutableParameters;
                if (String.IsNullOrEmpty(executable.ApplicationMainExecutableWorkingDirectory))
                    executionLogic.RelativeWorkingDirectory = null;
                else executionLogic.RelativeWorkingDirectory = QuickLeap.GetRelativePath(executable.ApplicationMainExecutableWorkingDirectory, AppBaseDirectory);
                executionLogic.OptionalModuleGuids = new string[] { };
                executionLogic.RequiredModuleGuids = new string[] { };
                if (executable.SelectedRequiredVrModule.ModuleGuid != Guid.Empty)
                    executionLogic.ReguiredVrModuleGuid = executable.SelectedRequiredVrModule.ModuleGuid.ToString();
                var processMonitorInstructions = new List<ProcessMonitorInstructionDto>();
                 foreach (Executable executableInfo in executable.ExecutablesInfo)
                 {
                    var instruction = new ProcessMonitorInstructionDto();
                    instruction.ExecutableRelativePathFileName = executableInfo.RelativeExecutableFilePath;
                     if (executableInfo.IsMainExecutable)
                         instruction.Instruction |= ProcessMonitorOption.IsMainExecutable;
                     if(executableInfo.KillOnExit)
                         instruction.Instruction |= ProcessMonitorOption.KillOnExit;
                     if(executableInfo.KillProcessOnNotResponding)
                         instruction.Instruction |= ProcessMonitorOption.KillProcessOnNotResponding;
                     processMonitorInstructions.Add(instruction);
                }
                executionLogic.MonitorInstructions = processMonitorInstructions.ToArray();
                retval.Add(executionLogic);
            }
            return retval;
        }
        #endregion Methods

        public void Dispose()
        {
            _whenContainerCreationStartedSubject?.Dispose();
            _whenProgressChangedSubject?.Dispose();
            _whenContainerCreationEndedSubject?.Dispose();
        }
    }
}
