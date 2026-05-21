#region Licence
/****************************************************************
 *  Filename: EditAppDetailInfoViewModel.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LeapVR.Content.Creator.Logic;
using LeapVR.Shared.Lib.Helper;


namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class EditAppDetailInfoViewModel : ValidatingScreen, IStepScreenEdit
    {
        #region Fields & Properties
        public LeapVrContainerEditor ContainerEditor { get; }
        public IStepScreenWizard Previous { get; set; }
        public IStepScreenWizard Next { get; set; }
        public bool CanGoNext => true;
        public bool CanGoPrevious => true;
        public bool CanGoExit => true;

        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }
        #endregion

        #region Constructors
        public EditAppDetailInfoViewModel(LeapVrContainerEditor containerEditor)
        {
            QuickLeap.AssertNotNull(containerEditor);
            ContainerEditor = containerEditor;

            _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();
        }
        #endregion

    }
}
