#region Licence
/****************************************************************
 *  Filename: AdministrationViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-16
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
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels
{
    public class AdministrationViewModel : InputControllerScreen,
        IAdministrationViewModel,
        IHandle<UIAdminAccessApprovedEvent>

    {
        #region Fields & Properties

        private KeypadViewModel _keypadViewModel;
        private AdministrationConductorViewModel _administrationConductorViewModel;
        private readonly KeypadViewModel _keypad;
        private readonly IUIMessageBroker _messageBroker;
        private readonly ISecurityController _securityController;
        private readonly DispatcherTimer _administrationAccessTimeoutTimer;

        public KeypadViewModel KeypadViewModel
        {
            get => _keypadViewModel;
            set
            {
                _keypadViewModel = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsAdministrationAccessApproved);
            }
        }

        public AdministrationConductorViewModel AdministrationConductorViewModel
        {
            get => _administrationConductorViewModel;
            set
            {
                _administrationConductorViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsAdministrationAccessApproved => KeypadViewModel == null;

        #endregion

        #region Constructors
        public AdministrationViewModel(
            IViewInputHandler viewInputHandler,
            IUIMessageBroker messageBroker,
            ISecurityController securityController,
            KeypadViewModel keypadViewModel,
            AdministrationConductorViewModel administrationConductorViewModel):base(viewInputHandler)
        {
            QuickLeap.AssertNotNull(
                messageBroker,
                securityController,
                keypadViewModel,
                administrationConductorViewModel);

            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _keypad = keypadViewModel;
            _securityController = securityController;
            _administrationConductorViewModel = administrationConductorViewModel;
            _administrationAccessTimeoutTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _administrationAccessTimeoutTimer.Tick += OnAdministrationAccessCheck;
            AddControllerInputAction(ControllerInput.Cancel, Back);
        }
        #endregion

        #region Methods
        protected override void OnActivate()
        {
            if(!_securityController.IsSecurityEnabled)
            {
                UnlockSystem();
            }
            else
            {
                KeypadViewModel = _keypad;
                KeypadViewModel.ResetKeyPad();
                _administrationAccessTimeoutTimer.Start();
            }
            base.OnActivate();
        }

        private void OnAdministrationAccessCheck(object timer,EventArgs args)
        {
            if (_keypadViewModel == null)
            {
                _administrationAccessTimeoutTimer.Stop();
                return;
            }
            if (_keypadViewModel.LastTimeKeyPressed.Add(_securityController.SystemInactivityTimeout) <=
                DateTime.Now)
            {
                _administrationAccessTimeoutTimer.Stop();
                _messageBroker.Publish(new UIAdminAccessDismissEvent(ViewDismissReason.Timeout));
            }
        }

        public void Handle(UIAdminAccessApprovedEvent @event)
        {
            UnlockSystem();
        }

        public void Back()
        {
            _administrationAccessTimeoutTimer.Stop();
            var message = new UIAdminAccessDismissEvent(ViewDismissReason.ActivelyClose);
            _messageBroker.Publish(message);
        }

        public void UnlockSystem()
        {
            KeypadViewModel = null;
            _administrationAccessTimeoutTimer?.Stop();
            ScreenExtensions.TryActivate(AdministrationConductorViewModel);
        }
        #endregion
    }
}
