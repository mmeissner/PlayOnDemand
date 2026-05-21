#region Licence
/****************************************************************
 *  Filename: SessionBaseViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-1
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Threading;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public abstract class SessionBaseViewModel : Screen, ISessionViewModel, IHandle<IUISessionUpdatedEvent>
    {
        #region Fields & Properties
        private TimeSpan _played;
        private readonly IUIMessageBroker _messageBroker;
        private readonly DispatcherTimer _updateTimer;
        private IUISessionData _sessionData;
        /// <summary>
        /// Get or set the display information of the rate.
        /// </summary>
        /// <summary>
        /// Get or set the display information of how long the user has already played.
        /// </summary>
        public TimeSpan Played
        {
            get => _played;
            set
            {
                _played = value;
                NotifyOfPropertyChange(() => Played);
            }
        }
        #endregion

        #region Constructors
        protected SessionBaseViewModel(IUIMessageBroker messageBroker, IUISession session)
        {
            _messageBroker = messageBroker;
            _sessionData = session;
            OnSessionUpdatedBase(session);
            _updateTimer = new DispatcherTimer();
            _updateTimer.Tick += UpdateView;
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Start();
        }

        private void UpdateView(object sender, EventArgs e)
        {
            OnSessionUpdatedBase(_sessionData);
        }
        #endregion

        private void OnSessionUpdatedBase(IUISessionData sessionData)
        {
            Played = DateTime.UtcNow - sessionData.Started;
            NotifyOfPropertyChange(() => Played);
            OnSessionUpdated(sessionData);
        }
        /// <summary>
        /// Gets called during construction of <see cref="SessionBaseViewModel"/>
        /// and on each update of the Session
        /// </summary>
        /// <param name="session">The Session</param>
        protected abstract void OnSessionUpdated(IUISessionData session);
        public void Handle(IUISessionUpdatedEvent message)
        {
            _sessionData = message.Session;
            OnSessionUpdatedBase(_sessionData);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _messageBroker.Subscribe(this);
        }
        protected override void OnDeactivate(bool close)
        {
            _updateTimer.Stop();
            _messageBroker?.Unsubscribe(this);
            base.OnDeactivate(close);
        }
    }
}