#region Licence
/****************************************************************
 *  Filename: LoginIntentionBaseViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-5
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
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    /// <summary>
    /// Base class standing for <see cref="T:LeapVR.Shell.UI.Abstract.ILoginIntentionViewModel" /> that handles cancelling, expiring and changing when <see cref="P:LeapVR.VBox.Controllers.Interfaces.ISystemController.WhenCultureInfoChange" /> happens.
    /// </summary>

    public abstract class LoginIntentionBaseViewModel : InputControllerScreen, ILoginIntentionViewModel, IHandle<IUILanguageChangedEvent>, IHandle<IUILoginIntentionExpiredEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Func<LoginDecisionType,Task> _sendLoginDecision;
        private readonly IUIMessageBroker _messageBroker;
        private readonly ILoginIntention _loginIntention;
        private readonly DispatcherTimer _countDownTimer = new DispatcherTimer();
        private readonly DateTime _startShowCountdown;
        private readonly object _repeatProtectionLock = new object();
        private int _timeLeftToAutoCancel;
        private bool _displayCountingDown;
        private bool _canSend = true;
        private string _info;

        public UiConfig UiConfig { get; }

        public Guid LoginIntentionId { get; private set; }
        public bool IsCancelled { get; private set; }
        public bool IsExpired { get; private set; }
        public bool IsTimeout { get; private set; }
        /// <summary>
        /// Get how much time left before auto cancel.
        /// </summary>
        public int TimeLeftToAutoCancel
        {
            get => _timeLeftToAutoCancel;
            set
            {
                _timeLeftToAutoCancel = value;
                NotifyOfPropertyChange(() => TimeLeftToAutoCancel);
            }
        }

        /// <summary>
        /// Indicates whether to display the countdown or not
        /// </summary>
        public bool DisplayCountingDown
        {
            get => _displayCountingDown;
            set
            {
                _displayCountingDown = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Get information
        /// </summary>
        public string Information
        {
            get => _info;
            set
            {
                _info = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        protected LoginIntentionBaseViewModel(
            IUIMessageBroker messageBroker,
            ILoginIntention loginIntention,
            IViewInputHandler inputHandler,
            UiConfig uiConfig
            ):base(inputHandler)
        {

            _sendLoginDecision = loginIntention.SendLoginDecisionAsync;
            _loginIntention = loginIntention;
            LoginIntentionId = loginIntention.IntentionId;
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            UiConfig = uiConfig;
            _startShowCountdown = _loginIntention.IntentionConfirmationExpiresOnUtc - TimeSpan.FromSeconds(UiConfig.TimeBeforeDisplayAutoCancelAlarm.TotalSeconds);
           
            _countDownTimer.Tick += new EventHandler(CountDownTimerTick);
            _countDownTimer.Interval = TimeSpan.FromSeconds(1);
            _countDownTimer.Start();
            SetIntentionInfo();
        }
        #endregion

        #region Public Methods
        public async Task Login()
        {
            if (!_canSend) return;
            if (!CanSendRepeateProtection()) return;
            await SendDecision(LoginDecisionType.Confirm);
        }
        public async Task Cancel()
        {
            if (!_canSend) return;
            if(!CanSendRepeateProtection())return;
            await SendDecision(LoginDecisionType.Cancel);
        }
        #endregion

        #region Handler
        public void Handle(IUILanguageChangedEvent message) { SetIntentionInfo(); }
        public void Handle(IUILoginIntentionExpiredEvent message)
        {
            if (message.Intention.IntentionId != _loginIntention.IntentionId)
            {
                Logger.Warn($"Received an LoginIntentionExpired Event with Id={_loginIntention.IntentionId} that does not match registerd Id={_loginIntention.IntentionId}.");
            }
            else
            {
                IsExpired = true;
                Logger.Info($"Login intention with Id={_loginIntention.IntentionId} expired.");
            }
        }
        #endregion

        private async Task SendDecision(LoginDecisionType decision)
        {
            Logger.Debug($"Try to send login decision={decision}.");
            try
            {
                if (decision == LoginDecisionType.Cancel) IsCancelled = true;
                await _sendLoginDecision(decision);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to make login decision.");
            }
        }
        private void SetIntentionInfo()
        {
            try
            {
                var sessionRate = _loginIntention.SessionRate;
                switch (sessionRate)
                {
                    case IPrepaidSessionRate prepaidSession:
                        Information = ShellClientHelper.GetTimeDisplay(prepaidSession.EffectiveDuration);
                        break;
                    case INoBillingSessionRate _:
                        Information = Language.Resources.Global_Session_PlayForFree;
                        break;
                    default:
                        throw new InvalidOperationException($"SessionRate is Unknown Type={sessionRate.GetType()}.");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Failed to change rate display");
            }
        }
        private bool CanSendRepeateProtection()
        {
            lock (_repeatProtectionLock)
            {
                if (!_canSend) return false;
                _canSend = false;
                return true;
            }
        }
        private void CountDownTimerTick(object sender, EventArgs eventArgs)
        {
            if(IsCancelled)
            {
                _countDownTimer.Stop();
                return;
            }

            if(!DisplayCountingDown && DateTime.UtcNow >= _startShowCountdown)
            {
                DisplayCountingDown = true;
            }
            TimeLeftToAutoCancel = (int)(_loginIntention.IntentionConfirmationExpiresOnUtc - DateTime.UtcNow).TotalSeconds;
            if(TimeLeftToAutoCancel > 0) return;
            IsTimeout = true;
            _countDownTimer.Stop();
        }

        public new void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageBroker.Unsubscribe(this);
            }
        }
        public new void Dispose()
        {
            Dispose(true);
            base.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
