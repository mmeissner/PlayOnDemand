#region Licence
/****************************************************************
 *  Filename: InformationShellBlockingViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-6
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
using System.Threading.Tasks;
using System.Windows.Media;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Blocker.Abstract;

namespace LeapVR.Shell.UI.Shell.Blocker.ViewModels
{
    /// <inheritdoc />
    /// <summary>
    /// Representing a view that blocks the shell with custimized information. Derived from <see cref="T:LeapVR.Shell.UI.Shell.Blocker.Abstract.BlockShellBaseViewModel" />
    /// </summary>
    public class InformationShellBlockingViewModel : BlockShellBaseViewModel
    {

        #region Fields & Properties
        private ImageSource _thumbnail;
        private string _information;
        private TimeSpan _minDisplayTime;

        public ImageSource Thumbnail
        {
            get => _thumbnail;
            private set
            {
                _thumbnail = value;
                NotifyOfPropertyChange();
            }
        }

        public string Information
        {
            get => _information;
            private set
            {
                _information = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors
        public InformationShellBlockingViewModel(TimeSpan minimumDisplayTime,ImageSource image,string message, IViewInputHandler inputHandler) : base(inputHandler)
        {
            _thumbnail = image;
            _information = message;
            _minDisplayTime = minimumDisplayTime;
            CloseAfterTime().Forget();
        }
        #endregion

        #region Methods
        private async Task CloseAfterTime()
        {
            await Task.Delay(_minDisplayTime);
            Close();
        }
        #endregion
    }
}
