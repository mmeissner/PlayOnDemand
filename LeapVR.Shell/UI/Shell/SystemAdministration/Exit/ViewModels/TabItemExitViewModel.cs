#region Licence
/****************************************************************
 *  Filename: TabItemExitViewModel.cs
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

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Exit.ViewModels
{
    public sealed class TabItemExitViewModel : TabItemSystemScreen
    {
        #region Fields & Properties

        private readonly Action _requestShutdown;
        public override int DisplayOrder => 1;
        public override string DisplayName
        {
            get { return Resources.System_Exit; }
            set { /* ignore */ }
        }
        #endregion

        #region Constructors

        public TabItemExitViewModel(IUIMessageBroker messageBroker,IStationController stationController) : base(messageBroker,"IconExit")
        {
            _requestShutdown = stationController.RequestShutdown;
        }
        protected override void HandleLanguageChange(IUILanguageChangedEvent message)
        {
        }
        #endregion

        #region Methods

        public void Exit()
        {
            _requestShutdown.Invoke();
        }

        #endregion
    }
}
