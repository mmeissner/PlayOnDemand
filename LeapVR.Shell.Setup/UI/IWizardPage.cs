#region Licence
/****************************************************************
 *  Filename: IWizardPage.cs
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
using LeapVR.Shell.Setup.UI.ViewModels;

namespace LeapVR.Shell.Setup.UI
{
    interface IWizardPage
    {
        bool BlockPrevious { get; }
        bool PageValid { get; }
        IEnumerable<WizardWorkTasks> WorkTasks();
    }
}
