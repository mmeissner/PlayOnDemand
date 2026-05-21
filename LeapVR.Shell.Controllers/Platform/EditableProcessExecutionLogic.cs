#region Licence
/****************************************************************
 *  Filename: EditableProcessExecutionLogic.cs
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using LeapVR.Shell.Categories.Annotations;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;

namespace LeapVR.Shell.Controllers.Platform {
    /// <inheritdoc />
    sealed class EditableProcessExecutionLogic : IEditableProcessExecutionLogic, INotifyPropertyChanged
    {
        #region Private Fields
        private readonly IAppPlatformData _platformData;
        private readonly IProcessExecutionLogic _processExecutionLogic;
        private readonly List<ProcessMonitorInstruction> _monitorInstructions;
        private EditableDiskEntity _executionFile;
        private string _displayName;
        #endregion

        #region Properties
        public bool IsNew { get; }
        public Guid ApplicationId => _platformData.ApplicationGuid;
        public Guid PlatformPluginId => _platformData.PlatformPluginId;
        public Guid ExecutionGuid => _processExecutionLogic.ExecutionGuid;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if(value == _displayName) return;
                _displayName = value;
                OnPropertyChanged();
            }
        }
        public IEditableDiskEntity ExecutionFile
        {
            get => _executionFile;
        }
        public string ExecutionParameters { get; set; }
        public string RelativeWorkingDirectory { get; set; }
        public IReadOnlyList<IEditableProcessMonitorInstruction> MonitorInstructions => _monitorInstructions;
        public string RequiredVrModuleGuid { get; private set; }
        public string[] RequiredModuleGuids { get; }
        public string[] OptionalModuleGuids { get; }
        #endregion

        #region Constructor
        public EditableProcessExecutionLogic(IAppPlatformData platformData, IProcessExecutionLogic processExecutionLogic)
        {
            IsNew = false;
            DisplayName = processExecutionLogic.DisplayName;
            ExecutionParameters = processExecutionLogic.ExecutionParameters;
            RelativeWorkingDirectory = processExecutionLogic.RelativeWorkingDirectory;
            RequiredVrModuleGuid = processExecutionLogic.ReguiredVrModuleGuid;
            RequiredModuleGuids = processExecutionLogic.RequiredModuleGuids;
            OptionalModuleGuids = processExecutionLogic.OptionalModuleGuids;
            _platformData = platformData;
            _processExecutionLogic = processExecutionLogic;
            _executionFile = new EditableDiskEntity(processExecutionLogic.ExecutionFile);
            _monitorInstructions = new List<ProcessMonitorInstruction>();
            if(processExecutionLogic.MonitorInstructions != null)
            {
                foreach(IProcessMonitorInstruction instruction in processExecutionLogic.MonitorInstructions)
                {
                    var item = new ProcessMonitorInstruction(instruction);
                    _monitorInstructions.Add(item);
                }
            }
        }

        public EditableProcessExecutionLogic(IAppPlatformData platformData) : this(platformData,new ProcessExecutionLogic(platformData))
        {
            IsNew = true;
        }
        #endregion

        #region Public Methods
        public bool ReplaceProcessMonitorInstructions(
                IEnumerable<IEditableProcessMonitorInstruction> monitorInstructions)
        {
            var instructions = monitorInstructions.ToArray();
            if(!instructions.Any(x => x.Instruction.HasFlag(ProcessMonitorOption.IsMainExecutable)) ||
               instructions.Any(x => String.IsNullOrWhiteSpace(x.ExecutableRelativePathFileName))) return false;
            var listProcessMonitorInstructions = new List<ProcessMonitorInstruction>();
            foreach(var editableProcessMonitorInstructionse in instructions)
            {
                if(editableProcessMonitorInstructionse is ProcessMonitorInstruction processMonitorInstruction)
                {
                    listProcessMonitorInstructions.Add(processMonitorInstruction);
                }
                else
                {
                    return false;
                }
            }
            _monitorInstructions.Clear();
            _monitorInstructions.AddRange(listProcessMonitorInstructions);
            return true;
        }

        public IEditableProcessMonitorInstruction CreateEditableProcessMonitorInstructions()
        {
            return new ProcessMonitorInstruction();
        }

        public bool ReplaceExecutionFile(IEditableDiskEntity entity)
        {
            if(entity == null || !entity.IsValid() || !(entity is EditableDiskEntity editableDiskEntity)) return false;
            _executionFile = editableDiskEntity;
            return true;
        }

        public void SetVrModule(ISelectableVrType vrModule) { RequiredVrModuleGuid = vrModule?.ToVrModuleGuidString(); }

        public bool IsValid()
        {
            if(ExecutionGuid.Equals(Guid.Empty) ||
               String.IsNullOrWhiteSpace(DisplayName) ||
               ExecutionFile == null ||
               String.IsNullOrWhiteSpace(ExecutionFile.Path) ||
               MonitorInstructions == null ||
               MonitorInstructions.Count == 0 ||
               !MonitorInstructions.Any(x => x.Instruction.HasFlag(ProcessMonitorOption.IsMainExecutable)))
                return false;
            return true;
        }

        public bool IsChanged()
        {
            if(IsNew) return false;
            bool isChanged = !DisplayName.Equals(_processExecutionLogic.DisplayName) ||
                             !ExecutionFile.DataEquals(_processExecutionLogic.ExecutionFile) ||
                             !ExecutionParameters.Equals(_processExecutionLogic.ExecutionParameters) ||
                             !RelativeWorkingDirectory.Equals(_processExecutionLogic.RelativeWorkingDirectory) ||
                             !IsDataEqual(_processExecutionLogic.MonitorInstructions, MonitorInstructions) ||
                             !_processExecutionLogic.ReguiredVrModuleGuid.Equals(RequiredVrModuleGuid)
                    ;
            return isChanged;
        }
        #endregion

        #region Internal Methods

        internal IProcessExecutionLogic Convert()
        {
            return new ProcessExecutionLogic(this);
        }

        #endregion

        #region Private Methods
        private bool IsDataEqual(
                IProcessMonitorInstruction[] processMonitorInstructions,
                IReadOnlyList<IEditableProcessMonitorInstruction> editableProcessMonitorInstructions)
        {
            if(processMonitorInstructions.Length != editableProcessMonitorInstructions.Count) return false;
            foreach(IProcessMonitorInstruction monitorInstruction in processMonitorInstructions)
            {
                var relatedEditableMonitorInstruction = editableProcessMonitorInstructions.FirstOrDefault(
                        x => x.ExecutableRelativePathFileName.Equals(
                                monitorInstruction.ExecutableRelativePathFileName));
                if(relatedEditableMonitorInstruction == null) return false;
                if(relatedEditableMonitorInstruction.Instruction != monitorInstruction.Instruction) return false;
            }

            return true;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}