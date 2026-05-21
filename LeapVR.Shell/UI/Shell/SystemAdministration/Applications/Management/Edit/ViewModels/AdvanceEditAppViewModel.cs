#region Licence
/****************************************************************
 *  Filename: AdvanceEditAppViewModel.cs
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
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.UI.Core;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.Edit.ViewModels
{
    public class AdvanceEditAppViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IAppExecutablesUpdate _executablesUpdate;
        private IEditableProcessExecutionLogic _selectedExecutionInstruction;

        public IObservableCollection<IEditableProcessExecutionLogic> ExecutionInstructions { get; }
        public IEditableProcessExecutionLogic SelectedExecutionInstruction
        {
            get => _selectedExecutionInstruction;
            set
            {
                if(Equals(value, _selectedExecutionInstruction)) return;
                _selectedExecutionInstruction = value;
                NotifyOfPropertyChange(() => SelectedExecutionInstruction);
                NotifyOfPropertyChange(() => CanEditExecutionInstruction);
                NotifyOfPropertyChange(() => CanRemoveExecutionInstruction);
            }
        }

        public AdvanceEditAppViewModel(IWindowManager windowManager,IAppExecutablesUpdate executablesUpdate)
        {
            _windowManager = windowManager;
            _executablesUpdate = executablesUpdate;
            ExecutionInstructions = new BindableCollection<IEditableProcessExecutionLogic>(executablesUpdate.GetExecutionLogics());
            SelectedExecutionInstruction = ExecutionInstructions.FirstOrDefault();
        }

        public void AddExecutionInstruction()
        {
            var newExecutionLogic = _executablesUpdate.CreateExecutionLogic();
            var viewModel = new AdvancedEditExecutionInstructionViewModel(_executablesUpdate.SelectableVrTypes,newExecutionLogic);
            var retval = _windowManager.ShowDialog(viewModel,null,ShellClientHelper.GetUniversalDialogSettings(1280,900));
            if(retval.HasValue && retval.Value)
            {
                if(_executablesUpdate.AddExecutionLogic(newExecutionLogic))
                {
                    ExecutionInstructions.Add(newExecutionLogic);
                }
            }
            NotifyOfPropertyChange(() => CanApply);
        }


        public bool CanEditExecutionInstruction => SelectedExecutionInstruction != null;
        public void EditExecutionInstruction()
        {
            //Open Dialog and let edit
            var viewModel = new AdvancedEditExecutionInstructionViewModel(_executablesUpdate.SelectableVrTypes,SelectedExecutionInstruction);
            _windowManager.ShowDialog(viewModel,null,ShellClientHelper.GetUniversalDialogSettings(1280,900));
            NotifyOfPropertyChange(() => CanApply);
        }

        public bool CanRemoveExecutionInstruction => SelectedExecutionInstruction != null && ExecutionInstructions.Count > 1;
        public void RemoveExecutionInstruction()
        {
            if(_executablesUpdate.RemoveExecutionLogic(SelectedExecutionInstruction))
            {
                ExecutionInstructions.Remove(SelectedExecutionInstruction);
            }
            NotifyOfPropertyChange(() => CanApply);
        }

        public bool CanApply => _executablesUpdate.ValidateUpdate();
        public void Apply()
        {
            if(_executablesUpdate.ApplyChanges())TryClose(true);
        }

        public void Cancel() { TryClose(false); }}
    }