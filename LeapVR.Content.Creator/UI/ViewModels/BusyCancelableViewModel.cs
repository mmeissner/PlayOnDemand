#region Licence
/****************************************************************
 *  Filename: BusyCancelableViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-5-19
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using LeapVR.Shared.Lib;
namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class BusyCancelableViewModel : Screen, IDisposable
    {
        #region Properties & Fields

        public bool CanRequestCancel => !_isCancelRequested && !_isOperationEnded;

        private bool _isCancelRequested;
        private readonly ReplaySubject<Empty> _whenCancelRequestedSubject;
        public IObservable<Empty> WhenCancelRequested { get; }

        private bool _isOperationEnded;
        private readonly ReplaySubject<Empty> _whenOperationEndedSubject;
        public IObservable<Empty> WhenOperationEnded { get; }

        #endregion Properties & Fields

        #region Constructors

        public BusyCancelableViewModel()
        {
            _whenCancelRequestedSubject = new ReplaySubject<Empty>();
            WhenCancelRequested = _whenCancelRequestedSubject.AsObservable();

            _whenOperationEndedSubject = new ReplaySubject<Empty>();
            WhenOperationEnded = _whenOperationEndedSubject.AsObservable();
        }

        #endregion Constructors

        #region Methods

        public void RequestCancel()
        {
            if (_isCancelRequested)
            {
                return;
            }
            _isCancelRequested = true;
            NotifyOfPropertyChange(nameof(CanRequestCancel));

            _whenCancelRequestedSubject.OnNext(Empty.Get);
            _whenCancelRequestedSubject.OnCompleted();
        }

        public void NotifyOperationEnded()
        {
            if (_isOperationEnded)
            {
                return;
            }
            _isOperationEnded = true;
            NotifyOfPropertyChange(nameof(CanRequestCancel));

            _whenOperationEndedSubject.OnNext(Empty.Get);
            _whenOperationEndedSubject.OnCompleted();
        }

        #endregion Methods

        public void Dispose()
        {
            _whenCancelRequestedSubject?.Dispose();
            _whenOperationEndedSubject?.Dispose();
        }
    }
}
