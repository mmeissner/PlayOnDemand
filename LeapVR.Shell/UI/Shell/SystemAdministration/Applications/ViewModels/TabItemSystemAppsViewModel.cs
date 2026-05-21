#region Licence
/****************************************************************
 *  Filename: TabItemSystemAppsViewModel.cs
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

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.ViewModels
{
    public class TabItemSystemAppsViewModel : TabItemSystemConductor<ITabItemAppManagementScreen>, ITabItemSystemScreen
    {
        public override int DisplayOrder => 20;
        public override string DisplayName
        {
            get { return Resources.System_Apps; }
            set { /* ignore */ }
        }

        public TabItemSystemAppsViewModel(IUIMessageBroker messageBroker,IEnumerable<ITabItemAppManagementScreen> tabs) : base(messageBroker,"IconStatisticsApplications")
        {
            Items.AddRange(tabs.OrderByDescending(tab => tab.DisplayOrder));
        }

        protected override ITabItemAppManagementScreen EnsureItem(ITabItemAppManagementScreen newItem)
        {
            var item = base.EnsureItem(newItem);
            return item;
        }

    }
}
