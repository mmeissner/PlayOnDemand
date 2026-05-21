#region Licence
/****************************************************************
 *  Filename: ApplyChangesViewModel.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shell.Setup.UI.ViewModels.WorkTask;

namespace LeapVR.Shell.Setup.UI.ViewModels
{
    public class ApplyChangesViewModel : Screen, IWizardPage
    {
        private readonly List<WizardWorkTasks> _workTasks;
        private bool _blockPrevious;
        private bool _isFinished;
        private bool _hasErrors;
        private bool _pageValid;
        public ApplyChangesViewModel(List<WizardWorkTasks> workTasks)
        {
            _workTasks = workTasks;
            WorkTasks = new BindableCollection<WorkTaskViewModel>();
        }

        public IObservableCollection<WorkTaskViewModel> WorkTasks { get; set; }
        public bool BlockPrevious
        {
            get { return _blockPrevious; }
            set
            {
                if (value == _blockPrevious) return;
                _blockPrevious = value;
                NotifyOfPropertyChange();
            }
        }
        public bool PageValid
        {
            get { return _pageValid; }
            set
            {
                if(value == _pageValid) return;
                _pageValid = value;
                NotifyOfPropertyChange();
            }
        }
        IEnumerable<WizardWorkTasks> IWizardPage.WorkTasks() { return new List<WizardWorkTasks>();}
        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                if(value == _isFinished) return;
                _isFinished = value;
                NotifyOfPropertyChange();
            }
        }
        public bool HasErrors
        {
            get { return _hasErrors; }
            set
            {
                if(value == _hasErrors) return;
                _hasErrors = value;
                NotifyOfPropertyChange();
            }
        }
        public async Task DoWorkTasks()
        {
            PageValid = false;
            BlockPrevious = true;
            var tasks = new List<Task>();
            foreach(WizardWorkTasks task in _workTasks)
            {
                var taskViewModel = new WorkTaskViewModel(task);
                WorkTasks.Add(taskViewModel);
                var worktask = taskViewModel.DoWork();
                tasks.Add(worktask);
            }
            NotifyOfPropertyChange(nameof(WorkTasks));
            await Task.WhenAll(tasks.ToArray());
            if(WorkTasks.Any(x => x.IsError))
            {
                HasErrors = true;
            }
            IsFinished = true;
            PageValid = true;
        }

    }
}
