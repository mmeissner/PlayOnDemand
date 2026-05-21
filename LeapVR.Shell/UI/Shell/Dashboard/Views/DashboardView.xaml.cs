#region Licence
/****************************************************************
 *  Filename: DashboardView.xaml.cs
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

using System.Windows;
using System.Windows.Controls;

namespace LeapVR.Shell.UI.Shell.Dashboard.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            this.Loaded += DashboardView_Loaded;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            //NoApplicationAvailableTips.Focus();
        }
    }
}
