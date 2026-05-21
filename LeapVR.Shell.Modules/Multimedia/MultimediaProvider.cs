#region Licence
/****************************************************************
 *  Filename: MultimediaProvider.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Multimedia;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Modules.Multimedia
{
    class MultimediaSet : IMultimediaSet
    {
        public string Identifier { get; }
        public IMultimediaPlaylist Playlist { get; }
        public IMultimediaSettings Settings { get; }
        public MultimediaSet(string identifier, IMultimediaPlaylist playlist, IMultimediaSettings settings)
        {
            Identifier = identifier;
            Playlist = playlist;
            Settings = settings;
        }
    }

    public class MultimediaProvider : IMultimediaProvider
    {
        private readonly IMultimediaSettingsRepository _settingsRepository;
        private readonly IPlaylistModule _playlistModule;
        public MultimediaProvider(IMultimediaSettingsRepository settingsRepository, IPlaylistModule playlistModule)
        {
            _settingsRepository = settingsRepository;
            _playlistModule = playlistModule;
        }

        public IMultimediaSet GetMultimediaSet(string identifier)
        {
            return new MultimediaSet(identifier,_playlistModule.GetPlaylist(identifier),_settingsRepository.Get(identifier));
        }
    }
}
