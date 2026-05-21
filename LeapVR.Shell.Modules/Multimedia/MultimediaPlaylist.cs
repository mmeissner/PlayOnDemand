#region Licence
/****************************************************************
 *  Filename: MultimediaPlaylist.cs
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
using System.Collections.ObjectModel;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Modules.Multimedia
{
    /// <summary>
    /// A Playlist that can be used with a Multimedia Module
    /// This object should not be shared by multiple multimedia modules
    /// </summary>
    public class MultimediaPlaylist : IMultimediaPlaylist
    {
        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _listLock = new object();
        private readonly ObservableCollection<Uri> _observableCollection;
        private readonly IMultimediaPlaylistData _playlistData;
        private readonly IMultimediaPlaylistRepository _playlistRepository;
        private int _currentTrackLastPositionIndex = -1;
        private Uri _currentTrackUri;
        private volatile bool _currentTrackRemoved;
        #endregion

        #region Public Methods
        public ReadOnlyObservableCollection<Uri> Tracks { get; }

        public string Identifier => _playlistData.Identifier;

        public (int index, Uri track) LastReturnedTrack => (_currentTrackLastPositionIndex, _currentTrackUri);

        public MultimediaPlaylist(
                IMultimediaPlaylistData multimediaPlaylistData, IMultimediaPlaylistRepository playlistRepository)
        {
            _playlistData = multimediaPlaylistData;
            _playlistRepository = playlistRepository;
            _observableCollection = new ObservableCollection<Uri>(_playlistData.Tracks);
            Tracks = new ReadOnlyObservableCollection<Uri>(_observableCollection);
        }

        public void SelectTrack(int index)
        {
            try
            {
                Logger.Debug("Try aquire lock");
                bool raiseEventOutsideOfLock = false;
                try
                {
                    lock(_listLock)
                    {
                        Logger.Debug("Lock Aquired");
                        if(index < 0)
                        {
                            Logger.Debug($"Invalid Index={index}");
                            return;
                        }

                        if(index <= _observableCollection.Count - 1)
                        {
                            Logger.Debug($"Changeing current Track to Index={index}");
                            _currentTrackLastPositionIndex = index;
                            _currentTrackUri = _observableCollection[index];
                            raiseEventOutsideOfLock = true;
                        }
                    }
                }
                finally
                {
                    Logger.Debug("Lock Released");
                }

                if(raiseEventOutsideOfLock) OnTrackSelected(_currentTrackUri, index);
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
        }

        public void Add(Uri track)
        {
            Logger.Debug("Try aquire lock");
            try
            {
                lock(_listLock)
                {
                    Logger.Debug("Lock Aquired");
                    _observableCollection.Add(track);
                    _playlistData.Tracks.Add(track);
                    _playlistRepository.Store(_playlistData);
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void Remove(Uri track)
        {
            Logger.Debug("Try aquire lock");
            try
            {            
                lock(_listLock)
                {                    
                    Logger.Debug("Lock Aquired");
                    if(_currentTrackUri != null && _currentTrackUri.Equals(track))
                    {
                        _currentTrackRemoved = true;
                    }

                    _observableCollection.Remove(track);
                    _playlistData.Tracks.Remove(track);
                    _playlistRepository.Store(_playlistData);
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void RemoveAt(int index)
        {
            Logger.Debug("Try aquire lock");
            try
            {
                if(!ValidateIndex(index))return;
                lock(_listLock)
                {                
                    Logger.Debug("Lock Aquired");
                    if(_currentTrackLastPositionIndex == index)
                    {
                        _currentTrackRemoved = true;
                    }

                    _observableCollection.RemoveAt(index);
                    _playlistData.Tracks.RemoveAt(index);
                    _playlistRepository.Store(_playlistData);
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void MoveUp(int indexTrack)
        {
            Logger.Debug("Try aquire lock");
            try
            {                
                if(!ValidateIndex(indexTrack))return;
                lock(_listLock)
                {
                    Logger.Debug("Lock Aquired");
                    var index = indexTrack;
                    if(index <= 0) return;
                    var newIndex = index - 1;
                    if(_currentTrackLastPositionIndex >= 0 && _currentTrackLastPositionIndex.Equals(indexTrack))
                    {
                        _currentTrackLastPositionIndex = newIndex;
                    }
                    _observableCollection.Move(index, newIndex);
                    _playlistData.Tracks.MoveItem(index, newIndex);
                    _playlistRepository.Store(_playlistData);
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void MoveDown(int indexTrack)
        {
            Logger.Debug("Try aquire lock");
            try
            {
                if(!ValidateIndex(indexTrack))return;
                lock(_listLock)
                {                    
                    Logger.Debug("Lock Aquired");
                    var index = indexTrack;
                    if(index < 0) return;
                    var maxIndex = _observableCollection.Count - 1;
                    var newIndex = index + 1;
                    if(newIndex > maxIndex) return;
                    if(_currentTrackLastPositionIndex >= 0 && _currentTrackLastPositionIndex.Equals(indexTrack))
                    {
                        _currentTrackLastPositionIndex = newIndex;
                    }
                    _observableCollection.Move(index, newIndex);
                    _playlistData.Tracks.MoveItem(index, newIndex);
                    _playlistRepository.Store(_playlistData);
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void Clear()
        {
            Logger.Debug("Try aquire lock");
            try
            {
                lock(_listLock)
                {
                    Logger.Debug("Lock Aquired");
                    _observableCollection.Clear();
                    _playlistData.Tracks.Clear();
                    _playlistRepository.Store(_playlistData);
                    CleanCurrentState();
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        public void Reset()
        {
            Logger.Debug("Try aquire lock");
            try
            {
                lock(_listLock)
                {
                    Logger.Debug("Lock Aquired");
                    CleanCurrentState();
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
        }

        /// <summary>
        /// Provides the Next Track Uri in the Playlist
        /// </summary>
        /// <param name="track"></param>
        /// <returns>false if no track is available or playlist is finished, but only if</returns>
        public bool TryGetNext(out Uri track)
        {
            Logger.Debug("Try aquire lock");
            try
            {
                lock(_listLock)
                {                    
                    Logger.Debug("Lock Aquired");
                    var nextTrackIndex = -1;
                    var highestIndex = _observableCollection.Count - 1;

                    //No Track ever played or Playlist ended
                    if(_currentTrackLastPositionIndex < 0)
                    {
                        if(highestIndex >= 0)
                        {
                            nextTrackIndex = 0;
                        }
                    }
                    //Play the other track that has now the same index
                    else if(_currentTrackRemoved)
                    {
                        nextTrackIndex = _currentTrackLastPositionIndex;
                    }

                    //Play the next track
                    else
                    {
                        nextTrackIndex = _currentTrackLastPositionIndex + 1;
                    }

                    //Check if next Track would be out of bounds
                    if(nextTrackIndex > highestIndex)
                    {
                        nextTrackIndex = -1;
                    }

                    //Reset Flags after processing
                    _currentTrackRemoved = false;

                    //Process NextTackIndex
                    //No Track Available
                    if(nextTrackIndex < 0)
                    {
                        _currentTrackLastPositionIndex = -1;
                        _currentTrackUri = null;
                        track = null;
                        return false;
                    }

                    //Set next Track
                    _currentTrackLastPositionIndex = nextTrackIndex;
                    _currentTrackUri = _observableCollection[nextTrackIndex];
                    track = _currentTrackUri;
                    return true;
                }
            }
            finally
            {
                Logger.Debug("Lock Released");
            }
            
        }        
        #endregion

        #region Events
        public event TrackSelectedHandler TrackSelected;
        #endregion

        #region Private Methods
        bool ValidateIndex(int index)
        {
            if(index < 0 || index > _observableCollection.Count-1)
            {
                Logger.Debug("Index is invalid");
                return false;
            }

            return true;
        }
        private void CleanCurrentState()
        {
            _currentTrackRemoved = false;
            _currentTrackLastPositionIndex = -1;
            _currentTrackUri = null;
        }
        protected virtual void OnTrackSelected(Uri selectedtrack, int index)
        {
            Logger.Debug($"Raising OnTrackSelected Event with Track={selectedtrack}, Index={index}");
            TrackSelected?.Invoke(selectedtrack, index);
        }
        #endregion
    }
}