#region Licence
/****************************************************************
 *  Filename: AdvancedEditExecutionInstructionViewModel.cs
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Shell.Categories.Annotations;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Language;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.Edit.ViewModels
{
    public class AdvancedEditExecutionInstructionViewModel: Screen
    {
        //TODO Preselect an Root Directory
#pragma warning disable IDE0044 // Add readonly modifier
        private readonly string _applicationRootDirectory = null;
#pragma warning restore IDE0044 // Add readonly modifier
        private string _automaticSetName;
        private string _automaticSetExecutable;
        private Executable _selectedExecutable;
        private ISelectableVrType _selectedRequiredVrModule;
        private Visibility _cancelButtonVisibility;
        private string _applicationRelativeWorkingDirectory;

        public IEditableProcessExecutionLogic EditableProcessExecutionLogic { get; }

        public string InstructionName
        {
            get => EditableProcessExecutionLogic.DisplayName;
            set
            {
                if (value == EditableProcessExecutionLogic.DisplayName) return;
                EditableProcessExecutionLogic.DisplayName = value;
                NotifyOfPropertyChange(() => InstructionName);
                NotifyOfPropertyChange(() => CanOk);
            }
        }

        public string ApplicationMainExecutablePath
        {
            get => EditableProcessExecutionLogic.ExecutionFile.Path;
            set
            {
                if (value == EditableProcessExecutionLogic.ExecutionFile.Path) return;
                EditableProcessExecutionLogic.ExecutionFile.Path = value;
                NotifyOfPropertyChange(() => ApplicationMainExecutablePath);
                NotifyOfPropertyChange(() => CanOk);
            }
        }

        public string ApplicationMainExecutableParameters
        {
            get => EditableProcessExecutionLogic.ExecutionParameters;
            set
            {
                if (value == EditableProcessExecutionLogic.ExecutionParameters) return;
                EditableProcessExecutionLogic.ExecutionParameters = value;
                NotifyOfPropertyChange(() => ApplicationMainExecutableParameters);
            }
        }

        public string ApplicationMainExecutableWorkingDirectory
        {
            get => _applicationRelativeWorkingDirectory;
            set
            {
                if (value == _applicationRelativeWorkingDirectory) return;
                _applicationRelativeWorkingDirectory = value;
                NotifyOfPropertyChange(() => ApplicationMainExecutableWorkingDirectory);
            }
        }

        public ObservableCollection<ISelectableVrType> RequiredVrModules { get; set; }

        public ISelectableVrType SelectedRequiredVrModule
        {
            get => _selectedRequiredVrModule;
            set
            {
                if (Equals(value, _selectedRequiredVrModule)) return;
                _selectedRequiredVrModule = value;
                EditableProcessExecutionLogic.SetVrModule(_selectedRequiredVrModule);
                NotifyOfPropertyChange(() => SelectedRequiredVrModule);
                if (!String.IsNullOrEmpty(_automaticSetName) && _automaticSetName.Equals(InstructionName))
                {
                    _automaticSetName = AutoInstructionName(_automaticSetExecutable);
                    InstructionName = _automaticSetName;
                }
            }
        }

        public ObservableCollection<Executable> Executables { get; set; }

        public Executable SelectedExecutable
        {
            get => _selectedExecutable;
            set
            {
                if (Equals(value, _selectedExecutable)) return;
                _selectedExecutable = value;
                NotifyOfPropertyChange(() => SelectedExecutable);
                NotifyOfPropertyChange(() => CanRemoveExecutable);
                NotifyOfPropertyChange(() => CanNameFromExecutable);
            }
        }

        public Visibility CancelButtonVisibility
        {
            get => _cancelButtonVisibility;
            set
            {
                if (value == _cancelButtonVisibility) return;
                _cancelButtonVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public AdvancedEditExecutionInstructionViewModel(IEnumerable<ISelectableVrType> selectableVrTypes,IEditableProcessExecutionLogic editableProcessExecutionLogic)
        {
            CancelButtonVisibility = Visibility.Visible;
            EditableProcessExecutionLogic = editableProcessExecutionLogic;
            RequiredVrModules = new ObservableCollection<ISelectableVrType>(selectableVrTypes);
            SelectedRequiredVrModule = RequiredVrModules.FirstOrDefault(x => x.IsMatch(editableProcessExecutionLogic.RequiredVrModuleGuid));
            Executables = GetExecutables(editableProcessExecutionLogic);
            Executables.CollectionChanged += Executables_CollectionChanged;
        }

        public bool CanNameFromExecutable => SelectedExecutable != null;
        public void NameFromExecutable()
        {
            _automaticSetExecutable = SelectedExecutable.ToName();
            _automaticSetName = AutoInstructionName(_automaticSetExecutable);
            InstructionName = _automaticSetName;
        }

        public void BrowseApplicationStartingProcessExecutablePath()
        {
            var result = SelectExeDialog(_applicationRootDirectory);
            if (String.IsNullOrEmpty(result)) return;
            ApplicationMainExecutablePath = result;
            if (!ValidateIsUnique(result)) return;
            var exe = EditableProcessExecutionLogic.CreateEditableProcessMonitorInstructions();
            exe.ExecutableRelativePathFileName = result;
            var executable = new Executable(exe) { IsMainExecutable = true, KillOnExit = true };
            AddExecutable(executable);
            NameFromExecutable();
        }

        public void AddExecutable()
        {
            var result = SelectExeDialog(_applicationRootDirectory);
            if (String.IsNullOrEmpty(result)) return;
            if (!ValidateIsUnique(result)) return;
            var newInstruction = EditableProcessExecutionLogic.CreateEditableProcessMonitorInstructions();
            newInstruction.ExecutableRelativePathFileName = result;
            var executable = new Executable(newInstruction)
                             {
                                     IsMainExecutable = true,
                                     KillOnExit = true
                             };
            AddExecutable(executable);
        }

        public bool CanRemoveExecutable => SelectedExecutable != null;
        public void RemoveExecutable()
        {
            Executables.Remove(SelectedExecutable);
            NotifyOfPropertyChange(() => CanOk);
        }


        public void BrowseWorkingDirectoryFolder()
        {
            var retval = SelectWorkingFolderDialog(_applicationRootDirectory);
            ApplicationMainExecutableWorkingDirectory = retval;
        }

        public bool CanOk =>
                !String.IsNullOrEmpty(ApplicationMainExecutablePath) &&
                Executables.Any() &&
                !String.IsNullOrEmpty(InstructionName);

        public void Ok()
        {
            if (!Executables.Any(e => e.IsMainExecutable))
            {
                var result = MessageBox.Show(Resources.System_Application_Advance_Edit_Warning_NoMainExecutable, Resources.System_Application_Advance_Edit_Warning, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            EditableProcessExecutionLogic.ReplaceProcessMonitorInstructions(from exe in Executables select exe.ToInstruction());
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }


        #region Private Methods

        private ObservableCollection<Executable> GetExecutables(IEditableProcessExecutionLogic executionLogic)
        {
            var retval = new ObservableCollection<Executable>();
            if(executionLogic.MonitorInstructions == null || !executionLogic.MonitorInstructions.Any()) return retval;
            foreach(IEditableProcessMonitorInstruction instruction in executionLogic.MonitorInstructions)
            {
                retval.Add(new Executable(instruction));
            }

            return retval;
        }

        private string AutoInstructionName(string executableName)
        {
            var retval = executableName;
            if (SelectedRequiredVrModule != null  && !SelectedRequiredVrModule.IsNonVrType)
            {
                retval = retval + " " + SelectedRequiredVrModule.DisplayName;
            }
            return retval;
        }

        private void Executables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => CanOk);
        }


        private bool ValidateIsUnique(string executableFilePathName)
        {
            if (Executables.Any(x =>
                                        x.ExecutableName.ToLowerInvariant().Equals(executableFilePathName.ToLowerInvariant())))
                return false;
            return true;
        }

        private void AddExecutable(Executable executable)
        {
            Executables.Add(executable);
            SelectedExecutable = executable;
        }

        private string SelectWorkingFolderDialog(string rootDirectory)
        {
            var folderSelectorDialog = new CommonOpenFileDialog
                                       {
                                               EnsureReadOnly = true,
                                               IsFolderPicker = true,
                                               AllowNonFileSystemItems = false,
                                               Multiselect = false,
                                               InitialDirectory = rootDirectory,
                                               Title = "Working Directory Location"
                                       };
            if (folderSelectorDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return null;
            }
            return folderSelectorDialog.FileName;
        }
        private string SelectExeDialog(string rootDirectory)
        {
            var ofd = new OpenFileDialog
                      {
                              InitialDirectory = rootDirectory,
                              Filter = $"{Resources.System_Application_Advance_Edit_ExecutableInfo_SelectExecutable} (*.exe)|*.exe;|{Resources.System_Application_Advance_Edit_ExecutableInfo_ExeFiles} (*.*)|*.*"
                      };
            return ofd.ShowDialog() != true ? null : ofd.FileName;
        }

        #endregion
    }

    public class Executable : INotifyPropertyChanged
    {
        private readonly IEditableProcessMonitorInstruction _instruction;
        private bool _killOnExit;
        private bool _killProcessOnNotResponding;
        private bool _isMainExecutable;
        public string ExecutableName => _instruction.ExecutableRelativePathFileName;
        public bool KillOnExit
        {
            get => _killOnExit;
            set
            {
                _killOnExit = value;
                SetFlagsFromBool();
                OnPropertyChanged();
            }
        }
        public bool KillProcessOnNotResponding
        {
            get => _killProcessOnNotResponding;
            set
            {
                _killProcessOnNotResponding = value;
                SetFlagsFromBool();
                OnPropertyChanged();
            }
        }
        public bool IsMainExecutable
        {
            get => _isMainExecutable;
            set
            {
                _isMainExecutable = value;
                SetFlagsFromBool();
                OnPropertyChanged();
            }
        }


        public string ToName()
        {
            if(ExecutableName != null)return Path.GetFileNameWithoutExtension(ExecutableName);
            return "Default";
        }


        public Executable(IEditableProcessMonitorInstruction instruction)
        {
            _instruction = instruction;
            if(instruction.Instruction.HasFlag(ProcessMonitorOption.IsMainExecutable)) _isMainExecutable = true;
            if(instruction.Instruction.HasFlag(ProcessMonitorOption.KillOnExit)) _killOnExit = true;
            if(instruction.Instruction.HasFlag(ProcessMonitorOption.KillProcessOnNotResponding))_killProcessOnNotResponding = true;
        }

        public IEditableProcessMonitorInstruction ToInstruction() => _instruction;

        private void SetFlagsFromBool()
        {
            ProcessMonitorOption processMonitorOption = ProcessMonitorOption.Ignore;
            if(_killOnExit) processMonitorOption = processMonitorOption | ProcessMonitorOption.KillOnExit;
            if(_isMainExecutable)processMonitorOption = processMonitorOption | ProcessMonitorOption.IsMainExecutable;
            if(_killProcessOnNotResponding)processMonitorOption = processMonitorOption | ProcessMonitorOption.KillProcessOnNotResponding;
            _instruction.Instruction = processMonitorOption;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
