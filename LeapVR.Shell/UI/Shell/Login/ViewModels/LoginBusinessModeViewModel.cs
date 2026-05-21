#region Licence
/****************************************************************
 *  Filename: LoginBusinessModeViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-14
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    public class LoginBusinessModeViewModel : LoginQrCodeBaseViewModel, ILoginModeViewModel
    {
        #region Constructors
        public LoginBusinessModeViewModel(
            QrCodeLoginSettingsBase qrCodeSettings,
            IUIMessageBroker messageBroker,
            LanguageSelectViewModel languageSelectViewModel,
            IViewInputHandler inputHandler,
            double? qrCodeWidth) : base(qrCodeSettings, messageBroker,languageSelectViewModel,inputHandler, qrCodeWidth) { }
        #endregion

        public LoginMode Mode { get; } = LoginMode.BusinessMode;
    }
}