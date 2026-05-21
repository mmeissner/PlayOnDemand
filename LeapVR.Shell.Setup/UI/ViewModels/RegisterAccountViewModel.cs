#region Licence
/****************************************************************
 *  Filename: RegisterAccountViewModel.cs
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
using System.Diagnostics;
using Caliburn.Micro;

namespace LeapVR.Shell.Setup.UI.ViewModels
{
    public class RegisterAccountViewModel :Screen, IWizardPage
    {
        public bool BlockPrevious => true;
        public bool PageValid => true;
        public IEnumerable<WizardWorkTasks> WorkTasks() { return new List<WizardWorkTasks>();}


        public void OpenWebsiteNewAccount()
        {
            Process.Start("https://activate.example.com/ActivationEntryForNewUser");
        }
        public void OpenWebsiteExistingAccount()
        {
            Process.Start("https://activate.example.com/ActivationEntryForExistingUser");
        }
    }
}
