#region Licence
/****************************************************************
 *  Filename: WorkTaskViewModel.cs
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
using System.Threading.Tasks;
using Caliburn.Micro;

namespace LeapVR.Shell.Setup.UI.ViewModels.WorkTask
{
    public class WorkTaskViewModel : Screen
    {
        private readonly WizardWorkTasks _workTasks;
        private bool _isProgress;
        private bool _isDone;
        private bool _isError;

        public string Key => _workTasks.ResourceKey;
        public bool IsProgress
        {
            get { return _isProgress; }
            set
            {
                if (value == _isProgress) return;
                _isProgress = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (value == _isDone) return;
                _isDone = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsError
        {
            get { return _isError; }
            set
            {
                if(value == _isError) return;
                _isError = value;
                NotifyOfPropertyChange();
            }
        }
        public WorkTaskViewModel(WizardWorkTasks workTasks) { _workTasks = workTasks; }

        public async Task DoWork()
        {
            IsProgress = true;
            await Work();
            IsDone = true;
        }

        private async Task Work()
        {
            var result = await _workTasks.WorkTask();
            if(result)
            {
                IsProgress = false;
                IsDone = true;
            }
            else
            {
                IsProgress = false;
                IsError = true;
            }
        }
    }
}
