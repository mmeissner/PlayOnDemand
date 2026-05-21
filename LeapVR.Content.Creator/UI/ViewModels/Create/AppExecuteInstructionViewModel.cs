#region Licence
/****************************************************************
 *  Filename: AppExecuteInstructionViewModel.cs
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
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Creator.Language;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LeapVR.Content.Creator.UI.ViewModels
{

    public class AppExecuteInstructionViewModel : Screen, IAppExecuteInstruction
    {
        private readonly string _applicationRootDirectory;
        private string _automaticSetName;
        private string _automaticSetExecutable;
        private Executable _selectedExecutable;
        private VRSelectableModule _selectedRequiredVrModule;
        private string _applicationMainExecutableParameters;
        private string _applicationMainExecutablePath;
        private string _instructionName;
        private Visibility _cancelButtonVisibility;
        private string _applicationRelativeWorkingDirectory;

        public string InstructionName
        {
            get => _instructionName;
            set
            {
                if (value == _instructionName) return;
                _instructionName = value;
                NotifyOfPropertyChange(() => InstructionName);
                NotifyOfPropertyChange(() => CanOk);
            }
        }

        public string ApplicationMainExecutablePath
        {
            get => _applicationMainExecutablePath;
            set
            {
                if (value == _applicationMainExecutablePath) return;
                _applicationMainExecutablePath = value;
                NotifyOfPropertyChange(() => ApplicationMainExecutablePath);
                NotifyOfPropertyChange(() => CanOk);
            }
        }
        public string ApplicationMainExecutableParameters
        {
            get => _applicationMainExecutableParameters;
            set
            {
                if (value == _applicationMainExecutableParameters) return;
                _applicationMainExecutableParameters = value;
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

        public ObservableCollection<VRSelectableModule> RequiredVrModules { get; set; }
        public VRSelectableModule SelectedRequiredVrModule
        {
            get => _selectedRequiredVrModule;
            set
            {
                if (Equals(value, _selectedRequiredVrModule)) return;
                _selectedRequiredVrModule = value;
                NotifyOfPropertyChange(() => SelectedRequiredVrModule);
                if (!String.IsNullOrEmpty(_automaticSetName) && _automaticSetName.Equals(_instructionName))
                {
                    _automaticSetName = AutoInstructionName(_automaticSetExecutable);
                    InstructionName = _automaticSetName;
                }
            }
        }

        public IEnumerable<Executable> ExecutablesInfo => Executables;
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

        public AppExecuteInstructionViewModel(IEnumerable<IVrModule> vrModules, string applicationRootDirectory)
        {
            _applicationRootDirectory = applicationRootDirectory;
            CancelButtonVisibility = Visibility.Visible;
            RequiredVrModules = new ObservableCollection<VRSelectableModule>(Convert2Selectable(vrModules));
            SelectedRequiredVrModule = RequiredVrModules.First();
            Executables = new ObservableCollection<Executable>();
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
            if (!ValidateExecutablePath(_applicationRootDirectory, result)) return;
            ApplicationMainExecutablePath = result;
            if (!ValidateIsUnique(result)) return;
            var executable = new Executable(_applicationRootDirectory, result) { IsMainExecutable = true, KillOnExit = true };
            AddExecutable(executable);
            NameFromExecutable();
        }

        public void AddExecutable()
        {
            var result = SelectExeDialog(_applicationRootDirectory);
            if (String.IsNullOrEmpty(result)) return;
            if (!ValidateExecutablePath(_applicationRootDirectory, result)) return;
            if (!ValidateIsUnique(result)) return;
            var executable = new Executable(_applicationRootDirectory, result)
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
            if (String.IsNullOrEmpty(retval) || !ValidateExecutablePath(_applicationRootDirectory, retval)) ApplicationMainExecutableWorkingDirectory = retval;
            else
            {
                ApplicationMainExecutableWorkingDirectory = retval;
            }
        }

        public bool CanOk =>
            !String.IsNullOrEmpty(ApplicationMainExecutablePath) &&
            Executables.Any() &&
            !String.IsNullOrEmpty(InstructionName);

        public void Ok()
        {
            if (!Executables.Any(e => e.IsMainExecutable))
            {
                var result = MessageBox.Show(Resources.NoneExecutableIsMainExecutable, Resources.Warning, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }

            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        private string AutoInstructionName(string executableName)
        {
            var retval = executableName;
            if (SelectedRequiredVrModule != null && SelectedRequiredVrModule.DisplayName != VRSelectableModule.NoneName)
            {
                retval = retval + " " + SelectedRequiredVrModule.DisplayName;
            }
            return retval;
        }

        private void Executables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => CanOk);
        }


        private bool ValidateExecutablePath(string applicationRootDirectory, string executableFilePathName)
        {
            if (string.IsNullOrEmpty(executableFilePathName)) return false;
            var isPathRelative = QuickLeap.TryGetRelativePath(executableFilePathName, applicationRootDirectory, out _);
            return isPathRelative;
        }

        private bool ValidateIsUnique(string executableFilePathName)
        {
            if (Executables.Any(x =>
                x.ExecutableFilePath.ToLowerInvariant().Equals(executableFilePathName.ToLowerInvariant())))
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
                Filter = $"{Resources.Global_Browse_Executable} (*.exe)|*.exe;|{Resources.Global_Browse_AllFiles} (*.*)|*.*"
            };
            return ofd.ShowDialog() != true ? null : ofd.FileName;
        }
        private List<VRSelectableModule> Convert2Selectable(IEnumerable<IVrModule> vrModules)
        {
            var retval = new List<VRSelectableModule>()
            {
                new VRSelectableModule()
            };
            if (vrModules != null) retval.AddRange(vrModules.Select(module => new VRSelectableModule(module)));
            return retval;
        }

    }


}
