#region Licence
/****************************************************************
 *  Filename: ImageContentViewModel.cs
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
using System.Windows.Media;

namespace LeapVR.Shell.UI.Universal.ContentHolder.ViewModels
{
    public class ImageContentViewModel : StringContentViewModel
    {

        #region Fields & Properties
        private ImageSource _imageContent;
        public ImageSource ImageContent
        {
            get => _imageContent;
            set
            {
                _imageContent = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        public ImageContentViewModel(Func<string> titleFunc, ImageSource image) : base(titleFunc)
        {
            ImageContent = image;
        }
        #endregion

        #region Methods

        #endregion
    }
}
