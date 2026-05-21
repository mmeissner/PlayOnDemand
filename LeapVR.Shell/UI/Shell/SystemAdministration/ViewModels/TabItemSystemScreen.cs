#region Licence
/****************************************************************
 *  Filename: TabItemSystemScreen.cs
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
using System.Linq;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels
{
    /// <summary>
    /// Representing a view model object contained by the <see cref="Conductor{T}" /> of system settings <see cref="TabControl"/>. This is an abstract class with basic implementation of <see cref="ITabItemSystemScreen"/>.
    /// </summary>
    public abstract class TabItemSystemScreen : TabItemScreen, ITabItemSystemScreen
    {
        protected TabItemSystemScreen(IUIMessageBroker messageBroker,string iconKey) : base(messageBroker,iconKey) { }
    }
    /// <summary>
    /// Representing a view model that contains child screens implementing <see cref="IScreen"/> interfaces. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">should be a class and derived from <see cref="IScreen"/></typeparam>
    public abstract class TabItemSystemConductor<T> : TabItemConductor<T>, ITabItemSystemScreen where T : class, IScreen
    {
        protected TabItemSystemConductor(IUIMessageBroker messageBroker,string iconKey) : base(messageBroker,iconKey) { }

        protected override void OnActivate()
        {
            if (Items.Count > 0) ActivateItem(Items.First());
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            DeactivateItem(ActiveItem, false);
            base.OnDeactivate(close);
        }
    }
   
}
