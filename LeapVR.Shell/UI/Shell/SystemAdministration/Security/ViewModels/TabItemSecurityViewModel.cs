#region Licence
/****************************************************************
 *  Filename: TabItemSecurityViewModel.cs
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
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Security.ViewModels
{
    public class TabItemSecurityViewModel : TabItemSystemScreen
    {

        #region Fields & Properties

        private readonly ISecurityController _securityController;
        private string _newPin = string.Empty;
        private PinChangePhase _phase;
        public override int DisplayOrder => 3;
        public override string DisplayName
        {
            get { return Resources.System_Settings_Security; }
            set { /* ignore */ }
        }
        private string _pinDisplay;
        public string PinDisplay
        {
            get => _pinDisplay;
            set
            {
                _pinDisplay = value;
                NotifyOfPropertyChange();
            }
        }

        private string _instructionMessage;
        public string InstructionMessage
        {
            get => _instructionMessage;
            set
            {
                _instructionMessage = value;
                NotifyOfPropertyChange();
            }
        }


        private bool _locked;
        public bool Locked
        {
            get => _locked;
            set
            {
                _locked = value;
                NotifyOfPropertyChange();
            }
        }


        #endregion

        #region Constructors
        public TabItemSecurityViewModel(IUIMessageBroker messageBroker,ISecurityController securityController) : base(messageBroker,"IconKey")
        {
            _securityController = securityController;
        }
        protected override void HandleLanguageChange(IUILanguageChangedEvent message) { }
        #endregion

        #region Methods

        protected override void OnActivate()
        {
            _newPin = string.Empty;
            _phase = PinChangePhase.EnterOldPin;
            PinDisplay = string.Empty;
            InstructionMessage = Resources.System_Settings_Security_EnterOldPin;
            Locked = false;
            base.OnActivate();
        }

        public void OnKeyPadValueChanged(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                PinDisplay = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                // There is no way to input characters other than digitals.
                sb.Append(char.IsDigit(c) ? '*' : c);
            }
            PinDisplay = sb.ToString();

        }

        public async void OnKeyPadInputComplete(string result)
        {
            switch (_phase)
            {
                case PinChangePhase.EnterOldPin:
                    var isValid = _securityController.UnlockAdminAccess(result);
                    if (isValid)
                    {
                        _phase = PinChangePhase.EnterNewPin;
                        InstructionMessage = Resources.System_Settings_Security_EnterNewPin;
                    }
                    else
                    {
                        _phase = PinChangePhase.EnterOldPin;
                        Locked = true;
                        InstructionMessage = Resources.System_Settings_Security_FailedToVerifyOldPin;
                        await Task.Delay(3000);
                        InstructionMessage = Resources.System_Settings_Security_EnterOldPin;
                    }
                    break;
                case PinChangePhase.EnterNewPin:
                    _newPin = result;
                    _phase = PinChangePhase.EnterNewPinAgain;
                    InstructionMessage = Resources.System_Settings_Security_EnterNewPinAgain;
                    break;
                case PinChangePhase.EnterNewPinAgain:
                    var isEqual = _newPin.Equals(result);
                    if (isEqual)
                    {
                        _phase = PinChangePhase.PinChangeSucceed;
                        InstructionMessage = Resources.System_Settings_Security_PinChangeSuccessful;
                        _securityController.SetSecurityCode(_newPin);
                    }
                    else
                    {
                        InstructionMessage = Resources.System_Settings_Security_PinNotEqual;
                    }

                    Locked = true;
                    await Task.Delay(3000);
                    _phase = PinChangePhase.EnterOldPin;
                    InstructionMessage = Resources.System_Settings_Security_EnterOldPin;

                    break;
            }
            Locked = false;
            PinDisplay = string.Empty;
        }

        #endregion
    }

    internal enum PinChangePhase
    {
        EnterOldPin,
        EnterNewPin,
        EnterNewPinAgain,
        PinChangeSucceed
    }
}
