#region Licence
/****************************************************************
 *  Filename: WarnStorageNotEmptyViewModel.cs
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
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;

namespace LeapVR.Shell.Setup.UI.ViewModels.Dialog
{
    public class WarnStorageNotEmptyViewModel : Screen
    {
        private string _testString = "MyTest";
        private readonly DialogHost _dialogHost;

        public WarnStorageNotEmptyViewModel(DialogHost dialogHost) { _dialogHost = dialogHost; }
        public string TestString
        {
            get { return _testString; }
            set
            {
                if(value == _testString) return;
                _testString = value;
                NotifyOfPropertyChange();
            }
        }
        public bool UserUnderstood { get; set; }
        public void Understand()
        {
            UserUnderstood = true;
            _dialogHost.IsOpen = false;
        }
        public void Cancel()
        {
            UserUnderstood = false;
            _dialogHost.IsOpen = false;
        }
    }
}
