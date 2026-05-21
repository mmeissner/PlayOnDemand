#region Licence
/****************************************************************
 *  Filename: StringContentViewModel.cs
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
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Universal.ContentHolder.ViewModels
{
    public class StringContentViewModel : Screen,IDialogContent
    {
        #region Fields & Properties

        private string _stringContent;
        private readonly Func<string> _stringContentFunc;
        public string StringContent
        {
            get => _stringContent;
            set
            {
                _stringContent = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        public StringContentViewModel(Func<string> stringContentFunc)
        {
            _stringContentFunc = stringContentFunc;
            StringContent = _stringContentFunc.Invoke();
        }
        #endregion

        #region Methods
        public void UpdateContentString()
        {
            StringContent = _stringContentFunc.Invoke();
        }
        #endregion
    }
}
