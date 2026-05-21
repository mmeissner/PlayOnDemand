#region Licence
/****************************************************************
 *  Filename: PlaylistModule.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Modules.Multimedia
{

    public class PlaylistModule : IPlaylistModule
    {
        private readonly ConcurrentDictionary<string,MultimediaPlaylist> _multimediaPlaylistsCache = new ConcurrentDictionary<string, MultimediaPlaylist>();
        private readonly IMultimediaPlaylistRepository _playlistRepository;
        public PlaylistModule(IMultimediaPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public IMultimediaPlaylist GetPlaylist(string identifier)
        {
            if(_multimediaPlaylistsCache.TryGetValue(identifier, out var playlist))
            {
                return playlist;
            }
            var retval = _playlistRepository.GetOrCreate(identifier);
            playlist = new MultimediaPlaylist(retval,_playlistRepository);
            playlist =_multimediaPlaylistsCache.AddOrUpdate(playlist.Identifier,playlist,((s, multimediaPlaylist) => multimediaPlaylist));
            return playlist;
        }

        public void DeletePlaylist(IMultimediaPlaylist playlist)
        {
            if(_multimediaPlaylistsCache.TryRemove(playlist.Identifier,out _))
            {
                _playlistRepository.Delete(playlist.Identifier);
            }
        }
    }
}
