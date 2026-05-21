#region Licence
/****************************************************************
 *  Filename: ApplicationThumbnailViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-25
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Reflection;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    
    public class ApplicationThumbnailViewModel : Screen
    {
        private ImageSource _source;
        private bool _isScreenModeSupported;
        private bool _isVrModeSupported;

        #region Fields & Properties
        public ImageSource Source
        {
            get => _source;
            set
            {
                if(Equals(value, _source)) return;
                _source = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsScreenModeSupported
        {
            get => _isScreenModeSupported;
            set
            {
                if(value == _isScreenModeSupported) return;
                _isScreenModeSupported = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsVrModeSupported
        {
            get => _isVrModeSupported;
            set
            {
                if(value == _isVrModeSupported) return;
                _isVrModeSupported = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors


        public ApplicationThumbnailViewModel(ImageSource image)
        {
            FreezeImage(image);
            Source = image;
        }

        public ApplicationThumbnailViewModel(ImageSource imageSource, bool isScreenModeSupported, bool isVrModeSupported)
        {
            FreezeImage(imageSource);
            Source = imageSource;
            IsScreenModeSupported = isScreenModeSupported;
            IsVrModeSupported = isVrModeSupported;
        }

        private void FreezeImage(ImageSource imageSource)
        {
            if(imageSource != null && imageSource.CanFreeze && !imageSource.IsFrozen)imageSource.Freeze();
        }
        #endregion
    }
}
