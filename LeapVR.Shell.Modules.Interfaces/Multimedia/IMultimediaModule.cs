#region Licence
/****************************************************************
 *  Filename: IMultimediaModule.cs
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
using System.Threading.Tasks;

namespace LeapVR.Shell.Modules.Interfaces.Multimedia {
    public interface IMultimediaModule {
        string Identifier { get; }
        IMultimediaPlaylist Playlist { get; }
        Uri CurrentlyPlaying { get; }
        bool AutoStart { get; set; }
        bool RepeatPlaylist { get; set; }
        double Volume { get; set; }
        IObservable<MultimediaMessage> WhenMultimediaMessage { get; }
        Task LoadAsync(IMultimediaPlaylist playlist);
        Task PlayAsync();
        Task PauseAsync();
        Task StopAsync();
    }

    public struct MultimediaMessage
    {
        public IMultimediaModule Sender;
        public MultimediaAction Action;
        public MultimediaResponse Response;
        public Uri Source;

        public MultimediaMessage(
                IMultimediaModule sender, MultimediaAction action, MultimediaResponse response, Uri source)
        {
            Sender = sender;
            Action = action;
            Response = response;
            Source = source;
        }
    }

    public enum MultimediaAction
    {
        OnLoad,
        OnPlay,
        OnPause,
        OnStop,
    }

    public enum MultimediaResponse
    {
        Success,
        LoadFailed,
        PlayFailed,
        PauseFailed,
        StopFailed,
    }

    public enum MultimediaReason
    {
        None,
        SourceUnavailable,
        NoSourceLoaded,
    }
}