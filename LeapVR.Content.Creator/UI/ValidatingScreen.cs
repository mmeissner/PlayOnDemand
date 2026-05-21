#region Licence
/****************************************************************
 *  Filename: ValidatingScreen.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;

namespace LeapVR.Content.Creator.UI
{
    public abstract  class ValidatingScreen : Screen, IDataErrorInfo, IDisposable
    {
        #region Properties & Fields

        public string Error { get; } = "";
        public string this[string columnName] => _validationErrors.ContainsKey(columnName) ? _validationErrors[columnName] : null;

        private readonly Dictionary<string, string> _validationErrors;
 
        protected virtual bool IsAllRequiredDataFilled => true;
        public bool IsValid => IsAllRequiredDataFilled && _validationErrors.All(kv => string.IsNullOrEmpty(kv.Value));

        private readonly Subject<bool> _whenRevalidatedSubject;
        public IObservable<bool> WhenRevalidated { get; }

        #endregion Properties & Fields

        #region Constructors

        protected ValidatingScreen()
        {
            _validationErrors = new Dictionary<string, string>();

            _whenRevalidatedSubject = new Subject<bool>();
            WhenRevalidated = _whenRevalidatedSubject.AsObservable();
        }

        #endregion Constructors

        #region Methods

        protected void UpdateValidationError(string propertyName, string error)
        {
            if (_validationErrors.ContainsKey(propertyName))
            {
                _validationErrors[propertyName] = error;
            }
            else
            {
                _validationErrors.Add(propertyName, error);
            }

            NotifyOfPropertyChange(nameof(IsValid));
            _whenRevalidatedSubject.OnNext(IsValid);
            OnRevalidated(IsValid);
        }

        protected virtual void OnRevalidated(bool isValid)
        {
            //
        }

        #endregion Methods

        public void Dispose()
        {
            _whenRevalidatedSubject?.Dispose();
        }
    }
}
