#region Licence
/****************************************************************
 *  Filename: StatusBarViewModel.cs
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
using System.ServiceModel.Channels;
using Caliburn.Micro;
using Humanizer;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Language;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class StatusBarViewModel : Conductor<Screen>.Collection.AllActive
        , IStatusBarViewModel
        , IDisposable
    {

        #region Fields & Properties
        private readonly IUIMessageBroker _messageBroker;
        public ClockViewModel ClockViewModel { get; set; }
        public ConnectionIndicatorViewModel ConnectionIndicatorViewModel { get; set; }
        public LanguageSelectViewModel LanguageSelectViewModel { get; set; }
        public StationDetailsViewModel StationDetailsViewModel { get; set; }
        #endregion

        #region Constructors

        public StatusBarViewModel(
            IUIMessageBroker messageBroker,
            ConnectionIndicatorViewModel connectionIndicatorViewModel,
            LanguageSelectViewModel languageSelectViewModel,
            StationDetailsViewModel stationDetailsViewModel,
            ClockViewModel clockViewModel
            )
        {
            QuickLeap.AssertNotNull(
                connectionIndicatorViewModel,
                languageSelectViewModel,
                stationDetailsViewModel,
                clockViewModel
                );
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);

            ClockViewModel = clockViewModel;
            ConnectionIndicatorViewModel = connectionIndicatorViewModel;
            LanguageSelectViewModel = languageSelectViewModel;
            StationDetailsViewModel = stationDetailsViewModel;
            Items.AddRange(new Screen[]{ClockViewModel,ConnectionIndicatorViewModel,StationDetailsViewModel,LanguageSelectViewModel});
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            _messageBroker.Unsubscribe(this);
            ClockViewModel?.Dispose();
            ConnectionIndicatorViewModel?.Dispose();
            LanguageSelectViewModel?.Dispose();
        }

        #endregion
    }
}
