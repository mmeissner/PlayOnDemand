#region Licence
/****************************************************************
 *  Filename: SessionViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Core;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    /// <summary>
    /// Implementation of <see cref="SessionBaseViewModel{T}"/> for prepaid session.
    /// </summary>
    public class SessionViewModel : SessionBaseViewModel
    {
        #region Private Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private TimeSpan _timeRemaining;
        private bool _paymentVisible;

        #endregion

        #region Public Properties
        public bool PaymentVisible
        {
            get { return _paymentVisible; }
            set
            {
                if(value == _paymentVisible) return;
                _paymentVisible = value;
                NotifyOfPropertyChange();
            }
        }
        //public decimal Balance
        //{
        //    get => _balance;
        //    set
        //    {
        //        _balance = value;
        //        NotifyOfPropertyChange(() => Balance);
        //        NotifyOfPropertyChange(() => BalanceString);
        //    }
        //}

        //public string BalanceString => _balance.ToString("0.############");
        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                NotifyOfPropertyChange(() => TimeRemaining);
            }
        }
        #endregion

        #region Constructors
        public SessionViewModel(IUIMessageBroker messageBroker, IUISession session) : base(messageBroker,session){}
        #endregion

        #region Methods
        protected override void OnSessionUpdated(IUISessionData session)
        {
            if(session.Type == SessionType.Limited)
            {
                if(session.SessionRate is IPrepaidSessionRate prepaidRate)
                {
                    TimeRemaining = session.Started.Add(prepaidRate.EffectiveDuration) - DateTime.UtcNow;

                    //Session could have been upgraded by Server with a PrepaidSessionRate
                    PaymentVisible = true;
                }
                else
                {
                    //In Case a session would be downgraded by Server to a NonPaid Session
                    PaymentVisible = false;
                }
                
            }
            else
            {
                //In Case a session would be downgraded by Server to a NonPaid Session
                PaymentVisible = false;
            }
        }
    }
    #endregion
}