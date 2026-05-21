#region Licence
/****************************************************************
 *  Filename: AdministrationConductorViewModel.cs
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

using System.Collections.Generic;
using System.Linq;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.SystemAdministration.Security.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels
{
    public class AdministrationConductorViewModel : TabItemConductor<ITabItemSystemScreen>
    {
        #region Constructors
        private readonly TabItemSecurityViewModel _securityTab;
        private bool _bHasSecurityTab;
        public AdministrationConductorViewModel(IUIMessageBroker messageBroker, ISecurityController securityController, IEnumerable<ITabItemSystemScreen> tabs) : base(messageBroker,"IconSystem")
        {
            var tabsToUse = new List<ITabItemSystemScreen>(tabs.OrderByDescending(tab => tab.DisplayOrder));
            securityController.WhenSecurityChanged += SecurityController_WhenSecurityChanged;
            //Remove ChangePin when Security is disabled

            foreach (ITabItemSystemScreen tab in tabs)
            {
                if (tab is TabItemSecurityViewModel securityTab)
                {
                    _securityTab = securityTab;

                    if(!securityController.IsSecurityEnabled) tabsToUse.Remove(securityTab);
                    else _bHasSecurityTab = true;
                    break;
                }
            }
            
            Items.AddRange(tabsToUse);
           
        }

        private void SecurityController_WhenSecurityChanged(bool isEnabled)
        {
            if(isEnabled == _bHasSecurityTab)return;
            if(isEnabled)
            {
                //Add
                for(int i = 0; i < Items.Count; i++)
                {
                    //Continue as long the other tabs have priority and its not the last tab
                    if(Items[i].DisplayOrder > _securityTab.DisplayOrder && (i != Items.Count -1))continue;
                    Items.Insert(i,_securityTab);
                    _bHasSecurityTab = true;
                    break;
                }
            }
            else
            {
                //Remove
                Items.Remove(_securityTab);
                _bHasSecurityTab = false;
            }
        }
        #endregion
    }
}
