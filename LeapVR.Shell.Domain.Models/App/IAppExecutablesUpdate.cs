#region Licence
/****************************************************************
 *  Filename: IAppExecutablesUpdate.cs
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
using System.Collections.ObjectModel;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;

namespace LeapVR.Shell.Domain.Models.App
{
    public interface IAppExecutablesUpdate
    {
        Guid ApplicationId { get; }
        IEnumerable<ISelectableVrType> SelectableVrTypes { get; }
        IEditableProcessExecutionLogic CreateExecutionLogic();
        IEnumerable<IEditableProcessExecutionLogic> GetExecutionLogics();
        bool AddExecutionLogic(IEditableProcessExecutionLogic executionLogic);
        bool RemoveExecutionLogic(IEditableProcessExecutionLogic executionLogic);
        bool ValidateUpdate();
        bool ApplyChanges();
    }

    public interface IEditableProcessExecutionLogic
    {
        bool IsNew { get; }
        Guid ApplicationId { get; }
        Guid PlatformPluginId { get; }
        Guid ExecutionGuid { get; }
        string DisplayName { get; set; }
        IEditableDiskEntity ExecutionFile { get; }
        string ExecutionParameters { get; set; }
        string RelativeWorkingDirectory { get; set; }
        IReadOnlyList<IEditableProcessMonitorInstruction> MonitorInstructions { get; }
        string RequiredVrModuleGuid { get; }
        string[] RequiredModuleGuids { get; }
        string[] OptionalModuleGuids { get; }
        bool ReplaceProcessMonitorInstructions(IEnumerable<IEditableProcessMonitorInstruction> monitorInstructions);
        IEditableProcessMonitorInstruction CreateEditableProcessMonitorInstructions();
        void SetVrModule(ISelectableVrType vrModule);
        bool IsValid();
        bool IsChanged();
    }

    public interface IEditableDiskEntity
    {
        Guid ApplicationGuid { get; }
        Guid PlatformGuid { get; }
        DiskEntityType Type { get; set; }
        /// <summary>
        /// Guid of the package the file belongs to.
        /// </summary>
        Guid PackageGuid { get; }

        /// <summary>
        /// Relative path to file
        /// </summary>
        string Path { get;set; }
        bool IsValid();
        bool DataEquals(IDiskEntity other);
    }

    public interface IEditableProcessMonitorInstruction : IProcessMonitorInstruction
    {
        /// <summary>
        /// The Executables FullFilePathName relative to the base Application Directory
        /// </summary>
        new string ExecutableRelativePathFileName { get; set; }

        /// <summary>
        /// Options that are applied to the process.
        /// </summary>
        new ProcessMonitorOption Instruction { get; set; }
    }

}