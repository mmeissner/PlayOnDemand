#region Licence
/****************************************************************
 *  Filename: LoginQrCodeBaseViewModel.cs
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
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    public abstract class LoginQrCodeBaseViewModel : InputControllerScreen, IDisposable, IHandle<IUISessionSetupChangedEvent>, IHandle<IUILanguageChangedEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private ImageSource _qrCode;
        private string _qrCodeUrl;
        private double? _qrCodeWith;
        private string _tip;
        public double? QrCodeWidth
        {
            get => _qrCodeWith;
            set
            {
                if (value.Equals(_qrCodeWith)) return;
                _qrCodeWith = value;
                NotifyOfPropertyChange();
            }
        }
        public ImageSource QrCode
        {
            get => _qrCode;
            set
            {
                _qrCode = value;
                NotifyOfPropertyChange();
            }
        }
        public string Tip
        {
            get => _tip;
            set
            {
                _tip = value;
                NotifyOfPropertyChange();
            }
        }
        public LanguageSelectViewModel LanguageSelect { get;} 
        #endregion
        public LoginQrCodeBaseViewModel(
            QrCodeLoginSettingsBase qrCodeSettings,
            IUIMessageBroker messageBroker,
            LanguageSelectViewModel languageSelectViewModel,
            IViewInputHandler viewInputHandler,
            double? qrCodeWidth
        ):base(viewInputHandler)
        {
            LanguageSelect = languageSelectViewModel;
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _qrCodeWith = qrCodeWidth ?? 400;
            UpdateQrCodeSettings(qrCodeSettings);
            AddControllerInputAction(ControllerInput.NextTwo, languageSelectViewModel.NavigateToNext);
            AddControllerInputAction(ControllerInput.PreviousTwo, languageSelectViewModel.NavigateToPrevious);
        }

        public void Handle(IUISessionSetupChangedEvent message)
        {
            if (message.Settings is QrCodeLoginSettingsBase qrCodeSessionSetup)
            {
                UpdateQrCodeSettings(qrCodeSessionSetup);
            }
            else
            {
                Logger.Debug($"{nameof(UISessionSetupChangedEvent)} ignored as it is not a {nameof(QrCodeLoginSettingsBase)}");
            }
        }

        private void UpdateQrCodeSettings(QrCodeLoginSettingsBase qrCodeSessionSetup)
        {
            Logger.Info($"Updateing QR Code Old={_qrCodeUrl} QR Code= {qrCodeSessionSetup.QrUrl}");
            _qrCodeUrl = qrCodeSessionSetup.QrUrl;
            if(string.IsNullOrWhiteSpace(qrCodeSessionSetup.QrUrl))
            {
                QrCode = null;
                Tip = "";
            }
            else
            {
                QrCode = ShellClientHelper.GenerateQrCode(qrCodeSessionSetup.QrUrl);
                Tip = Resources.StartTip_QRCode;
            }
        }

        public new void Dispose()
        {
            _messageBroker?.Unsubscribe(this);
            LanguageSelect?.Dispose();
            base.Dispose();
        }

        public void Handle(IUILanguageChangedEvent message)
        {
            //Show only QR Code tooltip if there is a qr code
            Tip = string.IsNullOrWhiteSpace(_qrCodeUrl) ? "" : Resources.StartTip_QRCode;
        }
    }
}
