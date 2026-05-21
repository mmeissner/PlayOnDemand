#region Licence
/****************************************************************
 *  Filename: CompleteViewModel.cs
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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Creator.Language;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public sealed class CompleteViewModel : Screen
    {

        #region Fields & Properties

        private readonly IWizardModule _creation;
        public IWizardModule Creation => _creation;

        private ImageSource _info;
        public ImageSource Info
        {
            get { return _info; }
            set
            {
                _info = value;
                NotifyOfPropertyChange();
            }
        }


        private string _error;
        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors
        public CompleteViewModel(IWizardModule creation)
        {
            _creation = creation;

            if (_creation.OccuredException == null)
            {
                Info = (BitmapSource)Application.Current.Resources["Image-Ok"];
                DisplayName = Resources.Complete_Done;
            }
            else
            {
                Info = (BitmapSource)Application.Current.Resources["Image-Error"];
                Error = Resources.Complete_ErrorMessage;
                DisplayName = Resources.Complete_Error;
            }
        }

        #endregion

        #region Methods

        public void Done()
        {
            TryClose();
        }

        #endregion
    }
}
