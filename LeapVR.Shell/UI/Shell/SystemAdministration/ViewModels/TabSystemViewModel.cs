#region Licence
/****************************************************************
 *  Filename: TabSystemViewModel.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Linq;
using System.Windows.Input;
using LeapVR.Shell.Language;
using LeapVR.Shell.Properties;
using LeapVR.Shell.UI.Abstract;
using LeapVR.VBox.Controllers.Interfaces;
using LeapVR.VBox.Controllers;
using LeapVR.Universal.WPF.Utilities;
using LeapVR.Shared.NetStandard;
using LeapVR.Shell.Core;

namespace LeapVR.Shell.UI.VrDashboard.SystemAdministration.ViewModels
{
    [Obsolete("moved to AdministrationViewModel")]
    public sealed class TabSystemViewModel : TabItemConductor<ITabItemSystemScreen>
    {
        #region Fields & Properties
        private readonly ISystemController _systemController;
        private readonly ISecurityController _securityController;
        private volatile bool _isPinInputing;
        private readonly TimeoutTimer _inactivityTimer;

        private string _pinDisplay;
        public string PinDisplay
        {
            get => string.IsNullOrEmpty(_pinDisplay) ? Resources.System_InputPin : _pinDisplay;
            private set
            {
                _pinDisplay = value;
                NotifyOfPropertyChange();
            }
        }
        private bool _isSystemLocked = true;
        public bool IsSystemLocked
        {
            get => _isSystemLocked;
            set
            {
                _isSystemLocked = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isInactivityTimeout;
        public bool IsInactivityTimeout
        {
            get => _isInactivityTimeout;
            set
            {
                _isInactivityTimeout = value;
                NotifyOfPropertyChange();
            }
        }


        public override string DisplayName
        {
            get { return Resources.Global_SystemAdministration; }
            set { /* ignore */ }
        }

        public string CurrentCultureName => _systemController.CurrentCulture?.Name;


        #endregion

        #region Constructors

        public TabSystemViewModel(ISystemController systemController, ISecurityController securityController, IEnumerable<ITabItemSystemScreen> tabs) : base("IconSystem", systemController)
        {
            _systemController = systemController;
            _securityController = securityController;
            _systemController.WhenCultureInfoChange.ObserveOnDispatcher().Subscribe(OnCurrentCultureChanged);


            _inactivityTimer = new TimeoutTimer(_securityController.SystemInactivityTimeout);
            _inactivityTimer.WhenTimeout.ObserveOnDispatcher().Subscribe(OnInactivityChange);
            _inactivityTimer.Start();

            Items.AddRange(tabs.OrderByDescending(tab => tab.DisplayOrder));
        }

        #endregion

        #region Methods

        protected override void OnActivate()
        {
            PinDisplay = string.Empty;
            if (!_securityController.IsSecurityEnabled)
            {
                UnlockSystem();
            }
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            LockSystem();
            base.OnDeactivate(close);
        }

        protected override ITabItemSystemScreen EnsureItem(ITabItemSystemScreen newItem)
        {
            //_inactivityTimer.Reset();
            return base.EnsureItem(newItem);
        }

        public void OnKeyPadValueChanged(string value)
        {
            _inactivityTimer.Reset();

            if (!_isPinInputing)
            {
                _isPinInputing = true;
            }

            if (string.IsNullOrEmpty(value))
            {
                PinDisplay = string.Empty;
                return;
            }

            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                // There is no way to input characters other than digitals.
                sb.Append(char.IsDigit(c) ? '*' : c);
            }
            PinDisplay = sb.ToString();
        }
        public async void OnKeyPadInputComplete(string result)
        {
            if (!string.IsNullOrEmpty(result) && _securityController.VerifyPin(result))
            {
                UnlockSystem();
            }
            else
            {
                _isPinInputing = false;

                // show reminding message to the user.
                PinDisplay = Resources.System_InvalidPin;

                // then start a new timer that ticks every interval.
                // If user had input a new char or time is up, reset the result and state.
                var sw = Stopwatch.StartNew();
                var totalMiliseconds = 3000;
                var loopInterval = 50;
                while (sw.ElapsedMilliseconds < totalMiliseconds)
                {
                    if (_isPinInputing)
                    {
                        return;
                    }
                    await Task.Delay(loopInterval);
                }
                PinDisplay = string.Empty;
                _isPinInputing = true;
            }
        }
        public void UnlockSystem()
        {
            PinDisplay = string.Empty;
            IsSystemLocked = false;
            _inactivityTimer?.Stop();
        }

        public void LockSystem()
        {
            DeactivateItem(ActiveItem, false);
            PinDisplay = string.Empty;
            IsSystemLocked = true;
            if (Items.Count > 0) ActivateItem(Items.First());
        }

        public void Chinese()
        {
            _systemController.SetCultureInfo("zh-CN");
        }

        public void English()
        {
            _systemController.SetCultureInfo("en-US");
        }

        private void OnInactivityChange(Empty empty)
        {
            IsInactivityTimeout = true;
        }

        private void OnCurrentCultureChanged(CultureInfo cultureInfo)
        {
            NotifyOfPropertyChange(() => CurrentCultureName);
            NotifyOfPropertyChange(() => PinDisplay);
        }

        #endregion

    }

    public enum PlaceholderChar
    {
        Default,
        Asterisk,
        Dot
    }

}
