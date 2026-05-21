#region Licence
/****************************************************************
 *  Filename: MediaPlayerViewModel.cs
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Multimedia;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Modules.Multimedia;
using LeapVR.Shell.UI.Universal.MediaPlayer.Views;
using LeapVR.Utilities.Steam.Steam;
using NLog;
using Unosquare.FFME;
using Unosquare.FFME.Events;
using Unosquare.FFME.Platform;
using MediaElement = Unosquare.FFME.MediaElement;

namespace LeapVR.Shell.UI.Universal.MediaPlayer.ViewModels
{
    public class MediaPlayerViewModel : Screen
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IMultimediaSet _multimediaSet;
        private Visibility _videoVisibility = Visibility.Hidden;
        private MultimediaModule _multimediaModule;

        public MultimediaModule MultimediaModule
        {
            get => _multimediaModule;
            set
            {
                if(Equals(value, _multimediaModule)) return;
                _multimediaModule = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility VideoVisibility
        {
            get => _videoVisibility;
            set
            {
                if(value == _videoVisibility) return;
                _videoVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public MediaPlayerViewModel(IMultimediaSet multimediaSet)
        {
            _multimediaSet = multimediaSet;
        }
        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if(view is MediaPlayerView mediaPlayer)
            {
                MultimediaModule = new MultimediaModule(this,mediaPlayer.MediaPlayer,_multimediaSet.Settings);
                await MultimediaModule.LoadAsync(_multimediaSet.Playlist);
                NotifyOfPropertyChange(nameof(MultimediaModule));
            }
        }
    }
}
