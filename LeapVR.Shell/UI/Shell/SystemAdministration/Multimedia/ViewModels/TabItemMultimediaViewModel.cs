#region Licence
/****************************************************************
 *  Filename: TabItemMultimediaViewModel.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.Modules.Multimedia;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Login.ViewModels;
using LeapVR.Shell.UI.Shell.Login.Views;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;
using LeapVR.Shell.UI.Universal.MediaPlayer.ViewModels;
using Microsoft.Win32;
using NLog;
using Unosquare.FFME;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Multimedia.ViewModels
{
    public class TabItemMultimediaViewModel: TabItemSystemScreen
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Lazy<IMultimediaModule> _multimediaModule = new Lazy<IMultimediaModule>(()=> MultimediaModule.Modules.FirstOrDefault(x => x.Identifier.Equals(GlobalConfig.GetGlobalConfiguration().BackgroundPlayerId)));
        private Uri _selectedPlaylistItem;
        private int _selectedPlaylistIndex;

        public override int DisplayOrder => 11;
        public override string DisplayName
        {
            get { return Resources.System_Multimedia; }
            set { /* ignore */ }
        }

        public MediaElement MultiMediaElement => _multimediaModule.Value.MultiMediaElement;

        public bool Repeat
        {
            get => _multimediaModule.Value.RepeatPlaylist;
            set
            {
                if(value == _multimediaModule.Value.RepeatPlaylist) return;
                _multimediaModule.Value.RepeatPlaylist = value;
                NotifyOfPropertyChange();
            }
        }


        public int VolumeInt
        {
            get => ToIntVolume(_multimediaModule.Value.Volume);
            set
            {
                _multimediaModule.Value.Volume = FromIntVolume(value);
                NotifyOfPropertyChange();
            }
        }

        public bool AutoStart
        {
            get => _multimediaModule.Value.AutoStart;
            set
            {
                if(value == _multimediaModule.Value.AutoStart) return;
                _multimediaModule.Value.AutoStart = value;
                NotifyOfPropertyChange();
            }
        }

        public ReadOnlyObservableCollection<Uri> PlaylistItems => _multimediaModule.Value.Playlist.Tracks;

        public Uri SelectedPlaylistItem
        {
            get => _selectedPlaylistItem;
            set
            {
                if(Equals(value, _selectedPlaylistItem)) return;
                _selectedPlaylistItem = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanRemove));
                NotifyOfPropertyChange(nameof(CanMoveUp));
                NotifyOfPropertyChange(nameof(CanMoveDown));
            }
        }

        public int SelectedPlaylistIndex
        {
            get => _selectedPlaylistIndex;
            set
            {
                if(value == _selectedPlaylistIndex) return;
                _selectedPlaylistIndex = value;
                NotifyOfPropertyChange();
            }
        }


        public TabItemMultimediaViewModel(IUIMessageBroker messageBroker) : base(
                messageBroker,
                "IconVideo")
        {}

        #region Public Methods

        public void PlaylistItemDoubleClick(Uri uri)
        {
            _multimediaModule.Value.Playlist.SelectTrack(_multimediaModule.Value.Playlist.Tracks.IndexOf(SelectedPlaylistItem));
        }

        public void Add()
        {
            var ofd = new OpenFileDialog
                      {
                              Multiselect = true,
                              InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                              Filter = $"{Resources.System_Installation_SelectVBoxFile} (MediaFiles)|*.mp4;*.mkv;*.avi;*.webm;*.mp3"
                      };
            var retval = ofd.ShowDialog();
            if(retval.HasValue && retval.Value)
            {
                foreach(string fileName in ofd.FileNames)
                {
                    if(String.IsNullOrWhiteSpace(fileName))continue;
                    _multimediaModule.Value.Playlist.Add(new Uri(fileName));
                }
            }
        }

        public bool CanMoveUp => SelectedPlaylistItem != null && _multimediaModule.Value.Playlist.Tracks.Count > 0;
        public void MoveUp()
        {
            _multimediaModule.Value.Playlist.MoveUp(_selectedPlaylistIndex);
        }
        public bool CanMoveDown => SelectedPlaylistItem != null && _multimediaModule.Value.Playlist.Tracks.Count > 0;
        public void MoveDown()
        {
            _multimediaModule.Value.Playlist.MoveDown(_selectedPlaylistIndex);
        }

        public bool CanRemove => SelectedPlaylistItem != null;

        public void Remove()
        {

            var indexRemoved = _selectedPlaylistIndex;
            _multimediaModule.Value.Playlist.RemoveAt(_selectedPlaylistIndex);
            SelectedPlaylistIndex = indexRemoved;
            NotifyOfPropertyChange(nameof(CanRemove));
            NotifyOfPropertyChange(nameof(CanMoveUp));
            NotifyOfPropertyChange(nameof(CanMoveDown));
        }
        #endregion


        #region Private Methods
        private double FromIntVolume(int value)
        {
            if(value <= 0) return 0;
            if(value >= 100)return 100;
            double retval = value / 100d;
            return retval;
        }

        private int ToIntVolume(double value)
        {
            return Convert.ToInt32(Math.Round(value * 100));
        }
        #endregion
        protected override void HandleLanguageChange(IUILanguageChangedEvent message)
        {
        }
    }

}
