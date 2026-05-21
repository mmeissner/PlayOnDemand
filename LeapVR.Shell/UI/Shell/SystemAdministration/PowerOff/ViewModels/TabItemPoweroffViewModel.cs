#region Licence
/****************************************************************
 *  Filename: TabItemPoweroffViewModel.cs
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
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.PowerOff.ViewModels
{
    public sealed class TabItemPoweroffViewModel : TabItemSystemScreen
    {
        #region Fields & Properties

        private readonly Action _requestPowerOff;
        public override int DisplayOrder => 0;
        public override string DisplayName
        {
            get { return Resources.System_Poweroff; }
            set { /* ignore */ }
        }
        #endregion

        public TabItemPoweroffViewModel(IUIMessageBroker messageBroker,IStationController stationController) : base(messageBroker,"IconPoweroff")
        {
            _requestPowerOff = stationController.RequestPowerOff;
        }

        public void Poweroff()
        {
            _requestPowerOff.Invoke();
        }
        protected override void HandleLanguageChange(IUILanguageChangedEvent message) { }
    }
}
