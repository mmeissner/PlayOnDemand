#region Licence
/****************************************************************
 *  Filename: PlatformStateProvider.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces.Platform;
using NLog;

namespace LeapVR.Shell.Modules.Platform
{
    /// <summary>
    /// This class is subject to an Published Message <see cref="IUIPlatformNotificationsAvailableEvent"/>
    /// It provides state notifications for platforms allowing the UI or other Modules to get the current State the Platform is in
    /// This is mainly published on an Execution of an Platform Application
    /// </summary>
    /// <seealso cref="LeapVR.Shell.Modules.Interfaces.Platform.IPlatformStateProvider" />
    /// <seealso cref="LeapVR.Shell.Domain.Models.UserInterface.EventMessages.IUIPlatformNotificationsAvailableEvent" />
    public class PlatformStateProvider : IPlatformStateProvider, IUIPlatformNotificationsAvailableEvent
    {
        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppPlatformInfo _platformInfo;
        private readonly IUIMessageBroker _uiMessageBroker;
        private readonly Subject<IPlatformStateDetails> _whenPlatformStateDetailsChange =
                new Subject<IPlatformStateDetails>();
        private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();
        private bool _isUIMessagePublished;
        #endregion

        #region Constructor
        public PlatformStateProvider(IAppPlatformInfo platformAppInfo, IUIMessageBroker messageBroker)
        {
            _platformInfo = platformAppInfo;
            _uiMessageBroker = messageBroker;
        }
        #endregion

        #region Public Methods
        public void Subscribe(
                Action<IPlatformStateDetails> changeNotification, Action notificationsEnded,
                SynchronizationContext context)
        {
            Logger.Debug(
                    $"Subscribed called for PlatformStateProvider for ApplicationGuid ={_platformInfo.ApplicationGuid}, PlatformId={_platformInfo.PlatformAppId}");
            _disposables.Add(
                    _whenPlatformStateDetailsChange.ObserveOn(context).
                                                    Subscribe(changeNotification, notificationsEnded));
        }

        /// <summary>
        /// Publishes the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="details">The details.</param>
        public void Publish(PlatformState state, string details = null)
        {
            EnsureProviderIsPublished();
            Logger.Debug(
                    $"Publishing PlatformStateDetails with State = {state} and Details = {details} for ApplicationGuid ={_platformInfo.ApplicationGuid}, PlatformId={_platformInfo.PlatformAppId}");
            var newState = new PlatformStateDetails(
                    _platformInfo.ApplicationGuid,
                    _platformInfo.PlatformGuid,
                    state,
                    details);
            _disposables.Add(newState);
            _whenPlatformStateDetailsChange.OnNext(newState);
        }

        /// <summary>
        /// Publishes the state that is cancelable.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="details">The details.</param>
        public CancellationToken PublishCancelable(PlatformState state, string details = null)
        {
            EnsureProviderIsPublished();
            Logger.Debug(
                    $"Publishing Cancelable PlatformStateDetails with State = {state} and Details = {details} for ApplicationGuid ={_platformInfo.ApplicationGuid}, PlatformId={_platformInfo.PlatformAppId}");
            var newState = PlatformStateDetails.GetCancelable(
                    _platformInfo.ApplicationGuid,
                    _platformInfo.PlatformGuid,
                    state,
                    out var cancellationToken,
                    details);
            _disposables.Add(newState);
            _whenPlatformStateDetailsChange.OnNext(newState);
            return cancellationToken;
        }


        public void SignalPublishEnd()
        {
            Logger.Debug(
                    $"PlatformStateProvider publishing OnComplete and disposing subscriptions for ApplicationGuid ={_platformInfo.ApplicationGuid}, PlatformId={_platformInfo.PlatformAppId}");
            _whenPlatformStateDetailsChange.OnCompleted();
            foreach(var disposable in _disposables) disposable.Dispose();
        }
        #endregion

        private void EnsureProviderIsPublished()
        {
            //Get into effect on first publishing to allow applications to Subscribe
            //It's only published once afterwards only Observable events are published
            if(!_isUIMessagePublished)
            {
                Logger.Debug(
                        $"First time Publish for PlatformStateProvider publishing itself for ApplicationGuid ={_platformInfo.ApplicationGuid}, PlatformId={_platformInfo.PlatformAppId}");
                _uiMessageBroker.Publish(this);
                _isUIMessagePublished = true;
            }
        }
    }

    /// <summary>
    /// Object that provides Details about the current state of an Platform Client during an Execution
    /// </summary>
    /// <seealso cref="LeapVR.Shell.Domain.Models.UserInterface.EventMessages.IPlatformStateDetails" />
    public class PlatformStateDetails : IPlatformStateDetails, IDisposable
    {
        private readonly bool _isCancelable;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isCanceled;

        public Guid ApplicationId { get; }
        public Guid PlatformId { get; }
        public PlatformState State { get; }
        public bool IsCanceled => _isCanceled;
        public bool IsCancelable => _isCancelable;
        public string Details { get; }

        PlatformStateDetails(
                Guid applicationId, Guid platformId, PlatformState state,
                CancellationTokenSource cancellationTokenSource, string details = null) : this(
                applicationId,
                platformId,
                state,
                details)
        {
            _isCancelable = true;
            _cancellationTokenSource = cancellationTokenSource;
        }
        public PlatformStateDetails(Guid applicationId, Guid platformId, PlatformState state, string details = null)
        {
            ApplicationId = applicationId;
            PlatformId = platformId;
            State = state;
            Details = details;
        }

        public static PlatformStateDetails GetCancelable(
                Guid applicationId, Guid platformId, PlatformState state, out CancellationToken cancellationToken,
                string details = null)
        {
            var cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            return new PlatformStateDetails(applicationId, platformId, state, cts, details);
        }

        public void Cancel()
        {
            if(!_isCancelable) return;
            if(_isCanceled) return;
            lock(_lock)
            {
                if(_isCanceled) return;
                if(_cancellationTokenSource.IsCancellationRequested)
                {
                    _isCanceled = true;
                    return;
                }

                _cancellationTokenSource.Cancel();
                _isCanceled = true;
            }
        }
        public void Dispose() { _cancellationTokenSource?.Dispose(); }
    }
}