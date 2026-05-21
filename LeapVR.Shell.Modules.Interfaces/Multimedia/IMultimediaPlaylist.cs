#region Licence
/****************************************************************
 *  Filename: IMultimediaPlaylist.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Modules.Interfaces.Multimedia
{
    public delegate void TrackSelectedHandler(Uri selectedTrack, int index);
    public interface IMultimediaPlaylist {
        string Identifier { get; }
        (int index, Uri track) LastReturnedTrack { get; }
        ReadOnlyObservableCollection<Uri> Tracks { get; }
        void SelectTrack(int index);
        void Add(Uri track);
        void Remove(Uri track);
        void RemoveAt(int index);
        void MoveUp(int indexTrack);
        void MoveDown(int indexTrack);
        void Clear();
        void Reset();
        /// <summary>
        /// Provides the Next Track Uri in the Playlist
        /// </summary>
        /// <param name="track"></param>
        /// <returns>false if no track is available or playlist is finished, but only if <see cref="RepeatPlaylist"/> is false</returns>
        bool TryGetNext(out Uri track);
        event TrackSelectedHandler TrackSelected;
    }
}
