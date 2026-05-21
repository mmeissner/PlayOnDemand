#region Licence
/****************************************************************
 *  Filename: IStepScreenWizard.cs
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
using Caliburn.Micro;
using LeapVR.Content.Creator.UI.ViewModels;

namespace LeapVR.Content.Creator
{
    public interface IStepScreenWizard : IScreen
    {
        IStepScreenWizard Previous { get; set; }
        IStepScreenWizard Next { get; set; }
        bool CanGoNext { get; }
        bool CanGoPrevious { get; }
        bool CanGoExit { get; }

        IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }
    }
}
