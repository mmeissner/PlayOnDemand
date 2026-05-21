#region Licence
/****************************************************************
 *  Filename: TabItemSystemStatisticsViewModel.cs
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
using System.Linq;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels
{
    public sealed class TabItemSystemStatisticsViewModel : TabItemSystemConductor<ITabItemStatisticsScreen>, ITabItemSystemScreen
    {

        #region Fields & Properties
        public override int DisplayOrder => 8;
        public override string DisplayName
        {
            get { return Resources.System_Statistics; }
            set { /* ignore */ }
        }

        #endregion

        #region Constructors
        public TabItemSystemStatisticsViewModel(IUIMessageBroker messageBroker,IEnumerable<ITabItemStatisticsScreen> tabs) : base(messageBroker,"IconStatistics")
        {
            Items.AddRange(tabs.OrderByDescending(tab => tab.DisplayOrder));
        }
        #endregion
    }
}
