#region Licence
/****************************************************************
 *  Filename: MultimediaModule.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LeapVR.Shell.Categories.Annotations;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Multimedia;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Repository.Interfaces.Entities;
using NLog;
using Unosquare.FFME.Events;
using Unosquare.FFME.Platform;
using MediaElement = Unosquare.FFME.MediaElement;

namespace LeapVR.Shell.Modules.Multimedia
{
    public interface IMultimediaModule : INotifyPropertyChanged
    {
        string Identifier { get; }
        object ViewModel { get; }
        MediaElement MultiMediaElement { get; }
        IMultimediaPlaylist Playlist { get; }
        bool AutoStart { get; set; }
        bool RepeatPlaylist { get; set; }
        double Volume { get; set; }
        IObservable<MultimediaMessage> WhenMultimediaMessage { get; }
        Task LoadAsync(IMultimediaPlaylist playlist);
        Task PlayAsync();
        Task PauseAsync();
        Task StopAsync();
    }

    public class MultimediaModule : IMultimediaModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object ControllersCollectionLock = new object();
        private static readonly object InitializationLock = new object();
        private static List<MultimediaModule> _modules;

        private readonly MediaElement _mediaElement;
        private readonly IMultimediaSettings _settings;
        private readonly Subject<MultimediaMessage> _whenMultimediaMessageSubject = new Subject<MultimediaMessage>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);


        private IMultimediaPlaylist _playlist = null;

        #region Static Public Properties
        public static bool Initialized { get;private set; }

        public static bool Error { get; private set; }

        public static IReadOnlyList<MultimediaModule> Modules => _modules;
        #endregion

        #region Public Properties
        public string Identifier => _settings.Identifier;
        public IMultimediaPlaylist Playlist => _playlist;
        public MediaElement MultiMediaElement => _mediaElement;
        public object ViewModel { get; }
        public bool AutoStart
        {
            get => _settings.AutoStart;
            set
            {
                if(value == _settings.AutoStart)return;
                _settings.AutoStart = value;
                if(!_mediaElement.IsPlaying && _playlist != null)
                {
                    Logger.Debug("Starting Task with Internal Play on AutoStart Property Change");
                    Task.Run(async ()=>await ExecuteActionWithLock(Internal_Play));
                }
                OnPropertyChanged();
                SaveSettings();
                //Handle AutoStart 
            }
        }
        public bool RepeatPlaylist
        {
            get => _settings.Repeat;
            set
            {
                if(value == _settings.Repeat)return;
                _settings.Repeat = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }
        public double Volume
        {
            get => _settings.Volume;
            set
            {
                _settings.Volume=GetValidMediaElementVolume(value);
                OnPropertyChanged();
                SaveSettings();
            }
        }
        public IObservable<MultimediaMessage> WhenMultimediaMessage { get; }
        #endregion

        #region Constructor
        public MultimediaModule(object viewmodel,MediaElement mediaElement, IMultimediaSettings settings)
        {
            if(!Initialized && !Error) Initialize();
            ViewModel = viewmodel;
            _mediaElement = mediaElement;
            _settings = settings;
            _mediaElement.MediaInitializing += MediaPlayerOnMediaInitializing;
            _mediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
            _mediaElement.MediaOpening += MediaPlayerOnMediaOpening;
            _mediaElement.MediaOpened += MediaPlayer_MediaOpened;
            _mediaElement.MediaChanged += MediaElement_MediaChanged;
            _mediaElement.MediaChanging += MediaElement_MediaChanging;
            _mediaElement.MediaClosed += MediaElement_MediaClosed; 
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
            _mediaElement.MediaFailed += MediaPlayer_MediaFailed;
            _mediaElement.MessageLogged += MediaPlayer_MessageLogged;
            _mediaElement.AudioDeviceStopped += OnAudioDeviceStopped;

            WhenMultimediaMessage = _whenMultimediaMessageSubject.AsObservable();

            lock(ControllersCollectionLock)
            {
                _modules.Add(this);
            }
        }
        #endregion

        #region Public Methods
        public async Task LoadAsync(IMultimediaPlaylist playlist){await ExecuteActionWithLock(Internal_Load,playlist);}
        public async Task PlayAsync() { await ExecuteActionWithLock(Internal_Play); }
        public async Task PauseAsync() { await Internal_Pause();}
        public async Task StopAsync() { await ExecuteActionWithLock(Internal_Stop);}

        #endregion

        #region Internal Static Methods
        internal static void Initialize()
        {
            lock(InitializationLock)
            lock(ControllersCollectionLock)
            {
                if(Initialized) return;
                if(_modules == null)_modules = new List<MultimediaModule>();
                // Change the default location of the ffmpeg binaries
                // Select the bitness that is right for the Application (32 vs 64)
                // You can get the binaries here: https://ffmpeg.zeranoe.com/builds
                MediaElement.FFmpegDirectory = GlobalConfig.GetGlobalConfiguration().ShellBinaryPath;

                // Multithreaded video enables the creation of independent
                // dispatcher threads to render video frames.
                MediaElement.EnableWpfMultithreadedVideo = GuiContext.Current.IsInDebugMode == false;
                try
                {
                    MediaElement.LoadFFmpeg();
                    Initialized = true;
                }
                catch(Exception exception)
                {
                    Error = true;
                    Logger.Error(exception);
                }
            }
        }
        #endregion

        #region Private Methods

        async Task ExecuteActionWithLock(Func<Task> taskAsync,[CallerMemberName] string memberName = "")
        {
            try
            {                
                Logger.Debug($"Caller = {memberName}, trying to enter semaphore");
                await _semaphore.WaitAsync();
                Logger.Debug($"Caller = {memberName}, entered semaphore");
                await taskAsync();
            }
            finally
            {
                Logger.Debug($"Caller = {memberName}, releasing semaphore");
                _semaphore.Release();;
            }
        }

        async Task ExecuteActionWithLock<T>(Func<T,Task> taskAsync,T value,[CallerMemberName] string memberName = "")
        {
            try
            {
                Logger.Debug($"Caller = {memberName}, trying to enter semaphore");
                await _semaphore.WaitAsync();
                Logger.Debug($"Caller = {memberName}, entered semaphore");
                await taskAsync(value);
            }
            finally
            {
                Logger.Debug($"Caller = {memberName}, releasing semaphore");
                _semaphore.Release();
            }
        }

        private double GetValidMediaElementVolume(double value)
        {
            double retval = 0;
            if(value > 1){retval = 1;}
            else if(value <= 0) retval = 0;
            else retval = value;
            return retval;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Logger.Debug($"OnCollectionChanged with Action={e.Action}");
            //Handle Current Track Removed
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //AutoPlay On but no Track was in List
                    if(AutoStart && !_mediaElement.IsPlaying)
                    {
                        Logger.Debug("Starting Task to PlayNextTrack as Autoplay is active and MediaElement is not Playing");
                        Task.Run(async ()=> await ExecuteActionWithLock(Play_Next_Track));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    //Current Playing Track was removed from Playlist
                    if(_playlist.LastReturnedTrack.index.Equals(e.OldStartingIndex))
                    {
                        Logger.Debug("Starting Task to PlayNextTrack as current Track was removed");
                        Task.Run(async ()=> await ExecuteActionWithLock(Play_Next_Track));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    //Playlist was emptied
                    if(_mediaElement.IsPlaying)
                    {                        
                        Logger.Debug("Starting Task to Stop as current Playlist was cleared and MediaElement is playing");
                        Task.Run(StopAsync);
                    }
                    break;
            }
        }

        /// <summary>
        /// Execute in semaphore Lock Only
        /// </summary>
        /// <param name="playlist">The playlist.</param>
        /// <returns></returns>
        private async Task Internal_Load(IMultimediaPlaylist playlist)
        {
            try
            {
                Logger.Debug("Internal_Load Started");

                //It's the same as we have
                if(playlist.Equals(_playlist))
                {
                    Logger.Debug("Playlist is already Loaded, doing nothing");
                    return;
                }

                //Remove old Playlist Notifications
                if(_playlist != null)
                {
                    Logger.Debug("Old Playlist detected, unsubcribing from Events");
                    ((INotifyCollectionChanged)_playlist.Tracks).CollectionChanged -= OnCollectionChanged;
                    _playlist.TrackSelected -= PlaylistOnTrackSelected;
                }

                //Stop Playback
                Task<Task> stopTask = null;
                if(_mediaElement.IsOpen)
                {
                    Logger.Debug("Detected Open Media on Playlist Change");
                    stopTask = _mediaElement.Stop().ContinueWith(x => _mediaElement.Close());
                }

                //Set New PlayList
                _playlist = playlist;

                //Setup Notifications
                if(_playlist != null)
                {
                    Logger.Debug("Subscribing to new Playlist as it is not null");
                    ((INotifyCollectionChanged)_playlist.Tracks).CollectionChanged += OnCollectionChanged;
                    _playlist.TrackSelected += PlaylistOnTrackSelected;
                }

                //Check for AutoStart
                if(AutoStart)
                {
                    Logger.Debug($"Autostart detected with stop task={stopTask != null}");
                    if(stopTask != null) await stopTask;
                    Logger.Debug("Calling internal Play");
                    await Internal_Play();
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Logger.Debug("Internal_Load Ended");
            }

        }

        private void PlaylistOnTrackSelected(Uri selectedtrack, int index) 
        { Task.Run(
                async () =>
                {
                    await _mediaElement.Open(selectedtrack).ContinueWith(x => _mediaElement.Play());
                    ApplyAppearanceForSource(selectedtrack);
                });
        }

        /// <summary>
        /// Execute in semaphore Lock Only
        /// </summary>
        private async Task Internal_Play()
        {
            try
            {
                Logger.Debug("Internal_Play Started");
                //Already Playing
                if(_mediaElement.IsPlaying)
                {
                    Logger.Debug("Detected Media is already Playing, nothing to do");
                    return;
                }

                //Is Paused
                if(_mediaElement.IsPaused && _mediaElement.IsOpen)
                {                    
                    Logger.Debug("Detected Media is already Paused and Open calling play");
                    await _mediaElement.Play();
                    return;
                }

                //Start Playing current Media that is open if one is opened
                if(_mediaElement.IsOpen)
                {
                    Logger.Debug("Detected Media is already Open calling play");
                    await _mediaElement.Play();
                    return;
                }

                //Start Playing next Track
                Logger.Debug("Try to play next Track");
                await Play_Next_Track();
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Logger.Debug("Internal_Play Ended");
            }
        }

        private async Task Internal_Pause()
        {
            try
            {
                //Already Playing
                Logger.Debug("Internal_Pause Started");
                if(_mediaElement.IsPlaying)
                {
                    await _mediaElement.Pause();
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Logger.Debug("Internal_Pause Ended");
            }
        }

        private async Task Internal_Stop()
        {
            try
            {
                Logger.Debug("Internal_Stop Started");
                //Already Playing
                if(_mediaElement.IsPlaying || _mediaElement.IsPaused)
                {
                    Logger.Debug("Paused or Playing Media detected, calling Stop");
                    await _mediaElement.Stop();
                }
                else
                {
                    Logger.Debug("No Playing or Paused Media detected");
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Logger.Debug("Internal_Stop Ended");
            }
        }

        /// <summary>
        /// Execute in semaphore Lock Only
        /// </summary>
        private async Task Play_Next_Track()
        {
            try
            {
                Logger.Debug("Play_Next_Track Started");
                if(TryGetNextTrack(out var track))
                {
                    //This Hack provides a smother playback of looping videos so that they seem close to neatless
                    if(_mediaElement.MediaInfo?.InputUrl != null && _mediaElement.MediaInfo.InputUrl.Equals(track.LocalPath))
                    {
                        //If the source is the same we do not need to reopen it but can rewind and play
                        await _mediaElement.Stop();
                        await _mediaElement.Play();
                    }
                    else
                    {
                        Logger.Debug("Could aquire next Track to Play, going to Open and Play it");
                        await _mediaElement.Open(track).ContinueWith(x => _mediaElement.Play());
                        ApplyAppearanceForSource(track);
                    }
                }
                else
                {
                    Logger.Debug("Could not aquire next Track to Play, calling Stop!");
                    await _mediaElement.Stop();
                    SetSourceFromUIThread(null);
                    ChangeMediaElementVisibility(Visibility.Hidden);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            finally
            {
                Logger.Debug("Play_Next_Track Ended");
            }
        }

        private bool TryGetNextTrack(out Uri track)
        {
            Logger.Debug("Try to get Next Track from Playlist");
            if(_playlist != null)
            {
                if(_playlist.TryGetNext(out track))
                {
                    Logger.Debug("Received Next Track from Playlist");
                    return true;
                }
                Logger.Debug("No Next Track in Playlist, Resetting Playlist");
                _playlist.Reset();
                if(RepeatPlaylist)
                {
                    Logger.Debug("Repeate Playlist is on try to get next track");
                    var retval = _playlist.TryGetNext(out track);
                    Logger.Info($"Repeate Playlist Success is = {retval}");
                    return retval;
                }
            }
            else
            {
                Logger.Info("No Playlist availible");
            }
            track = null;
            return false;
        }
        private void SaveSettings()
        {
            Logger.Debug("Saving Settings");
            _settings.Store();
        }
        #endregion

        #region MediaElement EventHandlers
        private void MediaElement_MediaStateChanged(object sender, MediaStateChangedRoutedEventArgs e)
        {
            Logger.Debug($"MediaState Changed Old={e.OldMediaState}, New={e.MediaState}");
            if(e.OldMediaState == MediaState.Play && e.MediaState == MediaState.Stop)
            {
                Logger.Debug("MediaState from Play to Stop detected, trying to play next track");
                Task.Run(async ()=> await ExecuteActionWithLock(Play_Next_Track));
            }
        }

        /// <summary>
        /// Called when the current audio device changes.
        /// Call <see cref="FFME.MediaElement.ChangeMedia"/> so the new default audio device gets selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void OnAudioDeviceStopped(object sender, EventArgs e) =>
                await _mediaElement?.ChangeMedia();

        private void MediaElement_MediaChanging(object sender, MediaOpeningEventArgs e)
        {
            Logger.Debug($"Media Changeing ={e.Info.InputUrl}");
        }

        private void MediaElement_MediaChanged(object sender, MediaOpenedRoutedEventArgs e)
        {
            Logger.Debug($"Media Changed ={e.Info.InputUrl}");
        }

        private void MediaPlayerOnMediaInitializing(object sender, MediaInitializingEventArgs e)
        {
            Logger.Debug($"Media Initialized ={e.Url}");

        }

        private void MediaPlayerOnMediaOpening(object sender, MediaOpeningEventArgs e)
        {
            Logger.Debug($"Media Opening ={e.Info.InputUrl}");
        }

        private void MediaPlayer_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            Logger.Debug($"Media Opened ={((MediaElement)sender).Source}");

        }

        private void MediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            Logger.Debug($"Media Ended={_mediaElement.Source}");
        }

        private void MediaElement_MediaClosed(object sender, System.Windows.RoutedEventArgs e)
        {
            Logger.Debug("Media Closed");
        }

        private void MediaPlayer_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            //_whenMultimediaMessageSubject.OnNext(new MultimediaMessage(this, MultimediaAction.OnLoad));
            Logger.Error(e.ErrorException);
        }

        private void MediaPlayer_MessageLogged(object sender, MediaLogMessageEventArgs e)
        {
            Logger.Trace(e.Message);
        }
        #endregion

        private void ApplyAppearanceForSource(Uri source)
        {
            string filename = null;
            ContentType contentType;
            if (source != null && !String.IsNullOrEmpty(source.OriginalString)){filename = source.Segments.LastOrDefault();}
            if (!String.IsNullOrEmpty(filename))
            {
                var extension = Path.GetExtension(filename);
                switch (extension.ToLowerInvariant())
                {
                    case ".jpg":
                        contentType = ContentType.Image;
                        break;
                    case ".png":
                        contentType = ContentType.Image;
                        break;
                    case ".bmp":
                        contentType = ContentType.Image;
                        break;
                    case ".gif":
                        contentType = ContentType.Image;
                        break;
                    case ".avi":
                        contentType = ContentType.Video;
                        break;
                    case ".mpg":
                        contentType = ContentType.Video;
                        break;
                    case ".mp4":
                        contentType = ContentType.Video;
                        break;
                    case ".wmv":
                        contentType = ContentType.Video;
                        break;
                    case ".mkv":
                        contentType = ContentType.Video;
                        break;
                    case ".flv":
                        contentType = ContentType.Video;
                        break;
                    case ".mp3":
                        contentType = ContentType.Audio;
                        break;
                    default:
                        contentType = ContentType.Unknown;
                        break;
                }
            }
            else
            {
                contentType = ContentType.Unknown;
            }

            switch (contentType)
            {   
                case ContentType.Unknown:
                case ContentType.Video:
                case ContentType.Image:
                    ChangeMediaElementVisibility(Visibility.Visible);
                    return;
                case ContentType.Audio:
                    ChangeMediaElementVisibility(Visibility.Collapsed);
                    return;
            }
        }

        private void SetSourceFromUIThread(Uri value)
        {
            if(Application.Current.CheckAccess())
            {
                _mediaElement.Source = value;
                return;
            }
            Application.Current.Dispatcher.Invoke(() => SetSourceFromUIThread(value));
        }

        private void ChangeMediaElementVisibility(Visibility value)
        {
            if(Application.Current.CheckAccess())
            {
                if(_mediaElement.Visibility.Equals(value))return;
                _mediaElement.Visibility = value;
                return;
            }
            Application.Current.Dispatcher.Invoke(() => ChangeMediaElementVisibility(value));
        }

        enum  ContentType
        {
            Unknown,
            Video,
            Audio,
            Image
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }  
}
