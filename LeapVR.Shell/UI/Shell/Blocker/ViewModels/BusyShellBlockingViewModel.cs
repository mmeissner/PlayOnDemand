#region Licence
/****************************************************************
 *  Filename: BusyShellBlockingViewModel.cs
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
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Shell.Blocker.Abstract;

namespace LeapVR.Shell.UI.Shell.Blocker.ViewModels
{
    public class BusyShellBlockingViewModel : BlockShellBaseViewModel
    {
        public BusyShellBlockingViewModel(ViewInputHandler inputHandler) : base(inputHandler)
        {
        }
    }
}
