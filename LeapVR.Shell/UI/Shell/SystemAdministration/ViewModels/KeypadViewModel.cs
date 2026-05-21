#region Licence
/****************************************************************
 *  Filename: KeypadViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-29
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
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.Views;
using LeapVR.Shell.UI.Usercontrols;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels
{
    public class KeypadViewModel : Screen, IHandle<IUILanguageChangedEvent>
    {
        #region Fields & Properties
        private readonly ILanguageSelector _languageSelector;
        private readonly ISecurityController _securityController;
        private readonly IUIMessageBroker _messageBroker;
        private KeyPad _keyPad;

        private volatile bool _isPinInputing;
        private string _pinDisplay;
        private DateTime _lastTimeKeyPressed;


        public override string DisplayName
        {
            get { return Resources.Global_SystemAdministration; }
            set { /* ignore */ }
        }


        public string PinDisplay
        {
            get => string.IsNullOrEmpty(_pinDisplay) ? Resources.System_InputPin : _pinDisplay;
            set
            {
                _pinDisplay = value;
                NotifyOfPropertyChange();
            }
        }

        public string CurrentCultureName => _languageSelector.CurrentCulture?.Name;

        public DateTime LastTimeKeyPressed
        {
            get { return _lastTimeKeyPressed; }
            set
            {
                if(value.Equals(_lastTimeKeyPressed)) return;
                _lastTimeKeyPressed = value;
                NotifyOfPropertyChange(() => LastTimeKeyPressed);
            }
        }
        #endregion

        #region Constructors

        public KeypadViewModel(ILanguageSelector
            languageSelector,IUIMessageBroker messageBroker,
            ISecurityController securityController)
        {
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _languageSelector = languageSelector;
            _securityController = securityController;
        }
        #endregion

        #region Methods

        public void ResetKeyPad()
        {
            _keyPad.Value = string.Empty;
            PinDisplay = string.Empty;
            LastTimeKeyPressed = DateTime.Now;
        }
        public void Chinese()
        {
            _lastTimeKeyPressed = DateTime.Now;
            _languageSelector?.ActivateCultureInfo(new CultureInfo("zh-CN"));
        }

        public void English()
        {
            _lastTimeKeyPressed = DateTime.Now;
            _languageSelector?.ActivateCultureInfo(new CultureInfo("en-US"));
        }

        public void Handle(IUILanguageChangedEvent uiLanguageChangedEvent)
        {
            ReactOnCultureInfoChanged(uiLanguageChangedEvent.NewCultureInfo);
        }

        private void ReactOnCultureInfoChanged(CultureInfo cultureInfo)
        {
            NotifyOfPropertyChange(() => CurrentCultureName);
            NotifyOfPropertyChange(() => PinDisplay);
        }

        public void OnKeypadValueChanged(string value)
        {
            _lastTimeKeyPressed = DateTime.Now;
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

        public async void OnKeypadInputComplete(string result)
        {
            _lastTimeKeyPressed = DateTime.Now;
            var unlockResult = _securityController.UnlockAdminAccess(result);
            if (!unlockResult)
            {
                //Set Flag to see if user restarted to input chars
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
                    //User made meanwhile an input, return and dont reset display
                    if (_isPinInputing)
                    {
                        return;
                    }
                    await Task.Delay(loopInterval);
                }
                //User made for some time no input, now we remove the error message
                PinDisplay = string.Empty;
                _isPinInputing = true;
            }
        }
        #endregion

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            _keyPad = ((KeypadView)view).KeyPad;
        }
    }
}
