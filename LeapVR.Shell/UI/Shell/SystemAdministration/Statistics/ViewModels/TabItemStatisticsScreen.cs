#region Licence
/****************************************************************
 *  Filename: TabItemStatisticsScreen.cs
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
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels
{
    /// <summary>
    /// Representing a view model object contained by the <see cref="Conductor{T}" /> of statistics <see cref="TabControl"/>. This is an abstract class with basic implementation of <see cref="ITabItemStatisticsScreen"/>.
    /// </summary>
    public abstract class TabItemStatisticsScreen : TabItemScreen, ITabItemStatisticsScreen
    {
        protected TabItemStatisticsScreen(IUIMessageBroker messageBroker,string iconKey) : base(messageBroker,iconKey)
        {

        }
    }
}
