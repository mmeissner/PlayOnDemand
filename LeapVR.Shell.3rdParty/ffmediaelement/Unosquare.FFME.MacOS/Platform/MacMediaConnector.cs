#region Licence
/****************************************************************
 *  Filename: MacMediaConnector.cs
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
namespace Unosquare.FFME.MacOS.Platform
{
    using Shared;
    using System;

    internal class MacMediaConnector : IMediaConnector
    {
        private readonly MediaElement Control = null;

        public MacMediaConnector(MediaElement control)
        {
            Control = control;
        }

        public void OnMessageLogged(MediaEngine sender, MediaLogMessage e)
        {
            if (e.MessageType == MediaLogMessageType.Trace) return;
            Console.WriteLine($"{e.MessageType,10} - {e.Message}");
        }

        public void OnBufferingEnded(MediaEngine sender) { }

        public void OnBufferingStarted(MediaEngine sender) { }

        public void OnMediaClosed(MediaEngine sender) { }

        public void OnMediaEnded(MediaEngine sender) { }

        public void OnMediaFailed(MediaEngine sender, Exception e) { }

        public void OnMediaOpened(MediaEngine sender, MediaInfo info) { }

        public void OnMediaOpening(MediaEngine sender, MediaOptions options, MediaInfo mediaInfo) { }

        public void OnMediaInitializing(MediaEngine sender, ContainerConfiguration config, string url) { }

        public void OnSeekingEnded(MediaEngine sender) { }

        public void OnSeekingStarted(MediaEngine sender) { }

        public void OnPositionChanged(MediaEngine sender, TimeSpan oldValue, TimeSpan newValue) { }

        public void OnMediaStateChanged(MediaEngine sender, PlaybackStatus oldValue, PlaybackStatus newValue) { }

        public void OnMediaChanging(MediaEngine sender, MediaOptions mediaOptions, MediaInfo mediaInfo) { }

        public void OnMediaChanged(MediaEngine sender, MediaInfo info) { }
    }
}
