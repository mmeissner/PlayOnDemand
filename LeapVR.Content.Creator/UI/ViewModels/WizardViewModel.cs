#region Licence
/****************************************************************
 *  Filename: WizardViewModel.cs
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
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.ContentCreator;
using LeapVR.Shared.Lib.Helper;
using NLog;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public sealed class WizardViewModel : Conductor<IStepScreenWizard>.Collection.OneActive
    {
        #region Fields & Properties

        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public bool CanBack => !_isWorking && ActiveItem?.Previous != null && ActiveItem.CanGoPrevious;
        public bool CanNext => !_isWorking && ActiveItem?.Next != null && ActiveItem.CanGoNext;
        public bool CanCreate => !_isWorking && ActiveItem != null && ActiveItem.CanGoNext && IsLastStep;
        public bool CanExit => !_isWorking && (ActiveItem == null || ActiveItem.CanGoExit);

        // Re-entry guard for Create(). Caliburn binds the button's IsEnabled to
        // CanCreate, but double-clicks land before the property change is
        // dispatched, so the second click still enters Create() and races the
        // first against the same .vbox file (one holds it open for read while
        // the other tries File.Replace -> IOException).
        private bool _isWorking;

        private readonly IStepScreenWizard _lastStep;
        public bool IsLastStep => ActiveItem != null && ActiveItem == _lastStep;

        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }

        private readonly IWizardModule _wizardModule;
        #endregion

        #region Constructors

        public WizardViewModel(IWizardModule wizardModule, IEnumerable<IStepScreenWizard> steps)
        {
            QuickLeap.AssertNotNull(wizardModule, steps);
            _wizardModule = wizardModule;

            ScreenExtensions.TryActivate(this); // http://stackoverflow.com/questions/26241193/why-is-caliburn-micros-used-with-modern-ui-onactivate-not-being-called-after

            _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();

            var stepsArray = steps.ToArray();
            for (int i = 0; i < stepsArray.Length; i++)
            {
                var previous = (i > 0) ? stepsArray[i - 1] : null;
                var current = stepsArray[i];
                var next = (i < (stepsArray.Length - 1)) ? stepsArray[i + 1] : null;

                current.Previous = previous;
                current.Next = next;
            }
            _lastStep = stepsArray.LastOrDefault();

            foreach (var step in stepsArray)
            {
                step.WhenBusyRequested.Subscribe(OnStepScreenBusyRequested);
                step.PropertyChanged += step_OnPropertyChanged;
            }

            Items.AddRange(stepsArray);
            ActivateItem(Items.FirstOrDefault());
        }

        

        #endregion

        #region Methods

        public void Back()
        {
            ActivateItem(ActiveItem.Previous);
            NotifyOfPropertyChange(nameof(CanBack));
            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanCreate));
            NotifyOfPropertyChange(nameof(IsLastStep));
        }

        public void Next()
        {
            ActivateItem(ActiveItem.Next);
            NotifyOfPropertyChange(nameof(CanBack));
            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanCreate));
            NotifyOfPropertyChange(nameof(IsLastStep));
        }

        public async void Create()
        {
            if (_isWorking) return; // double-click guard
            _isWorking = true;
            NotifyOfPropertyChange(nameof(CanCreate));
            NotifyOfPropertyChange(nameof(CanNext));
            NotifyOfPropertyChange(nameof(CanBack));
            NotifyOfPropertyChange(nameof(CanExit));
            try
            {
                await _wizardModule.DoWork();
                if (_wizardModule.OccuredException != null)
                {
                    Logger.Error(_wizardModule.OccuredException);
                }

                var windowManager = IoC.Get<IWindowManager>();
                var settings = new Dictionary<string, object>
                {
                    {"Width", 500},
                    {"Height", 400},
                    {"MaxWidth", 800},
                    {"MaxHeight", 500},
                    {"WindowStartupLocation", 2},
                    {"ResizeMode", 0},
                    {"WindowStyle", 0}
                };
                windowManager.ShowDialog(new CompleteViewModel(_wizardModule), settings: settings);

                var shell = IoC.Get<IShell>();
                shell.ExitWizard();
            }
            finally
            {
                _isWorking = false;
                NotifyOfPropertyChange(nameof(CanCreate));
                NotifyOfPropertyChange(nameof(CanNext));
                NotifyOfPropertyChange(nameof(CanBack));
                NotifyOfPropertyChange(nameof(CanExit));
            }
        }

        public void Exit()
        {
            var shell = IoC.Get<IShell>();
            shell.ExitWizard();
        }

        private void step_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IStepScreenWizard.CanGoNext):
                    NotifyOfPropertyChange(nameof(CanNext));
                    NotifyOfPropertyChange(nameof(CanCreate));
                    break;
                case nameof(IStepScreenWizard.CanGoPrevious):
                    NotifyOfPropertyChange(nameof(CanBack));
                    break;
                case nameof(IStepScreenWizard.CanGoExit):
                    NotifyOfPropertyChange(nameof(CanExit));
                    break;
            }
        }

        private void OnStepScreenBusyRequested(BusyCancelableViewModel busy)
        {
            _whenBusyRequestedSubject.OnNext(busy);
        }

        #endregion
    }
}